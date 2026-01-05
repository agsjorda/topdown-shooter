using System.Collections.Generic;
using UnityEngine;
using InventorySystem;

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
        public EquipmentSlotType slotType;
        public InventoryItem item;
    }

    public List<EquippedItem> equippedItems = new List<EquippedItem>();

    public void EquipItem(InventoryItem item, EquipmentSlotType slotType)
    {
        if (item == null || item.itemData == null) return;

        // Remove any existing item in this slot
        UnequipSlot(slotType);

        // Add new item
        equippedItems.Add(new EquippedItem {
            slotType = slotType,
            item = item
        });

        Debug.Log($"Equipped {item.itemData.itemName} in {slotType} slot");
    }

    public void UnequipSlot(EquipmentSlotType slotType)
    {
        var equippedItem = GetEquippedItem(slotType);
        if (equippedItem != null) {
            equippedItems.RemoveAll(x => x.slotType == slotType);
            Debug.Log($"Unequipped item from {slotType} slot");
        }
    }

    public InventoryItem GetEquippedItem(EquipmentSlotType slotType)
    {
        var equipped = equippedItems.Find(x => x.slotType == slotType);
        return equipped?.item;
    }

    // NEW: Helper method for InventoryController
    public bool AddItemToSlot(InventoryItem item, int slotIndex)
    {
        // This is a placeholder - you'll need to implement this based on your inventory system
        Debug.Log($"Would add item {item.itemData.itemName} to inventory slot {slotIndex}");
        return true;
    }
}