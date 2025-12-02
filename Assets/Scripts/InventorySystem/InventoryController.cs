using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InventoryController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory_Base inventory;
    [SerializeField] private InventoryUIConfig ui;

    private IReadOnlyList<BaseSlot> slots;

    void Awake()
    {
        if (inventory == null) inventory = Object.FindFirstObjectByType<Inventory_Base>();
        if (ui == null) ui = Object.FindFirstObjectByType<InventoryUIConfig>();
    }

    void OnEnable()
    {
        StartCoroutine(InitSlots());
        if (inventory != null) {
            inventory.OnInventoryChanged -= OnInventoryChanged;
            inventory.OnInventoryChanged += OnInventoryChanged;
        }
    }

    void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= OnInventoryChanged;
    }

    private IEnumerator InitSlots()
    {
        yield return null; // wait for UI to build
        slots = ui?.Slots;
        // initial sync
        SyncAllSlotsFromInventory();
    }

    private void OnInventoryChanged()
    {
        // refresh UI when the list changes
        SyncAllSlotsFromInventory();
    }

    // Sync all slots from inventory list
    private void SyncAllSlotsFromInventory()
    {
        if (slots == null || inventory == null) return;

        // clear all first
        for (int i = 0; i < slots.Count; i++)
            slots[i].ClearItem();

        // fill by index (null-safe)
        int max = Mathf.Min(slots.Count, inventory.itemList.Count);
        for (int i = 0; i < max; i++) {
            var invItem = inventory.itemList[i];
            if (invItem != null && invItem.itemData != null)
                slots[i].SetItem(invItem);
        }
    }
}