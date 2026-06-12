using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InventorySystem
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory System/Item Data")]
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

        [Header("Classification")]
        [Tooltip("Category drives tab filtering. Author categories as assets: Create > Inventory System > Item Category")]
        public ItemCategorySO category;

        [Tooltip("Equipment slot types this item can be equipped into. Empty = not equippable")]
        public List<EquipmentSlotTypeSO> compatibleSlots = new List<EquipmentSlotTypeSO>();

        [Header("Stacking")]
        public bool stackable = true;
        public int maxStack = 99;

        public bool CanEquipTo(EquipmentSlotTypeSO slot)
        {
            return slot != null && compatibleSlots != null && compatibleSlots.Contains(slot);
        }

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
}
