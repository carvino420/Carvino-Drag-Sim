using System;
using UnityEngine;

namespace Carvino
{
    /// <summary>Small persistent bridge between the garage and race scenes.</summary>
    public static class GarageSession
    {
        public static string VehicleId { get; private set; } = "hatch";
        public static string EngineId { get; private set; } = "d16";
        public static bool EngineIsNew { get; private set; }
        public static bool StarterUpgrades { get; private set; }
        public static int UpgradeMask { get; private set; }
        public static int OwnedPartMask { get; private set; }
        public static int VteCoins { get; private set; } = 25000;
        public static int PaintIndex { get; private set; }
        public static int WheelFinishIndex { get; private set; }
        public static TuneSettings Tune { get; private set; } = new TuneSettings();

        public static void SetBuild(string vehicleId, string engineId, bool starterUpgrades, bool engineIsNew = false, int upgradeMask = -1)
        {
            VehicleId = vehicleId;
            EngineId = engineId;
            EngineIsNew = engineIsNew;
            UpgradeMask = upgradeMask >= 0 ? upgradeMask : (starterUpgrades ? DefaultUpgradeMask : 0);
            StarterUpgrades = UpgradeMask != 0;
            OwnedPartMask = PlayerPrefs.HasKey("carvino.owned_parts") ? PlayerPrefs.GetInt("carvino.owned_parts") : UpgradeMask;
            VteCoins = PlayerPrefs.GetInt("carvino.vtecoins", 25000);
            PlayerPrefs.SetString("carvino.vehicle", vehicleId);
            PlayerPrefs.SetString("carvino.engine", engineId);
            PlayerPrefs.SetInt("carvino.engine_new", engineIsNew ? 1 : 0);
            PlayerPrefs.SetInt("carvino.upgrades", StarterUpgrades ? 1 : 0);
            PlayerPrefs.SetInt("carvino.upgrade_mask", UpgradeMask);
            SaveTune();
            PlayerPrefs.Save();
        }

        public static void Load()
        {
            VehicleId = PlayerPrefs.GetString("carvino.vehicle", VehicleId);
            EngineId = PlayerPrefs.GetString("carvino.engine", EngineId);
            EngineIsNew = PlayerPrefs.GetInt("carvino.engine_new", 0) == 1;
            UpgradeMask = PlayerPrefs.HasKey("carvino.upgrade_mask") ? PlayerPrefs.GetInt("carvino.upgrade_mask") : (PlayerPrefs.GetInt("carvino.upgrades", 0) == 1 ? DefaultUpgradeMask : 0);
            StarterUpgrades = UpgradeMask != 0;
            Tune.airFuelRatio = PlayerPrefs.GetFloat("carvino.afr", Tune.airFuelRatio);
            Tune.ignitionTiming = PlayerPrefs.GetFloat("carvino.timing", Tune.ignitionTiming);
            Tune.launchRpm = PlayerPrefs.GetFloat("carvino.launch", Tune.launchRpm);
            Tune.shiftRpm = PlayerPrefs.GetFloat("carvino.shift", Tune.shiftRpm);
            Tune.frontTirePressurePsi = PlayerPrefs.GetFloat("carvino.front_psi", Tune.frontTirePressurePsi);
            Tune.rearTirePressurePsi = PlayerPrefs.GetFloat("carvino.rear_psi", Tune.rearTirePressurePsi);
            Tune.boostGear1Psi = PlayerPrefs.GetFloat("carvino.boost_g1", Tune.boostGear1Psi);
            Tune.boostGear2Psi = PlayerPrefs.GetFloat("carvino.boost_g2", Tune.boostGear2Psi);
            Tune.boostGear3Psi = PlayerPrefs.GetFloat("carvino.boost_g3", Tune.boostGear3Psi);
            Tune.boostGear4Psi = PlayerPrefs.GetFloat("carvino.boost_g4", Tune.boostGear4Psi);
            Tune.boostGear5Psi = PlayerPrefs.GetFloat("carvino.boost_g5", Tune.boostGear5Psi);
            Tune.fuelType = (FuelType)PlayerPrefs.GetInt("carvino.fuel_type", (int)Tune.fuelType);
            Tune.frontRideHeightMm = PlayerPrefs.GetFloat("carvino.front_ride", Tune.frontRideHeightMm);
            Tune.rearRideHeightMm = PlayerPrefs.GetFloat("carvino.rear_ride", Tune.rearRideHeightMm);
            Tune.frontRebound = PlayerPrefs.GetFloat("carvino.front_rebound", Tune.frontRebound);
            Tune.rearRebound = PlayerPrefs.GetFloat("carvino.rear_rebound", Tune.rearRebound);
            Tune.antiSquat = PlayerPrefs.GetFloat("carvino.anti_squat", Tune.antiSquat);
            PaintIndex = PlayerPrefs.GetInt("carvino.paint", 0);
            WheelFinishIndex = PlayerPrefs.GetInt("carvino.wheels", 0);
        }

