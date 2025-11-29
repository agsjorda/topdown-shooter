using System.Collections.Generic;
using UnityEngine;

public class Inventory_Base : MonoBehaviour
{
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
        } else {
            Debug.Log("Inventory is full!");
        }
    }
}
