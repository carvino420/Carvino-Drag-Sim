#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Carvino.Editor
{
    /// <summary>
    /// PC Ultra+ import policy for the original 4K hero material library only.
    /// Lower-quality material libraries and legacy art are deliberately untouched.
    /// </summary>
    public sealed class HeroTextureImportRules : AssetPostprocessor
    {
        private const string TextureRoot = "Assets/Carvino/Art/Textures/Hero/carvino_hero_";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(TextureRoot, StringComparison.OrdinalIgnoreCase))
                return;

            var importer = (TextureImporter)assetImporter;
            var isNormal = assetPath.EndsWith("_normal.png", StringComparison.OrdinalIgnoreCase);
            var isMask = assetPath.EndsWith("_mask.png", StringComparison.OrdinalIgnoreCase);

            importer.textureType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !isNormal && !isMask;
            importer.alphaSource = isMask ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 16;
            importer.mipmapEnabled = true;
            importer.streamingMipmaps = true;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.crunchedCompression = false;

            var standalone = importer.GetPlatformTextureSettings("Standalone");
            standalone.overridden = true;
            standalone.maxTextureSize = 4096;
            standalone.format = TextureImporterFormat.BC7;
            standalone.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SetPlatformTextureSettings(standalone);
        }

        [MenuItem("Carvino/Art/Reimport 4K Hero Material Library")]
        public static void ReimportAll()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Carvino/Art/Textures/Hero" });
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.StartsWith(TextureRoot, StringComparison.OrdinalIgnoreCase))
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
