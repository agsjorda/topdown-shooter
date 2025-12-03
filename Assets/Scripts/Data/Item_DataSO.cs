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
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(_itemId)) {
                _itemId = Guid.NewGuid().ToString("N");
                EditorUtility.SetDirty(this);
            }
#endif
            return _itemId;
        }
    }

    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public bool stackable = true;
    public int maxStack = 99;

#if UNITY_EDITOR
    private void Reset()
    {
        _itemId = Guid.NewGuid().ToString("N");
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_itemId)) {
            _itemId = Guid.NewGuid().ToString("N");
            EditorUtility.SetDirty(this);
        }
    }
#endif
}