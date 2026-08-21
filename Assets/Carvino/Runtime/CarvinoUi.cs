using UnityEngine;

namespace Carvino
{
    /// <summary>Keeps the prototype UI readable from 1080p through 4K without changing its layout.</summary>
    public static class CarvinoUi
    {
        public static float Scale => Mathf.Clamp(Screen.height / 900f, 1f, 2.35f);
        public static float Width => Screen.width / Scale;
        public static float Height => Screen.height / Scale;

        public static Matrix4x4 Begin()
        {
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Scale, Scale, 1f));
            return previous;
        }

        public static void End(Matrix4x4 previous) => GUI.matrix = previous;
    }
}
