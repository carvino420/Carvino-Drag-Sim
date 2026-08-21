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
        private bool showTelemetry;
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
            if (Input.GetKeyDown(KeyCode.T)) showTelemetry = !showTelemetry;
            burningOut = state == RaceState.Garage && Input.GetKey(KeyCode.F);
            if (burningOut) simulation.Burnout(Time.deltaTime);
            if (CarvinoInput.StagePressed && state == RaceState.Garage) StartCountdown();
            if (CarvinoInput.LaunchPressed) TryLaunch();
            if (CarvinoInput.ShiftPressed) ShiftUp();
            if (IsTerminalResult)
            {
                if (Input.GetKeyDown(KeyCode.R)) ResetRun();
                if (Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton6)) SceneManager.LoadScene("Garage");
                if (Input.GetKeyDown(KeyCode.M)) SceneManager.LoadScene("MainMenu");
            }

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

        private bool IsTerminalResult => state == RaceState.Finished || state == RaceState.RedLight || state == RaceState.Failed;

        private void AwardRacePayout()
        {
            if (payoutAwarded) return;
            payoutAwarded = true;
            payout = PlayerWon ? raceEvent.winPayout : raceEvent.lossPayout;
            personalBest = RaceHistory.RecordCompletedPass(build, simulation, trackSurface, raceDistance, PlayerWon, RaceEventSession.IsCareerEvent);
            GarageSession.ApplyRunWear(build.engine, build.engineIsNew, simulation.State);
            GarageSession.AddVteCoins(payout);
        }

        private void AwardEngineFailure()
        {
            if (payoutAwarded) return;
            payoutAwarded = true;
            payout = 0;
            RaceHistory.RecordFailure();
            GarageSession.ApplyRunWear(build.engine, build.engineIsNew, simulation.State);
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
            DrawDriverHud();
            if (showTelemetry) DrawTelemetry();
            if (IsTerminalResult) DrawResultActions();
            CarvinoUi.End(previousMatrix);
        }

        private void DrawDriverHud()
        {
            GUI.Box(new Rect(18, 18, 346, 154), raceEvent.name + "  •  " + raceDistance.displayName);
            GUI.Label(new Rect(36, 48, 310, 20), "VS " + raceEvent.opponent.displayName + "   " + trackSurface.displayName, SmallLabel());
            GUI.Label(new Rect(36, 68, 166, 52), (simulation.SpeedMps * 2.23694f).ToString("0") + " MPH", SpeedStyle());
            GUI.Label(new Rect(212, 72, 124, 28), "GEAR " + gear, HeaderStyle(20));
            GUI.Label(new Rect(212, 102, 124, 22), simulation.EngineRpm.ToString("0") + " RPM", SmallLabel());
            GUI.Label(new Rect(36, 126, 300, 20), "ET " + simulation.ElapsedSeconds.ToString("0.000") + "    RT " + (reactionSeconds >= 0f ? reactionSeconds.ToString("0.000") : "--") + "    " + simulation.DistanceMeters.ToString("0") + "m", SmallLabel());
            string status = state == RaceState.Finished ? "TIME SLIP COMPLETE" : state == RaceState.Failed ? "ENGINE FAILURE — RETURN TO GARAGE" : state == RaceState.RedLight ? "RED LIGHT" : burningOut ? "BURNOUT — HOLD F, THEN B TO STAGE" : state == RaceState.Garage ? "HOLD F TO HEAT TIRES  •  B TO STAGE" : state == RaceState.Staged ? "STAGED — TREE IS COMING" : state == RaceState.Countdown ? "TREE ACTIVE — WAIT FOR GREEN" : "W TO THROTTLE  •  SHIFT TO SHIFT";
            GUI.Box(new Rect(CarvinoUi.Width * .5f - 220f, CarvinoUi.Height - 70f, 440f, 42f), status);
            GUI.Label(new Rect(20, CarvinoUi.Height - 31f, 360f, 20f), "T: telemetry  •  C: camera  •  R: reset", SmallLabel());
        }

        private void DrawTelemetry()
        {
            GUI.Box(new Rect(18, 184, 346, 188), "LIVE TELEMETRY");
            GUI.Label(new Rect(36, 218, 310, 20), "BOOST " + (simulation.State.ManifoldPressurePsi - 14.7f).ToString("0.0") + " psi   λ " + simulation.State.Lambda.ToString("0.00"), SmallLabel());
            GUI.Label(new Rect(36, 242, 310, 20), "COOLANT " + simulation.State.CoolantTempC.ToString("0") + "°C   KNOCK " + (simulation.State.KnockIntensity * 100f).ToString("0") + "%", SmallLabel());
            GUI.Label(new Rect(36, 266, 310, 20), "TIRE " + simulation.TireTemperatureC.ToString("0") + "°C   GRIP " + simulation.EffectiveTireGrip.ToString("0.00"), SmallLabel());
            GUI.Label(new Rect(36, 290, 310, 20), "60 FT " + Split(simulation.SixtyFootSeconds) + "   1/8 " + Split(simulation.EighthMileSeconds), SmallLabel());
            GUI.Label(new Rect(36, 314, 310, 20), simulation.State.Warning + "   POWER " + (simulation.State.PowerDerate * 100f).ToString("0") + "%", SmallLabel());
            GUI.Label(new Rect(36, 338, 310, 20), "T again closes telemetry", SmallLabel());
        }

        private static GUIStyle SmallLabel() => new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.82f, .86f, .9f) } };
        private static GUIStyle HeaderStyle(int size) => new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.94f, .24f, .1f) } };
        private static GUIStyle SpeedStyle() => new GUIStyle(GUI.skin.label) { fontSize = 34, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };

        private void DrawResultActions()
        {
            float panelX = CarvinoUi.Width - 470f;
            GUI.Box(new Rect(panelX, 52f, 412f, 346f), "TIME SLIP — RUN COMPLETE");
            GUI.Label(new Rect(panelX + 24f, 92f, 360f, 28f), RaceResult, new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, wordWrap = true, normal = { textColor = state == RaceState.Finished && PlayerWon ? new Color(.35f, .95f, .48f) : new Color(1f, .42f, .2f) } });
            GUI.Label(new Rect(panelX + 24f, 140f, 360f, 22f), $"ET  {simulation.ElapsedSeconds:0.000}s     TRAP  {simulation.FinishTrapMph:0.0} mph");
            GUI.Label(new Rect(panelX + 24f, 166f, 360f, 22f), $"REACTION  {(reactionSeconds >= 0f ? reactionSeconds.ToString("0.000") : "--")}s     60 FT  {Split(simulation.SixtyFootSeconds)}");
            GUI.Label(new Rect(panelX + 24f, 192f, 360f, 22f), $"1/8  {Split(simulation.EighthMileSeconds)} @ {simulation.EighthMileMph:0.0} mph");
            GUI.Label(new Rect(panelX + 24f, 218f, 360f, 22f), $"PAYOUT  +{payout:N0} V-TECoins     WALLET  {GarageSession.VteCoins:N0}");
            if (GUI.Button(new Rect(panelX + 24f, 260f, 114f, 44f), "RACE AGAIN [R]")) ResetRun();
            if (GUI.Button(new Rect(panelX + 148f, 260f, 106f, 44f), "GARAGE [G]")) SceneManager.LoadScene("Garage");
            if (GUI.Button(new Rect(panelX + 264f, 260f, 116f, 44f), "MENU [M]")) SceneManager.LoadScene("MainMenu");
            GUI.Label(new Rect(panelX + 24f, 318f, 360f, 20f), "Full stats, tire condition, and build data stay saved.");
        }

        private static string Split(float value) => value > 0f ? value.ToString("0.000") : "--";
    }
}
