using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carvino
{
    public sealed class MainMenuController : MonoBehaviour
    {
        private readonly string[] options = { "CAREER", "FREE PLAY", "GARAGE", "DYNO & TUNE", "PROFILE", "CONTROLS", "SETTINGS", "EXIT GAME" };
        private readonly string[] descriptions =
        {
            "Start here. Build a reputation, unlock tougher races, and earn V-TECoins.",
            "Choose any event, race distance, and track surface without career locks.",
            "Choose your vehicle, install parts, swap engines, repair wear, and check saved builds.",
            "Set fuel, AFR, timing, boost, tire pressure, launch RPM, shift RPM, and chassis setup.",
            "Set your driver name and review rank, wallet, wins, passes, and your current build.",
            "Review keyboard and controller controls before heading to the starting line.",
            "Set display mode, resolution, graphics, V-sync, engine volume, and camera distance.",
            "Close Carvino Drag Sim. Your local progress is saved automatically."
        };
        private int selected;
        private float previousVerticalAxis;

        private void Start() => GarageSession.Load();

        private void Update()
        {
            int navigation = ReadNavigation();
            if (navigation != 0) selected = (selected + navigation + options.Length) % options.Length;
            if (Input.GetKeyDown(KeyCode.G)) selected = 2;
            if (Input.GetKeyDown(KeyCode.R)) selected = 1;
            if (Input.GetKeyDown(KeyCode.D)) selected = 3;
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton7)) Activate(selected);
        }

        /// <summary>
        /// Supports keyboard, controller bumpers, and the legacy Input Manager's Vertical axis.
        /// Axis navigation fires only when the stick/D-pad crosses the threshold, preventing a held
        /// controller direction from skipping through the whole menu in one frame sequence.
        /// </summary>
        private int ReadNavigation()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.JoystickButton4)) return -1;
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.JoystickButton5)) return 1;

            float vertical = Input.GetAxisRaw("Vertical");
            int direction = 0;
            if (vertical >= 0.6f && previousVerticalAxis < 0.6f) direction = -1;
            if (vertical <= -0.6f && previousVerticalAxis > -0.6f) direction = 1;
            previousVerticalAxis = vertical;
            return direction;
        }

        private void Activate(int index)
        {
            switch (index)
            {
                case 0: SceneManager.LoadScene("Career"); break;
                case 1: SceneManager.LoadScene("RaceDay"); break;
                case 2: SceneManager.LoadScene("Garage"); break;
                case 3: SceneManager.LoadScene("Dyno"); break;
                case 4: SceneManager.LoadScene("Profile"); break;
                case 5: SceneManager.LoadScene("Controls"); break;
                case 6: SceneManager.LoadScene("Settings"); break;
                case 7: Application.Quit(); break;
            }
        }

        private void OnGUI()
        {
            Matrix4x4 previousMatrix = CarvinoUi.Begin();
            GUI.Box(new Rect(0, 0, CarvinoUi.Width, CarvinoUi.Height), GUIContent.none);
            GUI.Label(new Rect(58, 58, 760, 64), "CARVINO", TitleStyle(46, new Color(0.9f, 0.12f, 0.07f)));
            GUI.Label(new Rect(62, 118, 760, 38), "DRAG SIM", TitleStyle(26, Color.white));
            GUI.Label(new Rect(62, 160, 760, 24), "ALPHA v0.05  •  BUILD IT. TUNE IT. SEND IT.", TitleStyle(14, new Color(0.72f, 0.75f, 0.76f)));
            GUI.Label(new Rect(62, 194, 760, 20), $"{PlayerProfile.DriverName}  •  {CareerProgress.RankName}  •  {GarageSession.VteCoins:N0} V-TECoins", TitleStyle(13, new Color(0.56f, 0.6f, 0.62f)));

            for (int index = 0; index < options.Length; index++)
            {
                bool active = index == selected;
                Rect button = new Rect(62, 236 + index * 48, 310, 38);
                Color previous = GUI.color;
                GUI.color = active ? new Color(0.86f, 0.14f, 0.08f) : new Color(0.15f, 0.17f, 0.19f);
                if (GUI.Button(button, (active ? ">  " : "   ") + options[index], MenuStyle(active)) || (Event.current.type == EventType.KeyDown && false)) Activate(index);
                GUI.color = previous;
            }
            DrawGuidePanel();
            GUI.Label(new Rect(62, CarvinoUi.Height - 62, 730, 20), "Click a menu option, or use ↑ ↓ / left stick / D-pad and Enter / A / Start.", TitleStyle(12, new Color(0.7f, 0.72f, 0.74f)));
            CarvinoUi.End(previousMatrix);
        }

        private void DrawGuidePanel()
        {
            GUI.Box(new Rect(418, 236, 410, 410), "GAME GUIDE");
            GUI.Label(new Rect(446, 276, 350, 30), options[selected], TitleStyle(23, new Color(.94f, .22f, .1f)));
            GUI.Label(new Rect(446, 320, 340, 66), descriptions[selected], BodyStyle(15));
            GUI.Box(new Rect(446, 404, 350, 122), "NEW PLAYER ROUTE\n1. GARAGE — pick your car and parts\n2. DYNO & TUNE — make safe power\n3. CAREER — stage, race, earn, improve");
            GUI.Label(new Rect(446, 548, 340, 25), $"NEXT CAREER TARGET: {NextCareerTarget()}", TitleStyle(13, new Color(.68f, .82f, .96f)));
            GUI.Label(new Rect(446, 579, 340, 25), "One shared build, one shared simulation, one saved profile.", BodyStyle(12));
        }

        private static string NextCareerTarget()
        {
            if (RaceHistory.TotalWins < 1) return "win Local Grudge to unlock Track Night";
            if (RaceHistory.TotalWins < 3) return "win 3 career races to unlock Money Run";
            return "improve your personal best and build for the Money Run";
        }

        private static GUIStyle TitleStyle(int fontSize, Color color)
        {
            return new GUIStyle(GUI.skin.label) { fontSize = fontSize, fontStyle = FontStyle.Bold, normal = { textColor = color } };
        }

        private static GUIStyle MenuStyle(bool active)
        {
            return new GUIStyle(GUI.skin.button) { fontSize = 17, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, normal = { textColor = active ? Color.white : new Color(0.84f, 0.86f, 0.88f) } };
        }

        private static GUIStyle BodyStyle(int fontSize)
        {
            return new GUIStyle(GUI.skin.label) { fontSize = fontSize, wordWrap = true, normal = { textColor = new Color(.82f, .85f, .88f) } };
        }
    }
}
