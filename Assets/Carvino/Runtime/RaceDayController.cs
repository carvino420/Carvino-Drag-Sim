using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carvino
{
    public sealed class RaceDayController : MonoBehaviour
    {
        private void Start() => GarageSession.Load();

        private void OnGUI()
        {
            Matrix4x4 previousMatrix = CarvinoUi.Begin();
            GUI.Box(new Rect(18, 18, 920, 650), "CARVINO — FREE PLAY");
            GUI.Label(new Rect(46, 55, 720, 36), "RUN WHAT YOU BRUNG", new GUIStyle(GUI.skin.label) { fontSize = 27, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } });
            GUI.Label(new Rect(48, 94, 720, 22), $"Choose any available event, surface, and length. YOUR WALLET: {GarageSession.VteCoins:N0} V-TECoins", new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = new Color(0.72f, 0.76f, 0.78f) } });
            GUI.Label(new Rect(48, 120, 300, 22), "PASS LENGTH");
            DrawDistanceButton(RaceDistanceType.EighthMile, new Rect(166, 116, 150, 30));
            DrawDistanceButton(RaceDistanceType.QuarterMile, new Rect(324, 116, 150, 30));

            for (int i = 0; i < RaceEventSession.Events.Length; i++)
            {
                RaceEvent raceEvent = RaceEventSession.Events[i];
                int column = i % 2;
                int row = i / 2;
                float x = 48 + column * 410;
                float y = 164 + row * 102;
                bool selected = i == RaceEventSession.SelectedIndex;
                GUI.color = selected ? new Color(0.82f, 0.18f, 0.08f) : new Color(0.16f, 0.18f, 0.2f);
                if (GUI.Button(new Rect(x, y, 394, 86), raceEvent.name + "\n" + raceEvent.description + "\n" + raceEvent.opponent.displayName + "  •  WIN: " + raceEvent.winPayout.ToString("N0") + " VTC  •  LOSS: " + raceEvent.lossPayout.ToString("N0") + " VTC"))
                {
                    RaceEventSession.Select(i);
                    SceneManager.LoadScene("QuarterMilePrototype");
                }
                GUI.color = Color.white;
            }

            GUI.Label(new Rect(48, 486, 350, 24), "TRACK SURFACE — affects both drivers");
            DrawSurfaceButton(TrackSurfaceType.PreppedStrip, new Rect(48, 514, 254, 38));
            DrawSurfaceButton(TrackSurfaceType.Street, new Rect(314, 514, 254, 38));
            DrawSurfaceButton(TrackSurfaceType.DampStreet, new Rect(580, 514, 254, 38));
            if (GUI.Button(new Rect(48, 574, 240, 40), "BACK TO GAME HUB")) SceneManager.LoadScene("MainMenu");
            GUI.Label(new Rect(308, 585, 520, 22), RaceSurfaceSession.Selected.description);
            CarvinoUi.End(previousMatrix);
        }

        private static void DrawSurfaceButton(TrackSurfaceType type, Rect rect)
        {
            TrackSurfaceSpec surface = TrackSurfaceCatalog.Get(type);
            GUI.color = RaceSurfaceSession.SelectedType == type ? new Color(.74f, .21f, .08f) : Color.white;
            if (GUI.Button(rect, surface.displayName + "  " + surface.gripMultiplier.ToString("0.00") + " grip")) RaceSurfaceSession.Select(type);
            GUI.color = Color.white;
        }

        private static void DrawDistanceButton(RaceDistanceType type, Rect rect)
        {
            GUI.color = RaceDistanceSession.SelectedType == type ? new Color(.74f, .21f, .08f) : Color.white;
            if (GUI.Button(rect, RaceDistanceCatalog.Get(type).displayName)) RaceDistanceSession.Select(type);
            GUI.color = Color.white;
        }
    }
}
