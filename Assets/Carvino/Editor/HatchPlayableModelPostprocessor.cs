#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Carvino.Editor
{
    /// <summary>
    /// Turns the original Carvino playable hatch FBX hierarchy into a model-prefab
    /// LODGroup without coupling the runtime vehicle controller to editor code.
    /// </summary>
    public sealed class HatchPlayableModelPostprocessor : AssetPostprocessor
    {
        public const string StagedPath = "Assets/Carvino/Art/Models/Playable/Hatch93/CarvinoHatch_93_Playable.fbx";
        public const string GameplayPath = "Assets/Carvino/Art/Models/CarvinoHatch_93.fbx";

        private void OnPostprocessModel(GameObject root)
        {
            if (!assetPath.Equals(StagedPath, StringComparison.OrdinalIgnoreCase) &&
                !assetPath.Equals(GameplayPath, StringComparison.OrdinalIgnoreCase))
                return;

            Transform lod0 = FindRecursive(root.transform, "LOD0");
            Transform lod1 = FindRecursive(root.transform, "LOD1");
            Transform lod2 = FindRecursive(root.transform, "LOD2");
            if (lod0 == null || lod1 == null || lod2 == null)
                return; // Retains compatibility with the legacy single-LOD FBX until replacement.

            LODGroup group = root.GetComponent<LODGroup>();
            if (group == null) group = root.AddComponent<LODGroup>();
            group.animateCrossFading = false;
            group.fadeMode = LODFadeMode.None;
            group.SetLODs(new[]
            {
                new LOD(0.58f, lod0.GetComponentsInChildren<Renderer>(true)),
                new LOD(0.24f, lod1.GetComponentsInChildren<Renderer>(true)),
                new LOD(0.04f, lod2.GetComponentsInChildren<Renderer>(true))
            });
            group.RecalculateBounds();
        }

        private static Transform FindRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (int index = 0; index < parent.childCount; index++)
            {
                Transform found = FindRecursive(parent.GetChild(index), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
#endif
