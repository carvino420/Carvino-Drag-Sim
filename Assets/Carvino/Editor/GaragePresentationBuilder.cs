#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carvino.Editor
{
    /// <summary>Adds original, logo-free presentation lighting and wall detail to the Garage scene.</summary>
    public static class GaragePresentationBuilder
    {
        private const string ScenePath = "Assets/Carvino/Scenes/Garage.unity";
        private const string RootName = "Carvino PC Garage Presentation Pass";

        [MenuItem("Carvino/Art/Add PC Garage Presentation Pass")]
        public static void AddToGarageScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (GameObject.Find(RootName) != null) { Debug.Log("Carvino PC garage presentation pass already exists."); return; }

            GameObject root = new GameObject(RootName);
            Material wall = MakeMaterial(new Color(0.075f, 0.094f, 0.12f), 0.42f, 0.38f);
            Material trim = MakeMaterial(new Color(0.16f, 0.205f, 0.25f), 0.82f, 0.56f);
            Material glow = MakeEmission(new Color(0.12f, 0.72f, 1f), 1.45f);
            Material warmGlow = MakeEmission(new Color(1f, 0.34f, 0.09f), 0.95f);
            CreateWallPanels(root.transform, wall, trim, glow);
            CreatePresentationLighting(root.transform, warmGlow, glow);
            CreateFloorDetails(root.transform, trim, glow);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Carvino original PC garage presentation pass added to the Garage scene.");
        }

        private static void CreateWallPanels(Transform parent, Material wall, Material trim, Material glow)
        {
            const float wallZ = 7.1f;
            CreatePart(parent, "Presentation Rear Wall", PrimitiveType.Cube, new Vector3(0f, 3.3f, wallZ), new Vector3(15.8f, 6.5f, 0.16f), wall);
            for (int index = 0; index < 4; index++)
            {
                float x = -5.85f + index * 3.9f;
                CreatePart(parent, "Presentation Wall Divider " + index, PrimitiveType.Cube, new Vector3(x, 3.35f, wallZ - 0.1f), new Vector3(0.09f, 6.3f, 0.12f), trim);
                CreatePart(parent, "Presentation Wall Glow " + index, PrimitiveType.Cube, new Vector3(x, 3.35f, wallZ - 0.17f), new Vector3(0.025f, 5.15f, 0.03f), glow);
            }
            CreatePart(parent, "Presentation Wall Cap", PrimitiveType.Cube, new Vector3(0f, 6.42f, wallZ - 0.09f), new Vector3(15.95f, 0.16f, 0.14f), trim);
            CreatePart(parent, "Presentation Lower Wall Rail", PrimitiveType.Cube, new Vector3(0f, 1.05f, wallZ - 0.1f), new Vector3(15.95f, 0.09f, 0.12f), trim);
        }

        private static void CreatePresentationLighting(Transform parent, Material warmGlow, Material coolGlow)
        {
            CreateLight(parent, "Presentation Key Light", new Vector3(-4.3f, 5.55f, -1.2f), new Color(0.82f, 0.91f, 1f), 3.1f, 9f, new Vector3(45f, 33f, 0f));
            CreateLight(parent, "Presentation Fill Light", new Vector3(4.7f, 4.65f, 1.9f), new Color(0.22f, 0.58f, 1f), 2.15f, 7.5f, new Vector3(38f, -118f, 0f));
            CreateLight(parent, "Presentation Rim Light", new Vector3(0f, 4.8f, 6.35f), new Color(1f, 0.31f, 0.12f), 2.6f, 6.2f, new Vector3(42f, 180f, 0f));
            CreatePart(parent, "Presentation Key Light Housing", PrimitiveType.Cube, new Vector3(-4.3f, 5.72f, -1.1f), new Vector3(1.55f, 0.18f, 0.5f), MakeMaterial(new Color(0.055f, 0.07f, 0.09f), 0.76f, 0.46f));
            CreatePart(parent, "Presentation Key Light Strip", PrimitiveType.Cube, new Vector3(-4.3f, 5.6f, -1.1f), new Vector3(1.18f, 0.035f, 0.16f), coolGlow);
            CreatePart(parent, "Presentation Rim Light Strip", PrimitiveType.Cube, new Vector3(0f, 4.9f, 6.23f), new Vector3(4.65f, 0.05f, 0.05f), warmGlow);
        }

        private static void CreateFloorDetails(Transform parent, Material trim, Material glow)
        {
            CreatePart(parent, "Presentation Bay Centerline", PrimitiveType.Cube, new Vector3(0f, 0.018f, -2.1f), new Vector3(0.08f, 0.025f, 5.65f), glow);
            CreatePart(parent, "Presentation Bay Rear Strip", PrimitiveType.Cube, new Vector3(0f, 0.018f, 3.55f), new Vector3(6.55f, 0.025f, 0.08f), glow);
            CreatePart(parent, "Presentation Lift Plate", PrimitiveType.Cube, new Vector3(0f, 0.026f, -0.55f), new Vector3(3.65f, 0.035f, 2.4f), trim);
            CreatePart(parent, "Presentation Lift Inset", PrimitiveType.Cube, new Vector3(0f, 0.047f, -0.55f), new Vector3(3.05f, 0.018f, 1.8f), MakeMaterial(new Color(0.045f, 0.06f, 0.08f), 0.55f, 0.34f));
        }

        private static void CreateLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range, Vector3 rotation)
        {
            GameObject lightObject = new GameObject(name); lightObject.transform.SetParent(parent, true); lightObject.transform.position = position; lightObject.transform.eulerAngles = rotation;
            Light light = lightObject.AddComponent<Light>(); light.type = LightType.Spot; light.color = color; light.intensity = intensity; light.range = range; light.spotAngle = 72f; light.shadows = LightShadows.None; light.renderMode = LightRenderMode.Auto;
        }

        private static GameObject CreatePart(Transform parent, string name, PrimitiveType primitiveType, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType); part.name = name; part.transform.SetParent(parent, true); part.transform.position = position; part.transform.localScale = scale; part.GetComponent<Renderer>().sharedMaterial = material; Object.DestroyImmediate(part.GetComponent<Collider>()); return part;
        }

        private static Material MakeMaterial(Color color, float metallic, float smoothness)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")); material.color = color; material.SetFloat("_Metallic", metallic); material.SetFloat("_Smoothness", smoothness); material.SetFloat("_Glossiness", smoothness); return material;
        }

        private static Material MakeEmission(Color color, float intensity)
        {
            Material material = MakeMaterial(color * 0.16f, 0.15f, 0.4f); material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", color * intensity); return material;
        }
    }
}
#endif
