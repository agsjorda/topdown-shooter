using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InventoryController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory_Base inventory;
    [SerializeField] private InventoryUIConfig ui;

    public IReadOnlyList<Slot> Slots => ui?.Slots;
    public Inventory_Base Inventory => inventory;

    void OnEnable()
    {
        StartCoroutine(Initialize());
    }

    void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= SyncInventory;
    }

    private IEnumerator Initialize()
    {
        yield return null;

        if (ui == null || inventory == null) yield break;

        inventory.OnInventoryChanged -= SyncInventory;
        inventory.OnInventoryChanged += SyncInventory;
        SyncInventory();
    }

    private void SyncInventory()
    {
        if (ui?.Slots == null || inventory?.itemList == null) return;

        var slots = ui.Slots;

        // Clear all slots first
        for (int i = 0; i < slots.Count; i++)
            slots[i].ClearItem();

        // Fill slots with items - respect null items as empty slots
        int maxSlotsToFill = Mathf.Min(slots.Count, inventory.maxInventorySize);
        for (int i = 0; i < maxSlotsToFill; i++) {
            if (i < inventory.itemList.Count) {
                var item = inventory.itemList[i];
                if (item != null) {
                    slots[i].SetItem(item);
                }
                // If item is null, slot stays cleared
            }
            // Slots beyond itemList.Count remain cleared
        }
    }

    public void MoveItem(int fromIndex, int toIndex)
    {
        if (inventory == null) return;

        // Use the new SmartMoveItem method
        inventory.SmartMoveItem(fromIndex, toIndex);
    }

    // Add these methods to your existing InventoryController class:

    public Inventory_Item GetItemAtSlot(int slotIndex)
    {
        // Return the item at the specified slot
        if (slotIndex >= 0 && slotIndex < inventory.itemList.Count) {
            return inventory.itemList[slotIndex];
        }
        return null;
    }

    public bool AddItemToSlot(Inventory_Item item, int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventory.maxInventorySize) {
            // Ensure list is large enough
            while (inventory.itemList.Count <= slotIndex) {
                inventory.itemList.Add(null);
            }

            // If slot is empty, place item there
            if (inventory.itemList[slotIndex] == null) {
                inventory.itemList[slotIndex] = item;
                inventory.NotifyInventoryChanged();
                return true;
            }
        }
        return false;
    }

    public void RemoveItemAtSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventory.itemList.Count) {
            inventory.itemList[slotIndex] = null;
            inventory.NotifyInventoryChanged();
        }
    }

    // Keep SwapSlots for backward compatibility
    public void SwapSlots(int fromIndex, int toIndex)
    {
        if (inventory != null) {
            inventory.SwapItems(fromIndex, toIndex);
        }
    }
}