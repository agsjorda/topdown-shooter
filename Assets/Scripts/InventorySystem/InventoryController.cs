using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InventoryController : MonoBehaviour
{
    [SerializeField] private Inventory_Base inventory;
    [SerializeField] private InventoryUIConfig ui;

    public IReadOnlyList<Slot> Slots => ui?.Slots; // Expose slots for drag controller

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
        for (int i = 0; i < slots.Count; i++)
            slots[i].ClearItem();

        int itemCount = Mathf.Min(slots.Count, inventory.itemList.Count);
        for (int i = 0; i < itemCount; i++) {
            var item = inventory.itemList[i];
            if (item?.itemData != null)
                slots[i].SetItem(item);
        }
    }
}