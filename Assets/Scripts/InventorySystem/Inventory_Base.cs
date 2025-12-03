using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Base : MonoBehaviour
{
    public event Action OnInventoryChanged;

    public int maxInventorySize = 24;
    public List<Inventory_Item> itemList = new List<Inventory_Item>();

    public bool CanAddItem() => itemList.Count < maxInventorySize;

    public void AddItem(Inventory_Item itemToAdd)
    {
        if (itemToAdd == null) {
            Debug.LogError("Cannot add null item");
            return;
        }

        if (!CanAddItem()) {
            Debug.Log("Inventory full");
            return;
        }

        // Find the first empty slot
        int emptySlotIndex = FindFirstEmptySlot();

        if (emptySlotIndex >= 0) {
            // Insert at the first empty slot
            InsertAtSlot(emptySlotIndex, itemToAdd);
        } else {
            // No empty slots found, add to the end
            itemList.Add(itemToAdd);
        }

        NotifyInventoryChanged();
        Debug.Log($"Added item to slot {emptySlotIndex}: {itemToAdd.itemData.itemName}");
    }

    // NEW: Find first empty slot (null or beyond list)
    private int FindFirstEmptySlot()
    {
        // Check existing slots for null (empty)
        for (int i = 0; i < itemList.Count; i++) {
            if (itemList[i] == null) {
                return i;
            }
        }

        // If no null slots but we have space, return the next index
        if (itemList.Count < maxInventorySize) {
            return itemList.Count;
        }

        // No empty slots available
        return -1;
    }

    // NEW: Insert item at specific slot
    private void InsertAtSlot(int slotIndex, Inventory_Item item)
    {
        if (slotIndex < 0 || slotIndex >= maxInventorySize) {
            Debug.LogError($"Invalid slot index: {slotIndex}");
            return;
        }

        // If slot is beyond current list size, expand list
        while (itemList.Count <= slotIndex) {
            itemList.Add(null);
        }

        // Place item at the slot
        itemList[slotIndex] = item;
    }

    public void SwapItems(int indexA, int indexB)
    {
        if (itemList == null ||
            indexA < 0 || indexA >= itemList.Count ||
            indexB < 0 || indexB >= itemList.Count ||
            indexA == indexB)
            return;

        (itemList[indexA], itemList[indexB]) = (itemList[indexB], itemList[indexA]);
        NotifyInventoryChanged();
    }

    // Keep your existing SmartMoveItem method
    public void SmartMoveItem(int fromIndex, int toIndex)
    {
        if (itemList == null || fromIndex == toIndex) return;

        // Ensure indices are valid
        if (fromIndex < 0 || fromIndex >= itemList.Count) return;
        if (toIndex < 0 || toIndex >= maxInventorySize) return;

        // Expand list if needed
        while (itemList.Count <= toIndex) {
            itemList.Add(null);
        }

        var itemToMove = itemList[fromIndex];

        // Check what's at target position
        var targetItem = itemList[toIndex];

        if (targetItem == null) {
            // Target is empty - simple move
            itemList[fromIndex] = null;
            itemList[toIndex] = itemToMove;
        } else {
            // Target has item - swap
            itemList[fromIndex] = targetItem;
            itemList[toIndex] = itemToMove;
        }

        NotifyInventoryChanged();
        Debug.Log($"Smart move: {fromIndex} -> {toIndex}");
    }

    public void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }
}