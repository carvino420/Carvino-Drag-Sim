using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carvino
{
    public sealed class DynoController : MonoBehaviour
    {
        private DragBuild build;
        private DragSimulation dyno;
        private string statusMessage = "Choose a base map, then fine-tune it for your build.";
        private bool showChassisSetup;

        private void Start()
        {
            GarageSession.Load();
            VehicleSpec vehicle = GarageSession.VehicleId == "pickup" ? CarvinoCatalog.Vehicles[1] : CarvinoCatalog.Vehicles[0];
            build = new DragBuild { vehicle = vehicle, engine = CarvinoCatalog.FindEngine(GarageSession.EngineId), engineIsNew = GarageSession.EngineIsNew, engineHealth = GarageSession.GetEngineHealth(GarageSession.EngineId, GarageSession.EngineIsNew), tune = GarageSession.Tune };
            GarageSession.ApplyUpgrades(build);
            dyno = new DragSimulation(build);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A)) ChangeAfr(-0.1f);
            if (Input.GetKeyDown(KeyCode.D)) ChangeAfr(0.1f);
            if (Input.GetKeyDown(KeyCode.W)) ChangeTiming(1f);
            if (Input.GetKeyDown(KeyCode.S)) ChangeTiming(-1f);
            if (Input.GetKeyDown(KeyCode.Z)) ChangeLaunch(-200f);
            if (Input.GetKeyDown(KeyCode.X)) ChangeLaunch(200f);
            if (Input.GetKeyDown(KeyCode.C)) ChangeShift(-200f);
            if (Input.GetKeyDown(KeyCode.V)) ChangeShift(200f);
            if (Input.GetKeyDown(KeyCode.Q)) ChangeFrontPressure(-1f);
            if (Input.GetKeyDown(KeyCode.E)) ChangeFrontPressure(1f);
            if (Input.GetKeyDown(KeyCode.R)) ChangeRearPressure(-1f);
            if (Input.GetKeyDown(KeyCode.T)) ChangeRearPressure(1f);
            if (Input.GetKeyDown(KeyCode.F)) ChangeAllBoost(-1f);
            if (Input.GetKeyDown(KeyCode.G)) ChangeAllBoost(1f);
            if (Input.GetKeyDown(KeyCode.H)) showChassisSetup = !showChassisSetup;
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0)) SceneManager.LoadScene("RaceDay");
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton6)) SceneManager.LoadScene("Garage");
        }

        private void ChangeAfr(float amount)
        {
            float stoich = FuelCatalog.Get(build.tune.fuelType).stoichAfr;
            build.tune.airFuelRatio = Mathf.Clamp(build.tune.airFuelRatio + amount, stoich * 0.68f, stoich * 1.10f);
            Save();
        }
        private void ChangeFuel(FuelType fuelType)
        {
            float lambda = build.tune.Lambda;
            build.tune.fuelType = fuelType;
            build.tune.airFuelRatio = Mathf.Clamp(lambda * FuelCatalog.Get(fuelType).stoichAfr, FuelCatalog.Get(fuelType).stoichAfr * 0.68f, FuelCatalog.Get(fuelType).stoichAfr * 1.10f);
            statusMessage = FuelCatalog.Get(fuelType).displayName + " selected. Lambda was preserved.";
            Save();
        }
        private void ChangeFrontRide(float amount) { build.tune.frontRideHeightMm = Mathf.Clamp(build.tune.frontRideHeightMm + amount, 75f, 185f); Save(); }
        private void ChangeRearRide(float amount) { build.tune.rearRideHeightMm = Mathf.Clamp(build.tune.rearRideHeightMm + amount, 75f, 185f); Save(); }
        private void ChangeFrontRebound(float amount) { build.tune.frontRebound = Mathf.Clamp(build.tune.frontRebound + amount, 1f, 10f); Save(); }
        private void ChangeRearRebound(float amount) { build.tune.rearRebound = Mathf.Clamp(build.tune.rearRebound + amount, 1f, 10f); Save(); }
        private void ChangeAntiSquat(float amount) { build.tune.antiSquat = Mathf.Clamp01(build.tune.antiSquat + amount); Save(); }
        private void ChangeTiming(float amount) { build.tune.ignitionTiming = Mathf.Clamp(build.tune.ignitionTiming + amount, 0f, 35f); Save(); }
        private void ChangeLaunch(float amount) { build.tune.launchRpm = Mathf.Clamp(build.tune.launchRpm + amount, 2000f, build.engine.redlineRpm); Save(); }
        private void ChangeShift(float amount) { build.tune.shiftRpm = Mathf.Clamp(build.tune.shiftRpm + amount, 3000f, build.engine.redlineRpm); Save(); }
        private void ChangeFrontPressure(float amount) { build.tune.frontTirePressurePsi = Mathf.Clamp(build.tune.frontTirePressurePsi + amount, 12f, 45f); Save(); }
        private void ChangeRearPressure(float amount) { build.tune.rearTirePressurePsi = Mathf.Clamp(build.tune.rearTirePressurePsi + amount, 12f, 45f); Save(); }
        private void ChangeAllBoost(float amount)
        {
            if (!build.HasTurbo) return;
            build.tune.ChangeAllBoost(amount, build.Turbo.turboMaxBoostPsi + 3f);
            Save();
        }
        private void ApplyPreset(int index)
        {
            TunePreset preset = TunePresets.All[index];
            if (preset.requiresTurbo && !build.HasTurbo)
            {
                statusMessage = "Install the Street Turbo Kit before using that map.";
                return;
            }
            if (!GarageSession.TryBuyTunePreset(preset))
            {
                statusMessage = "Not enough V-TECoins for that tune preset.";
                return;
            }
            TunePresets.Apply(build.tune, preset);
            statusMessage = preset.displayName + " loaded. Fine tune it at your own risk.";
            Save();
        }
        private void Save() { GarageSession.SaveTune(); dyno = new DragSimulation(build); }

        private void OnGUI()
        {
            Matrix4x4 previousMatrix = CarvinoUi.Begin();
            GUI.Box(new Rect(16, 16, 760, 548), "CARVINO WORKS — DYNO CELL");
            GUI.Label(new Rect(36, 54, 500, 24), $"{build.vehicle.displayName}  •  {build.engine.displayName}  •  {(build.engineIsNew ? "NEW" : "USED 93%")}");
            GUI.Label(new Rect(500, 54, 228, 24), $"{FuelCatalog.Get(build.tune.fuelType).displayName}  λ {build.tune.Lambda:0.00}");
            if (GUI.Button(new Rect(480, 78, 74, 24), "PUMP")) ChangeFuel(FuelType.PumpGas);
            if (GUI.Button(new Rect(558, 78, 74, 24), "E50")) ChangeFuel(FuelType.E50);
            if (GUI.Button(new Rect(636, 78, 74, 24), "E85")) ChangeFuel(FuelType.E85);
            GUI.Box(new Rect(36, 88, 300, 250), "TUNE SETTINGS");
            GUI.Label(new Rect(58, 122, 250, 22), $"AFR: {build.tune.airFuelRatio:0.0}  λ {build.tune.Lambda:0.00} [A/D]");
            GUI.Label(new Rect(58, 152, 250, 22), $"Timing: {build.tune.ignitionTiming:0}°   [W/S]");
            GUI.Label(new Rect(58, 182, 250, 22), $"Launch: {build.tune.launchRpm:0} RPM   [Z/X]");
            GUI.Label(new Rect(58, 212, 250, 22), $"Shift: {build.tune.shiftRpm:0} RPM   [C/V]");
            GUI.Label(new Rect(58, 242, 250, 22), $"Front pressure: {build.tune.frontTirePressurePsi:0} psi   [Q/E]");
            GUI.Label(new Rect(58, 272, 250, 22), $"Rear pressure: {build.tune.rearTirePressurePsi:0} psi   [R/T]");
            GUI.Label(new Rect(58, 302, 280, 22), build.HasTurbo ? $"Boost by gear: {build.tune.boostGear1Psi:0}/{build.tune.boostGear2Psi:0}/{build.tune.boostGear3Psi:0}/{build.tune.boostGear4Psi:0}/{build.tune.boostGear5Psi:0} psi [F/G]" : "Install the Street Turbo Kit in garage to unlock boost.");
            GUI.Label(new Rect(58, 330, 250, 22), $"Peak: {build.Horsepower:0} hp");
            if (GUI.Button(new Rect(342, 120, 38, 22), "-")) ChangeAfr(-0.1f);
            if (GUI.Button(new Rect(342, 148, 38, 22), "+")) ChangeAfr(0.1f);
            if (GUI.Button(new Rect(342, 176, 38, 22), "T+")) ChangeTiming(1f);
            if (GUI.Button(new Rect(342, 204, 38, 22), "T-")) ChangeTiming(-1f);
            if (GUI.Button(new Rect(342, 232, 38, 22), "B-")) ChangeAllBoost(-1f);
            if (GUI.Button(new Rect(342, 260, 38, 22), "B+")) ChangeAllBoost(1f);

            GUI.Box(new Rect(370, 88, 360, 230), "DYNO SNAPSHOT");
            GUI.Label(new Rect(394, 120, 300, 22), "RPM        HP        Torque");
            float[] rpms = { 2500f, 4000f, 5500f, 7000f, 8500f };
            for (int i = 0; i < rpms.Length; i++)
            {
                float rpm = Mathf.Min(rpms[i], build.engine.redlineRpm);
                GUI.Label(new Rect(394, 150 + i * 28, 300, 22), $"{rpm,5:0}   {dyno.DynoHorsepowerAtRpm(rpm),7:0} hp   {dyno.DynoTorqueAtRpm(rpm),7:0} lb-ft");
            }
            GUI.Label(new Rect(394, 292, 312, 20), "TUNE CHECK: " + TuneReadinessMessage());
            GUI.Box(new Rect(36, 360, 694, 74), "TUNE PRESETS");
            for (int index = 0; index < TunePresets.All.Length; index++)
            {
                TunePreset preset = TunePresets.All[index];
                string ownership = GarageSession.OwnsTunePreset(preset.id) ? "OWNED" : preset.price.ToString("N0") + " VTC";
                if (GUI.Button(new Rect(48 + index * 222, 390, 210, 32), preset.displayName + "\n" + ownership)) ApplyPreset(index);
            }
            if (GUI.Button(new Rect(36, 446, 220, 46), "TAKE TUNE TO RACE DAY")) SceneManager.LoadScene("RaceDay");
            if (GUI.Button(new Rect(266, 446, 220, 46), "BACK TO GARAGE")) SceneManager.LoadScene("Garage");
            if (GUI.Button(new Rect(496, 446, 234, 46), "CHASSIS SETUP [H]")) showChassisSetup = !showChassisSetup;
            GUI.Box(new Rect(36, 500, 694, 30), statusMessage);
            if (showChassisSetup) DrawChassisSetup();
            CarvinoUi.End(previousMatrix);
        }

        private void DrawChassisSetup()
        {
            GUI.Box(new Rect(202, 102, 520, 310), "CHASSIS / LAUNCH SETUP");
            GUI.Label(new Rect(226, 142, 320, 22), $"Front ride height: {build.tune.frontRideHeightMm:0} mm");
            GUI.Label(new Rect(226, 184, 320, 22), $"Rear ride height: {build.tune.rearRideHeightMm:0} mm");
            GUI.Label(new Rect(226, 226, 320, 22), $"Front rebound: {build.tune.frontRebound:0.0} / 10");
            GUI.Label(new Rect(226, 268, 320, 22), $"Rear rebound: {build.tune.rearRebound:0.0} / 10");
            GUI.Label(new Rect(226, 310, 320, 22), $"Anti-squat: {build.tune.antiSquat * 100f:0}%");
            if (GUI.Button(new Rect(556, 138, 52, 26), "-10")) ChangeFrontRide(-10f);
            if (GUI.Button(new Rect(614, 138, 52, 26), "+10")) ChangeFrontRide(10f);
            if (GUI.Button(new Rect(556, 180, 52, 26), "-10")) ChangeRearRide(-10f);
            if (GUI.Button(new Rect(614, 180, 52, 26), "+10")) ChangeRearRide(10f);
            if (GUI.Button(new Rect(556, 222, 52, 26), "-")) ChangeFrontRebound(-1f);
            if (GUI.Button(new Rect(614, 222, 52, 26), "+")) ChangeFrontRebound(1f);
            if (GUI.Button(new Rect(556, 264, 52, 26), "-")) ChangeRearRebound(-1f);
            if (GUI.Button(new Rect(614, 264, 52, 26), "+")) ChangeRearRebound(1f);
            if (GUI.Button(new Rect(556, 306, 52, 26), "-10")) ChangeAntiSquat(-.10f);
            if (GUI.Button(new Rect(614, 306, 52, 26), "+10")) ChangeAntiSquat(.10f);
            GUI.Label(new Rect(226, 354, 440, 36), "FWD cars generally want controlled front lift. RWD cars can use anti-squat and rear rebound to plant the tire.");
        }

        // This mirrors the shared simulation's published warning thresholds so players can
        // understand a risky combination before a pass. It intentionally does not change
        // any engine, fuel, or failure calculation.
        private string TuneReadinessMessage()
        {
            float lambda = build.tune.Lambda;
            FuelSpec fuel = FuelCatalog.Get(build.tune.fuelType);
            if (lambda > 0.93f) return "LEAN — add fuel before a hard pass";
            if (lambda < 0.72f) return "VERY RICH — safe but down on power";

            float timingLimit = fuel.type == FuelType.PumpGas ? 24f : fuel.type == FuelType.E50 ? 28f : 31f;
            if (build.tune.ignitionTiming > timingLimit)
                return "TIMING AGGRESSIVE FOR " + fuel.displayName;

            if (build.HasTurbo)
            {
                float requestedBoost = build.tune.BoostForGear(5);
                if (requestedBoost > build.Turbo.turboSafeBoostPsi)
                    return "BOOST ABOVE TURBO SAFE RANGE";
                if (fuel.fuelVolumeMultiplier > build.FuelCapacityMultiplier)
                    return "FUEL SYSTEM NEEDS AN UPGRADE";
                return "BOOST MAP READY — CHECK LAUNCH";
            }

            return "NA BASELINE READY — SET LAUNCH RPM";
        }
    }
}
