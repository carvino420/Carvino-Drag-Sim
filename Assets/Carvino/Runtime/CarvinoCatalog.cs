using System;
using System.Collections.Generic;
using UnityEngine;

namespace Carvino
{
    public enum DrivetrainLayout { Fwd, Rwd }

    [Serializable]
    public sealed class EngineSpec
    {
        public string id;
        public string displayName;
        public float displacementLiters;
        public float peakHorsepower;
        public float peakTorqueLbFt;
        // Data-owned torque peak lets one engine simulation distinguish a rev-happy
        // four-cylinder from a low-rpm truck engine without special-case code.
        public float peakTorqueRpm;
        public float redlineRpm;
        public float massKg;
        public int price;
    }

    [Serializable]
    public sealed class UpgradeSpec
    {
        public string id;
        public string displayName;
        public float powerMultiplier = 1f;
        public float torqueMultiplier = 1f;
        public float gripMultiplier = 1f;
        public float massDeltaKg;
        public int price;
        // A zero value means this is a naturally aspirated part. Turbo parts use the
        // fields below so boost is driven by hardware limits rather than a flat HP bonus.
        public float turboMaxBoostPsi;
        public float turboSpoolRpm;
        public float turboEfficiency = 1f;
        public float turboSafeBoostPsi;
        public float fuelCapacityMultiplier = 1f;
        public float drivetrainEfficiencyMultiplier = 1f;
        public float drivenTractionMultiplier = 1f;
        public float shiftCutMultiplier = 1f;
        // Empty keeps the vehicle's street tire. Tire upgrades select a data-owned
        // compound rather than adding another hard-coded traction branch.
        public string tireCompoundId;
    }

    [Serializable]
    public sealed class VehicleSpec
    {
        public string id;
        public string displayName;
        public DrivetrainLayout drivetrain;
        public float chassisMassKg;
        public float dragCoefficient;
        public float frontalAreaM2;
        public float tireGrip;
        public List<string> compatibleEngineIds = new List<string>();
    }

    public static class CarvinoCatalog
    {
        public static readonly IReadOnlyList<EngineSpec> Engines = new[]
        {
            new EngineSpec { id = "d16", displayName = "1.6L SOHC I4", displacementLiters = 1.6f, peakHorsepower = 125f, peakTorqueLbFt = 106f, peakTorqueRpm = 5400f, redlineRpm = 7200f, massKg = 145f, price = 900 },
            new EngineSpec { id = "b20", displayName = "2.0L DOHC I4", displacementLiters = 2.0f, peakHorsepower = 145f, peakTorqueLbFt = 133f, peakTorqueRpm = 5200f, redlineRpm = 7000f, massKg = 160f, price = 1800 },
            new EngineSpec { id = "k20", displayName = "2.0L High-Rev I4", displacementLiters = 2.0f, peakHorsepower = 200f, peakTorqueLbFt = 142f, peakTorqueRpm = 6800f, redlineRpm = 8400f, massKg = 155f, price = 3600 },
            new EngineSpec { id = "k24", displayName = "2.4L Torque I4", displacementLiters = 2.4f, peakHorsepower = 205f, peakTorqueLbFt = 171f, peakTorqueRpm = 5200f, redlineRpm = 7600f, massKg = 170f, price = 4200 },
            new EngineSpec { id = "v6_43", displayName = "4.3L V6", displacementLiters = 4.3f, peakHorsepower = 180f, peakTorqueLbFt = 245f, peakTorqueRpm = 3600f, redlineRpm = 5200f, massKg = 195f, price = 1200 },
            new EngineSpec { id = "i6_42", displayName = "4.2L Inline-Six", displacementLiters = 4.2f, peakHorsepower = 270f, peakTorqueLbFt = 275f, peakTorqueRpm = 4600f, redlineRpm = 6300f, massKg = 235f, price = 2600 },
            new EngineSpec { id = "sbc_350", displayName = "5.7L Small-Block V8", displacementLiters = 5.7f, peakHorsepower = 275f, peakTorqueLbFt = 350f, peakTorqueRpm = 4000f, redlineRpm = 6000f, massKg = 260f, price = 2400 },
            new EngineSpec { id = "ls_53", displayName = "5.3L Modern V8", displacementLiters = 5.3f, peakHorsepower = 320f, peakTorqueLbFt = 340f, peakTorqueRpm = 4400f, redlineRpm = 6200f, massKg = 210f, price = 4800 },
            new EngineSpec { id = "big_block_74", displayName = "7.4L Big-Block V8", displacementLiters = 7.4f, peakHorsepower = 425f, peakTorqueLbFt = 500f, peakTorqueRpm = 3800f, redlineRpm = 5800f, massKg = 325f, price = 8200 }
        };

