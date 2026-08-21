using System;
using UnityEngine;

namespace Carvino
{
    public enum WheelCorner { FrontLeft, FrontRight, RearLeft, RearRight }

    [Serializable]
    public sealed class TireCompoundSpec
    {
        public string id;
        public float idealPressurePsi;
        public float pressureWindowPsi;
        public float optimalTemperatureC;
        public float coldGripMultiplier;
        public float peakGripMultiplier;
        public float hotGripMultiplier;
        public float carcassStiffnessNPerMeterAtIdealPressure;
        public float referenceWidthMm;
        public float loadSensitivity;
    }

    public static class TireCompoundCatalog
    {
        public static readonly TireCompoundSpec StreetRadial = new TireCompoundSpec
        {
            id = "street_radial", idealPressurePsi = 30f, pressureWindowPsi = 10f,
            optimalTemperatureC = 50f, coldGripMultiplier = .82f, peakGripMultiplier = 1f,
            hotGripMultiplier = .76f, carcassStiffnessNPerMeterAtIdealPressure = 245000f,
            referenceWidthMm = 235f, loadSensitivity = .13f
        };

        public static readonly TireCompoundSpec DragSlick = new TireCompoundSpec
        {
            id = "drag_slick", idealPressurePsi = 18f, pressureWindowPsi = 8f,
            optimalTemperatureC = 64f, coldGripMultiplier = .67f, peakGripMultiplier = 1.12f,
            hotGripMultiplier = .82f, carcassStiffnessNPerMeterAtIdealPressure = 180000f,
            referenceWidthMm = 275f, loadSensitivity = .09f
        };

        public static TireCompoundSpec Get(string id)
        {
            return id == DragSlick.id ? DragSlick : StreetRadial;
        }
    }

    [Serializable]
    public sealed class TireContactPatch
    {
        public float LoadNewtons;
        public float TemperatureC = 22f;
        public float DeflectionMeters;
        public float SlipRatio;
        public float FrictionCoefficient;
    }

    /// <summary>
    /// Straight-line tire model with inner/center/outer contact patches per wheel.
    /// It is deliberately compact now, but exposes the same load/deflection data that
    /// later suspension, alignment, and uneven-surface simulation will consume.
    /// </summary>
    [Serializable]
    public sealed class TireAssembly
    {
        public WheelCorner corner;
        public float PressurePsi = 28f;
        public float WidthMm = 235f;
        public TireCompoundSpec Compound = TireCompoundCatalog.StreetRadial;
        public TireContactPatch[] Patches { get; } = { new TireContactPatch(), new TireContactPatch(), new TireContactPatch() };

        public float AverageTemperatureC
        {
            get
            {
                float total = 0f;
                foreach (TireContactPatch patch in Patches) total += patch.TemperatureC;
                return total / Patches.Length;
            }
        }

        public float AverageDeflectionMeters
        {
            get
            {
                float total = 0f;
                foreach (TireContactPatch patch in Patches) total += patch.DeflectionMeters;
                return total / Patches.Length;
            }
        }

        public float Update(float wheelLoadN, float requestedForceN, float surfaceGrip, float deltaTime)
        {
            TireCompoundSpec compound = Compound ?? TireCompoundCatalog.StreetRadial;
            float pressureError = (PressurePsi - compound.idealPressurePsi) / Mathf.Max(1f, compound.pressureWindowPsi);
            float centerWeight = Mathf.Clamp(.46f + pressureError * .13f, .25f, .67f);
            float shoulderWeight = (1f - centerWeight) * .5f;
            float pressureRatio = Mathf.Clamp(PressurePsi / Mathf.Max(1f, compound.idealPressurePsi), .55f, 1.55f);
            float widthRatio = Mathf.Clamp(WidthMm / Mathf.Max(1f, compound.referenceWidthMm), .72f, 1.35f);
            float carcassStiffness = compound.carcassStiffnessNPerMeterAtIdealPressure * pressureRatio * Mathf.Sqrt(widthRatio);
            float pressureGrip = Mathf.Lerp(1f, .76f, Mathf.Clamp01(Mathf.Abs(pressureError)));
            float widthGrip = Mathf.Lerp(.94f, 1.06f, Mathf.InverseLerp(.72f, 1.35f, widthRatio));
            float totalCapacity = 0f;
            for (int index = 0; index < Patches.Length; index++)
            {
                TireContactPatch patch = Patches[index];
                float patchWeight = index == 1 ? centerWeight : shoulderWeight;
                patch.LoadNewtons = Mathf.Max(0f, wheelLoadN * patchWeight);
                patch.DeflectionMeters = Mathf.Clamp(patch.LoadNewtons / Mathf.Max(100000f, carcassStiffness), .002f, .08f);
                float idealDeflection = Mathf.Lerp(.020f, .034f, Mathf.InverseLerp(.55f, 1.55f, 1f / pressureRatio));
                float deflectionGrip = Mathf.Lerp(1f, .80f, Mathf.Clamp01(Mathf.Abs(patch.DeflectionMeters - idealDeflection) / .035f));
                float temperatureGrip = TemperatureGrip(compound, patch.TemperatureC);
                float loadSensitivity = Mathf.Lerp(1f, 1f - compound.loadSensitivity, Mathf.InverseLerp(500f, 4500f, patch.LoadNewtons));
                patch.FrictionCoefficient = surfaceGrip * temperatureGrip * loadSensitivity * deflectionGrip * pressureGrip * widthGrip;
                float patchCapacity = patch.LoadNewtons * patch.FrictionCoefficient;
                float requestedPatchForce = requestedForceN * patchWeight;
                patch.SlipRatio = patchCapacity > 1f ? Mathf.Max(0f, requestedPatchForce / patchCapacity - 1f) : 1f;
                float heatInput = patch.SlipRatio * 3.1f + Mathf.Clamp01(requestedPatchForce / Mathf.Max(1f, patchCapacity)) * .18f;
                patch.TemperatureC = Mathf.Clamp(patch.TemperatureC + (heatInput - .055f) * deltaTime, 22f, 125f);
                totalCapacity += patchCapacity;
            }
            return totalCapacity;
        }

        private static float TemperatureGrip(TireCompoundSpec compound, float temperatureC)
        {
            if (temperatureC <= compound.optimalTemperatureC)
            {
                float warmup = Mathf.InverseLerp(22f, compound.optimalTemperatureC, temperatureC);
                return Mathf.Lerp(compound.coldGripMultiplier, compound.peakGripMultiplier, warmup);
            }

            float overheat = Mathf.InverseLerp(compound.optimalTemperatureC, 125f, temperatureC);
            return Mathf.Lerp(compound.peakGripMultiplier, compound.hotGripMultiplier, overheat);
        }

        public void Burnout(float deltaTime)
        {
            foreach (TireContactPatch patch in Patches)
            {
                patch.TemperatureC = Mathf.Clamp(patch.TemperatureC + deltaTime * 7f, 22f, 125f);
                patch.SlipRatio = 0.75f;
            }
        }
    }
}
