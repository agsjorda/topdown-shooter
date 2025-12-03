using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class InventoryController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument targetDocument;
    [SerializeField] private Inventory_Base inventory;
    [SerializeField] private InventoryUIConfig ui;

    public BaseSlot[] Slots;
    private IReadOnlyList<BaseSlot> slots;

    void Awake()
    {
        if (inventory == null) inventory = Object.FindFirstObjectByType<Inventory_Base>();
        if (ui == null) ui = Object.FindFirstObjectByType<InventoryUIConfig>();
    }

    void OnEnable()
    {
        if (ui != null) {
            ui.RebuildUI();
        }

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
        yield return null;

        if (ui != null && ui.Slots != null) {
            slots = ui.Slots;
            Slots = slots.ToArray();
        } else {
            Debug.LogError("UI Config or slots not found!");
            yield break;
        }

        SyncAllSlotsFromInventory();
    }

    private void OnInventoryChanged()
    {
        SyncAllSlotsFromInventory();
    }

    private void SyncAllSlotsFromInventory()
    {
        if (slots == null || inventory == null) return;

        // Clear all slots first
        for (int i = 0; i < slots.Count; i++) {
            slots[i].ClearItem();
        }

        // Fill slots with inventory items
        int max = Mathf.Min(slots.Count, inventory.itemList.Count);
        for (int i = 0; i < max; i++) {
            var invItem = inventory.itemList[i];
            if (invItem != null && invItem.itemData != null) {
                slots[i].SetItem(invItem);
            }
        }
    }
}