        public static void SaveTune()
        {
            PlayerPrefs.SetFloat("carvino.afr", Tune.airFuelRatio);
            PlayerPrefs.SetFloat("carvino.timing", Tune.ignitionTiming);
            PlayerPrefs.SetFloat("carvino.launch", Tune.launchRpm);
            PlayerPrefs.SetFloat("carvino.shift", Tune.shiftRpm);
            PlayerPrefs.SetFloat("carvino.front_psi", Tune.frontTirePressurePsi);
            PlayerPrefs.SetFloat("carvino.rear_psi", Tune.rearTirePressurePsi);
            PlayerPrefs.SetFloat("carvino.boost_g1", Tune.boostGear1Psi);
            PlayerPrefs.SetFloat("carvino.boost_g2", Tune.boostGear2Psi);
            PlayerPrefs.SetFloat("carvino.boost_g3", Tune.boostGear3Psi);
            PlayerPrefs.SetFloat("carvino.boost_g4", Tune.boostGear4Psi);
            PlayerPrefs.SetFloat("carvino.boost_g5", Tune.boostGear5Psi);
            PlayerPrefs.SetInt("carvino.fuel_type", (int)Tune.fuelType);
            PlayerPrefs.SetFloat("carvino.front_ride", Tune.frontRideHeightMm);
            PlayerPrefs.SetFloat("carvino.rear_ride", Tune.rearRideHeightMm);
            PlayerPrefs.SetFloat("carvino.front_rebound", Tune.frontRebound);
            PlayerPrefs.SetFloat("carvino.rear_rebound", Tune.rearRebound);
            PlayerPrefs.SetFloat("carvino.anti_squat", Tune.antiSquat);
            PlayerPrefs.SetInt("carvino.owned_parts", OwnedPartMask);
            PlayerPrefs.SetInt("carvino.vtecoins", VteCoins);
            PlayerPrefs.SetInt("carvino.paint", PaintIndex);
            PlayerPrefs.SetInt("carvino.wheels", WheelFinishIndex);
            PlayerPrefs.Save();
        }

        public static void SetAppearance(int paintIndex, int wheelFinishIndex)
        {
            PaintIndex = Mathf.Max(0, paintIndex);
            WheelFinishIndex = Mathf.Max(0, wheelFinishIndex);
            SaveTune();
        }

        public static bool OwnsPart(int index) => (OwnedPartMask & (1 << index)) != 0;

        public static bool TryBuyPart(int index)
        {
            if (index < 0 || index >= CarvinoCatalog.Upgrades.Count) return false;
            if (OwnsPart(index)) return true;
            int price = CarvinoCatalog.Upgrades[index].price;
            if (VteCoins < price) return false;
            VteCoins -= price;
            OwnedPartMask |= 1 << index;
            SaveTune();
            return true;
        }

        public static bool OwnsEngine(string engineId, bool isNew)
        {
            string key = EngineOwnershipKey(engineId, isNew);
            string owned = PlayerPrefs.GetString("carvino.owned_engines", EngineOwnershipKey("d16", false));
            return Array.IndexOf(owned.Split('|'), key) >= 0;
        }

        public static bool TryBuyEngine(EngineSpec engine, bool isNew)
        {
            if (OwnsEngine(engine.id, isNew)) return true;
            int cost = isNew ? engine.price * 2 : engine.price;
            if (VteCoins < cost) return false;
            VteCoins -= cost;
            string key = EngineOwnershipKey(engine.id, isNew);
            string existing = PlayerPrefs.GetString("carvino.owned_engines", EngineOwnershipKey("d16", false));
            PlayerPrefs.SetString("carvino.owned_engines", existing + "|" + key);
            SaveTune();
            return true;
        }

        public static float GetEngineHealth(string engineId, bool isNew)
        {
            return GetEngineCondition(engineId, isNew).OverallHealth;
        }

        public static EngineCondition GetEngineCondition(string engineId, bool isNew)
        {
            float legacyHealth = PlayerPrefs.GetFloat(EngineHealthKey(engineId, isNew), isNew ? 1f : 0.93f);
            EngineCondition condition = new EngineCondition
            {
                rings = LoadComponentHealth(engineId, isNew, "rings", legacyHealth),
                bearings = LoadComponentHealth(engineId, isNew, "bearings", legacyHealth),
                headGasket = LoadComponentHealth(engineId, isNew, "head_gasket", legacyHealth),
                valvetrain = LoadComponentHealth(engineId, isNew, "valvetrain", legacyHealth),
                turbo = LoadComponentHealth(engineId, isNew, "turbo", legacyHealth),
                lastDamageCause = PlayerPrefs.GetString(EngineConditionKey(engineId, isNew) + ".last_cause", "NORMAL WEAR")
            };
            return condition;
        }

