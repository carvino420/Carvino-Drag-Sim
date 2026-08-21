using UnityEngine;

namespace Carvino
{
    public enum FuelType { PumpGas, E50, E85 }

    public sealed class FuelSpec
    {
        public FuelType type;
        public string displayName;
        public float stoichAfr;
        public float knockRiskMultiplier;
        public float fuelVolumeMultiplier;
        public float powerMultiplier;
    }

    /// <summary>Fuel math stays in lambda internally so AFR labels remain correct for every fuel.</summary>
    public static class FuelCatalog
    {
        public static readonly FuelSpec PumpGas = new FuelSpec { type = FuelType.PumpGas, displayName = "PUMP GAS", stoichAfr = 14.7f, knockRiskMultiplier = 1f, fuelVolumeMultiplier = 1f, powerMultiplier = 1f };
        public static readonly FuelSpec E50 = new FuelSpec { type = FuelType.E50, displayName = "E50", stoichAfr = 11.75f, knockRiskMultiplier = 0.78f, fuelVolumeMultiplier = 1.18f, powerMultiplier = 1.015f };
        public static readonly FuelSpec E85 = new FuelSpec { type = FuelType.E85, displayName = "E85", stoichAfr = 9.85f, knockRiskMultiplier = 0.58f, fuelVolumeMultiplier = 1.36f, powerMultiplier = 1.035f };

        public static FuelSpec Get(FuelType type)
        {
            switch (type)
            {
                case FuelType.E50: return E50;
                case FuelType.E85: return E85;
                default: return PumpGas;
            }
        }
    }
}
