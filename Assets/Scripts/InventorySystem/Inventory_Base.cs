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

    public bool CanAddItem() => itemList.Count < maxInventorySize;

    public void AddItem(Inventory_Item itemToAdd)
    {
        if (CanAddItem()) {
            itemList.Add(itemToAdd);
            Debug.Log($"Added item: {itemToAdd.itemData.itemName}");
            Debug.Log($"Current inventory size: {itemList.Count}/{maxInventorySize}");
            Debug.Log("Inventory contents:");
            foreach (var item in itemList) {
                Debug.Log($" - {item.itemData.itemName}");
            }
            InventoryChanged?.Invoke(); // tell UI to update
        } else {
            Debug.Log("Inventory is full!");
        }
    }
}
