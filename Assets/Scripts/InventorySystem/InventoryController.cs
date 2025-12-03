using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class InventoryController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument targetDocument;
    [SerializeField] private Inventory_Base inventory;
    [SerializeField] private InventoryUIConfig ui;

    public BaseSlot[] Slots { get; private set; }
    private IReadOnlyList<BaseSlot> slots;

    void Awake()
    {
        inventory ??= FindObjectOfType<Inventory_Base>();
        ui ??= FindObjectOfType<InventoryUIConfig>();
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

            // FIXED: IReadOnlyList doesn't have CopyTo, so convert to array properly
            Slots = new BaseSlot[slots.Count];
            for (int i = 0; i < slots.Count; i++) {
                Slots[i] = slots[i];
            }
        } else {
            Debug.LogError("UI Config or slots not found!");
            yield break;
        }

        SyncAllSlotsFromInventory();
    }

    private void OnInventoryChanged() => SyncAllSlotsFromInventory();

    private void SyncAllSlotsFromInventory()
    {
        if (slots == null || inventory == null) return;

        // Clear only necessary slots
        int slotsToClear = Mathf.Min(slots.Count, inventory.maxInventorySize);
        for (int i = 0; i < slotsToClear; i++) {
            slots[i].ClearItem();
        }

        // Fill slots with items
        int itemsToDisplay = Mathf.Min(slots.Count, inventory.itemList.Count);
        for (int i = 0; i < itemsToDisplay; i++) {
            var invItem = inventory.itemList[i];
            if (invItem?.itemData != null) {
                slots[i].SetItem(invItem);
            }
        }
    }
}