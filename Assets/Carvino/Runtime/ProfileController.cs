using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carvino
{
    public sealed class ProfileController : MonoBehaviour
    {
        private string driverName;
        private string notice;

        private void Start()
        {
            GarageSession.Load();
            driverName = PlayerProfile.DriverName;
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton6)) SceneManager.LoadScene("MainMenu");
        }

        private void OnGUI()
        {
            Matrix4x4 previous = CarvinoUi.Begin();
            GUI.Box(new Rect(18, 18, 760, 574), "CARVINO — DRIVER PROFILE");
            GUI.Label(new Rect(46, 58, 650, 32), "DRIVER PROFILE", HeaderStyle(25, Color.white));
            GUI.Label(new Rect(46, 96, 650, 22), "Offline prototype profile — online account, cloud saves, and leaderboards come later.", HeaderStyle(13, new Color(.70f, .74f, .76f)));
            GUI.Label(new Rect(46, 130, 180, 22), "Driver name");
            driverName = GUI.TextField(new Rect(190, 126, 250, 30), driverName, 18);
            if (GUI.Button(new Rect(454, 126, 170, 30), "SAVE NAME")) { PlayerProfile.SetDriverName(driverName); driverName = PlayerProfile.DriverName; notice = "PROFILE SAVED"; }
            GUI.Box(new Rect(46, 178, 630, 126), "CAREER SUMMARY");
            DrawRow(70, 212, "Rank", CareerProgress.RankName);
            DrawRow(70, 242, "Career wins", RaceHistory.TotalWins.ToString());
            DrawRow(70, 272, "Total passes", RaceHistory.TotalPasses.ToString());
            GUI.Box(new Rect(46, 328, 630, 138), "CURRENT BUILD");
            VehicleSpec vehicle = CarvinoCatalog.Vehicles[GarageSession.VehicleId == "pickup" ? 1 : 0];
            EngineSpec engine = CarvinoCatalog.FindEngine(GarageSession.EngineId);
            DrawRow(70, 362, "Vehicle", vehicle.displayName);
            DrawRow(70, 392, "Engine", engine.displayName + (GarageSession.EngineIsNew ? " — new" : " — used"));
            DrawRow(70, 422, "Installed parts", CountParts(GarageSession.UpgradeMask) + " / " + CarvinoCatalog.Upgrades.Count);
            DrawRow(70, 452, "Wallet", GarageSession.VteCoins.ToString("N0") + " V-TECoins");
            if (GUI.Button(new Rect(46, 500, 240, 40), "BACK TO GAME HUB")) SceneManager.LoadScene("MainMenu");
            if (GUI.Button(new Rect(304, 500, 240, 40), "OPEN CAREER")) SceneManager.LoadScene("Career");
            if (!string.IsNullOrEmpty(notice)) GUI.Label(new Rect(46, 550, 300, 20), notice, HeaderStyle(13, new Color(.55f, .9f, .72f)));
            CarvinoUi.End(previous);
        }

        private static int CountParts(int mask)
        {
            int total = 0;
            for (int index = 0; index < CarvinoCatalog.Upgrades.Count; index++) if ((mask & (1 << index)) != 0) total++;
            return total;
        }

        private static void DrawRow(float x, float y, string label, string value)
        {
            GUI.Label(new Rect(x, y, 200, 22), label);
            GUI.Label(new Rect(x + 220, y, 320, 22), value, HeaderStyle(14, new Color(.92f, .94f, .96f)));
        }

        private static GUIStyle HeaderStyle(int size, Color color) => new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = FontStyle.Bold, normal = { textColor = color } };
    }
}
