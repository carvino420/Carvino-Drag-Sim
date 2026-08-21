using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carvino
{
    public sealed class ControlsController : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton6)) SceneManager.LoadScene("MainMenu");
        }

        private void OnGUI()
        {
            Matrix4x4 previous = CarvinoUi.Begin();
            GUI.Box(new Rect(18, 18, 760, 510), "CARVINO — CONTROLS");
            GUI.Label(new Rect(46, 58, 640, 30), "RACE CONTROLS", new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold });
            DrawRow(46, 110, "Throttle", "W  /  Controller A");
            DrawRow(46, 148, "Burnout", "Hold F");
            DrawRow(46, 186, "Stage", "B  /  Controller B");
            DrawRow(46, 224, "Shift", "Left/Right Shift  /  Controller RB");
            DrawRow(46, 262, "Camera", "C  /  Controller LB");
            DrawRow(46, 300, "Reset pass", "R  /  Controller Start");
            DrawRow(46, 338, "Back", "Esc  /  Controller Back");
            GUI.Box(new Rect(46, 382, 676, 72), "Garage: click any button, or use arrow keys / bumper buttons to browse vehicles and engines.  Dyno: use the on-screen buttons; H opens chassis setup.");
            if (GUI.Button(new Rect(46, 470, 220, 36), "BACK TO TITLE")) SceneManager.LoadScene("MainMenu");
            CarvinoUi.End(previous);
        }

        private static void DrawRow(float x, float y, string action, string control)
        {
            GUI.Label(new Rect(x, y, 220, 24), action);
            GUI.Label(new Rect(x + 245, y, 360, 24), control);
        }
    }
}
