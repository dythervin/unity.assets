using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dythervin.Assets
{
    public static partial class Assets
    {
        private static readonly HashSet<Object> Buffer = new HashSet<Object>();
        private static readonly HashSet<Object> Buffer2 = new HashSet<Object>();
        private static readonly HashSet<Component> BufferComponents = new HashSet<Component>();

        public static HashSet<Object> LoadAll(Type type, string path = DefaultPath)
        {
            Buffer.Clear();
            return LoadAll(type, Buffer, path);
        }

        public static HashSet<Object> LoadAll(Type type, HashSet<Object> output, string path = DefaultPath)
        {
#if UNITY_EDITOR
            string[] guids = FindAssetsGuids(type, path);
            int duplicates = 0;
            foreach (string guid in guids)
            {
                var assets = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Object));
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

        public static Object LoadFirst(Type type, string path = DefaultPath)
        {
#if UNITY_EDITOR
            foreach (string guid in FindAssetsGuids(type, path))
            {
                var asset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Object));
                return asset;
            }
#endif
            return null;
        }

        public static HashSet<Object> LoadAll(Type targetType, Type mainType, HashSet<Object> assetsFound, HashSet<Object> assetsBase,
            string path = DefaultPath)
        {
            foreach (var baseObj in LoadAll(mainType, assetsBase, path))
            {
                if (targetType.IsInstanceOfType(baseObj))
                    assetsFound.Add(baseObj);
            }

            return assetsFound;
        }

        public static HashSet<Object> LoadAll(Type targetType, Type mainType, string path = DefaultPath)
        {
            return LoadAll(targetType, mainType, new HashSet<Object>(), new HashSet<Object>(), path);
        }

        public static HashSet<Component> LoadAllInGameObjectChild(Type type, string path = DefaultPath)
        {
            BufferComponents.Clear();
            foreach (GameObject gameObject in LoadAll<GameObject>(path))
            foreach (Component component in gameObject.GetComponentsInChildren(type, true))
                BufferComponents.Add(component);
            return BufferComponents;
        }

        public static HashSet<Component> LoadAllInGameObject(Type type, string path = DefaultPath)
        {
            BufferComponents.Clear();
            foreach (GameObject gameObject in LoadAll<GameObject>(path))
            foreach (Component component in gameObject.GetComponents(type))
                BufferComponents.Add(component);
            return BufferComponents;
        }

        public static HashSet<Object> LoadAllInterface(Type type, ObjFlags flags = ObjFlags.GameObject | ObjFlags.ScriptableObject,
            string path = DefaultPath)
        {
            Buffer2.Clear();
            return LoadAllInterface(type, Buffer2, flags, path);
        }

        public static HashSet<Object> LoadAllInterface(Type type, HashSet<Object> assetsFound, ObjFlags flags = ObjFlags.GameObject | ObjFlags.ScriptableObject,
            string path = DefaultPath)
        {
            if (flags.HasFlagFast(ObjFlags.GameObjectChild))
            {
                foreach (Component component in LoadAllInGameObjectChild(type, path))
                    assetsFound.Add(component);
            }
            else if (flags.HasFlagFast(ObjFlags.GameObject))
            {
                foreach (var component in LoadAllInGameObject(type, path))
                    assetsFound.Add(component);
            }

            if (flags.HasFlagFast(ObjFlags.ScriptableObject))
            {
                foreach (ScriptableObject scriptableObject in LoadAll<ScriptableObject>(path))
                    if (type.IsInstanceOfType(scriptableObject))
                        assetsFound.Add(scriptableObject);
            }

            return assetsFound;
        }
    }
}