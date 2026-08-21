using UnityEngine;

namespace Carvino
{
    /// <summary>Local prototype identity; account authentication is deliberately deferred until backend work begins.</summary>
    public static class PlayerProfile
    {
        private const string DriverNameKey = "carvino.profile.driver_name";
        public static string DriverName => PlayerPrefs.GetString(DriverNameKey, "CARVINO RACER");

        public static void SetDriverName(string name)
        {
            string clean = string.IsNullOrWhiteSpace(name) ? "CARVINO RACER" : name.Trim().ToUpperInvariant();
            PlayerPrefs.SetString(DriverNameKey, clean.Length > 18 ? clean.Substring(0, 18) : clean);
            PlayerPrefs.Save();
        }
    }
}