        public static int RepairCost(EngineSpec engine, bool isNew)
        {
            float targetHealth = isNew ? 1f : 0.98f;
            EngineCondition condition = GetEngineCondition(engine.id, isNew);
            float weightedRepair = 0f;
            foreach (EngineComponentWearSpec spec in EngineComponentWearCatalog.All)
                weightedRepair += Mathf.Max(0f, targetHealth - condition.GetHealth(spec.id)) * spec.repairWeight;
            return Mathf.CeilToInt(weightedRepair * engine.price * 1.2f);
        }

        public static bool TryRepairEngine(EngineSpec engine, bool isNew)
        {
            if (!OwnsEngine(engine.id, isNew)) return false;
            int cost = RepairCost(engine, isNew);
            if (VteCoins < cost) return false;
            VteCoins -= cost;
            EngineCondition condition = GetEngineCondition(engine.id, isNew);
            condition.RepairTo(isNew ? 1f : 0.98f);
            SaveEngineCondition(engine.id, isNew, condition);
            SaveTune();
            return true;
        }

        public static void ApplyRunWear(EngineSpec engine, bool isNew, float runDamage)
        {
            ApplyRunWear(engine, isNew, EngineWearReport.Legacy(runDamage));
        }

        public static void ApplyRunWear(EngineSpec engine, bool isNew, EngineState state)
        {
            ApplyRunWear(engine, isNew, EngineWearReport.FromState(state));
        }

        public static void ApplyRunWear(EngineSpec engine, bool isNew, EngineWearReport report)
        {
            EngineCondition condition = GetEngineCondition(engine.id, isNew);
            condition.ApplyWear(report);
            SaveEngineCondition(engine.id, isNew, condition);
            PlayerPrefs.Save();
        }

        public static void AddVteCoins(int amount)
        {
            VteCoins = Mathf.Max(0, VteCoins + amount);
            SaveTune();
        }

        public static bool OwnsTunePreset(string presetId)
        {
            string owned = PlayerPrefs.GetString("carvino.owned_tunes", "safe_street");
            return Array.IndexOf(owned.Split('|'), presetId) >= 0;
        }

        public static bool TryBuyTunePreset(TunePreset preset)
        {
            if (OwnsTunePreset(preset.id)) return true;
            if (VteCoins < preset.price) return false;
            VteCoins -= preset.price;
            string existing = PlayerPrefs.GetString("carvino.owned_tunes", "safe_street");
            PlayerPrefs.SetString("carvino.owned_tunes", existing + "|" + preset.id);
            SaveTune();
            return true;
        }

        private static string EngineOwnershipKey(string engineId, bool isNew) => engineId + (isNew ? ":new" : ":used");
        private static string EngineHealthKey(string engineId, bool isNew) => "carvino.engine_health." + engineId + (isNew ? ".new" : ".used");
        private static string EngineConditionKey(string engineId, bool isNew) => "carvino.engine_condition." + engineId + (isNew ? ".new" : ".used");

        private static float LoadComponentHealth(string engineId, bool isNew, string componentId, float fallback)
        {
            return Mathf.Clamp(PlayerPrefs.GetFloat(EngineConditionKey(engineId, isNew) + "." + componentId, fallback), .05f, 1f);
        }

        private static void SaveEngineCondition(string engineId, bool isNew, EngineCondition condition)
        {
            string key = EngineConditionKey(engineId, isNew);
            foreach (EngineComponentWearSpec spec in EngineComponentWearCatalog.All)
                PlayerPrefs.SetFloat(key + "." + spec.id, condition.GetHealth(spec.id));
            PlayerPrefs.SetString(key + ".last_cause", condition.lastDamageCause);
            // Keep the original aggregate key current so older builds and existing
            // save tools continue to read a valid condition value.
            PlayerPrefs.SetFloat(EngineHealthKey(engineId, isNew), condition.OverallHealth);
        }

        public static void ApplyUpgrades(DragBuild build)
        {
            for (int index = 0; index < CarvinoCatalog.Upgrades.Count; index++)
                if ((UpgradeMask & (1 << index)) != 0) build.upgrades.Add(CarvinoCatalog.Upgrades[index]);
        }

        public const int DefaultUpgradeMask = (1 << 0) | (1 << 1) | (1 << 3);
    }
}
