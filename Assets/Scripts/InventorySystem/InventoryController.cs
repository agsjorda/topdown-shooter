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
        // SyncInventory(); // Disabled: TabFilterManager handles slot population
    }

    // Disabled: Only TabFilterManager should update slots
    private void SyncInventory() { }

    public void MoveItem(int fromIndex, int toIndex)
    {
        if (inventory == null) return;
        inventory.SmartMoveItem(fromIndex, toIndex);
    }

    public Inventory_Item GetItemAtSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventory.itemList.Count) {
            return inventory.itemList[slotIndex];
        }
        return null;
    }

    public bool AddItemToSlot(Inventory_Item item, int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inventory.maxInventorySize) {
            while (inventory.itemList.Count <= slotIndex) {
                inventory.itemList.Add(null);
            }
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

    public void SwapSlots(int fromIndex, int toIndex)
    {
        if (inventory != null) {
            inventory.SwapItems(fromIndex, toIndex);
        }
    }
}