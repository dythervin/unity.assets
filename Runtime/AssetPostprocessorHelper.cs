using System;
using UnityEditor;

namespace Dythervin.Assets
{
    internal class AssetPostprocessorHelper
#if UNITY_EDITOR
        : AssetPostprocessor
#endif
    {
        internal static event Action OnRefresh;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            OnRefresh?.Invoke();
        }
    }

    internal class AssetModificationProcessorHelper
#if UNITY_EDITOR
        : AssetModificationProcessor
#endif
    {
        internal static event Action OnBeforeSave;

        private static string[] OnWillSaveAssets(string[] paths)
        {
            OnBeforeSave?.Invoke();
            return paths;
        }
    }
}