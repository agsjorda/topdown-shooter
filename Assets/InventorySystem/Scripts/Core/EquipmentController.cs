using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    /// <summary>
    /// Default implementation of IEquipmentSystem for managing equipped items.
    /// Handles equipping, unequipping, and tracking equipped items across different slot types.
    /// </summary>
    [DisallowMultipleComponent]
    public class EquipmentController : MonoBehaviour, IEquipmentSystem
    {
        [System.Serializable]
        public class EquippedItem
        {
            public EquipmentSlotTypeSO slotType;
            public InventoryItem item;
        }

        public List<EquippedItem> equippedItems = new List<EquippedItem>();

        [Tooltip("Log equipment operations to the Console")]
        [SerializeField] private bool debugLogging = false;

        public event Action<EquipmentSlotTypeSO, InventoryItem> OnEquipmentChanged;

        public void EquipItem(InventoryItem item, EquipmentSlotTypeSO slotType)
        {
            if (item == null || item.itemData == null) return;

            // Remove any existing item in this slot (without firing a separate unequip event;
            // listeners get a single change notification with the new item)
            equippedItems.RemoveAll(x => x.slotType == slotType);

            equippedItems.Add(new EquippedItem {
                slotType = slotType,
                item = item
            });

            OnEquipmentChanged?.Invoke(slotType, item);
            Log($"Equipped {item.itemData.itemName} in {slotType} slot");
        }

        public void UnequipSlot(EquipmentSlotTypeSO slotType)
        {
            var equippedItem = GetEquippedItem(slotType);
            if (equippedItem != null) {
                equippedItems.RemoveAll(x => x.slotType == slotType);
                OnEquipmentChanged?.Invoke(slotType, null);
                Log($"Unequipped item from {slotType} slot");
            }
        }

        public InventoryItem GetEquippedItem(EquipmentSlotTypeSO slotType)
        {
            var equipped = equippedItems.Find(x => x.slotType == slotType);
            return equipped?.item;
        }

        private void Log(string message)
        {
            if (debugLogging) Debug.Log(message);
        }
    }
}
