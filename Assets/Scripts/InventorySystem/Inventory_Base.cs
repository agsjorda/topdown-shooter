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

        itemList.Add(itemToAdd);
        NotifyInventoryChanged();
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

    // FIXED VERSION: Proper move that doesn't shift other items
    public void MoveItem(int fromIndex, int toIndex)
    {
        if (itemList == null || fromIndex == toIndex) return;

        // Validate indices
        if (fromIndex < 0 || fromIndex >= itemList.Count) {
            Debug.LogError($"Invalid fromIndex: {fromIndex}, itemList count: {itemList.Count}");
            return;
        }

        if (toIndex < 0 || toIndex >= maxInventorySize) {
            Debug.LogError($"Invalid toIndex: {toIndex}, maxInventorySize: {maxInventorySize}");
            return;
        }

        Debug.Log($"Moving item from {fromIndex} to {toIndex}");

        // Get the item to move
        var itemToMove = itemList[fromIndex];

        // If target slot is beyond current list size, we need to expand the list
        if (toIndex >= itemList.Count) {
            // Expand list with nulls up to the target index
            while (itemList.Count <= toIndex) {
                itemList.Add(null);
            }

            // Remove from original position
            itemList.RemoveAt(fromIndex);

            // Insert null at original position to maintain positions
            if (fromIndex < itemList.Count) {
                itemList.Insert(fromIndex, null);
            }

            // Place item at target position
            itemList[toIndex] = itemToMove;
        } else {
            // Target position is within current list bounds
            // Check if target slot is empty
            if (itemList[toIndex] == null) {
                // Target is empty - just move item there
                itemList[fromIndex] = null;
                itemList[toIndex] = itemToMove;
            } else {
                // Target has item - swap them
                (itemList[fromIndex], itemList[toIndex]) = (itemList[toIndex], itemList[fromIndex]);
            }
        }

        // Clean up trailing nulls (optional - keeps list compact)
        RemoveTrailingNulls();

        NotifyInventoryChanged();

        Debug.Log($"Move completed. List count: {itemList.Count}");
    }

    // Helper method to remove null entries at the end
    private void RemoveTrailingNulls()
    {
        for (int i = itemList.Count - 1; i >= 0; i--) {
            if (itemList[i] == null)
                itemList.RemoveAt(i);
            else
                break; // Stop when we find a non-null item
        }
    }

    // NEW: Alternative method that maintains exact slot positions
    public void MoveItemToEmptySlot(int fromIndex, int toIndex)
    {
        if (itemList == null || fromIndex == toIndex) return;

        if (fromIndex < 0 || fromIndex >= itemList.Count) return;
        if (toIndex < 0 || toIndex >= maxInventorySize) return;

        // Ensure list is large enough
        while (itemList.Count <= toIndex) {
            itemList.Add(null);
        }

        var itemToMove = itemList[fromIndex];

        // Check if target is actually empty
        if (itemList[toIndex] != null) {
            Debug.LogWarning($"Target slot {toIndex} is not empty! Using swap instead.");
            SwapItems(fromIndex, toIndex);
            return;
        }

        // Move item
        itemList[fromIndex] = null;
        itemList[toIndex] = itemToMove;

        NotifyInventoryChanged();
        Debug.Log($"Moved item from {fromIndex} to empty slot {toIndex}");
    }

    // NEW: Smart move that handles all cases correctly
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