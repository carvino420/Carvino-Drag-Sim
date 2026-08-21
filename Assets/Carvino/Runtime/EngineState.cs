using System;
using UnityEngine;

namespace Carvino
{
    /// <summary>Single runtime engine state shared by race, dyno, UI, audio, and future failure systems.</summary>
    [Serializable]
    public sealed class EngineState
    {
        public float Rpm = 900f;
        public float TorqueLbFt;
        public float Horsepower;
        public float ManifoldPressurePsi = 14.7f;
        public float Lambda = 0.87f;
        public float IgnitionTimingDegrees;
        public float KnockIntensity;
        public float CoolantTempC = 82f;
        public float OilTempC = 78f;
        public float TurboSpeedRpm;
        public float InjectorDutyPercent;
        public float Damage;
        public float BaselineDamage;
        public float KnockWear;
        public float HeatWear;
        public float FuelWear;
        public float TurboWear;
        public float OverRevWear;
        public bool IsFailed;
        public float SafetyPowerMultiplier = 1f;
        public string Warning = "SYSTEMS NORMAL";

        /// <summary>Damage added during this pass, excluding condition loaded from the save.</summary>
        public float RunDamage => Mathf.Clamp01(Damage - BaselineDamage);

        public string DominantDamageCause
        {
            get
            {
                float highest = KnockWear;
                string cause = highest > 0f ? "DETONATION / KNOCK" : "NORMAL WEAR";
                if (HeatWear > highest) { highest = HeatWear; cause = "COOLING SYSTEM OVERHEAT"; }
                if (FuelWear > highest) { highest = FuelWear; cause = "FUEL STARVATION / LEAN RUN"; }
                if (TurboWear > highest) { highest = TurboWear; cause = "TURBO OVERSPEED"; }
                if (OverRevWear > highest) cause = "ENGINE OVER-REV";
                return cause;
            }
        }

        public float PowerDerate
        {
            get
            {
                if (IsFailed) return 0.12f;
                float damageDerate = Damage > 0.55f ? Mathf.Lerp(0.82f, 0.48f, Mathf.InverseLerp(0.55f, 0.9f, Damage)) : 1f - Damage * 0.32f;
                return Mathf.Clamp01(damageDerate * SafetyPowerMultiplier);
            }
        }
    }
}
