#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Carvino.Editor
{
    public static class PrototypeSceneBuilder
    {
        [MenuItem("Carvino/Build v0.01 Prototype Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

              Camera camera = new GameObject("Race Camera").AddComponent<Camera>();
              camera.transform.position = new Vector3(-2.6f, 3.8f, -9f);
              camera.clearFlags = CameraClearFlags.SolidColor;
              camera.backgroundColor = new Color(0.012f, 0.022f, 0.042f);
              camera.allowHDR = true;
              RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
              RenderSettings.ambientLight = new Color(0.055f, 0.075f, 0.12f);
              RenderSettings.fog = true;
              RenderSettings.fogColor = new Color(0.018f, 0.03f, 0.055f);
              RenderSettings.fogDensity = .0032f;

              var light = new GameObject("Moonlight").AddComponent<Light>();
              light.type = LightType.Directional;
              light.intensity = .48f;
              light.color = new Color(.48f, .63f, 1f);
              light.transform.rotation = Quaternion.Euler(42f, -28f, 0f);

              CreateTexturedPrimitive("Asphalt", PrimitiveType.Cube, new Vector3(0f, -0.25f, 210f), new Vector3(15.5f, 0.5f, 460f), "Assets/Carvino/Art/Textures/drag_strip_asphalt_01.png", new Color(.22f, .235f, .26f), new Vector2(4f, 120f));
              CreatePrimitive("Grass", PrimitiveType.Cube, new Vector3(0f, -0.65f, 210f), new Vector3(115f, 0.35f, 460f), new Color(.035f, .10f, .055f));
              CreatePrimitive("Left Barrier", PrimitiveType.Cube, new Vector3(-8.15f, 0.45f, 210f), new Vector3(0.4f, 0.9f, 460f), new Color(.45f, .46f, .46f));
              CreatePrimitive("Right Barrier", PrimitiveType.Cube, new Vector3(8.15f, 0.45f, 210f), new Vector3(0.4f, 0.9f, 460f), new Color(.45f, .46f, .46f));
              CreatePrimitive("Center Divider", PrimitiveType.Cube, new Vector3(0f, 0.32f, 210f), new Vector3(0.32f, 0.65f, 460f), new Color(0.54f, 0.55f, 0.54f));
            CreatePrimitive("Left Start Line", PrimitiveType.Cube, new Vector3(-3.8f, 0.015f, 0f), new Vector3(7.3f, 0.03f, 0.45f), Color.white);
            CreatePrimitive("Right Start Line", PrimitiveType.Cube, new Vector3(3.8f, 0.015f, 0f), new Vector3(7.3f, 0.03f, 0.45f), Color.white);
            CreatePrimitive("Left Finish Line", PrimitiveType.Cube, new Vector3(-3.8f, 0.015f, DragSimulation.QuarterMileMeters), new Vector3(7.3f, 0.03f, 0.65f), Color.white);
            CreatePrimitive("Right Finish Line", PrimitiveType.Cube, new Vector3(3.8f, 0.015f, DragSimulation.QuarterMileMeters), new Vector3(7.3f, 0.03f, 0.65f), Color.white);
            CreatePrimitive("Starter Apron", PrimitiveType.Cube, new Vector3(0f, 0.02f, 7f), new Vector3(1.6f, 0.04f, 15f), new Color(0.18f, 0.29f, 0.2f));
            CreatePrimitive("Timing Booth", PrimitiveType.Cube, new Vector3(-12f, 2.2f, 25f), new Vector3(5f, 4.5f, 6f), new Color(0.18f, 0.15f, 0.12f));
            CreateGrandstand("Left Grandstand", new Vector3(-16f, 0f, 42f), -1f);
            CreateGrandstand("Right Grandstand", new Vector3(16f, 0f, 42f), 1f);
            CreateLightPole("Light Pole Left", new Vector3(-13f, 0f, 7f));
            CreateLightPole("Light Pole Right", new Vector3(13f, 0f, 7f));
              CreateLightPole("Light Pole Down Track", new Vector3(-13f, 0f, 115f));
              CreateLightPole("Light Pole Down Track Right", new Vector3(13f, 0f, 115f));
              for (int index = 0; index < 6; index++)
              {
                  float distance = 175f + index * 42f;
                  CreateLightPole("Track Flood Left " + index, new Vector3(-13f, 0f, distance));
                  CreateLightPole("Track Flood Right " + index, new Vector3(13f, 0f, distance + 20f));
                  CreateBarrierReflectors(distance);
              }
            CreateStartingLineProps();
              CreateRuralTreeLine();

            GameObject vehicle = CreateHatch("Player Hatch", new Vector3(-3.8f, 0.05f, 0f), new Color(0.78f, 0.08f, 0.05f));
            GameObject playerPickup = CreatePickup("Player Pickup", new Vector3(-3.8f, 0.05f, 0f), new Color(0.72f, 0.12f, 0.06f));
            GameObject rival = CreatePickup("Rival Pickup", new Vector3(3.8f, 0.05f, 0f), new Color(0.07f, 0.36f, 0.72f));
            var cameraFollow = camera.gameObject.AddComponent<FollowCamera>();
            var cameraData = new SerializedObject(cameraFollow);
            cameraData.FindProperty("target").objectReferenceValue = vehicle.transform;
            cameraData.ApplyModifiedPropertiesWithoutUndo();

            Renderer[] bulbs = new Renderer[5];
            for (int i = 0; i < bulbs.Length; i++)
            {
                Color color = i < 3 ? new Color(1f, 0.62f, 0f) : i == 3 ? Color.green : Color.red;
                GameObject bulb = CreatePrimitive("Tree Bulb " + i, PrimitiveType.Sphere, new Vector3(-0.8f, 4.4f - i * 0.7f, 4.7f), Vector3.one * 0.42f, color * 0.12f);
                bulbs[i] = bulb.GetComponent<Renderer>();
            }
            CreatePrimitive("Tree Backboard", PrimitiveType.Cube, new Vector3(-0.8f, 3f, 5.05f), new Vector3(1.15f, 4.7f, 0.28f), new Color(0.045f, 0.05f, 0.055f));
            CreatePrimitive("Tree Frame Left", PrimitiveType.Cube, new Vector3(-1.42f, 3.05f, 4.63f), new Vector3(0.12f, 5.15f, 0.12f), new Color(0.12f, 0.13f, 0.14f));
            CreatePrimitive("Tree Frame Right", PrimitiveType.Cube, new Vector3(-0.18f, 3.05f, 4.63f), new Vector3(0.12f, 5.15f, 0.12f), new Color(0.12f, 0.13f, 0.14f));
            CreatePrimitive("Tree Frame Top", PrimitiveType.Cube, new Vector3(-0.8f, 5.55f, 4.63f), new Vector3(1.35f, 0.12f, 0.12f), new Color(0.12f, 0.13f, 0.14f));
            CreatePrimitive("Tree Stand", PrimitiveType.Cylinder, new Vector3(-0.8f, 0.75f, 5f), new Vector3(0.16f, 0.75f, 0.16f), new Color(0.12f, 0.12f, 0.12f));
            var controller = new GameObject("Race Controller").AddComponent<PrototypeRaceController>();
              EngineAudioSynth engineAudio = new GameObject("Engine Audio").AddComponent<EngineAudioSynth>();
              BurnoutVisualEffects hatchSmoke = CreateBurnoutSmoke("Hatch Burnout Smoke", vehicle.transform, new Vector3(0f, .38f, -1.3f));
              BurnoutVisualEffects pickupSmoke = CreateBurnoutSmoke("Pickup Burnout Smoke", playerPickup.transform, new Vector3(0f, .46f, -1.55f));
              GameObject hatchFlash = CreateExhaustFlash("Hatch Shift Flash", vehicle.transform, new Vector3(.62f, .54f, -2.2f));
              GameObject pickupFlash = CreateExhaustFlash("Pickup Shift Flash", playerPickup.transform, new Vector3(.72f, .56f, -2.56f));
              var serialized = new SerializedObject(controller);
            serialized.FindProperty("vehicleVisual").objectReferenceValue = vehicle.transform;
            serialized.FindProperty("hatchVisual").objectReferenceValue = vehicle.transform;
            serialized.FindProperty("pickupVisual").objectReferenceValue = playerPickup.transform;
            serialized.FindProperty("opponentVisual").objectReferenceValue = rival.transform;
            serialized.FindProperty("followCamera").objectReferenceValue = cameraFollow;
              serialized.FindProperty("engineAudio").objectReferenceValue = engineAudio;
              serialized.FindProperty("exhaustFlashes").arraySize = 2;
              serialized.FindProperty("exhaustFlashes").GetArrayElementAtIndex(0).objectReferenceValue = hatchFlash;
              serialized.FindProperty("exhaustFlashes").GetArrayElementAtIndex(1).objectReferenceValue = pickupFlash;
            serialized.FindProperty("treeBulbs").arraySize = bulbs.Length;
            for (int i = 0; i < bulbs.Length; i++) serialized.FindProperty("treeBulbs").GetArrayElementAtIndex(i).objectReferenceValue = bulbs[i];
              serialized.ApplyModifiedPropertiesWithoutUndo();
              hatchSmoke.SetRaceController(controller);
              pickupSmoke.SetRaceController(controller);

            string folder = "Assets/Carvino/Scenes";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Carvino", "Scenes");
            EditorSceneManager.SaveScene(scene, folder + "/QuarterMilePrototype.unity");
              BuildMainMenuScene(folder);
              BuildControlsScene(folder);
              BuildSettingsScene(folder);
              BuildCareerScene(folder);
              BuildProfileScene(folder);
              BuildGarageScene(folder);
            BuildDynoScene(folder);
            BuildRaceDayScene(folder);
            EditorBuildSettings.scenes = new[]
            {
                  new EditorBuildSettingsScene(folder + "/MainMenu.unity", true),
                  new EditorBuildSettingsScene(folder + "/Controls.unity", true),
                  new EditorBuildSettingsScene(folder + "/Settings.unity", true),
                  new EditorBuildSettingsScene(folder + "/Career.unity", true),
                  new EditorBuildSettingsScene(folder + "/Profile.unity", true),
                  new EditorBuildSettingsScene(folder + "/Garage.unity", true),
                new EditorBuildSettingsScene(folder + "/Dyno.unity", true),
                new EditorBuildSettingsScene(folder + "/RaceDay.unity", true),
                new EditorBuildSettingsScene(folder + "/QuarterMilePrototype.unity", true)
            };
            AssetDatabase.SaveAssets();
              Debug.Log("Carvino v0.01 prototype scene created.");
        }

          private static void BuildMainMenuScene(string folder)
        {
            var menu = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = new GameObject("Menu Camera").AddComponent<Camera>();
            camera.transform.position = new Vector3(8f, 4.8f, -11f);
            camera.transform.LookAt(new Vector3(0f, 1f, 1f));
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.05f);
            var light = new GameObject("Menu Key Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.color = new Color(1f, 0.72f, 0.57f);
            light.transform.rotation = Quaternion.Euler(48f, -26f, 0f);
            CreatePrimitive("Menu Floor", PrimitiveType.Cube, new Vector3(0f, -0.3f, 0f), new Vector3(28f, 0.6f, 24f), new Color(0.055f, 0.06f, 0.07f));
            CreatePrimitive("Menu Back Wall", PrimitiveType.Cube, new Vector3(0f, 4f, 8f), new Vector3(28f, 8f, 0.45f), new Color(0.1f, 0.06f, 0.05f));
            CreateTexturedPrimitive("Menu Night Strip Render", PrimitiveType.Cube, new Vector3(0f, 4.6f, 7.68f), new Vector3(25f, 14f, 0.12f), "Assets/Carvino/Art/Textures/carvino_dragway_night_02.png", Color.white, Vector2.one, true);
            CreateHatch("Menu Hatch", new Vector3(2.2f, 0.05f, 1.3f), new Color(0.78f, 0.07f, 0.04f));
            CreatePickup("Menu Pickup", new Vector3(-3.8f, 0.05f, 1.3f), new Color(0.08f, 0.18f, 0.42f));
            new GameObject("Main Menu Controller").AddComponent<MainMenuController>();
              EditorSceneManager.SaveScene(menu, folder + "/MainMenu.unity");
          }

          private static void BuildControlsScene(string folder)
          {
              var controls = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
              Camera camera = new GameObject("Controls Camera").AddComponent<Camera>();
              camera.transform.position = new Vector3(0f, 3f, -10f);
              camera.transform.LookAt(new Vector3(0f, 2.5f, 0f));
              camera.clearFlags = CameraClearFlags.SolidColor;
              camera.backgroundColor = new Color(.02f, .025f, .04f);
              Light light = new GameObject("Controls Light").AddComponent<Light>();
              light.type = LightType.Directional;
              light.intensity = .7f;
              light.color = new Color(.55f, .68f, 1f);
              light.transform.rotation = Quaternion.Euler(38f, -20f, 0f);
              CreatePrimitive("Controls Floor", PrimitiveType.Cube, new Vector3(0f, -.25f, 0f), new Vector3(26f, .5f, 18f), new Color(.05f, .06f, .08f));
              CreateTexturedPrimitive("Controls Track Render", PrimitiveType.Cube, new Vector3(0f, 4.7f, 7.7f), new Vector3(25f, 14f, .12f), "Assets/Carvino/Art/Textures/carvino_dragway_night_02.png", Color.white, Vector2.one, true);
              new GameObject("Controls Controller").AddComponent<ControlsController>();
              EditorSceneManager.SaveScene(controls, folder + "/Controls.unity");
          }

          private static void BuildSettingsScene(string folder)
          {
              var settings = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
              Camera camera = new GameObject("Settings Camera").AddComponent<Camera>();
              camera.transform.position = new Vector3(0f, 3f, -10f);
              camera.transform.LookAt(new Vector3(0f, 2.5f, 0f));
              camera.clearFlags = CameraClearFlags.SolidColor;
              camera.backgroundColor = new Color(.015f, .02f, .032f);
              Light light = new GameObject("Settings Light").AddComponent<Light>();
              light.type = LightType.Directional;
              light.intensity = .72f;
              light.color = new Color(.65f, .32f, .24f);
              light.transform.rotation = Quaternion.Euler(38f, -20f, 0f);
              CreatePrimitive("Settings Floor", PrimitiveType.Cube, new Vector3(0f, -.25f, 0f), new Vector3(26f, .5f, 18f), new Color(.045f, .05f, .065f));
              CreateTexturedPrimitive("Settings Garage Render", PrimitiveType.Cube, new Vector3(0f, 4.7f, 7.7f), new Vector3(25f, 14f, .12f), "Assets/Carvino/Art/Textures/carvino_garage_moonlit_02.png", Color.white, Vector2.one, true);
              new GameObject("Settings Controller").AddComponent<SettingsController>();
              EditorSceneManager.SaveScene(settings, folder + "/Settings.unity");
          }

          private static void BuildCareerScene(string folder)
          {
              var career = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
              Camera camera = new GameObject("Career Camera").AddComponent<Camera>();
              camera.transform.position = new Vector3(8f, 4.5f, -11f);
              camera.transform.LookAt(new Vector3(0f, 1f, 1f));
              camera.clearFlags = CameraClearFlags.SolidColor;
              camera.backgroundColor = new Color(.022f, .032f, .05f);
              Light light = new GameObject("Career Light").AddComponent<Light>();
              light.type = LightType.Directional;
              light.intensity = 1.0f;
              light.color = new Color(1f, .52f, .25f);
              light.transform.rotation = Quaternion.Euler(42f, -30f, 0f);
              CreatePrimitive("Career Asphalt", PrimitiveType.Cube, new Vector3(0f, -.3f, 0f), new Vector3(28f, .6f, 24f), new Color(.05f, .055f, .06f));
              CreateTexturedPrimitive("Career Dragway Render", PrimitiveType.Cube, new Vector3(0f, 4.6f, 7.68f), new Vector3(25f, 14f, .12f), "Assets/Carvino/Art/Textures/carvino_dragway_night_02.png", Color.white, Vector2.one, true);
              CreateHatch("Career Hatch", new Vector3(2.2f, .05f, 1.3f), new Color(.78f, .07f, .04f));
              new GameObject("Career Controller").AddComponent<CareerController>();
              EditorSceneManager.SaveScene(career, folder + "/Career.unity");
          }

          private static void BuildProfileScene(string folder)
          {
              var profile = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
              Camera camera = new GameObject("Profile Camera").AddComponent<Camera>();
              camera.transform.position = new Vector3(0f, 3f, -10f);
              camera.transform.LookAt(new Vector3(0f, 2.5f, 0f));
              camera.clearFlags = CameraClearFlags.SolidColor;
              camera.backgroundColor = new Color(.015f, .02f, .032f);
              Light light = new GameObject("Profile Light").AddComponent<Light>();
              light.type = LightType.Directional;
              light.intensity = .7f;
              light.color = new Color(.35f, .55f, 1f);
              light.transform.rotation = Quaternion.Euler(38f, -20f, 0f);
              CreatePrimitive("Profile Floor", PrimitiveType.Cube, new Vector3(0f, -.25f, 0f), new Vector3(26f, .5f, 18f), new Color(.045f, .05f, .065f));
              CreateTexturedPrimitive("Profile Garage Render", PrimitiveType.Cube, new Vector3(0f, 4.7f, 7.7f), new Vector3(25f, 14f, .12f), "Assets/Carvino/Art/Textures/carvino_garage_moonlit_02.png", Color.white, Vector2.one, true);
              new GameObject("Profile Controller").AddComponent<ProfileController>();
              EditorSceneManager.SaveScene(profile, folder + "/Profile.unity");
          }

        private static void BuildRaceDayScene(string folder)
        {
            var raceDay = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = new GameObject("Race Day Camera").AddComponent<Camera>();
            camera.transform.position = new Vector3(8f, 4.5f, -11f);
            camera.transform.LookAt(new Vector3(0f, 1f, 1f));
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.04f, 0.06f);
            var light = new GameObject("Race Day Sunset").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.color = new Color(1f, 0.65f, 0.44f);
            light.transform.rotation = Quaternion.Euler(42f, -30f, 0f);
            CreatePrimitive("Race Day Asphalt", PrimitiveType.Cube, new Vector3(0f, -0.3f, 0f), new Vector3(28f, 0.6f, 24f), new Color(0.07f, 0.075f, 0.08f));
            CreatePrimitive("Race Day Wall", PrimitiveType.Cube, new Vector3(0f, 4f, 8f), new Vector3(28f, 8f, 0.45f), new Color(0.10f, 0.06f, 0.05f));
              CreateTexturedPrimitive("Race Day Night Strip Render", PrimitiveType.Cube, new Vector3(0f, 4.6f, 7.68f), new Vector3(25f, 14f, 0.12f), "Assets/Carvino/Art/Textures/carvino_dragway_night_02.png", Color.white, Vector2.one, true);
            CreateHatch("Race Day Hatch", new Vector3(2.2f, 0.05f, 1.3f), new Color(0.72f, 0.08f, 0.04f));
            CreatePickup("Race Day Pickup", new Vector3(-3.6f, 0.05f, 1.3f), new Color(0.06f, 0.16f, 0.35f));
            new GameObject("Race Day Controller").AddComponent<RaceDayController>();
            EditorSceneManager.SaveScene(raceDay, folder + "/RaceDay.unity");
        }

        private static void BuildDynoScene(string folder)
        {
            var dyno = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = new GameObject("Dyno Camera").AddComponent<Camera>();
            camera.transform.position = new Vector3(8f, 5f, -10f);
            camera.transform.LookAt(new Vector3(0f, 0.9f, 0f));
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.07f, 0.09f);
            var light = new GameObject("Dyno Lights").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            light.transform.rotation = Quaternion.Euler(46f, -25f, 0f);
            CreatePrimitive("Dyno Floor", PrimitiveType.Cube, new Vector3(0f, -0.25f, 0f), new Vector3(24f, 0.5f, 20f), new Color(0.07f, 0.07f, 0.08f));
            CreatePrimitive("Dyno Rollers", PrimitiveType.Cylinder, new Vector3(0f, 0.15f, 1.2f), new Vector3(1.2f, 0.2f, 1.2f), new Color(0.32f, 0.32f, 0.34f));
            CreateVehicle("Dyno Display Car", new Vector3(0f, 0.05f, 0f), new Color(0.12f, 0.26f, 0.74f));
            new GameObject("Dyno Controller").AddComponent<DynoController>();
            EditorSceneManager.SaveScene(dyno, folder + "/Dyno.unity");
        }

        private static void BuildGarageScene(string folder)
        {
            var garage = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = new GameObject("Garage Camera").AddComponent<Camera>();
            camera.transform.position = new Vector3(8f, 5f, -10f);
            camera.transform.LookAt(new Vector3(0f, 0.9f, 0f));
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.11f);
            var light = new GameObject("Garage Lights").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(46f, -25f, 0f);
            CreatePrimitive("Garage Floor", PrimitiveType.Cube, new Vector3(0f, -0.25f, 0f), new Vector3(24f, 0.5f, 20f), new Color(0.12f, 0.12f, 0.13f));
            CreateTexturedPrimitive("Back Wall", PrimitiveType.Cube, new Vector3(0f, 4f, 8f), new Vector3(24f, 8f, 0.5f), "Assets/Carvino/Art/Textures/garage_wall_concrete_01.png", new Color(0.5f, 0.5f, 0.5f), new Vector2(5f, 2f));
            CreateTexturedPrimitive("Garage Environment Render", PrimitiveType.Cube, new Vector3(0f, 4.8f, 7.67f), new Vector3(18f, 10f, 0.12f), "Assets/Carvino/Art/Textures/carvino_garage_moonlit_02.png", Color.white, Vector2.one, true);
            GameObject garageHatch = CreateHatch("Garage Hatch", new Vector3(0f, 0.05f, 0f), new Color(0.75f, 0.12f, 0.07f));
            GameObject garagePickup = CreatePickup("Garage Pickup", new Vector3(0f, 0.05f, 0f), new Color(0.72f, 0.12f, 0.06f));
            GameObject engineBayDisplay = new GameObject("Engine Inspection Bay");
            GameObject engineStand = CreatePrimitive("Engine Stand", PrimitiveType.Cylinder, new Vector3(5f, 0.6f, 1.5f), new Vector3(0.6f, 1.2f, 0.6f), new Color(0.16f, 0.17f, 0.18f));
            engineStand.transform.SetParent(engineBayDisplay.transform, true);
            GameObject engineBlock = CreatePrimitive("Swap Engine", PrimitiveType.Cube, new Vector3(5f, 1.5f, 1.5f), new Vector3(1.6f, 0.7f, 0.8f), new Color(0.3f, 0.38f, 0.42f));
            engineBlock.transform.SetParent(engineBayDisplay.transform, true);
            GameObject intake = CreatePrimitive("Performance Intake", PrimitiveType.Cylinder, new Vector3(4.1f, 2.15f, 1.45f), new Vector3(0.32f, 0.7f, 0.32f), new Color(0.07f, 0.12f, 0.16f));
            intake.transform.SetParent(engineBayDisplay.transform, true);
            intake.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            GameObject exhaust = CreatePrimitive("Performance Exhaust", PrimitiveType.Cylinder, new Vector3(5.9f, 1.35f, 1.45f), new Vector3(0.17f, 0.75f, 0.17f), new Color(0.42f, 0.43f, 0.44f));
            exhaust.transform.SetParent(engineBayDisplay.transform, true);
            exhaust.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            GameObject ecu = CreatePrimitive("Tuned ECU", PrimitiveType.Cube, new Vector3(5f, 2.25f, 1.5f), new Vector3(0.62f, 0.25f, 0.4f), new Color(0.07f, 0.32f, 0.2f));
            ecu.transform.SetParent(engineBayDisplay.transform, true);
            GameObject slick = CreatePrimitive("Drag Slick Display", PrimitiveType.Cylinder, new Vector3(6.5f, 0.7f, 1.5f), new Vector3(0.72f, 0.38f, 0.72f), new Color(0.025f, 0.025f, 0.03f));
            slick.transform.SetParent(engineBayDisplay.transform, true);
            slick.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            GameObject weight = CreatePrimitive("Weight Reduction Parts", PrimitiveType.Cube, new Vector3(3.7f, 0.6f, 1.5f), new Vector3(0.55f, 0.45f, 0.8f), new Color(0.65f, 0.5f, 0.16f));
            weight.transform.SetParent(engineBayDisplay.transform, true);
            GameObject turbo = CreatePrimitive("Street Turbo", PrimitiveType.Sphere, new Vector3(6.55f, 1.58f, 1.5f), new Vector3(0.82f, 0.82f, 0.42f), new Color(0.28f, 0.3f, 0.32f));
            turbo.transform.SetParent(engineBayDisplay.transform, true);
            GameObject compressorInlet = CreatePrimitive("Street Turbo Inlet", PrimitiveType.Cylinder, new Vector3(6.55f, 1.58f, 1.22f), new Vector3(0.28f, 0.16f, 0.28f), new Color(0.08f, 0.09f, 0.1f));
            compressorInlet.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            compressorInlet.transform.SetParent(turbo.transform, true);
            GarageController garageController = new GameObject("Garage Controller").AddComponent<GarageController>();
            GarageInspectionController inspectionController = new GameObject("Garage Inspection Controller").AddComponent<GarageInspectionController>();
            var inspectionData = new SerializedObject(inspectionController);
            inspectionData.FindProperty("hatch").objectReferenceValue = garageHatch.transform;
            inspectionData.FindProperty("pickup").objectReferenceValue = garagePickup.transform;
            inspectionData.FindProperty("engineBayDisplay").objectReferenceValue = engineBayDisplay;
            inspectionData.ApplyModifiedPropertiesWithoutUndo();
            var garageData = new SerializedObject(garageController);
            garageData.FindProperty("hatchDisplay").objectReferenceValue = garageHatch.transform;
            garageData.FindProperty("pickupDisplay").objectReferenceValue = garagePickup.transform;
            garageData.FindProperty("engineBlock").objectReferenceValue = engineBlock.GetComponent<Renderer>();
            garageData.FindProperty("intakeVisual").objectReferenceValue = intake;
            garageData.FindProperty("exhaustVisual").objectReferenceValue = exhaust;
            garageData.FindProperty("ecuVisual").objectReferenceValue = ecu;
            garageData.FindProperty("slickVisual").objectReferenceValue = slick;
            garageData.FindProperty("weightReductionVisual").objectReferenceValue = weight;
            garageData.FindProperty("turboVisual").objectReferenceValue = turbo;
            garageData.FindProperty("inspectionController").objectReferenceValue = inspectionController;
            garageData.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(garage, folder + "/Garage.unity");
        }

          private static GameObject CreatePrimitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            var material = new Material(Shader.Find("Standard")) { color = color };
            go.GetComponent<Renderer>().sharedMaterial = material;
              return go;
          }

          private static BurnoutVisualEffects CreateBurnoutSmoke(string name, Transform parent, Vector3 localPosition)
          {
              GameObject smoke = new GameObject(name);
              smoke.transform.SetParent(parent, false);
              smoke.transform.localPosition = localPosition;
              smoke.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
              ParticleSystem particles = smoke.AddComponent<ParticleSystem>();
              ParticleSystem.MainModule main = particles.main;
              main.loop = true;
              main.startLifetime = new ParticleSystem.MinMaxCurve(.75f, 1.55f);
              main.startSpeed = new ParticleSystem.MinMaxCurve(.8f, 2.3f);
              main.startSize = new ParticleSystem.MinMaxCurve(.45f, 1.1f);
              main.startColor = new Color(.72f, .76f, .8f, .36f);
              main.simulationSpace = ParticleSystemSimulationSpace.World;
              ParticleSystem.EmissionModule emission = particles.emission;
              emission.rateOverTime = 0f;
              ParticleSystem.ShapeModule shape = particles.shape;
              shape.shapeType = ParticleSystemShapeType.Cone;
              shape.angle = 26f;
              shape.radius = .48f;
              ParticleSystem.ColorOverLifetimeModule fade = particles.colorOverLifetime;
              fade.enabled = true;
              Gradient gradient = new Gradient();
              gradient.SetKeys(new[] { new GradientColorKey(new Color(.65f, .7f, .75f), 0f), new GradientColorKey(new Color(.32f, .36f, .42f), 1f) }, new[] { new GradientAlphaKey(.32f, 0f), new GradientAlphaKey(0f, 1f) });
              fade.color = gradient;
              ParticleSystemRenderer renderer = smoke.GetComponent<ParticleSystemRenderer>();
              renderer.renderMode = ParticleSystemRenderMode.Billboard;
              return smoke.AddComponent<BurnoutVisualEffects>();
          }

          private static GameObject CreateExhaustFlash(string name, Transform parent, Vector3 localPosition)
          {
              GameObject flash = CreatePrimitive(name, PrimitiveType.Sphere, parent.position + localPosition, new Vector3(.34f, .22f, .58f), new Color(1f, .22f, .02f));
              flash.transform.SetParent(parent, true);
              Material material = flash.GetComponent<Renderer>().sharedMaterial;
              material.EnableKeyword("_EMISSION");
              material.SetColor("_EmissionColor", new Color(1f, .18f, .015f) * 4f);
              flash.SetActive(false);
              return flash;
          }

        private static GameObject CreateTexturedPrimitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, string texturePath, Color tint, Vector2 tiling, bool flipVertical = false)
        {
            GameObject go = CreatePrimitive(name, type, position, scale, tint);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null) return go;

            Material material = go.GetComponent<Renderer>().sharedMaterial;
            material.mainTexture = texture;
            material.mainTextureScale = new Vector2(tiling.x, flipVertical ? -tiling.y : tiling.y);
            material.mainTextureOffset = flipVertical ? new Vector2(0f, 1f) : Vector2.zero;
            return go;
        }

        private static GameObject CreateTrapezoid(string name, Vector3 position, float width, float bottomLength, float topLength, float height, Color color)
        {
            float halfWidth = width * 0.5f;
            float halfBottom = bottomLength * 0.5f;
            float halfTop = topLength * 0.5f;
            Mesh mesh = new Mesh { name = name + " Mesh" };
            mesh.vertices = new[]
            {
                new Vector3(-halfWidth, 0f, -halfBottom), new Vector3(halfWidth, 0f, -halfBottom), new Vector3(halfWidth, 0f, halfBottom), new Vector3(-halfWidth, 0f, halfBottom),
                new Vector3(-halfWidth, height, -halfTop), new Vector3(halfWidth, height, -halfTop), new Vector3(halfWidth, height, halfTop), new Vector3(-halfWidth, height, halfTop)
            };
            mesh.triangles = new[]
            {
                0, 2, 1, 0, 3, 2,
                4, 5, 6, 4, 6, 7,
                0, 1, 5, 0, 5, 4,
                1, 2, 6, 1, 6, 5,
                2, 3, 7, 2, 7, 6,
                3, 0, 4, 3, 4, 7
            };
            mesh.RecalculateNormals();
            GameObject go = new GameObject(name);
            go.transform.position = position;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard")) { color = color };
            return go;
        }

        /// <summary>Builds an original low-poly vehicle shell from a side silhouette.
        /// Profile points are clockwise in local z/y space; the extrusion is vehicle width.</summary>
        private static GameObject CreateProfiledBody(string name, Vector3 position, float width, Vector2[] profile, Color color)
        {
            int count = profile.Length;
            float halfWidth = width * 0.5f;
            Vector3[] vertices = new Vector3[count * 2];
            for (int i = 0; i < count; i++)
            {
                vertices[i] = new Vector3(-halfWidth, profile[i].y, profile[i].x);
                vertices[i + count] = new Vector3(halfWidth, profile[i].y, profile[i].x);
            }

            var triangles = new System.Collections.Generic.List<int>();
            // Side faces.
            for (int i = 1; i < count - 1; i++)
            {
                triangles.Add(0); triangles.Add(i + 1); triangles.Add(i);
                triangles.Add(count); triangles.Add(count + i); triangles.Add(count + i + 1);
            }
            // Extruded perimeter.
            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                triangles.Add(i); triangles.Add(next); triangles.Add(count + next);
                triangles.Add(i); triangles.Add(count + next); triangles.Add(count + i);
            }

            Mesh mesh = new Mesh { name = name + " Mesh" };
            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            GameObject go = new GameObject(name);
            go.transform.position = position;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard")) { color = color };
            return go;
        }

        private static GameObject CreateVehicle(string name, Vector3 position, Color paint)
        {
            return CreateHatch(name, position, paint);
        }

        private static GameObject CreateHatch(string name, Vector3 position, Color paint)
        {
            GameObject imported = CreateImportedVehicle("Assets/Carvino/Art/Models/CarvinoHatch_93.fbx", name, position);
            if (imported != null) return imported;
            GameObject root = new GameObject(name);
            root.transform.position = position;
            GameObject body = CreateProfiledBody("1993 Hatch Body", position, 2.02f, new[]
            {
                new Vector2(-2.12f, 0.38f), new Vector2(2.14f, 0.38f), new Vector2(2.14f, 0.98f), new Vector2(1.86f, 1.20f),
                new Vector2(0.66f, 1.28f), new Vector2(0.26f, 1.92f), new Vector2(-0.92f, 1.88f), new Vector2(-1.68f, 1.18f), new Vector2(-2.12f, 0.94f)
            }, paint);
            body.transform.SetParent(root.transform, true);
            GameObject hood = CreatePrimitive("Hatch Hood", PrimitiveType.Cube, position + new Vector3(0f, 1.18f, 1.08f), new Vector3(1.84f, 0.16f, 1.35f), paint * 0.82f);
            hood.transform.SetParent(root.transform, true);
            GameObject cabin = CreateProfiledBody("1993 Hatch Glass", position, 2.045f, new[]
            {
                new Vector2(-1.55f, 1.12f), new Vector2(0.21f, 1.13f), new Vector2(0.38f, 1.77f), new Vector2(-0.84f, 1.74f)
            }, new Color(0.075f, 0.13f, 0.17f));
            cabin.transform.SetParent(root.transform, true);
              GameObject bumper = CreatePrimitive("Hatch Front Bumper", PrimitiveType.Cube, position + new Vector3(0f, 0.55f, 2.08f), new Vector3(2.04f, 0.3f, 0.16f), new Color(0.08f, 0.08f, 0.09f));
              bumper.transform.SetParent(root.transform, true);
              GameObject rearBumper = CreatePrimitive("Hatch Rear Bumper", PrimitiveType.Cube, position + new Vector3(0f, 0.55f, -2.08f), new Vector3(2.04f, 0.28f, 0.16f), new Color(0.07f, 0.07f, 0.08f));
              rearBumper.transform.SetParent(root.transform, true);
              GameObject hatchSpoiler = CreatePrimitive("Hatch Rear Lip", PrimitiveType.Cube, position + new Vector3(0f, 1.52f, -1.82f), new Vector3(2.02f, 0.12f, 0.3f), paint * 0.72f);
              hatchSpoiler.transform.SetParent(root.transform, true);
              foreach (float side in new[] { -1.035f, 1.035f })
              {
                  GameObject sideMolding = CreatePrimitive("Hatch Side Molding", PrimitiveType.Cube, position + new Vector3(side, 0.82f, -0.03f), new Vector3(0.035f, 0.09f, 3.42f), new Color(0.06f, 0.065f, 0.07f));
                  sideMolding.transform.SetParent(root.transform, true);
                  GameObject doorSeam = CreatePrimitive("Hatch Door Seam", PrimitiveType.Cube, position + new Vector3(side, 1.13f, 0.14f), new Vector3(0.025f, 0.72f, 0.03f), new Color(0.05f, 0.055f, 0.06f));
                  doorSeam.transform.SetParent(root.transform, true);
                  GameObject doorHandle = CreatePrimitive("Hatch Door Handle", PrimitiveType.Cube, position + new Vector3(side, 1.20f, 0.62f), new Vector3(0.055f, 0.07f, 0.25f), new Color(0.12f, 0.13f, 0.14f));
                  doorHandle.transform.SetParent(root.transform, true);
              }
            GameObject grille = CreatePrimitive("Hatch Grille", PrimitiveType.Cube, position + new Vector3(0f, 0.78f, 2.065f), new Vector3(1.12f, 0.25f, 0.05f), new Color(0.025f, 0.03f, 0.035f));
            grille.transform.SetParent(root.transform, true);
            foreach (float side in new[] { -0.63f, 0.63f })
            {
                GameObject headlight = CreatePrimitive("Hatch Headlight", PrimitiveType.Cube, position + new Vector3(side, 0.91f, 2.07f), new Vector3(0.45f, 0.2f, 0.06f), new Color(0.82f, 0.9f, 0.94f));
                headlight.transform.SetParent(root.transform, true);
                GameObject taillight = CreatePrimitive("Hatch Taillight", PrimitiveType.Cube, position + new Vector3(side, 0.86f, -2.07f), new Vector3(0.42f, 0.18f, 0.06f), new Color(0.62f, 0.025f, 0.02f));
                taillight.transform.SetParent(root.transform, true);
            }
            Vector3[] wheelPositions =
            {
                new Vector3(-1.08f, 0.43f, -1.35f), new Vector3(1.08f, 0.43f, -1.35f),
                new Vector3(-1.08f, 0.43f, 1.35f), new Vector3(1.08f, 0.43f, 1.35f)
            };
            foreach (Vector3 wheelPosition in wheelPositions)
            {
                GameObject wheel = CreatePrimitive("Wheel", PrimitiveType.Cylinder, position + wheelPosition, new Vector3(0.62f, 0.28f, 0.62f), new Color(0.035f, 0.035f, 0.04f));
                wheel.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                wheel.transform.SetParent(root.transform, true);
                GameObject rim = CreatePrimitive("Hatch Rim", PrimitiveType.Cylinder, position + wheelPosition + new Vector3(wheelPosition.x < 0f ? -0.3f : 0.3f, 0f, 0f), new Vector3(0.34f, 0.05f, 0.34f), new Color(0.68f, 0.7f, 0.72f));
                rim.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                rim.transform.SetParent(root.transform, true);
            }
            return root;
        }

        private static GameObject CreatePickup(string name, Vector3 position, Color paint)
        {
            GameObject imported = CreateImportedVehicle("Assets/Carvino/Art/Models/CarvinoPickup_91.fbx", name, position);
            if (imported != null) return imported;
            GameObject root = new GameObject(name);
            root.transform.position = position;
            GameObject frame = CreateProfiledBody("1991 Pickup Body", position, 2.2f, new[]
            {
                new Vector2(-2.50f, 0.38f), new Vector2(2.46f, 0.38f), new Vector2(2.46f, 1.04f), new Vector2(2.16f, 1.24f),
                new Vector2(0.74f, 1.28f), new Vector2(0.34f, 1.96f), new Vector2(-0.90f, 1.96f), new Vector2(-1.23f, 1.30f), new Vector2(-2.42f, 1.30f), new Vector2(-2.50f, 0.90f)
            }, paint);
            frame.transform.SetParent(root.transform, true);
            GameObject hood = CreatePrimitive("Pickup Hood", PrimitiveType.Cube, position + new Vector3(0f, 1.12f, 1.65f), new Vector3(2.02f, 0.18f, 1.15f), paint * 0.82f);
            hood.transform.SetParent(root.transform, true);
            GameObject cab = CreateProfiledBody("1991 Pickup Glass", position, 2.225f, new[]
            {
                new Vector2(-1.06f, 1.24f), new Vector2(0.28f, 1.24f), new Vector2(0.45f, 1.80f), new Vector2(-0.78f, 1.80f)
            }, new Color(0.075f, 0.13f, 0.17f));
            cab.transform.SetParent(root.transform, true);
              GameObject bed = CreatePrimitive("Pickup Bed", PrimitiveType.Cube, position + new Vector3(0f, 1.0f, -1.12f), new Vector3(2.05f, 0.42f, 1.7f), paint * 0.86f);
              bed.transform.SetParent(root.transform, true);
              GameObject bedLiner = CreatePrimitive("Pickup Bed Liner", PrimitiveType.Cube, position + new Vector3(0f, 1.23f, -1.12f), new Vector3(1.82f, 0.06f, 1.48f), new Color(0.055f, 0.058f, 0.062f));
              bedLiner.transform.SetParent(root.transform, true);
              foreach (float side in new[] { -1.08f, 1.08f })
              {
                  GameObject bedRail = CreatePrimitive("Pickup Bed Rail", PrimitiveType.Cube, position + new Vector3(side, 1.27f, -1.12f), new Vector3(0.08f, 0.075f, 1.86f), new Color(0.16f, 0.17f, 0.18f));
                  bedRail.transform.SetParent(root.transform, true);
                  GameObject sideMolding = CreatePrimitive("Pickup Side Molding", PrimitiveType.Cube, position + new Vector3(side, 0.83f, 0.02f), new Vector3(0.035f, 0.09f, 4.15f), new Color(0.075f, 0.078f, 0.082f));
                  sideMolding.transform.SetParent(root.transform, true);
              }
              GameObject tailgate = CreatePrimitive("Pickup Tailgate", PrimitiveType.Cube, position + new Vector3(0f, 0.8f, -2.37f), new Vector3(2.16f, 0.55f, 0.14f), paint);
              tailgate.transform.SetParent(root.transform, true);
              GameObject tailgateInset = CreatePrimitive("Pickup Tailgate Inset", PrimitiveType.Cube, position + new Vector3(0f, 0.84f, -2.45f), new Vector3(1.48f, 0.24f, 0.025f), paint * 0.64f);
              tailgateInset.transform.SetParent(root.transform, true);
              GameObject tailgateHandle = CreatePrimitive("Pickup Tailgate Handle", PrimitiveType.Cube, position + new Vector3(0f, 1.03f, -2.46f), new Vector3(0.26f, 0.075f, 0.03f), new Color(0.08f, 0.085f, 0.09f));
              tailgateHandle.transform.SetParent(root.transform, true);
              GameObject rearBumper = CreatePrimitive("Pickup Rear Bumper", PrimitiveType.Cube, position + new Vector3(0f, 0.5f, -2.48f), new Vector3(2.24f, 0.18f, 0.16f), new Color(0.38f, 0.39f, 0.4f));
              rearBumper.transform.SetParent(root.transform, true);
              GameObject rearWindowDivider = CreatePrimitive("Pickup Rear Window Divider", PrimitiveType.Cube, position + new Vector3(0f, 1.58f, -0.96f), new Vector3(0.09f, 0.58f, 0.035f), new Color(0.045f, 0.05f, 0.055f));
              rearWindowDivider.transform.SetParent(root.transform, true);
              GameObject frontBumper = CreatePrimitive("Pickup Front Bumper", PrimitiveType.Cube, position + new Vector3(0f, 0.54f, 2.48f), new Vector3(2.22f, 0.2f, 0.14f), new Color(0.28f, 0.29f, 0.3f));
              frontBumper.transform.SetParent(root.transform, true);
            GameObject grille = CreatePrimitive("Pickup Grille", PrimitiveType.Cube, position + new Vector3(0f, 0.8f, 2.43f), new Vector3(1.25f, 0.3f, 0.06f), new Color(0.025f, 0.03f, 0.035f));
            grille.transform.SetParent(root.transform, true);
            foreach (float side in new[] { -0.68f, 0.68f })
            {
                GameObject headlight = CreatePrimitive("Pickup Headlight", PrimitiveType.Cube, position + new Vector3(side, 0.94f, 2.42f), new Vector3(0.4f, 0.22f, 0.06f), new Color(0.82f, 0.9f, 0.94f));
                headlight.transform.SetParent(root.transform, true);
                GameObject taillight = CreatePrimitive("Pickup Taillight", PrimitiveType.Cube, position + new Vector3(side, 0.88f, -2.43f), new Vector3(0.36f, 0.22f, 0.06f), new Color(0.62f, 0.025f, 0.02f));
                taillight.transform.SetParent(root.transform, true);
            }
            foreach (Vector3 wheelPosition in new[]
            {
                new Vector3(-1.18f, 0.4f, -1.55f), new Vector3(1.18f, 0.4f, -1.55f),
                new Vector3(-1.18f, 0.4f, 1.52f), new Vector3(1.18f, 0.4f, 1.52f)
            })
            {
                GameObject wheel = CreatePrimitive("Pickup Wheel", PrimitiveType.Cylinder, position + wheelPosition, new Vector3(0.72f, 0.34f, 0.72f), new Color(0.025f, 0.025f, 0.03f));
                wheel.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                wheel.transform.SetParent(root.transform, true);
                GameObject rim = CreatePrimitive("Pickup Rim", PrimitiveType.Cylinder, position + wheelPosition + new Vector3(wheelPosition.x < 0f ? -0.36f : 0.36f, 0f, 0f), new Vector3(0.4f, 0.05f, 0.4f), new Color(0.68f, 0.7f, 0.72f));
                rim.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                rim.transform.SetParent(root.transform, true);
            }
            return root;
        }

        private static GameObject CreateImportedVehicle(string assetPath, string name, Vector3 position)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null) return null;
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            return instance;
        }

          private static void CreateStartingLineProps()
        {
            Color coneOrange = new Color(0.9f, 0.24f, 0.045f);
            CreateCone("Cone Left", new Vector3(-6.9f, 0.22f, 3.2f), coneOrange);
            CreateCone("Cone Right", new Vector3(6.9f, 0.22f, 3.2f), coneOrange);
            CreateCone("Cone Center A", new Vector3(-0.52f, 0.22f, 8.2f), coneOrange);
            CreateCone("Cone Center B", new Vector3(0.52f, 0.22f, 8.2f), coneOrange);
            CreatePrimitive("Track Barrel", PrimitiveType.Cylinder, new Vector3(0f, 0.7f, 8f), new Vector3(0.52f, 0.7f, 0.52f), new Color(0.74f, 0.55f, 0.05f));
            CreatePrimitive("Track Barrel Lid", PrimitiveType.Cylinder, new Vector3(0f, 1.42f, 8f), new Vector3(0.54f, 0.05f, 0.54f), new Color(0.15f, 0.16f, 0.16f));
            CreatePrimitive("Starting Equipment Cart", PrimitiveType.Cube, new Vector3(0f, 0.42f, 10.2f), new Vector3(1.15f, 0.75f, 0.82f), new Color(0.16f, 0.17f, 0.18f));
            CreatePrimitive("Cart Screen", PrimitiveType.Cube, new Vector3(0f, 1.12f, 10.2f), new Vector3(0.78f, 0.55f, 0.12f), new Color(0.06f, 0.18f, 0.13f));
            CreatePrimitive("Start Gantry Left", PrimitiveType.Cylinder, new Vector3(-1.18f, 4.1f, 10.2f), new Vector3(0.07f, 3.2f, 0.07f), new Color(0.18f, 0.19f, 0.19f));
            CreatePrimitive("Start Gantry Right", PrimitiveType.Cylinder, new Vector3(1.18f, 4.1f, 10.2f), new Vector3(0.07f, 3.2f, 0.07f), new Color(0.18f, 0.19f, 0.19f));
            CreatePrimitive("Start Gantry Beam", PrimitiveType.Cube, new Vector3(0f, 7.2f, 10.2f), new Vector3(2.55f, 0.12f, 0.12f), new Color(0.18f, 0.19f, 0.19f));
            CreatePrimitive("Gantry Cable Left", PrimitiveType.Cylinder, new Vector3(-0.95f, 5.95f, 10.2f), new Vector3(0.025f, 1.15f, 0.025f), new Color(0.08f, 0.08f, 0.08f));
            CreatePrimitive("Gantry Cable Right", PrimitiveType.Cylinder, new Vector3(0.95f, 5.95f, 10.2f), new Vector3(0.025f, 1.15f, 0.025f), new Color(0.08f, 0.08f, 0.08f));
            CreatePrimitive("Water Box", PrimitiveType.Cube, new Vector3(-3.8f, 0.012f, -5.5f), new Vector3(6.8f, 0.025f, 4.2f), new Color(0.09f, 0.11f, 0.12f));
            CreatePrimitive("Left Rubber Lane", PrimitiveType.Cube, new Vector3(-3.8f, 0.018f, 28f), new Vector3(2.9f, 0.01f, 52f), new Color(0.025f, 0.027f, 0.028f));
            CreatePrimitive("Right Rubber Lane", PrimitiveType.Cube, new Vector3(3.8f, 0.018f, 28f), new Vector3(2.9f, 0.01f, 52f), new Color(0.025f, 0.027f, 0.028f));
            CreatePrimitive("Timing Tower", PrimitiveType.Cube, new Vector3(11f, 3f, 16f), new Vector3(3.2f, 6f, 3f), new Color(0.28f, 0.29f, 0.28f));
              CreatePrimitive("Timing Window", PrimitiveType.Cube, new Vector3(11f, 4.2f, 14.46f), new Vector3(2.45f, 1.1f, 0.08f), new Color(0.05f, 0.14f, 0.19f));
              CreatePrimitive("Water Box Glow", PrimitiveType.Cube, new Vector3(-3.8f, .025f, -5.5f), new Vector3(6.6f, .01f, 4f), new Color(.06f, .16f, .21f));
              CreatePrimitive("Left Staging Rubber", PrimitiveType.Cube, new Vector3(-3.8f, .024f, 3.5f), new Vector3(3.8f, .012f, 16f), new Color(.012f, .014f, .018f));
              CreatePrimitive("Right Staging Rubber", PrimitiveType.Cube, new Vector3(3.8f, .024f, 3.5f), new Vector3(3.8f, .012f, 16f), new Color(.012f, .014f, .018f));
          }

          private static void CreateBarrierReflectors(float distance)
          {
              Color reflector = new Color(1f, .5f, .06f);
              foreach (float side in new[] { -8.37f, 8.37f })
              {
                  GameObject marker = CreatePrimitive("Barrier Reflector", PrimitiveType.Cube, new Vector3(side, .86f, distance), new Vector3(.08f, .14f, .42f), reflector);
                  Material material = marker.GetComponent<Renderer>().sharedMaterial;
                  material.EnableKeyword("_EMISSION");
                  material.SetColor("_EmissionColor", reflector * 1.4f);
              }
          }

        private static void CreateCone(string name, Vector3 position, Color color)
        {
            GameObject basePart = CreatePrimitive(name + " Base", PrimitiveType.Cylinder, position, new Vector3(0.28f, 0.08f, 0.28f), color);
            GameObject body = CreatePrimitive(name + " Body", PrimitiveType.Cylinder, position + new Vector3(0f, 0.27f, 0f), new Vector3(0.16f, 0.3f, 0.16f), color);
            body.transform.SetParent(basePart.transform, true);
        }

        private static void CreateRuralTreeLine()
        {
            for (int index = 0; index < 16; index++)
            {
                float distance = 24f + index * 24f;
                CreateTrackTree("Left Track Tree " + index, new Vector3(-28f - (index % 3) * 4f, 0f, distance), 1f + (index % 4) * 0.12f);
                CreateTrackTree("Right Track Tree " + index, new Vector3(28f + (index % 3) * 4f, 0f, distance + 8f), 1.05f + (index % 5) * 0.1f);
            }
        }

        private static void CreateTrackTree(string name, Vector3 position, float scale)
        {
            GameObject trunk = CreatePrimitive(name + " Trunk", PrimitiveType.Cylinder, position + new Vector3(0f, 2.1f * scale, 0f), new Vector3(0.28f * scale, 2.1f * scale, 0.28f * scale), new Color(0.18f, 0.12f, 0.07f));
            GameObject crown = CreatePrimitive(name + " Crown", PrimitiveType.Sphere, position + new Vector3(0f, 5.4f * scale, 0f), new Vector3(3.4f * scale, 3.5f * scale, 3.4f * scale), new Color(0.09f, 0.25f, 0.08f));
            crown.transform.SetParent(trunk.transform, true);
        }

        private static void CreateGrandstand(string name, Vector3 origin, float side)
        {
            GameObject root = new GameObject(name);
            for (int row = 0; row < 6; row++)
            {
                float height = 0.45f + row * 0.5f;
                float offset = row * 0.68f * side;
                GameObject seat = CreatePrimitive("Bleacher Row " + row, PrimitiveType.Cube, origin + new Vector3(offset, height, row * 2.1f), new Vector3(7f, 0.22f, 1.7f), new Color(0.35f, 0.38f, 0.4f));
                seat.transform.SetParent(root.transform, true);
            }
            GameObject topRail = CreatePrimitive("Grandstand Rail", PrimitiveType.Cube, origin + new Vector3(3.65f * side, 3.4f, 5.3f), new Vector3(0.1f, 0.65f, 13f), new Color(0.18f, 0.2f, 0.22f));
            topRail.transform.SetParent(root.transform, true);
        }

          private static void CreateLightPole(string name, Vector3 position)
          {
              GameObject pole = CreatePrimitive(name, PrimitiveType.Cylinder, position + new Vector3(0f, 6f, 0f), new Vector3(0.16f, 6f, 0.16f), new Color(0.2f, 0.22f, 0.24f));
              GameObject lamp = CreatePrimitive(name + " Lamp", PrimitiveType.Cube, position + new Vector3(0f, 11.8f, 0f), new Vector3(1.6f, 0.35f, 0.65f), new Color(0.92f, 0.9f, 0.72f));
              lamp.transform.SetParent(pole.transform, true);
              Material lampMaterial = lamp.GetComponent<Renderer>().sharedMaterial;
              lampMaterial.EnableKeyword("_EMISSION");
              lampMaterial.SetColor("_EmissionColor", new Color(1f, .63f, .24f) * 1.8f);
              Light flood = new GameObject(name + " Floodlight").AddComponent<Light>();
              flood.type = LightType.Spot;
              flood.color = new Color(1f, .69f, .42f);
              flood.intensity = 6.5f;
              flood.range = 40f;
              flood.spotAngle = 92f;
              flood.transform.position = position + new Vector3(0f, 11.5f, 0f);
              flood.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
          }
    }
}
#endif
