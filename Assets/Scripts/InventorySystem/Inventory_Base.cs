using System;
using System.Collections.Generic;
using UnityEngine;

//This class holds the base inventory functionality
public class Inventory_Base : MonoBehaviour
{
    // Notify UI to refresh when contents change
    public event Action OnInventoryChanged;

    //Maximum number of items the inventory can hold
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

        // Add the item to the list
        itemList.Add(itemToAdd);

        Debug.Log($"Added item: {itemToAdd.itemData.itemName}. Total items: {itemList.Count}");

        // Notify UI to update
        OnInventoryChanged?.Invoke();
    }

    // Add this method to the Inventory_Base class
    public void SwapItems(int indexA, int indexB)
    {
        if (itemList == null) return;
        if (indexA < 0 || indexA >= itemList.Count) return;
        if (indexB < 0 || indexB >= itemList.Count) return;
        if (indexA == indexB) return;

        var temp = itemList[indexA];
        itemList[indexA] = itemList[indexB];
        itemList[indexB] = temp;

        OnInventoryChanged?.Invoke();
    }
}