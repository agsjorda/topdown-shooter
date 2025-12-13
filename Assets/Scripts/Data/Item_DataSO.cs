using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
public class Item_DataSO : ScriptableObject
{
    [SerializeField, HideInInspector] private string _itemId;

    public string itemId {
        get {
            // Auto-generate ID if missing (works in editor and builds)
            if (string.IsNullOrEmpty(_itemId)) {
                _itemId = Guid.NewGuid().ToString("N");
#if UNITY_EDITOR
                // Mark dirty only in editor to save the change
                if (!Application.isPlaying) {
                    EditorUtility.SetDirty(this);
                }
#endif
            }
            return _itemId;
        }
    }

    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public ItemType itemType;
    public bool stackable = true;
    public int maxStack = 99;

#if UNITY_EDITOR
    private void Reset()
    {
        _itemId = Guid.NewGuid().ToString("N");
    }

    private void OnValidate()
    {
        // Only run in Editor, not during Play Mode exit
        if (!Application.isPlaying && string.IsNullOrEmpty(_itemId)) {
            _itemId = Guid.NewGuid().ToString("N");
            EditorUtility.SetDirty(this);
        }
    }
#endif
}