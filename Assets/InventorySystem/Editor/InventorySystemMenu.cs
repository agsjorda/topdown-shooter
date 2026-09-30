using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InventorySystem.Editor
{
    /// <summary>
    /// GameObject > Inventory System > Inventory UI: drops the ready-made InventoryUI prefab
    /// (Panel Renderer + all inventory components, pre-wired) into the open scene.
    /// </summary>
    internal static class InventorySystemMenu
    {
        private const string PrefabName = "InventoryUI";

        [MenuItem("GameObject/Inventory System/Inventory UI", false, 10)]
        private static void CreateInventoryUI(MenuCommand command)
        {
            var prefab = FindStarterPrefab();
            if (prefab == null) {
                Debug.LogError($"[InventorySystem] Could not find the '{PrefabName}' prefab. It ships in InventorySystem/Starter.");
                return;
            }

            var parent = command.context as GameObject;
            var scene = parent != null ? parent.scene : SceneManager.GetActiveScene();

            if (FindInScene<InventoryUIController>(scene).Count > 0 &&
                !EditorUtility.DisplayDialog("Inventory UI",
                    $"Scene '{scene.name}' already has an InventoryUIController. Add another Inventory UI anyway?",
                    "Add", "Cancel")) {
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            GameObjectUtility.SetParentAndAlign(instance, parent);
            Undo.RegisterCreatedObjectUndo(instance, "Create Inventory UI");

            // A scene that already owns the data (e.g. on the player) keeps it; InventoryService
            // looks these up by type, so a second copy would split the inventory.
            RemoveDuplicate<InventoryModel>(instance, scene);
            RemoveDuplicate<EquipmentController>(instance, scene);

            Selection.activeGameObject = instance;
        }

        private static void RemoveDuplicate<T>(GameObject instance, Scene scene) where T : Component
        {
            var own = instance.GetComponent<T>();
            if (own == null) return;
            foreach (var existing in FindInScene<T>(scene)) {
                if (existing == own) continue;
                Undo.DestroyObjectImmediate(own);
                Debug.Log($"[InventorySystem] Scene already has a {typeof(T).Name} on '{existing.gameObject.name}'; the new Inventory UI uses that one.");
                return;
            }
        }

        private static List<T> FindInScene<T>(Scene scene) where T : Component
        {
            var found = new List<T>();
            if (!scene.IsValid() || !scene.isLoaded) return found;
            foreach (var root in scene.GetRootGameObjects())
                found.AddRange(root.GetComponentsInChildren<T>(true));
            return found;
        }

        private static GameObject FindStarterPrefab()
        {
            GameObject fallback = null;
            foreach (var guid in AssetDatabase.FindAssets($"{PrefabName} t:Prefab")) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset == null || asset.name != PrefabName || asset.GetComponent<InventoryUIController>() == null) continue;
                if (path.Replace('\\', '/').Contains("/Starter/")) return asset;
                fallback ??= asset;
            }
            return fallback;
        }
    }
}
