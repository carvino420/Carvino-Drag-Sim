#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carvino.Editor
{
    /// <summary>
    /// Builds a lightweight, original starter-side timing pavilion from Unity primitives.
    /// It deliberately contains no copied logos, signage, imagery, or brand marks.
    /// </summary>
    public static class TracksideStarterPavilionBuilder
    {
        private const string ScenePath = "Assets/Carvino/Scenes/QuarterMilePrototype.unity";
        private const string RootName = "Carvino Starter Pavilion";

        [MenuItem("Carvino/Art/Add Original Starter Pavilion")]
        public static void AddToPrototypeScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (GameObject.Find(RootName) != null)
            {
                Debug.Log("Carvino starter pavilion already exists in the quarter-mile scene.");
                return;
            }

            GameObject root = new GameObject(RootName);
            Material concrete = MakeMaterial(new Color(0.19f, 0.21f, 0.23f), 0.05f, 0.24f);
            Material darkSteel = MakeMaterial(new Color(0.055f, 0.07f, 0.09f), 0.72f, 0.46f);
            Material paintedSteel = MakeMaterial(new Color(0.08f, 0.16f, 0.21f), 0.58f, 0.42f);
            Material glass = MakeMaterial(new Color(0.09f, 0.3f, 0.38f), 0.08f, 0.82f);
            Material rail = MakeMaterial(new Color(0.35f, 0.4f, 0.43f), 0.9f, 0.55f);
            Material lamp = MakeEmission(new Color(1f, 0.54f, 0.2f), 2.35f);
            Material accent = MakeEmission(new Color(0.05f, 0.65f, 1f), 1.5f);

            // The pavilion sits clear of the left barrier and viewing frustum at the starting end.
            Vector3 origin = new Vector3(-16.9f, 0f, 17.5f);
            CreatePart(root.transform, "Pavilion Concrete Pad", PrimitiveType.Cube, origin + new Vector3(0f, 0.13f, 0f), new Vector3(6.4f, 0.26f, 5.2f), concrete);
            CreatePart(root.transform, "Pavilion Main Room", PrimitiveType.Cube, origin + new Vector3(0f, 2.05f, 0.65f), new Vector3(5.55f, 3.65f, 3.5f), paintedSteel);
            CreatePart(root.transform, "Pavilion Roof", PrimitiveType.Cube, origin + new Vector3(0f, 4.05f, 0.48f), new Vector3(6.25f, 0.28f, 4.05f), darkSteel);
            CreatePart(root.transform, "Pavilion Roof Overhang", PrimitiveType.Cube, origin + new Vector3(0f, 3.88f, -1.66f), new Vector3(6.55f, 0.18f, 0.7f), darkSteel);

            CreatePart(root.transform, "Pavilion Front Window", PrimitiveType.Cube, origin + new Vector3(0f, 2.48f, -1.16f), new Vector3(4.62f, 1.48f, 0.04f), glass);
            CreatePart(root.transform, "Pavilion Window Mullion Center", PrimitiveType.Cube, origin + new Vector3(0f, 2.48f, -1.21f), new Vector3(0.08f, 1.68f, 0.08f), darkSteel);
            CreatePart(root.transform, "Pavilion Window Mullion Left", PrimitiveType.Cube, origin + new Vector3(-1.53f, 2.48f, -1.21f), new Vector3(0.08f, 1.68f, 0.08f), darkSteel);
            CreatePart(root.transform, "Pavilion Window Mullion Right", PrimitiveType.Cube, origin + new Vector3(1.53f, 2.48f, -1.21f), new Vector3(0.08f, 1.68f, 0.08f), darkSteel);
            CreatePart(root.transform, "Pavilion Lower Accent", PrimitiveType.Cube, origin + new Vector3(0f, 1.55f, -1.24f), new Vector3(4.82f, 0.06f, 0.04f), accent);

            CreatePart(root.transform, "Pavilion Entry Landing", PrimitiveType.Cube, origin + new Vector3(0f, 0.44f, -2.35f), new Vector3(3.25f, 0.22f, 1.12f), concrete);
            CreatePart(root.transform, "Pavilion Top Step", PrimitiveType.Cube, origin + new Vector3(0f, 0.22f, -2.88f), new Vector3(3.65f, 0.22f, 0.6f), concrete);
            CreatePart(root.transform, "Pavilion Lower Step", PrimitiveType.Cube, origin + new Vector3(0f, 0.1f, -3.31f), new Vector3(4.0f, 0.2f, 0.42f), concrete);
            CreatePart(root.transform, "Pavilion Door", PrimitiveType.Cube, origin + new Vector3(0f, 1.82f, -1.26f), new Vector3(1.02f, 2.3f, 0.055f), darkSteel);
            CreatePart(root.transform, "Pavilion Door Handle", PrimitiveType.Cube, origin + new Vector3(0.32f, 1.82f, -1.31f), new Vector3(0.05f, 0.42f, 0.04f), rail);

            CreateRail(root.transform, "Pavilion Left Rail", origin + new Vector3(-2.1f, 1.02f, -2.8f), rail);
            CreateRail(root.transform, "Pavilion Right Rail", origin + new Vector3(2.1f, 1.02f, -2.8f), rail);

            CreatePart(root.transform, "Pavilion Left Floodlight", PrimitiveType.Cube, origin + new Vector3(-2.42f, 4.42f, -1.62f), new Vector3(0.46f, 0.24f, 0.28f), lamp);
            CreatePart(root.transform, "Pavilion Right Floodlight", PrimitiveType.Cube, origin + new Vector3(2.42f, 4.42f, -1.62f), new Vector3(0.46f, 0.24f, 0.28f), lamp);
            CreatePart(root.transform, "Pavilion Light Canopy", PrimitiveType.Cube, origin + new Vector3(0f, 4.35f, -1.63f), new Vector3(5.2f, 0.11f, 0.38f), darkSteel);
            CreatePart(root.transform, "Pavilion Entry Light", PrimitiveType.Cube, origin + new Vector3(0f, 3.6f, -1.7f), new Vector3(0.92f, 0.08f, 0.08f), lamp);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Carvino original starter pavilion added to the quarter-mile scene.");
        }

        private static void CreateRail(Transform parent, string name, Vector3 center, Material material)
        {
            CreatePart(parent, name + " Upright A", PrimitiveType.Cylinder, center + new Vector3(-0.62f, 0f, 0f), new Vector3(0.045f, 0.72f, 0.045f), material);
            CreatePart(parent, name + " Upright B", PrimitiveType.Cylinder, center + new Vector3(0.62f, 0f, 0f), new Vector3(0.045f, 0.72f, 0.045f), material);
            CreatePart(parent, name + " Top", PrimitiveType.Cube, center + new Vector3(0f, 0.7f, 0f), new Vector3(1.35f, 0.06f, 0.06f), material);
        }

        private static GameObject CreatePart(Transform parent, string name, PrimitiveType primitiveType, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(parent, true);
            part.transform.position = position;
            part.transform.localScale = scale;
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
            Material material = MakeMaterial(color * 0.17f, 0.12f, 0.38f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
            return material;
        }
    }
}
#endif
