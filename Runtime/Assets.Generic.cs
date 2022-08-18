using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.Assets
{
    public static partial class Assets
    {
        private static string[] FindAssetsGuids<T>(string path)
        {
            return FindAssetsGuids(typeof(T), path);
        }

        public static HashSet<TObject> LoadAll<TObject>(HashSet<TObject> output, string path = DefaultPath)
            where TObject : Object
        {
#if UNITY_EDITOR
            string[] guids = FindAssetsGuids<TObject>(path);
            int duplicates = 0;
            foreach (string guid in guids)
            {
                var assets = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Object)) as TObject;
                if (!output.Add(assets))
                    duplicates++;
            }

            if (LogEnabled)
                Debug.Log($"{(duplicates > 0 ? $"Duplicates: {duplicates}\n" : string.Empty)} Found unique: {output.Count}");
            return output;
#else
            return null;
#endif
        }

        public static IEnumerator LoadAllRoutine<TObject>(HashSet<TObject> output, System.Action<float> onProgressUpdated, string path = DefaultPath,
            int yieldEvery = 5)
            where TObject : Object
        {
#if UNITY_EDITOR
            string[] guids = FindAssetsGuids<TObject>(path);
            int duplicates = 0;
            float i = 0;
            foreach (string guid in guids)
            {
                var assets = AssetDatabase.LoadAssetAtPath<TObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (!output.Add(assets))
                    duplicates++;

                if (i % yieldEvery != 0)
                    continue;

                onProgressUpdated.Invoke(i / guids.Length);
                yield return null;
            }

            onProgressUpdated.Invoke(1);
            Debug.Log($"{(duplicates > 0 ? $"Duplicates: {duplicates}\n" : string.Empty)} Found unique: {output.Count}");
#else
            yield return null;
#endif
        }

        public static TObject LoadFirst<TObject>(string path = DefaultPath)
            where TObject : Object
        {
            return (TObject)LoadFirst(typeof(TObject), path);
        }

        public static HashSet<T> LoadAll<T>(string path = DefaultPath)
            where T : Object
        {
            var assetsFound = new HashSet<T>();
            LoadAll(assetsFound, path);
            return assetsFound;
        }

        public static HashSet<TTarget> LoadAll<TBase, TTarget>(HashSet<TTarget> assetsFound, HashSet<TBase> assetsBase, string path = DefaultPath)
            where TBase : Object
        {
            foreach (TBase baseObj in LoadAll(assetsBase, path))
            {
                if (baseObj is TTarget target)
                    assetsFound.Add(target);
            }

            return assetsFound;
        }

        public static HashSet<TTarget> LoadAll<TBase, TTarget>(string path = DefaultPath)
            where TBase : Object
        {
            return LoadAll(new HashSet<TTarget>(), new HashSet<TBase>(), path);
        }

        public static HashSet<T> LoadAllInGameObjectChild<T>(string path = DefaultPath)
            where T : class
        {
            var assetsFound = new HashSet<T>();
            foreach (GameObject gameObject in LoadAll<GameObject>(path))
            foreach (T component in gameObject.GetComponentsInChildren<T>(true))
                assetsFound.Add(component);
            return assetsFound;
        }

        public static HashSet<T> LoadAllInGameObject<T>(string path = DefaultPath)
            where T : class
        {
            var assetsFound = new HashSet<T>();
            foreach (GameObject gameObject in LoadAll<GameObject>(path))
            foreach (T component in gameObject.GetComponents<T>())
                assetsFound.Add(component);
            return assetsFound;
        }

        public static HashSet<T> LoadAllInterface<T>(ObjFlags flags = ObjFlags.GameObject | ObjFlags.ScriptableObject, string path = DefaultPath)
            where T : class
        {
            var assetsFound = new HashSet<T>();
            if (flags.HasFlagFast(ObjFlags.GameObjectChild))
            {
                foreach (T component in LoadAllInGameObjectChild<T>(path))
                    assetsFound.Add(component);
            }
            else if (flags.HasFlagFast(ObjFlags.GameObject))
            {
                foreach (T component in LoadAllInGameObject<T>(path))
                    assetsFound.Add(component);
            }


            if ((flags & ObjFlags.ScriptableObject) != 0)
            {
                foreach (ScriptableObject scriptableObject in LoadAll<ScriptableObject>(path))
                    if (scriptableObject is T t)
                        assetsFound.Add(t);
            }

            return assetsFound;
        }
    }
}