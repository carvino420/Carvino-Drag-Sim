using UnityEngine;

namespace Carvino
{
    public sealed class TunePreset
    {
        public string id;
        public string displayName;
        public int price;
        public bool requiresTurbo;
        public FuelType fuelType;
        public float afr;
        public float timing;
        public float launchRpm;
        public float shiftRpm;
        public float boostGear1;
        public float boostGear2;
        public float boostGear3;
        public float boostGear4;
        public float boostGear5;
    }

    public static class TunePresets
    {
        public static readonly TunePreset[] All =
        {
            new TunePreset { id = "safe_street", displayName = "SAFE STREET BASE", price = 0, fuelType = FuelType.PumpGas, afr = 12.9f, timing = 16f, launchRpm = 3600f, shiftRpm = 6200f, boostGear1 = 5f, boostGear2 = 7f, boostGear3 = 8f, boostGear4 = 9f, boostGear5 = 9f },
            new TunePreset { id = "drag_base", displayName = "DRAG BASE MAP", price = 650, requiresTurbo = true, fuelType = FuelType.PumpGas, afr = 12.5f, timing = 19f, launchRpm = 4400f, shiftRpm = 7000f, boostGear1 = 7f, boostGear2 = 9f, boostGear3 = 11f, boostGear4 = 12f, boostGear5 = 12f },
            new TunePreset { id = "high_boost", displayName = "HIGH-BOOST E85", price = 1800, requiresTurbo = true, fuelType = FuelType.E85, afr = 8.6f, timing = 23f, launchRpm = 5000f, shiftRpm = 7400f, boostGear1 = 10f, boostGear2 = 12f, boostGear3 = 14f, boostGear4 = 15f, boostGear5 = 16f }
        };

        public static void Apply(TuneSettings tune, TunePreset preset)
        {
            tune.fuelType = preset.fuelType;
            tune.airFuelRatio = preset.afr;
            tune.ignitionTiming = preset.timing;
            tune.launchRpm = preset.launchRpm;
            tune.shiftRpm = preset.shiftRpm;
            tune.boostGear1Psi = preset.boostGear1;
            tune.boostGear2Psi = preset.boostGear2;
            tune.boostGear3Psi = preset.boostGear3;
            tune.boostGear4Psi = preset.boostGear4;
            tune.boostGear5Psi = preset.boostGear5;
        }
    }
}
