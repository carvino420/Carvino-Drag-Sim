using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carvino
{
    public sealed class CareerController : MonoBehaviour
    {
        private string notice;

        private void Start() => GarageSession.Load();
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton6)) SceneManager.LoadScene("MainMenu");
        }

        private void OnGUI()
        {
            Matrix4x4 previous = CarvinoUi.Begin();
            GUI.Box(new Rect(18, 18, 920, 760), "CARVINO — CAREER");
            GUI.Label(new Rect(46, 54, 760, 34), "BUILD YOUR NAME AT THE DRAGWAY", HeaderStyle(26, Color.white));
            GUI.Label(new Rect(48, 94, 760, 22), $"PROFILE: {PlayerProfile.DriverName}  •  RANK: {CareerProgress.RankName}  •  {RaceHistory.TotalWins} WINS / {RaceHistory.TotalPasses} PASSES  •  {GarageSession.VteCoins:N0} VTC", HeaderStyle(14, new Color(.72f, .76f, .78f)));
            GUI.Label(new Rect(48, 124, 320, 22), "CAREER RACE LENGTH");
            DrawDistance(RaceDistanceType.EighthMile, new Rect(240, 118, 150, 32));
            DrawDistance(RaceDistanceType.QuarterMile, new Rect(398, 118, 150, 32));

            for (int i = 0; i < RaceEventSession.Events.Length; i++)
            {
                RaceEvent raceEvent = RaceEventSession.Events[i];
                bool unlocked = CareerProgress.IsEventUnlocked(i);
                int column = i % 2;
                int row = i / 2;
                float x = 48 + column * 410;
                float y = 174 + row * 142;
                GUI.color = unlocked ? new Color(.22f, .24f, .27f) : new Color(.11f, .12f, .14f);
                string state = unlocked ? "ENTER EVENT" : "LOCKED — " + CareerProgress.UnlockText(i);
                if (GUI.Button(new Rect(x, y, 394, 118), raceEvent.name + "\n" + raceEvent.description + "\nRival: " + raceEvent.opponent.displayName + "  •  WIN: " + raceEvent.winPayout.ToString("N0") + " VTC\n" + state))
                {
                    if (!unlocked) notice = CareerProgress.UnlockText(i);
                    else { RaceEventSession.Select(i); SceneManager.LoadScene("QuarterMilePrototype"); }
                }
                GUI.color = Color.white;
            }
            GUI.Label(new Rect(48, 616, 780, 22), string.IsNullOrEmpty(notice) ? "Career wins unlock tougher races. Your car, parts, engine health, and tune all carry over." : notice, HeaderStyle(14, new Color(.96f, .62f, .24f)));
            if (GUI.Button(new Rect(48, 666, 240, 40), "BACK TO GAME HUB")) SceneManager.LoadScene("MainMenu");
            if (GUI.Button(new Rect(304, 666, 240, 40), "OPEN GARAGE")) SceneManager.LoadScene("Garage");
            CarvinoUi.End(previous);
        }

        private static void DrawDistance(RaceDistanceType type, Rect rect)
        {
            GUI.color = RaceDistanceSession.SelectedType == type ? new Color(.74f, .21f, .08f) : Color.white;
            if (GUI.Button(rect, RaceDistanceCatalog.Get(type).displayName)) RaceDistanceSession.Select(type);
            GUI.color = Color.white;
        }

        private static GUIStyle HeaderStyle(int size, Color color) => new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = FontStyle.Bold, normal = { textColor = color } };
    }
}
