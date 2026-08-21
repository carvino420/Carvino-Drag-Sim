#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carvino.Editor
{
    /// <summary>
    /// Adds original, logo-free visual framing to the prototype drag strip without
    /// changing race collision, timing, camera, or gameplay code.
    /// </summary>
    public static class TracksideRacePresentationBuilder
    {
        private const string ScenePath = "Assets/Carvino/Scenes/QuarterMilePrototype.unity";
        private const string RootName = "Carvino Race Presentation";

        [MenuItem("Carvino/Art/Add Race Presentation Pass")]
        public static void AddToPrototypeScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (GameObject.Find(RootName) != null)
            {
                Debug.Log("Carvino race presentation pass already exists in the quarter-mile scene.");
                return;
            }

            GameObject root = new GameObject(RootName);
            Material safetyConcrete = MakeMaterial(new Color(0.32f, 0.34f, 0.35f), 0.02f, 0.42f);
            Material darkMetal = MakeMaterial(new Color(0.055f, 0.07f, 0.09f), 0.72f, 0.4f);
            Material blueMetal = MakeMaterial(new Color(0.04f, 0.15f, 0.22f), 0.62f, 0.45f);
            Material laneAccent = MakeEmission(new Color(0.1f, 0.62f, 1f), 1.25f);
            Material warmLamp = MakeEmission(new Color(1f, 0.42f, 0.1f), 1.8f);
            Material redLamp = MakeEmission(new Color(1f, 0.11f, 0.05f), 1.9f);

            AddStarterFraming(root.transform, safetyConcrete, darkMetal, blueMetal, laneAccent, warmLamp);
            AddFinishFraming(root.transform, safetyConcrete, darkMetal, laneAccent, redLamp);
            AddTracksideMarkers(root.transform, safetyConcrete, warmLamp);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Carvino original race presentation pass added to the quarter-mile scene.");
        }

        private static void AddStarterFraming(Transform parent, Material concrete, Material steel, Material painted, Material accent, Material lamp)
        {
            CreatePart(parent, "Starter Left Safety Pedestal", PrimitiveType.Cube, new Vector3(-9.85f, 0.34f, 3.3f), new Vector3(1.05f, 0.68f, 2.2f), concrete);
            CreatePart(parent, "Starter Right Safety Pedestal", PrimitiveType.Cube, new Vector3(9.85f, 0.34f, 3.3f), new Vector3(1.05f, 0.68f, 2.2f), concrete);
            CreatePart(parent, "Starter Left Beacon Post", PrimitiveType.Cylinder, new Vector3(-9.85f, 1.64f, 3.3f), new Vector3(0.12f, 0.95f, 0.12f), steel);
            CreatePart(parent, "Starter Right Beacon Post", PrimitiveType.Cylinder, new Vector3(9.85f, 1.64f, 3.3f), new Vector3(0.12f, 0.95f, 0.12f), steel);
            CreatePart(parent, "Starter Left Beacon", PrimitiveType.Sphere, new Vector3(-9.85f, 2.65f, 3.3f), new Vector3(0.35f, 0.35f, 0.35f), lamp);
            CreatePart(parent, "Starter Right Beacon", PrimitiveType.Sphere, new Vector3(9.85f, 2.65f, 3.3f), new Vector3(0.35f, 0.35f, 0.35f), lamp);

            const float z = 13.4f;
            CreatePart(parent, "Starter Truss Left Upright", PrimitiveType.Cylinder, new Vector3(-10.4f, 3.0f, z), new Vector3(0.18f, 3.0f, 0.18f), painted);
            CreatePart(parent, "Starter Truss Right Upright", PrimitiveType.Cylinder, new Vector3(10.4f, 3.0f, z), new Vector3(0.18f, 3.0f, 0.18f), painted);
            CreatePart(parent, "Starter Truss Top", PrimitiveType.Cube, new Vector3(0f, 5.85f, z), new Vector3(21.0f, 0.20f, 0.25f), steel);
            CreatePart(parent, "Starter Truss Accent", PrimitiveType.Cube, new Vector3(0f, 5.55f, z - 0.16f), new Vector3(19.8f, 0.075f, 0.05f), accent);
            CreatePart(parent, "Starter Truss Left Brace", PrimitiveType.Cube, new Vector3(-8.0f, 4.65f, z), new Vector3(4.75f, 0.10f, 0.15f), steel, new Vector3(0f, 0f, -36f));
            CreatePart(parent, "Starter Truss Right Brace", PrimitiveType.Cube, new Vector3(8.0f, 4.65f, z), new Vector3(4.75f, 0.10f, 0.15f), steel, new Vector3(0f, 0f, 36f));
        }

        private static void AddFinishFraming(Transform parent, Material concrete, Material steel, Material accent, Material lamp)
        {
            const float z = 402.5f;
            CreatePart(parent, "Finish Left Footing", PrimitiveType.Cube, new Vector3(-10.7f, 0.26f, z), new Vector3(1.35f, 0.52f, 2.2f), concrete);
            CreatePart(parent, "Finish Right Footing", PrimitiveType.Cube, new Vector3(10.7f, 0.26f, z), new Vector3(1.35f, 0.52f, 2.2f), concrete);
            CreatePart(parent, "Finish Left Arch Upright", PrimitiveType.Cylinder, new Vector3(-10.7f, 3.3f, z), new Vector3(0.2f, 3.05f, 0.2f), steel);
            CreatePart(parent, "Finish Right Arch Upright", PrimitiveType.Cylinder, new Vector3(10.7f, 3.3f, z), new Vector3(0.2f, 3.05f, 0.2f), steel);
            CreatePart(parent, "Finish Arch Top", PrimitiveType.Cube, new Vector3(0f, 6.15f, z), new Vector3(21.8f, 0.24f, 0.30f), steel);
            CreatePart(parent, "Finish Arch Accent", PrimitiveType.Cube, new Vector3(0f, 5.83f, z - 0.19f), new Vector3(20.5f, 0.08f, 0.06f), accent);
            CreatePart(parent, "Finish Left Lamp", PrimitiveType.Cube, new Vector3(-9.55f, 6.04f, z - 0.22f), new Vector3(0.68f, 0.16f, 0.12f), lamp);
            CreatePart(parent, "Finish Right Lamp", PrimitiveType.Cube, new Vector3(9.55f, 6.04f, z - 0.22f), new Vector3(0.68f, 0.16f, 0.12f), lamp);
        }

        private static void AddTracksideMarkers(Transform parent, Material concrete, Material lamp)
        {
            int[] distances = { 67, 135, 202, 270, 337 };
            for (int i = 0; i < distances.Length; i++)
            {
                float z = distances[i];
                CreatePart(parent, "Left Distance Block " + distances[i], PrimitiveType.Cube, new Vector3(-10.55f, 0.25f, z), new Vector3(0.62f, 0.50f, 0.84f), concrete);
                CreatePart(parent, "Right Distance Block " + distances[i], PrimitiveType.Cube, new Vector3(10.55f, 0.25f, z), new Vector3(0.62f, 0.50f, 0.84f), concrete);
                CreatePart(parent, "Left Distance Lamp " + distances[i], PrimitiveType.Sphere, new Vector3(-10.55f, 0.82f, z), new Vector3(0.18f, 0.18f, 0.18f), lamp);
                CreatePart(parent, "Right Distance Lamp " + distances[i], PrimitiveType.Sphere, new Vector3(10.55f, 0.82f, z), new Vector3(0.18f, 0.18f, 0.18f), lamp);
            }
        }

        private static GameObject CreatePart(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material, Vector3? euler = null)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, true);
            part.transform.position = position;
            part.transform.localScale = scale;
            if (euler.HasValue) part.transform.eulerAngles = euler.Value;
            part.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            return part;
        }

        private static Material MakeMaterial(Color color, float metallic, float smoothness)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = color;
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Glossiness", smoothness);
            return material;
        }

        private static Material MakeEmission(Color color, float intensity)
        {
            Material material = MakeMaterial(color * 0.16f, 0.08f, 0.35f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
            return material;
        }
    }
}
#endif