        public static readonly IReadOnlyList<VehicleSpec> Vehicles = new[]
        {
            new VehicleSpec { id = "hatch", displayName = "1993 Three-Door Hatch", drivetrain = DrivetrainLayout.Fwd, chassisMassKg = 920f, dragCoefficient = 0.34f, frontalAreaM2 = 1.95f, tireGrip = 0.92f, compatibleEngineIds = new List<string> { "d16", "b20", "k20", "k24" } },
            new VehicleSpec { id = "pickup", displayName = "1991 Compact Pickup", drivetrain = DrivetrainLayout.Rwd, chassisMassKg = 1320f, dragCoefficient = 0.42f, frontalAreaM2 = 2.35f, tireGrip = 0.96f, compatibleEngineIds = new List<string> { "v6_43", "i6_42", "sbc_350", "ls_53", "big_block_74" } }
        };

        public static readonly IReadOnlyList<UpgradeSpec> Upgrades = new[]
        {
            new UpgradeSpec { id = "intake", displayName = "Raccon7 Street Intake", powerMultiplier = 1.035f, torqueMultiplier = 1.02f, price = 350 },
            new UpgradeSpec { id = "exhaust", displayName = "Raccon7 Free-Flow Exhaust", powerMultiplier = 1.045f, torqueMultiplier = 1.025f, massDeltaKg = -4f, price = 550 },
            new UpgradeSpec { id = "ecu", displayName = "VoltFire Tuned ECU", powerMultiplier = 1.04f, torqueMultiplier = 1.035f, price = 700 },
            new UpgradeSpec { id = "slicks", displayName = "Hookline Drag Slicks", gripMultiplier = 1.10f, tireCompoundId = "drag_slick", massDeltaKg = 3f, price = 900 },
            new UpgradeSpec { id = "weight", displayName = "Street Weight Reduction", massDeltaKg = -70f, price = 800 },
            new UpgradeSpec { id = "turbo_street", displayName = "Raccon7 Street Turbo Kit", massDeltaKg = 14f, price = 1700, turboMaxBoostPsi = 14f, turboSpoolRpm = 3100f, turboEfficiency = 0.79f, turboSafeBoostPsi = 12f },
            new UpgradeSpec { id = "injectors_850", displayName = "PulseMax 850cc Injectors", price = 780, fuelCapacityMultiplier = 1.75f },
            new UpgradeSpec { id = "fuel_pump_340", displayName = "FlowForce 340 Fuel Pump", price = 480, fuelCapacityMultiplier = 1.35f },
            new UpgradeSpec { id = "clutch_stage1", displayName = "GripJaw Stage 1 Clutch", price = 1050, drivetrainEfficiencyMultiplier = 1.015f, shiftCutMultiplier = .68f },
            new UpgradeSpec { id = "gearset_close", displayName = "Raccon7 Close-Ratio Gearset", price = 1450, drivetrainEfficiencyMultiplier = 1.02f, shiftCutMultiplier = .82f },
            new UpgradeSpec { id = "limited_slip", displayName = "LockRight Limited-Slip", price = 1200, drivenTractionMultiplier = 1.10f },
            new UpgradeSpec { id = "axles_hd", displayName = "ForgeLine Heavy-Duty Axles", price = 950, drivetrainEfficiencyMultiplier = 1.01f }
        };

        public static EngineSpec FindEngine(string id)
        {
            foreach (EngineSpec engine in Engines)
                if (engine.id == id) return engine;
            return Engines[0];
        }
    }
}
