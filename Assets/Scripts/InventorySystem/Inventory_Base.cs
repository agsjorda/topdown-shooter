using System;
using System.Collections.Generic;
using UnityEngine;

//This class holds the base inventory functionality
public class Inventory_Base : MonoBehaviour
{
    // Notify UI to refresh when contents change
    public event Action InventoryChanged;

    //Maximum number of items the inventory can hold
    public int maxInventorySize = 24;
    public List<Inventory_Item> itemList = new List<Inventory_Item>();

    // Cache to track changes
    private int _lastItemCount = 0;

    public bool CanAddItem()
    {
        if (itemList == null) return false;
        return itemList.Count < maxInventorySize;
    }

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

        // Add the item to the list
        itemList.Add(itemToAdd);

        Debug.Log($"Added item: {itemToAdd.itemData.itemName}. Total items: {itemList.Count}");

        // Notify UI to update
        InventoryChanged?.Invoke();
    }

    // Get item at specific slot index
    public Inventory_Item GetItemAtSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= itemList.Count)
            return null;

        return itemList[slotIndex];
    }

    // Update to check for changes
    void Update()
    {
        if (itemList.Count != _lastItemCount) {
            _lastItemCount = itemList.Count;
            InventoryChanged?.Invoke();
        }
    }
}