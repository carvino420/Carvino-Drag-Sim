using System;
using System.Collections.Generic;
using UnityEngine;

namespace Carvino
{
    /// <summary>
    /// Data describing how one repairable engine component responds to the risks
    /// already calculated by the authoritative engine simulation.
    /// </summary>
    [Serializable]
    public sealed class EngineComponentWearSpec
    {
        public string id;
        public string displayName;
        public float repairWeight;
        public float basePassWear;
        public float knockSensitivity;
        public float heatSensitivity;
        public float fuelStarvationSensitivity;
        public float turboOverspeedSensitivity;
        public float overRevSensitivity;
    }

    public static class EngineComponentWearCatalog
    {
        public static readonly IReadOnlyList<EngineComponentWearSpec> All = new[]
        {
            new EngineComponentWearSpec { id = "rings", displayName = "Piston Rings", repairWeight = .22f, basePassWear = .00045f, knockSensitivity = 1.20f, heatSensitivity = .35f, fuelStarvationSensitivity = 1.05f, overRevSensitivity = .15f },
            new EngineComponentWearSpec { id = "bearings", displayName = "Rod / Main Bearings", repairWeight = .27f, basePassWear = .00035f, knockSensitivity = 1.00f, heatSensitivity = .60f, fuelStarvationSensitivity = .25f, overRevSensitivity = .95f },
            new EngineComponentWearSpec { id = "head_gasket", displayName = "Head Gasket", repairWeight = .20f, basePassWear = .00020f, knockSensitivity = 1.25f, heatSensitivity = 1.20f, fuelStarvationSensitivity = .40f, turboOverspeedSensitivity = .20f, overRevSensitivity = .15f },
            new EngineComponentWearSpec { id = "valvetrain", displayName = "Valvetrain", repairWeight = .18f, basePassWear = .00030f, knockSensitivity = .25f, heatSensitivity = .35f, fuelStarvationSensitivity = .15f, overRevSensitivity = 1.40f },
            new EngineComponentWearSpec { id = "turbo", displayName = "Turbocharger", repairWeight = .13f, basePassWear = .00015f, knockSensitivity = .10f, heatSensitivity = .45f, fuelStarvationSensitivity = .25f, turboOverspeedSensitivity = 1.65f, overRevSensitivity = .15f }
        };
    }

    /// <summary>Persistent condition for one owned engine variant.</summary>
    [Serializable]
    public sealed class EngineCondition
    {
        public float rings = 1f;
        public float bearings = 1f;
        public float headGasket = 1f;
        public float valvetrain = 1f;
        public float turbo = 1f;
        public string lastDamageCause = "NORMAL WEAR";

        public float OverallHealth
        {
            get
            {
                float weightedHealth = 0f;
                float totalWeight = 0f;
                foreach (EngineComponentWearSpec spec in EngineComponentWearCatalog.All)
                {
                    weightedHealth += GetHealth(spec.id) * spec.repairWeight;
                    totalWeight += spec.repairWeight;
                }
                return totalWeight > 0f ? Mathf.Clamp01(weightedHealth / totalWeight) : 1f;
            }
        }

        public string WeakestComponent
        {
            get
            {
                EngineComponentWearSpec weakest = null;
                float lowest = float.MaxValue;
                foreach (EngineComponentWearSpec spec in EngineComponentWearCatalog.All)
                {
                    float health = GetHealth(spec.id);
                    if (health >= lowest) continue;
                    lowest = health;
                    weakest = spec;
                }
                return weakest != null ? weakest.displayName : "Engine";
            }
        }

        public void ApplyWear(EngineWearReport report)
        {
            float riskTotal = report.KnockWear + report.HeatWear + report.FuelWear + report.TurboWear + report.OverRevWear;
            bool hasMeasuredCause = riskTotal > .000001f;
            float severity = Mathf.Clamp01(report.RunDamage) * (report.Catastrophic ? .40f : .12f);

            foreach (EngineComponentWearSpec spec in EngineComponentWearCatalog.All)
            {
                float causeWear;
                if (hasMeasuredCause)
                {
                    causeWear = (report.KnockWear * spec.knockSensitivity
                        + report.HeatWear * spec.heatSensitivity
                        + report.FuelWear * spec.fuelStarvationSensitivity
                        + report.TurboWear * spec.turboOverspeedSensitivity
                        + report.OverRevWear * spec.overRevSensitivity) / riskTotal;
                }
                else
                {
                    // Legacy callers only supplied a general damage value. Keeping a
                    // neutral distribution preserves those saves and call sites.
                    causeWear = .55f;
                }

                float wear = spec.basePassWear + severity * Mathf.Clamp(causeWear, 0f, 1.65f);
                SetHealth(spec.id, GetHealth(spec.id) - wear);
            }

            if (hasMeasuredCause || report.Catastrophic || report.RunDamage > .02f)
                lastDamageCause = string.IsNullOrEmpty(report.DominantCause) ? "UNDIAGNOSED ENGINE DAMAGE" : report.DominantCause;
        }

        public void RepairTo(float targetHealth)
        {
            targetHealth = Mathf.Clamp01(targetHealth);
            foreach (EngineComponentWearSpec spec in EngineComponentWearCatalog.All)
                SetHealth(spec.id, targetHealth);
            lastDamageCause = "REBUILT / SERVICED";
        }

        public float GetHealth(string componentId)
        {
            switch (componentId)
            {
                case "rings": return rings;
                case "bearings": return bearings;
                case "head_gasket": return headGasket;
                case "valvetrain": return valvetrain;
                case "turbo": return turbo;
                default: return 1f;
            }
        }

        public void SetHealth(string componentId, float health)
        {
            health = Mathf.Clamp(health, .05f, 1f);
            switch (componentId)
            {
                case "rings": rings = health; break;
                case "bearings": bearings = health; break;
                case "head_gasket": headGasket = health; break;
                case "valvetrain": valvetrain = health; break;
                case "turbo": turbo = health; break;
            }
        }
    }

    /// <summary>One pass's causes, produced only from the shared EngineState.</summary>
    [Serializable]
    public sealed class EngineWearReport
    {
        public float RunDamage;
        public float KnockWear;
        public float HeatWear;
        public float FuelWear;
        public float TurboWear;
        public float OverRevWear;
        public bool Catastrophic;
        public string DominantCause;

        public static EngineWearReport FromState(EngineState state)
        {
            return new EngineWearReport
            {
                RunDamage = state.RunDamage,
                KnockWear = state.KnockWear,
                HeatWear = state.HeatWear,
                FuelWear = state.FuelWear,
                TurboWear = state.TurboWear,
                OverRevWear = state.OverRevWear,
                Catastrophic = state.IsFailed,
                DominantCause = state.DominantDamageCause
            };
        }

        public static EngineWearReport Legacy(float runDamage)
        {
            return new EngineWearReport
            {
                RunDamage = Mathf.Clamp01(runDamage),
                Catastrophic = runDamage >= .9f,
                DominantCause = runDamage >= .9f ? "UNDIAGNOSED ENGINE FAILURE" : "NORMAL WEAR"
            };
        }
    }
}
