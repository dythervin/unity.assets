using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dythervin.Assets
{
    public static partial class Assets
    {
        public static event Action OnRefresh
        {
            add => AssetPostprocessorHelper.OnRefresh += value;
            remove => AssetPostprocessorHelper.OnRefresh -= value;
        }
        public static event Action OnBeforeSave
        {
            add => AssetModificationProcessorHelper.OnBeforeSave += value;
            remove => AssetModificationProcessorHelper.OnBeforeSave -= value;
        }

        public const string DefaultPath = "Assets";
        private static bool LogEnabled => false;
        private static readonly string[] PathBuffer = new string[1];

        public static bool FileExist(string path)
        {
            return File.Exists(path);
        }

        public static void CreateDir(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public static string[] FindAssetsGuids(Type type, string path = DefaultPath)
        {
#if UNITY_EDITOR
            if (Directory.Exists(path))
            {
                string full = type.FullName.Replace("UnityEngine.", string.Empty);

                int index = path.IndexOf("/Assets", StringComparison.Ordinal);
                if (index >= 0)
                    path = path.Remove(0, index + 1);

                if (LogEnabled)
                    Debug.Log($"Searching {full} in {path}...");
                PathBuffer[0] = path;
                return AssetDatabase.FindAssets($"t:{full}", PathBuffer);
            }
#endif

            return Array.Empty<string>();
        }

        [System.Flags]
        public enum ObjFlags
        {
            None,
            GameObject = 1 << 0,
            GameObjectChild = 1 << 1,
            ScriptableObject = 1 << 2
        }

        public static bool HasFlagFast(this Assets.ObjFlags value, Assets.ObjFlags flag)
        {
            return (value & flag) != 0;
        }
    }
}