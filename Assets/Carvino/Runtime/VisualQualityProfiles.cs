using System;
using UnityEngine;

namespace Carvino
{
    /// <summary>The asset family a profile expects future content loaders to request.</summary>
    public enum VisualContentTier
    {
        Mobile,
        Pc
    }

    /// <summary>
    /// Stable, data-only presentation settings. Gameplay simulation must never depend on these values.
    /// The content tier is also the contract future Addressables/asset-bundle loaders can use.
    /// </summary>
    [Serializable]
    public sealed class VisualQualityProfile
    {
        public readonly string id;
        public readonly string displayName;
        public readonly VisualContentTier contentTier;
        public readonly string unityQualityName;
        public readonly int targetFrameRate;
        public readonly int textureMipmapLimit;
        public readonly float lodBias;
        public readonly float shadowDistance;
        public readonly int antiAliasing;
        public readonly bool realtimeReflections;

        public string ContentVariant => contentTier == VisualContentTier.Pc ? "PC" : "Mobile";

        public VisualQualityProfile(
            string id,
            string displayName,
            VisualContentTier contentTier,
            string unityQualityName,
            int targetFrameRate,
            int textureMipmapLimit,
            float lodBias,
            float shadowDistance,
            int antiAliasing,
            bool realtimeReflections)
        {
            this.id = id;
            this.displayName = displayName;
            this.contentTier = contentTier;
            this.unityQualityName = unityQualityName;
            this.targetFrameRate = targetFrameRate;
            this.textureMipmapLimit = textureMipmapLimit;
            this.lodBias = lodBias;
            this.shadowDistance = shadowDistance;
            this.antiAliasing = antiAliasing;
            this.realtimeReflections = realtimeReflections;
        }
    }

    /// <summary>Single authority for platform-specific visual defaults and their runtime application.</summary>
    public static class VisualQualityProfiles
    {
        private const string ProfilePreference = "settings.visualQualityProfile";

        private static readonly VisualQualityProfile[] PcProfiles =
        {
            new VisualQualityProfile("pc.performance", "PC PERFORMANCE", VisualContentTier.Pc, "Low", 120, 1, 0.75f, 35f, 0, false),
            new VisualQualityProfile("pc.balanced", "PC BALANCED", VisualContentTier.Pc, "High", 60, 0, 1.15f, 65f, 0, true),
            new VisualQualityProfile("pc.high", "PC HIGH", VisualContentTier.Pc, "Very High", 60, 0, 1.6f, 95f, 2, true),
            new VisualQualityProfile("pc.ultra", "PC ULTRA", VisualContentTier.Pc, "Ultra", 90, 0, 2f, 150f, 4, true),
            // Ultra+ is deliberately a real PC-only preset: it maps to the highest Unity
            // quality asset, keeps full-resolution mips, and leaves headroom for 4K displays.
            new VisualQualityProfile("pc.ultra_plus", "PC ULTRA+", VisualContentTier.Pc, "Ultra+", 120, 0, 2.5f, 220f, 8, true)
        };

        private static readonly VisualQualityProfile[] MobileProfiles =
        {
            new VisualQualityProfile("mobile.battery", "MOBILE BATTERY", VisualContentTier.Mobile, "Very Low", 30, 1, 0.5f, 18f, 0, false),
            new VisualQualityProfile("mobile.balanced", "MOBILE BALANCED", VisualContentTier.Mobile, "Low", 45, 1, 0.7f, 28f, 0, false),
            new VisualQualityProfile("mobile.high", "MOBILE HIGH", VisualContentTier.Mobile, "Medium", 60, 0, 1f, 40f, 0, false)
        };

        public static event Action<VisualQualityProfile> ProfileChanged;

        public static VisualQualityProfile Current { get; private set; }
        public static VisualContentTier CurrentContentTier => Current != null ? Current.contentTier : (IsMobileBuild ? VisualContentTier.Mobile : VisualContentTier.Pc);
        public static string CurrentContentVariant => Current != null ? Current.ContentVariant : (IsMobileBuild ? "Mobile" : "PC");

        public static bool IsMobileBuild
        {
            get
            {
#if UNITY_ANDROID || UNITY_IOS
                return true;
#else
                return Application.isMobilePlatform;
#endif
            }
        }

        public static VisualQualityProfile[] GetProfiles(bool mobile)
        {
            VisualQualityProfile[] source = mobile ? MobileProfiles : PcProfiles;
            return (VisualQualityProfile[])source.Clone();
        }

        public static VisualQualityProfile[] GetProfilesForCurrentPlatform() => GetProfiles(IsMobileBuild);

        public static VisualQualityProfile ResolveSavedProfile(VisualQualityProfile[] profiles, int legacyQualityIndex)
        {
            if (profiles == null || profiles.Length == 0) throw new ArgumentException("A platform must provide at least one visual quality profile.", nameof(profiles));

            string savedId = PlayerPrefs.GetString(ProfilePreference, string.Empty);
            for (int index = 0; index < profiles.Length; index++)
                if (string.Equals(profiles[index].id, savedId, StringComparison.Ordinal)) return profiles[index];

            // Migrates the original PERFORMANCE/BALANCED/HIGH integer setting without resetting players.
            int migratedIndex = Mathf.Clamp(legacyQualityIndex, 0, profiles.Length - 1);
            return profiles[migratedIndex];
        }

        public static int IndexOf(VisualQualityProfile[] profiles, VisualQualityProfile profile)
        {
            if (profiles == null || profile == null) return 0;
            for (int index = 0; index < profiles.Length; index++)
                if (profiles[index].id == profile.id) return index;
            return 0;
        }

        public static void SaveSelection(VisualQualityProfile profile)
        {
            if (profile == null) return;
            PlayerPrefs.SetString(ProfilePreference, profile.id);
        }

        public static void Apply(VisualQualityProfile profile, bool vSync)
        {
            if (profile == null) return;

            int qualityLevel = FindUnityQualityLevel(profile.unityQualityName);
            QualitySettings.SetQualityLevel(qualityLevel, true);
            QualitySettings.globalTextureMipmapLimit = profile.textureMipmapLimit;
            QualitySettings.lodBias = profile.lodBias;
            QualitySettings.shadowDistance = profile.shadowDistance;
            QualitySettings.antiAliasing = profile.antiAliasing;
            QualitySettings.realtimeReflectionProbes = profile.realtimeReflections;
            QualitySettings.vSyncCount = vSync ? 1 : 0;
            Application.targetFrameRate = vSync ? -1 : profile.targetFrameRate;

            Current = profile;
            ProfileChanged?.Invoke(profile);
        }

        private static int FindUnityQualityLevel(string qualityName)
        {
            string[] names = QualitySettings.names;
            for (int index = 0; index < names.Length; index++)
                if (string.Equals(names[index], qualityName, StringComparison.OrdinalIgnoreCase)) return index;
            return Mathf.Clamp((names.Length - 1) / 2, 0, Mathf.Max(0, names.Length - 1));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplySavedProfileAtBoot()
        {
            VisualQualityProfile[] profiles = GetProfilesForCurrentPlatform();
            // Fresh PC installs should show the project at its intended presentation level.
            // Existing saved choices (including the legacy integer) remain untouched.
            int defaultQuality = IsMobileBuild ? 1 : profiles.Length - 1;
            int legacyQuality = PlayerPrefs.GetInt("settings.quality", defaultQuality);
            bool vSync = PlayerPrefs.GetInt("settings.vSync", IsMobileBuild ? 0 : 1) == 1;
            Apply(ResolveSavedProfile(profiles, legacyQuality), vSync);
        }
    }
}
