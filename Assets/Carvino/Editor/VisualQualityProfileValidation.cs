#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Carvino.Editor
{
    /// <summary>Headless smoke checks for the PC/mobile visual-profile contract.</summary>
    public static class VisualQualityProfileValidation
    {
        public static void Validate()
        {
            VisualQualityProfile[] pc = VisualQualityProfiles.GetProfiles(false);
            VisualQualityProfile[] mobile = VisualQualityProfiles.GetProfiles(true);
            Require(pc.Length >= 4, "PC needs performance through ultra visual profiles.");
            Require(mobile.Length >= 3, "Mobile needs battery, balanced, and high visual profiles.");

            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            ValidateSet(pc, VisualContentTier.Pc, ids);
            ValidateSet(mobile, VisualContentTier.Mobile, ids);
            Require(pc[pc.Length - 1].lodBias > mobile[mobile.Length - 1].lodBias, "PC high-end LOD budget must exceed mobile.");
            Require(pc[pc.Length - 1].shadowDistance > mobile[mobile.Length - 1].shadowDistance, "PC high-end shadow budget must exceed mobile.");
            Debug.Log("Carvino visual quality profile validation passed.");
        }

        private static void ValidateSet(VisualQualityProfile[] profiles, VisualContentTier expectedTier, HashSet<string> ids)
        {
            foreach (VisualQualityProfile profile in profiles)
            {
                Require(profile != null, "Visual profile entry cannot be null.");
                Require(!string.IsNullOrWhiteSpace(profile.id), "Visual profile ID cannot be empty.");
                Require(ids.Add(profile.id), "Visual profile IDs must be globally unique: " + profile.id);
                Require(profile.contentTier == expectedTier, profile.id + " has the wrong content tier.");
                Require(profile.targetFrameRate >= 30, profile.id + " has an invalid frame-rate target.");
                Require(profile.lodBias > 0f && profile.shadowDistance > 0f, profile.id + " has an invalid visual budget.");
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Carvino visual profile validation failed: " + message);
        }
    }
}
#endif
