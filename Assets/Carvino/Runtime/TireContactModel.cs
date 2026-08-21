using System;
using UnityEngine;

namespace Carvino
{
    public enum WheelCorner { FrontLeft, FrontRight, RearLeft, RearRight }

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
        private static readonly float[] PatchWeights = { 0.27f, 0.46f, 0.27f };

        public WheelCorner corner;
        public float PressurePsi = 28f;
        public float WidthMm = 235f;
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
            float totalCapacity = 0f;
            for (int index = 0; index < Patches.Length; index++)
            {
                TireContactPatch patch = Patches[index];
                patch.LoadNewtons = Mathf.Max(0f, wheelLoadN * PatchWeights[index]);
                float pressureStiffness = Mathf.Max(140000f, PressurePsi * 10500f);
                patch.DeflectionMeters = Mathf.Clamp(patch.LoadNewtons / pressureStiffness, 0.002f, 0.08f);
                float contactLengthFactor = Mathf.Clamp01(0.35f + patch.DeflectionMeters * 12f);
                float temperatureFactor = Mathf.Clamp01(1f - Mathf.Abs(patch.TemperatureC - 58f) / 45f);
                float gripAtTemperature = Mathf.Lerp(0.72f, 1.08f, temperatureFactor);
                float loadSensitivity = Mathf.Lerp(1f, 0.86f, Mathf.InverseLerp(500f, 4500f, patch.LoadNewtons));
                patch.FrictionCoefficient = surfaceGrip * gripAtTemperature * loadSensitivity * contactLengthFactor;
                float patchCapacity = patch.LoadNewtons * patch.FrictionCoefficient;
                float requestedPatchForce = requestedForceN * PatchWeights[index];
                patch.SlipRatio = patchCapacity > 1f ? Mathf.Max(0f, requestedPatchForce / patchCapacity - 1f) : 1f;
                float heatInput = patch.SlipRatio * 3.1f + Mathf.Clamp01(requestedPatchForce / Mathf.Max(1f, patchCapacity)) * 0.18f;
                patch.TemperatureC = Mathf.Clamp(patch.TemperatureC + (heatInput - 0.055f) * deltaTime, 22f, 125f);
                totalCapacity += patchCapacity;
            }
            return totalCapacity;
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
