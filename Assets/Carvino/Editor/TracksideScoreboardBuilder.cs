#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carvino.Editor
{
    /// <summary>
    /// Adds a pair of original, logo-free finish-line scoreboards to the prototype strip.
    /// This stays editor-only: the shipped scene contains ordinary Unity primitives.
    /// </summary>
    public static class TracksideScoreboardBuilder
    {
        private const string ScenePath = "Assets/Carvino/Scenes/QuarterMilePrototype.unity";
        private const string RootName = "Carvino Trackside Scoreboards";

        [MenuItem("Carvino/Art/Add Trackside Scoreboards")]
        public static void AddToPrototypeScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (GameObject.Find(RootName) != null)
            {
                Debug.Log("Carvino scoreboards already exist in the quarter-mile scene.");
                return;
            }

            GameObject root = new GameObject(RootName);
            CreateScoreboard(root.transform, "Left Finish Scoreboard", new Vector3(-12.8f, 0f, 397.5f), new Color(0.18f, 0.76f, 1f));
            CreateScoreboard(root.transform, "Right Finish Scoreboard", new Vector3(12.8f, 0f, 397.5f), new Color(1f, 0.3f, 0.1f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Carvino original trackside scoreboards added to the quarter-mile scene.");
        }

        private static void CreateScoreboard(Transform parent, string name, Vector3 origin, Color accent)
        {
            GameObject board = new GameObject(name);
            board.transform.SetParent(parent, false);

            Material steel = CreateMaterial(new Color(0.10f, 0.12f, 0.15f));
            Material concrete = CreateMaterial(new Color(0.27f, 0.29f, 0.30f));
            Material face = CreateMaterial(new Color(0.012f, 0.018f, 0.026f));
            Material glow = CreateEmissionMaterial(accent, 2.1f);
            Material paleGlow = CreateEmissionMaterial(new Color(0.74f, 0.88f, 1f), 1.4f);

            CreatePart(board.transform, "Concrete Footing", PrimitiveType.Cube, origin + new Vector3(0f, 0.22f, 0f), new Vector3(3.7f, 0.44f, 1.4f), concrete);
            CreatePart(board.transform, "Left Upright", PrimitiveType.Cylinder, origin + new Vector3(-1.25f, 3.5f, 0f), new Vector3(0.16f, 3.5f, 0.16f), steel);
            CreatePart(board.transform, "Right Upright", PrimitiveType.Cylinder, origin + new Vector3(1.25f, 3.5f, 0f), new Vector3(0.16f, 3.5f, 0.16f), steel);
            CreatePart(board.transform, "Lower Cross Brace", PrimitiveType.Cube, origin + new Vector3(0f, 1.2f, 0f), new Vector3(2.8f, 0.12f, 0.18f), steel);
            CreatePart(board.transform, "Upper Cross Brace", PrimitiveType.Cube, origin + new Vector3(0f, 5.72f, 0f), new Vector3(2.8f, 0.12f, 0.18f), steel);

            CreatePart(board.transform, "Display Housing", PrimitiveType.Cube, origin + new Vector3(0f, 5.05f, 0f), new Vector3(3.45f, 1.85f, 0.30f), steel);
            CreatePart(board.transform, "Display Face", PrimitiveType.Cube, origin + new Vector3(0f, 5.05f, -0.18f), new Vector3(3.12f, 1.52f, 0.03f), face);
            CreatePart(board.transform, "Top Accent", PrimitiveType.Cube, origin + new Vector3(0f, 5.85f, -0.21f), new Vector3(3.12f, 0.10f, 0.035f), glow);

            // Abstract split-time bars: no copied logos, text, or real track branding.
            CreatePart(board.transform, "Left Time Bar", PrimitiveType.Cube, origin + new Vector3(-0.73f, 5.24f, -0.21f), new Vector3(0.94f, 0.23f, 0.04f), paleGlow);
            CreatePart(board.transform, "Right Time Bar", PrimitiveType.Cube, origin + new Vector3(0.73f, 5.24f, -0.21f), new Vector3(0.94f, 0.23f, 0.04f), paleGlow);
            CreatePart(board.transform, "Speed Bar", PrimitiveType.Cube, origin + new Vector3(0f, 4.67f, -0.21f), new Vector3(2.25f, 0.17f, 0.04f), glow);

            CreatePart(board.transform, "Left Flood", PrimitiveType.Cube, origin + new Vector3(-1.55f, 6.08f, -0.02f), new Vector3(0.38f, 0.22f, 0.38f), glow);
            CreatePart(board.transform, "Right Flood", PrimitiveType.Cube, origin + new Vector3(1.55f, 6.08f, -0.02f), new Vector3(0.38f, 0.22f, 0.38f), glow);
        }

        private static GameObject CreatePart(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, true);
            part.transform.position = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            return part;
        }

        private static Material CreateMaterial(Color color)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = color;
            return material;
        }

        private static Material CreateEmissionMaterial(Color color, float intensity)
        {
            Material material = CreateMaterial(color * 0.2f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
            return material;
        }
    }
}
#endif
