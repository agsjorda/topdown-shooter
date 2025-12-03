using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
public class Item_DataSO : ScriptableObject
{
    [SerializeField, HideInInspector] private string _itemId; // Changed from public to private with [HideInInspector]

    // Public property that returns the ID
    public string itemId {
        get {
            // Ensure we always have an ID
            if (string.IsNullOrEmpty(_itemId)) {
#if UNITY_EDITOR
                _itemId = GenerateGuid();
                EditorUtility.SetDirty(this);
#else
                _itemId = "INVALID_ID";
#endif
            }
            return _itemId;
        }
    }

    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public bool stackable = true;
    public int maxStack = 99;

    // Constructor-like behavior for new assets
#if UNITY_EDITOR
    private void Reset()
    {
        _itemId = GenerateGuid();
    }

    private void OnValidate()
    {
        // Ensure ID exists when asset is loaded or modified
        if (string.IsNullOrEmpty(_itemId)) {
            _itemId = GenerateGuid();
            EditorUtility.SetDirty(this);
        }
    }

    [ContextMenu("Generate New ID")]
    private void GenerateNewId()
    {
        string oldId = _itemId;
        _itemId = GenerateGuid();
        EditorUtility.SetDirty(this);
        Debug.Log($"ID changed from {oldId} to {_itemId}");
    }

    private string GenerateGuid()
    {
        return Guid.NewGuid().ToString("N"); // "N" = 32 digits without hyphens
    }
#endif
}