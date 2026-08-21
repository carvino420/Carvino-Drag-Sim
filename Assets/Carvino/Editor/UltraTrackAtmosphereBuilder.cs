#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Carvino.Editor
{
    /// <summary>
    /// A visual-only PC Ultra+ pass for the quarter-mile.  It adds a carefully
    /// limited lighting rig, reflective prep-lane treatment, and emissive lane
    /// details without changing cameras, collision, race logic, or vehicle art.
    /// </summary>
    public static class UltraTrackAtmosphereBuilder
    {
        private const string ScenePath = "Assets/Carvino/Scenes/QuarterMilePrototype.unity";
        private const string RootName = "Carvino Ultra Track Atmosphere";

        [MenuItem("Carvino/Art/Add PC Ultra Track Atmosphere")]
        public static void Apply()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject previous = GameObject.Find(RootName);
            if (previous != null)
            {
                Object.DestroyImmediate(previous);
            }

            GameObject root = new GameObject(RootName);
            root.isStatic = true;

            Material rubberedAsphalt = MakeMaterial(new Color(0.028f, 0.038f, 0.048f), 0.12f, 0.78f);
            Material brushedSteel = MakeMaterial(new Color(0.10f, 0.14f, 0.17f), 0.84f, 0.49f);
            Material safetyPaint = MakeMaterial(new Color(0.22f, 0.27f, 0.29f), 0.10f, 0.35f);
            Material coolAccent = MakeEmission(new Color(0.025f, 0.38f, 1.0f), 1.05f);
            Material warmAccent = MakeEmission(new Color(1.0f, 0.28f, 0.065f), 1.35f);

            AddPrepLaneDetail(root.transform, rubberedAsphalt, safetyPaint, coolAccent);
            AddTracksideLighting(root.transform, brushedSteel, coolAccent, warmAccent);
            ConfigureNightAtmosphere(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Carvino PC Ultra+ track atmosphere applied to the quarter-mile scene.");
        }

        private static void AddPrepLaneDetail(Transform parent, Material asphalt, Material paint, Material accent)
        {
            // Thin, visual-only rubber lanes give the launch area a deeper prepared-strip look.
            for (int lane = -1; lane <= 1; lane += 2)
            {
                float x = lane * 3.35f;
                CreatePart(parent, lane < 0 ? "Ultra Left Rubber Lane" : "Ultra Right Rubber Lane",
                    PrimitiveType.Cube, new Vector3(x, 0.012f, 72f), new Vector3(2.35f, 0.014f, 143f), asphalt);
                CreatePart(parent, lane < 0 ? "Ultra Left Launch Box" : "Ultra Right Launch Box",
                    PrimitiveType.Cube, new Vector3(x, 0.020f, 11.5f), new Vector3(2.52f, 0.02f, 18f), paint);
            }

            CreatePart(parent, "Ultra Centerline Inset", PrimitiveType.Cube,
                new Vector3(0f, 0.026f, 64f), new Vector3(0.09f, 0.02f, 125f), accent);

            // Low guide strips frame the staging view without reading as copied venue signage.
            CreatePart(parent, "Ultra Left Barrier Guide", PrimitiveType.Cube,
                new Vector3(-10.0f, 0.95f, 36f), new Vector3(0.055f, 0.055f, 63f), accent);
            CreatePart(parent, "Ultra Right Barrier Guide", PrimitiveType.Cube,
                new Vector3(10.0f, 0.95f, 36f), new Vector3(0.055f, 0.055f, 63f), accent);
        }

        private static void AddTracksideLighting(Transform parent, Material steel, Material cool, Material warm)
        {
            float[] zPositions = { 28f, 76f, 142f, 226f, 316f };
            for (int i = 0; i < zPositions.Length; i++)
            {
                float z = zPositions[i];
                AddLightTower(parent, "Ultra Left Light Tower " + i, new Vector3(-14.8f, 0f, z), steel, cool, warm);
                AddLightTower(parent, "Ultra Right Light Tower " + i, new Vector3(14.8f, 0f, z), steel, cool, warm);
            }
        }

        private static void AddLightTower(Transform parent, string name, Vector3 basePosition, Material steel, Material cool, Material warm)
        {
            GameObject tower = new GameObject(name);
            tower.transform.SetParent(parent, true);
            CreatePart(tower.transform, "Base", PrimitiveType.Cylinder, basePosition + new Vector3(0f, 0.13f, 0f), new Vector3(0.52f, 0.13f, 0.52f), steel);
            CreatePart(tower.transform, "Mast", PrimitiveType.Cylinder, basePosition + new Vector3(0f, 3.65f, 0f), new Vector3(0.105f, 3.55f, 0.105f), steel);
            CreatePart(tower.transform, "Crossbar", PrimitiveType.Cube, basePosition + new Vector3(0f, 6.75f, 0f), new Vector3(1.55f, 0.11f, 0.20f), steel);
            CreatePart(tower.transform, "Cool Lamp", PrimitiveType.Cube, basePosition + new Vector3(-0.48f, 6.56f, -0.08f), new Vector3(0.42f, 0.26f, 0.22f), cool);
            CreatePart(tower.transform, "Warm Lamp", PrimitiveType.Cube, basePosition + new Vector3(0.48f, 6.56f, -0.08f), new Vector3(0.42f, 0.26f, 0.22f), warm);
            CreateSpotLight(tower.transform, "Track Fill", basePosition + new Vector3(0f, 6.35f, -0.25f), new Vector3(66f, 0f, 0f), new Color(0.38f, 0.62f, 1f), 3.0f, 11f, 48f);
        }

        private static void ConfigureNightAtmosphere(Transform parent)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.012f, 0.022f, 0.042f);
            RenderSettings.fogDensity = 0.00225f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.035f, 0.065f, 0.13f);
            RenderSettings.ambientEquatorColor = new Color(0.055f, 0.065f, 0.085f);
            RenderSettings.ambientGroundColor = new Color(0.012f, 0.014f, 0.018f);
            RenderSettings.ambientIntensity = 1.05f;

            GameObject moon = new GameObject("Ultra Cool Moon Key");
            moon.transform.SetParent(parent, true);
            Light key = moon.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(0.46f, 0.63f, 1f);
            key.intensity = 0.36f;
            key.shadows = LightShadows.Soft;
            key.shadowStrength = 0.68f;
            key.shadowResolution = LightShadowResolution.High;
            moon.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        }

        private static void CreateSpotLight(Transform parent, string name, Vector3 position, Vector3 euler, Color color, float intensity, float range, float angle)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, true);
            lightObject.transform.position = position;
            lightObject.transform.eulerAngles = euler;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.spotAngle = angle;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.35f;
            light.renderMode = LightRenderMode.Auto;
        }

        private static GameObject CreatePart(Transform parent, string name, PrimitiveType primitiveType, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(parent, true);
            part.transform.position = position;
            part.transform.localScale = scale;
            part.isStatic = true;
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
            Material material = MakeMaterial(color * 0.14f, 0.12f, 0.46f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
            return material;
        }
    }
}
#endif
