#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carvino.Editor
{
    /// <summary>
    /// Adds an original, logo-free PC garage presentation pass using compact static primitives.
    /// The helper is editor-only; the placed props become ordinary scene geometry at build time.
    /// </summary>
    public static class GarageBayDressingBuilder
    {
        private const string ScenePath = "Assets/Carvino/Scenes/Garage.unity";
        private const string RootName = "Carvino PC Garage Bay Dressing";

        [MenuItem("Carvino/Art/Add PC Garage Bay Dressing")]
        public static void AddToGarageScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (GameObject.Find(RootName) != null)
            {
                Debug.Log("Carvino PC garage bay dressing already exists.");
                return;
            }

            GameObject root = new GameObject(RootName);
            Material charcoal = CreateMaterial(new Color(0.045f, 0.055f, 0.07f), 0.68f);
            Material steel = CreateMaterial(new Color(0.18f, 0.22f, 0.28f), 0.8f);
            Material worktop = CreateMaterial(new Color(0.105f, 0.13f, 0.17f), 0.55f);
            Material accent = CreateEmissionMaterial(new Color(0.08f, 0.7f, 1f), 1.8f);
            Material warm = CreateEmissionMaterial(new Color(1f, 0.44f, 0.1f), 1.25f);

            CreateWorkbench(root.transform, new Vector3(-7.75f, 0f, 5.8f), charcoal, steel, worktop, accent);
            CreateToolLocker(root.transform, new Vector3(7.65f, 0f, 5.85f), charcoal, steel, accent);
            CreateOverheadLights(root.transform, warm);
            CreateFloorBayMarkers(root.transform, accent);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Carvino original PC garage bay dressing added to the Garage scene.");
        }

        private static void CreateWorkbench(Transform parent, Vector3 origin, Material charcoal, Material steel, Material worktop, Material accent)
        {
            CreatePart(parent, "Workbench Body", PrimitiveType.Cube, origin + new Vector3(0f, 1.05f, 0f), new Vector3(4.2f, 1.65f, 0.82f), charcoal);
            CreatePart(parent, "Workbench Top", PrimitiveType.Cube, origin + new Vector3(0f, 1.92f, 0f), new Vector3(4.36f, 0.16f, 0.96f), worktop);
            CreatePart(parent, "Workbench Lower Shelf", PrimitiveType.Cube, origin + new Vector3(0f, 0.46f, 0f), new Vector3(3.78f, 0.12f, 0.66f), steel);
            CreatePart(parent, "Workbench Glow Strip", PrimitiveType.Cube, origin + new Vector3(0f, 1.53f, -0.43f), new Vector3(3.86f, 0.055f, 0.028f), accent);
            for (int index = 0; index < 3; index++)
            {
                CreatePart(parent, "Workbench Drawer " + index, PrimitiveType.Cube, origin + new Vector3(-0.92f + index * 0.92f, 0.95f, -0.43f), new Vector3(0.72f, 0.38f, 0.035f), steel);
            }
            CreatePart(parent, "Workbench Vise", PrimitiveType.Cube, origin + new Vector3(1.35f, 2.12f, 0f), new Vector3(0.52f, 0.25f, 0.4f), steel);
            CreatePart(parent, "Workbench Vise Jaw", PrimitiveType.Cube, origin + new Vector3(1.35f, 2.28f, -0.12f), new Vector3(0.58f, 0.12f, 0.1f), steel);
        }

        private static void CreateToolLocker(Transform parent, Vector3 origin, Material charcoal, Material steel, Material accent)
        {
            CreatePart(parent, "Tool Locker", PrimitiveType.Cube, origin + new Vector3(0f, 2f, 0f), new Vector3(2.5f, 4f, 0.92f), charcoal);
            CreatePart(parent, "Tool Locker Door Left", PrimitiveType.Cube, origin + new Vector3(-0.6f, 2.05f, -0.48f), new Vector3(1.02f, 3.52f, 0.04f), steel);
            CreatePart(parent, "Tool Locker Door Right", PrimitiveType.Cube, origin + new Vector3(0.6f, 2.05f, -0.48f), new Vector3(1.02f, 3.52f, 0.04f), steel);
            CreatePart(parent, "Tool Locker Accent", PrimitiveType.Cube, origin + new Vector3(0f, 3.48f, -0.51f), new Vector3(1.96f, 0.075f, 0.03f), accent);
            CreatePart(parent, "Tool Locker Handle Left", PrimitiveType.Cube, origin + new Vector3(-0.17f, 2f, -0.53f), new Vector3(0.07f, 0.7f, 0.035f), charcoal);
            CreatePart(parent, "Tool Locker Handle Right", PrimitiveType.Cube, origin + new Vector3(0.17f, 2f, -0.53f), new Vector3(0.07f, 0.7f, 0.035f), charcoal);
        }

        private static void CreateOverheadLights(Transform parent, Material warm)
        {
            for (int index = 0; index < 3; index++)
            {
                float x = -4.8f + index * 4.8f;
                CreatePart(parent, "Garage Ceiling Light Housing " + index, PrimitiveType.Cube, new Vector3(x, 6.8f, -0.5f), new Vector3(3.45f, 0.12f, 0.44f), CreateMaterial(new Color(0.11f, 0.13f, 0.16f), 0.55f));
                CreatePart(parent, "Garage Ceiling Light Tube " + index, PrimitiveType.Cube, new Vector3(x, 6.69f, -0.5f), new Vector3(2.95f, 0.035f, 0.13f), warm);
            }
        }

        private static void CreateFloorBayMarkers(Transform parent, Material accent)
        {
            CreatePart(parent, "Bay Marker Left", PrimitiveType.Cube, new Vector3(-3.3f, 0.015f, -3.25f), new Vector3(0.06f, 0.025f, 7.2f), accent);
            CreatePart(parent, "Bay Marker Right", PrimitiveType.Cube, new Vector3(3.3f, 0.015f, -3.25f), new Vector3(0.06f, 0.025f, 7.2f), accent);
            CreatePart(parent, "Bay Marker Front", PrimitiveType.Cube, new Vector3(0f, 0.015f, 0.2f), new Vector3(6.66f, 0.025f, 0.06f), accent);
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

        private static Material CreateMaterial(Color color, float metallic)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = color;
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Glossiness", 0.48f);
            return material;
        }

        private static Material CreateEmissionMaterial(Color color, float intensity)
        {
            Material material = CreateMaterial(color * 0.18f, 0.15f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
            return material;
        }
    }
}
#endif
