#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Carvino.Editor
{
    /// <summary>
    /// Applies consistent PC import settings to the self-authored Carvino PBR material library.
    /// It intentionally scopes itself to new carvino_pbr_ textures and never alters legacy art.
    /// </summary>
    public sealed class PbrTextureImportRules : AssetPostprocessor
    {
        private const string TextureRoot = "Assets/Carvino/Art/Textures/carvino_pbr_";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(TextureRoot, System.StringComparison.OrdinalIgnoreCase))
                return;

            var importer = (TextureImporter)assetImporter;
            var isNormal = assetPath.EndsWith("_normal.png", System.StringComparison.OrdinalIgnoreCase);
            var isMask = assetPath.EndsWith("_mask.png", System.StringComparison.OrdinalIgnoreCase);

            importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !isNormal && !isMask;
            importer.alphaSource = isMask ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 8;
            importer.mipmapEnabled = true;
            importer.streamingMipmaps = true;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.crunchedCompression = false;

            var standalone = importer.GetPlatformTextureSettings("Standalone");
            standalone.overridden = true;
            standalone.maxTextureSize = 2048;
            standalone.format = TextureImporterFormat.BC7;
            standalone.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SetPlatformTextureSettings(standalone);
        }

        [MenuItem("Carvino/Art/Reimport Original PBR Material Library")]
        public static void ReimportAll()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Carvino/Art/Textures" });
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.StartsWith(TextureRoot, System.StringComparison.OrdinalIgnoreCase))
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
        }
    }
}
#endif
