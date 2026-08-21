#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Carvino.Editor
{
    /// <summary>Corrects vertically mirrored image backdrops on cube faces without rebuilding scenes.</summary>
    public static class BackdropOrientationFixer
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/Carvino/Scenes/MainMenu.unity",
            "Assets/Carvino/Scenes/Controls.unity",
            "Assets/Carvino/Scenes/Settings.unity",
            "Assets/Carvino/Scenes/Career.unity",
            "Assets/Carvino/Scenes/Profile.unity",
            "Assets/Carvino/Scenes/RaceDay.unity",
            "Assets/Carvino/Scenes/Garage.unity"
        };

        [MenuItem("Carvino/Fix Interface Backdrop Orientation")]
        public static void Apply()
        {
            foreach (string path in ScenePaths)
            {
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                foreach (Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (renderer == null || renderer.sharedMaterial == null || !renderer.gameObject.name.Contains("Render")) continue;
                    Material material = new Material(renderer.sharedMaterial);
                    renderer.sharedMaterial = material;
                    Vector2 scale = material.mainTextureScale;
                    material.mainTextureScale = new Vector2(Mathf.Abs(scale.x), -Mathf.Abs(scale.y));
                    material.mainTextureOffset = new Vector2(0f, 1f);
                    EditorUtility.SetDirty(material);
                    EditorUtility.SetDirty(renderer);
                }
                EditorSceneManager.SaveOpenScenes();
            }
            AssetDatabase.SaveAssets();
            Debug.Log("Carvino interface backdrops corrected.");
        }
    }
}
#endif
