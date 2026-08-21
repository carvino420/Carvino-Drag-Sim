using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carvino
{
    public sealed class PrototypeRaceController : MonoBehaviour
    {
        private enum RaceState { Garage, Staged, Countdown, Racing, Finished, RedLight, Failed }

        [SerializeField] private Transform vehicleVisual;
        [SerializeField] private Transform hatchVisual;
        [SerializeField] private Transform pickupVisual;
        [SerializeField] private Transform opponentVisual;
        [SerializeField] private FollowCamera followCamera;
        [SerializeField] private EngineAudioSynth engineAudio;
        [SerializeField] private Renderer[] treeBulbs;
        [SerializeField] private GameObject[] exhaustFlashes;
        private DragBuild build;
        private DragSimulation simulation;
        private TrackSurfaceSpec trackSurface;
        private RaceDistanceSpec raceDistance;
        private DragBuild opponentBuild;
        private DragSimulation opponentSimulation;
        private RaceState state;
        private float stateTimer;
        private int gear = 1;
        private bool upgradesInstalled;
        private string selectedVehicle = "hatch";
        private string selectedEngine = "d16";
        private float greenTimestamp;
        private float reactionSeconds = -1f;
        private bool opponentFinished;
        private bool opponentFailed;
        private int opponentGear;
        private float opponentReactionSeconds;
        private float opponentReferenceEt;
        private float opponentReferenceMph;
        private bool burningOut;
        private bool payoutAwarded;
        private int payout;
        private float exhaustFlashTimer;
        private bool personalBest;
        private RaceEvent raceEvent;
        private float OpponentReactionSeconds => opponentReactionSeconds;
        private float OpponentEtSeconds => opponentReferenceEt > 0f ? opponentReferenceEt : (raceEvent != null ? raceEvent.opponentEtSeconds : 12.8f);
        public bool IsBurningOut => burningOut;

        private void Start()
        {
            GarageSession.Load();
            selectedVehicle = GarageSession.VehicleId;
            selectedEngine = GarageSession.EngineId;
            upgradesInstalled = GarageSession.StarterUpgrades;
            raceEvent = RaceEventSession.Selected;
            ResetRun();
        }

        private void Update()
        {
            if (CarvinoInput.HatchPressed) { selectedVehicle = "hatch"; selectedEngine = "d16"; ResetRun(); }
            if (CarvinoInput.PickupPressed) { selectedVehicle = "pickup"; selectedEngine = "v6_43"; ResetRun(); }
            if (CarvinoInput.NextEnginePressed) CycleEngine();
            if (CarvinoInput.ToggleUpgradesPressed) { upgradesInstalled = !upgradesInstalled; ResetRun(); }
            if (CarvinoInput.ResetPressed) ResetRun();
            burningOut = state == RaceState.Garage && Input.GetKey(KeyCode.F);
            if (burningOut) simulation.Burnout(Time.deltaTime);
            if (CarvinoInput.StagePressed && state == RaceState.Garage) StartCountdown();
            if (CarvinoInput.LaunchPressed) TryLaunch();
            if (CarvinoInput.ShiftPressed) ShiftUp();
            if ((state == RaceState.Finished || state == RaceState.RedLight || state == RaceState.Failed) && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton6)))
                SceneManager.LoadScene("Garage");

            if (state == RaceState.Staged || state == RaceState.Countdown) TickCountdown();
            exhaustFlashTimer = Mathf.Max(0f, exhaustFlashTimer - Time.deltaTime);
            if (exhaustFlashes != null)
                foreach (GameObject flash in exhaustFlashes)
                    if (flash != null) flash.SetActive(exhaustFlashTimer > 0f && flash.transform.root.gameObject.activeInHierarchy);
            if (state == RaceState.Racing)
            {
                UpdateOpponent();
                if (!simulation.Finished)
                {
                    if (reactionSeconds < 0f && CarvinoInput.Throttle > 0.01f) reactionSeconds = Time.time - greenTimestamp;
                    simulation.Step(Time.deltaTime, CarvinoInput.Throttle, gear);
                    vehicleVisual.position = new Vector3(-3.8f, 0.05f, simulation.DistanceMeters);
                    if (simulation.State.IsFailed)
                    {
                        state = RaceState.Failed;
                        AwardEngineFailure();
                    }
                    else if (simulation.Finished)
                    {
                        state = RaceState.Finished;
                        AwardRacePayout();
                    }
                }
            }
            if (engineAudio != null) engineAudio.SetEngineState(simulation.EngineRpm, CarvinoInput.Throttle, state == RaceState.Racing);
        }

        private void ResetRun()
        {
            VehicleSpec vehicle = CarvinoCatalog.Vehicles[selectedVehicle == "hatch" ? 0 : 1];
            build = new DragBuild { vehicle = vehicle, engine = CarvinoCatalog.FindEngine(selectedEngine), engineIsNew = GarageSession.EngineIsNew, engineHealth = GarageSession.GetEngineHealth(selectedEngine, GarageSession.EngineIsNew) };
            build.tune = GarageSession.Tune;
            build.launchRpm = build.tune.launchRpm;
            build.shiftRpm = build.tune.shiftRpm;
            if (upgradesInstalled && GarageSession.UpgradeMask != 0) GarageSession.ApplyUpgrades(build);
            else if (upgradesInstalled)
            {
                build.upgrades.Add(CarvinoCatalog.Upgrades[0]);
                build.upgrades.Add(CarvinoCatalog.Upgrades[1]);
                build.upgrades.Add(CarvinoCatalog.Upgrades[3]);
            }
            trackSurface = RaceSurfaceSession.Selected;
            raceDistance = RaceDistanceSession.Selected;
            simulation = new DragSimulation(build, trackSurface, raceDistance.meters);
            CreateOpponentRun();
            if (engineAudio != null) engineAudio.Configure(build.engine);
            SetPlayerVisual();
            state = RaceState.Garage;
            stateTimer = 0f;
            gear = 1;
            reactionSeconds = -1f;
            opponentFinished = false;
            opponentFailed = false;
            opponentGear = 1;
            payoutAwarded = false;
            payout = 0;
            personalBest = false;
            exhaustFlashTimer = 0f;
            SetTree(-1);
            if (vehicleVisual != null)
            {
                vehicleVisual.position = new Vector3(-3.8f, 0.05f, 0f);
                vehicleVisual.localScale = Vector3.one;
            }
            if (opponentVisual != null) opponentVisual.position = new Vector3(3.8f, 0.05f, 0f);
        }

        private void SetPlayerVisual()
        {
            bool hatchSelected = selectedVehicle == "hatch";
            if (hatchVisual != null) hatchVisual.gameObject.SetActive(hatchSelected);
            if (pickupVisual != null) pickupVisual.gameObject.SetActive(!hatchSelected);
            vehicleVisual = hatchSelected ? hatchVisual : pickupVisual;
            if (followCamera != null && vehicleVisual != null) followCamera.SetTarget(vehicleVisual);
        }

        private void CycleEngine()
        {
            var ids = build.vehicle.compatibleEngineIds;
            int index = Mathf.Max(0, ids.IndexOf(selectedEngine));
            selectedEngine = ids[(index + 1) % ids.Count];
            ResetRun();
        }

        private void StartCountdown()
        {
            state = RaceState.Staged;
            stateTimer = 0f;
            SetTree(-1);
        }

        private void TickCountdown()
        {
            stateTimer += Time.deltaTime;
            if (state == RaceState.Staged)
            {
                if (stateTimer < 0.8f) return;
                state = RaceState.Countdown;
                stateTimer = 0f;
            }

            int amber = Mathf.FloorToInt(stateTimer / 0.5f);
            if (amber < 3) SetTree(amber);
            if (stateTimer >= 1.5f)
            {
                state = RaceState.Racing;
                greenTimestamp = Time.time;
                SetTree(3);
            }
        }

        private void TryLaunch()
        {
            if (state == RaceState.Countdown || state == RaceState.Staged)
            {
                state = RaceState.RedLight;
                SetTree(4);
                return;
            }
            if (state == RaceState.Racing) return;
        }

        private void UpdateOpponent()
        {
            if (opponentVisual == null || opponentFinished) return;
            float elapsedFromGreen = Time.time - greenTimestamp;
            if (elapsedFromGreen > OpponentReactionSeconds)
            {
                StepOpponent(opponentSimulation, Time.deltaTime);
                opponentFinished = opponentSimulation.Finished;
                opponentFailed = opponentSimulation.State.IsFailed;
            }
            opponentVisual.position = new Vector3(3.8f, 0.05f, opponentSimulation.DistanceMeters);
        }

        private void CreateOpponentRun()
        {
            AiDriverSpec driver = raceEvent.opponent;
            VehicleSpec vehicle = driver.vehicleId == "pickup" ? CarvinoCatalog.Vehicles[1] : CarvinoCatalog.Vehicles[0];
            opponentBuild = new DragBuild
            {
                vehicle = vehicle,
                engine = CarvinoCatalog.FindEngine(driver.engineId),
                engineIsNew = true,
                engineHealth = 1f,
                tune = new TuneSettings
                {
                    airFuelRatio = driver.airFuelRatio,
                    ignitionTiming = driver.ignitionTiming,
                    launchRpm = driver.launchRpm,
                    shiftRpm = driver.shiftRpm
                }
            };
            for (int index = 0; index < CarvinoCatalog.Upgrades.Count; index++)
                if ((driver.upgradeMask & (1 << index)) != 0) opponentBuild.upgrades.Add(CarvinoCatalog.Upgrades[index]);
            opponentReactionSeconds = driver.reactionSeconds;
            opponentSimulation = new DragSimulation(opponentBuild, trackSurface, raceDistance.meters);
            opponentGear = 1;
            DragSimulation reference = new DragSimulation(opponentBuild, trackSurface, raceDistance.meters);
            int referenceGear = 1;
            for (int frame = 0; frame < 3000 && !reference.Finished && !reference.State.IsFailed; frame++)
            {
                if (reference.EngineRpm >= OpponentShiftRpm(reference) && referenceGear < 5)
                {
                    referenceGear++;
                    reference.BeginShift(opponentBuild.ShiftCutSeconds);
                }
                reference.Step(.02f, OpponentThrottle(reference.ElapsedSeconds), referenceGear);
            }
            opponentReferenceEt = reference.Finished ? reference.ElapsedSeconds : raceEvent.opponentEtSeconds;
            opponentReferenceMph = reference.FinishTrapMph;
        }

        private void StepOpponent(DragSimulation rival, float deltaTime)
        {
            if (rival.EngineRpm >= OpponentShiftRpm(rival) && opponentGear < 5)
            {
                opponentGear++;
                rival.BeginShift(opponentBuild.ShiftCutSeconds);
            }
            rival.Step(deltaTime, OpponentThrottle(rival.ElapsedSeconds), opponentGear);
        }

        private float OpponentThrottle(float elapsed) => elapsed < .22f ? raceEvent.opponent.launchThrottle : 1f;
        private float OpponentShiftRpm(DragSimulation rival)
        {
            float variation = Mathf.Sin(rival.ElapsedSeconds * 4.71f) * raceEvent.opponent.shiftVariationRpm;
            return Mathf.Clamp(raceEvent.opponent.shiftRpm + variation, 3000f, opponentBuild.engine.redlineRpm - 80f);
        }

        private string RaceResult
        {
            get
            {
                if (state == RaceState.Failed) return "ENGINE FAILURE — NO RACE PAYOUT";
                if (state != RaceState.Finished) return string.Empty;
                float playerPackage = simulation.ElapsedSeconds + Mathf.Max(0f, reactionSeconds);
                float rivalPackage = OpponentEtSeconds + OpponentReactionSeconds;
                return playerPackage <= rivalPackage ? "WIN — better package than the rival" : "RIVAL WINS — improve launch, grip, or shifts";
            }
        }

        private bool PlayerWon
        {
            get
            {
                if (state != RaceState.Finished) return false;
                float playerPackage = simulation.ElapsedSeconds + Mathf.Max(0f, reactionSeconds);
                float rivalPackage = OpponentEtSeconds + OpponentReactionSeconds;
                return playerPackage <= rivalPackage;
            }
        }

        private void AwardRacePayout()
        {
            if (payoutAwarded) return;
            payoutAwarded = true;
            payout = PlayerWon ? raceEvent.winPayout : raceEvent.lossPayout;
            personalBest = RaceHistory.RecordCompletedPass(build, simulation, trackSurface, raceDistance, PlayerWon);
            GarageSession.ApplyRunWear(build.engine, build.engineIsNew, simulation.State.Damage);
            GarageSession.AddVteCoins(payout);
        }

        private void AwardEngineFailure()
        {
            if (payoutAwarded) return;
            payoutAwarded = true;
            payout = 0;
            RaceHistory.RecordFailure();
            GarageSession.ApplyRunWear(build.engine, build.engineIsNew, 1f);
        }

        private void ShiftUp()
        {
            if (state != RaceState.Racing || gear >= 5) return;
            gear++;
            simulation.BeginShift(build.ShiftCutSeconds);
            exhaustFlashTimer = .085f;
        }

        private void SetTree(int active)
        {
            for (int i = 0; i < treeBulbs.Length; i++)
            {
                if (treeBulbs[i] == null) continue;
                Color baseColor = i < 3 ? new Color(1f, 0.62f, 0f) : i == 3 ? Color.green : Color.red;
                treeBulbs[i].material.color = i == active ? baseColor : baseColor * 0.12f;
            }
        }

        private void OnGUI()
        {
            Matrix4x4 previousMatrix = CarvinoUi.Begin();
            GUI.Box(new Rect(16, 16, 440, 486), "CARVINO DRAG SIM — v0.05 BUILD + RACE FOUNDATION");
            GUI.Label(new Rect(32, 48, 360, 24), $"Vehicle: {build.vehicle.displayName}");
            GUI.Label(new Rect(32, 72, 360, 24), $"Engine: {build.engine.displayName}");
            GUI.Label(new Rect(32, 96, 360, 24), $"Power: {build.Horsepower:0} hp   Mass: {build.MassKg:0} kg");
            GUI.Label(new Rect(32, 120, 360, 24), $"Distance: {simulation.DistanceMeters:0.0} / {simulation.FinishDistanceMeters:0.0} m  •  {raceDistance.displayName}");
            GUI.Label(new Rect(32, 144, 380, 24), $"Speed: {simulation.SpeedMps * 2.23694f:0.0} mph   RPM: {simulation.EngineRpm:0}   Gear: {gear}");
            GUI.Label(new Rect(32, 168, 405, 24), $"ET: {simulation.ElapsedSeconds:0.000}s @ {simulation.FinishTrapMph:0.0} mph   RT: {(reactionSeconds >= 0f ? reactionSeconds.ToString("0.000") : "--")}");
            GUI.Label(new Rect(32, 192, 405, 24), $"60: {Split(simulation.SixtyFootSeconds)}  •  330: {Split(simulation.ThreeThirtyFootSeconds)}  •  1/8: {Split(simulation.EighthMileSeconds)} @ {simulation.EighthMileMph:0.0}");
            GUI.Label(new Rect(32, 216, 405, 24), $"1000 ft: {Split(simulation.ThousandFootSeconds)}  •  {raceEvent.opponent.displayName}: {OpponentEtSeconds:0.000}s / {OpponentReactionSeconds:0.000} RT");
            string status = state == RaceState.Finished ? "TIME SLIP COMPLETE — ESC/Back to garage" : state == RaceState.Failed ? "ENGINE FAILED — ESC/Back to garage for repairs" : state == RaceState.RedLight ? "RED LIGHT — ESC/Back to garage" : burningOut ? "BURNOUT — hold F to heat tires, then B to stage" : state == RaceState.Garage ? "Hold F for burnout  •  B to stage" : state == RaceState.Staged ? "STAGED — tree is coming" : state == RaceState.Countdown ? "TREE ACTIVE — wait for green" : "RACING — hold W, SHIFT to shift";
            GUI.Label(new Rect(32, 240, 405, 24), status);
            GUI.Label(new Rect(32, 264, 405, 24), RaceResult);
            if (payoutAwarded) GUI.Label(new Rect(32, 282, 405, 20), $"PAYOUT: +{payout:N0} V-TECoins  •  Wallet: {GarageSession.VteCoins:N0}");
            GUI.Label(new Rect(32, 306, 405, 24), "1/2 vehicle  •  E engine  •  U upgrades");
            GUI.Label(new Rect(32, 330, 405, 24), $"Tune: {build.tune.airFuelRatio:0.0} AFR  •  {build.tune.ignitionTiming:0}° timing  •  {build.shiftRpm:0} shift RPM");
            GUI.Label(new Rect(32, 354, 405, 20), $"{simulation.State.Warning}  •  power {simulation.State.PowerDerate * 100f:0}%");
            GUI.Label(new Rect(32, 378, 405, 20), $"λ {simulation.State.Lambda:0.00}  •  boost {simulation.State.ManifoldPressurePsi - 14.7f:0.0} psi  •  coolant {simulation.State.CoolantTempC:0}°C  •  knock {simulation.State.KnockIntensity * 100f:0}%");
            GUI.Label(new Rect(32, 402, 405, 20), $"Tires: {simulation.TireTemperatureC:0}°C  •  F {build.tune.frontTirePressurePsi:0} psi / R {build.tune.rearTirePressurePsi:0} psi  •  grip {simulation.EffectiveTireGrip:0.00}");
            GUI.Label(new Rect(32, 426, 405, 20), $"Deflection: FL {simulation.Tires[0].AverageDeflectionMeters * 1000f:0.0} mm  FR {simulation.Tires[1].AverageDeflectionMeters * 1000f:0.0} mm  RL {simulation.Tires[2].AverageDeflectionMeters * 1000f:0.0} mm  RR {simulation.Tires[3].AverageDeflectionMeters * 1000f:0.0} mm");
            GUI.Label(new Rect(32, 450, 405, 20), $"{trackSurface.displayName}: {trackSurface.gripMultiplier:0.00} grip  •  Rival: {(opponentFailed ? "ENGINE TROUBLE" : opponentFinished ? "FINISHED" : "RUNNING")}  •  {opponentReferenceMph:0} mph");
            string bestEt = RaceHistory.BestEt(build, trackSurface, raceDistance) > 0f ? RaceHistory.BestEt(build, trackSurface, raceDistance).ToString("0.000") + "s" : "--";
            GUI.Label(new Rect(32, 474, 405, 20), $"{raceDistance.displayName} PB: {bestEt}  •  {RaceHistory.BestTrapMph(build, trackSurface, raceDistance):0.0} mph  •  Career: {RaceHistory.TotalWins} wins / {RaceHistory.TotalPasses} passes{(personalBest ? "  •  NEW PB!" : string.Empty)}");
            CarvinoUi.End(previousMatrix);
        }

        private static string Split(float value) => value > 0f ? value.ToString("0.000") : "--";
    }
}
