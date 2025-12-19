using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryModel : MonoBehaviour, IInventory
{
    public event Action OnInventoryChanged;

    public int maxInventorySize = 24;
    //Item list can contain nulls representing empty slots
    public List<InventoryItem> itemList = new List<InventoryItem>();

    public IReadOnlyList<InventoryItem> Items => itemList;
    public int MaxInventorySize => maxInventorySize;

    public bool CanAddItem() => itemList.Count < maxInventorySize;

    public void AddItem(InventoryItem itemToAdd)
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

    public void RemoveItemAt(int index)
    {
        if (!IsValidSlotIndex(index)) return;
        itemList[index] = null;
        NotifyInventoryChanged();
    }

    public void MoveItem(int fromIndex, int toIndex)
    {
        SmartMoveItem(fromIndex, toIndex);
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

    public InventoryItem GetItemAt(int index)
    {
        if (index < 0 || index >= maxInventorySize)
            return null;
        while (itemList.Count <= index)
            itemList.Add(null);
        return itemList[index];
    }

    public void SetItemAt(int index, InventoryItem item)
    {
        if (index >= 0 && index < maxInventorySize) {
            while (itemList.Count <= index)
                itemList.Add(null);
            itemList[index] = item;
            NotifyInventoryChanged();
        }
    }

    public bool IsValidSlotIndex(int index)
    {
        return index >= 0 && index < maxInventorySize;
    }

    private int FindFirstEmptySlot()
    {
        for (int i = 0; i < itemList.Count; i++) {
            if (itemList[i] == null) {
                return i;
            }
        }
        if (itemList.Count < maxInventorySize) {
            return itemList.Count;
        }
        return -1;
    }

    private void InsertAtSlot(int slotIndex, InventoryItem item)
    {
        if (slotIndex < 0 || slotIndex >= maxInventorySize) {
            Debug.LogError($"Invalid slot index: {slotIndex}");
            return;
        }
        while (itemList.Count <= slotIndex) {
            itemList.Add(null);
        }
        itemList[slotIndex] = item;
    }

    public void SmartMoveItem(int fromIndex, int toIndex)
    {
        if (itemList == null || fromIndex == toIndex) return;
        if (fromIndex < 0 || fromIndex >= itemList.Count) return;
        if (toIndex < 0 || toIndex >= maxInventorySize) return;
        while (itemList.Count <= toIndex) {
            itemList.Add(null);
        }
        var itemToMove = itemList[fromIndex];
        var targetItem = itemList[toIndex];
        if (targetItem == null) {
            itemList[fromIndex] = null;
            itemList[toIndex] = itemToMove;
        } else {
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
