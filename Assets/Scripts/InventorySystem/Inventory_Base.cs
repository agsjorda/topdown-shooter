using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Base : MonoBehaviour
{
    public event Action OnInventoryChanged;
    public int maxInventorySize = 24;
    public List<Inventory_Item> itemList = new List<Inventory_Item>();

    // Changed from property to method to fix CS1955
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
        OnInventoryChanged?.Invoke();
    }

    public void SwapItems(int indexA, int indexB)
    {
        if (itemList == null ||
            indexA < 0 || indexA >= itemList.Count ||
            indexB < 0 || indexB >= itemList.Count ||
            indexA == indexB)
            return;

        (itemList[indexA], itemList[indexB]) = (itemList[indexB], itemList[indexA]);
        OnInventoryChanged?.Invoke();
    }
}