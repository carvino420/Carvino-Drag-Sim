using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carvino
{
    /// <summary>Local-only presentation and comfort preferences for the prototype.</summary>
    public sealed class SettingsController : MonoBehaviour
    {
        private static readonly string[] ResolutionNames = { "DESKTOP", "1080P", "1440P", "4K" };
        private static readonly Vector2Int[] Resolutions =
        {
            new Vector2Int(0, 0), new Vector2Int(1920, 1080), new Vector2Int(2560, 1440), new Vector2Int(3840, 2160)
        };
        private static readonly string[] QualityNames = { "PERFORMANCE", "BALANCED", "HIGH" };
        private static readonly string[] CameraNames = { "STANDARD", "CLOSER", "WIDER" };
        private int resolution;
        private int quality;
        private int cameraStyle;
        private bool fullscreen;
        private bool vSync;
        private float engineVolume;
        private string notice;

        public static float EngineVolume => PlayerPrefs.GetFloat("settings.engineVolume", 0.85f);
        public static int CameraStyle => PlayerPrefs.GetInt("settings.cameraStyle", 0);

        private void Start()
        {
            resolution = PlayerPrefs.GetInt("settings.resolution", 0);
            quality = PlayerPrefs.GetInt("settings.quality", 1);
            cameraStyle = PlayerPrefs.GetInt("settings.cameraStyle", 0);
            fullscreen = PlayerPrefs.GetInt("settings.fullscreen", Screen.fullScreen ? 1 : 0) == 1;
            vSync = PlayerPrefs.GetInt("settings.vSync", 1) == 1;
            engineVolume = EngineVolume;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton6)) Back();
        }

        private void Back()
        {
            Save();
            SceneManager.LoadScene("MainMenu");
        }

        private void Save()
        {
            PlayerPrefs.SetInt("settings.resolution", resolution);
            PlayerPrefs.SetInt("settings.quality", quality);
            PlayerPrefs.SetInt("settings.cameraStyle", cameraStyle);
            PlayerPrefs.SetInt("settings.fullscreen", fullscreen ? 1 : 0);
            PlayerPrefs.SetInt("settings.vSync", vSync ? 1 : 0);
            PlayerPrefs.SetFloat("settings.engineVolume", engineVolume);
            PlayerPrefs.Save();
        }

        private void ApplyDisplay()
        {
            Save();
            int qualityLevel = quality == 0 ? 0 : quality == 1 ? Mathf.Max(0, (QualitySettings.names.Length - 1) / 2) : Mathf.Max(0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(qualityLevel, true);
            QualitySettings.vSyncCount = vSync ? 1 : 0;
            Vector2Int selected = Resolutions[resolution];
            if (selected.x == 0) Screen.fullScreen = fullscreen;
            else Screen.SetResolution(selected.x, selected.y, fullscreen);
            notice = "SETTINGS APPLIED";
        }

        private void OnGUI()
        {
            Matrix4x4 previous = CarvinoUi.Begin();
            GUI.Box(new Rect(18, 18, 760, 620), "CARVINO — SETTINGS");
            GUI.Label(new Rect(46, 58, 650, 28), "DISPLAY & EXPERIENCE", HeaderStyle());
            GUI.Label(new Rect(46, 96, 480, 22), "Display mode");
            if (GUI.Button(new Rect(425, 90, 250, 32), fullscreen ? "FULLSCREEN" : "WINDOWED")) fullscreen = !fullscreen;
            GUI.Label(new Rect(46, 140, 480, 22), "Resolution");
            resolution = Selector(425, 134, resolution, ResolutionNames);
            GUI.Label(new Rect(46, 184, 480, 22), "Graphics preset");
            quality = Selector(425, 178, quality, QualityNames);
            GUI.Label(new Rect(46, 228, 480, 22), "V-sync");
            if (GUI.Button(new Rect(425, 222, 250, 32), vSync ? "ON" : "OFF")) vSync = !vSync;

            GUI.Label(new Rect(46, 286, 650, 28), "AUDIO & CAMERA", HeaderStyle());
            GUI.Label(new Rect(46, 328, 300, 22), $"Engine audio: {engineVolume * 100f:0}%");
            engineVolume = GUI.HorizontalSlider(new Rect(355, 334, 320, 18), engineVolume, 0f, 1f);
            GUI.Label(new Rect(46, 372, 480, 22), "Race camera distance");
            cameraStyle = Selector(425, 366, cameraStyle, CameraNames);
            GUI.Box(new Rect(46, 420, 629, 72), "4K is an optional Windows display mode for capable monitors. The prototype automatically scales its interface for high-resolution displays. Changes are saved locally.");

            if (GUI.Button(new Rect(46, 526, 250, 40), "APPLY SETTINGS")) ApplyDisplay();
            if (GUI.Button(new Rect(316, 526, 250, 40), "BACK TO TITLE")) Back();
            GUI.Label(new Rect(46, 582, 630, 22), string.IsNullOrEmpty(notice) ? "Esc / Controller Back returns to the title." : notice, NoticeStyle());
            CarvinoUi.End(previous);
        }

        private static int Selector(float x, float y, int value, string[] names)
        {
            if (GUI.Button(new Rect(x, y, 38, 32), "<")) value = (value + names.Length - 1) % names.Length;
            GUI.Box(new Rect(x + 44, y, 162, 32), names[value]);
            if (GUI.Button(new Rect(x + 212, y, 38, 32), ">")) value = (value + 1) % names.Length;
            return value;
        }

        private static GUIStyle HeaderStyle() => new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.93f, 0.18f, 0.09f) } };
        private static GUIStyle NoticeStyle() => new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.55f, 0.9f, 0.72f) } };
    }
}
