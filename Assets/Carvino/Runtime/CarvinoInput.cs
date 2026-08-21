using UnityEngine;

namespace Carvino
{
    /// <summary>Single-player keyboard and controller bindings for the prototype.</summary>
    public static class CarvinoInput
    {
        public static bool HatchPressed => KeyDown(KeyCode.Alpha1);
        public static bool PickupPressed => KeyDown(KeyCode.Alpha2);
        public static bool NextEnginePressed => KeyDown(KeyCode.E) || JoystickDown(KeyCode.JoystickButton3);
        public static bool ToggleUpgradesPressed => KeyDown(KeyCode.U) || JoystickDown(KeyCode.JoystickButton2);
        public static bool StagePressed => KeyDown(KeyCode.B) || JoystickDown(KeyCode.JoystickButton1);
        public static bool LaunchPressed => KeyDown(KeyCode.Space);
        public static bool ShiftPressed => KeyDown(KeyCode.LeftShift) || KeyDown(KeyCode.RightShift) || JoystickDown(KeyCode.JoystickButton5);
        public static bool ResetPressed => KeyDown(KeyCode.R) || JoystickDown(KeyCode.JoystickButton7);

        public static float Throttle
        {
            get
            {
                float keyboard = Input.GetKey(KeyCode.W) ? 1f : 0f;
                float controller = Input.GetKey(KeyCode.JoystickButton0) ? 1f : 0f;
                return Mathf.Max(keyboard, controller);
            }
        }

        private static bool KeyDown(KeyCode key) => Input.GetKeyDown(key);

        private static bool JoystickDown(KeyCode key) => Input.GetKeyDown(key);
    }
}
