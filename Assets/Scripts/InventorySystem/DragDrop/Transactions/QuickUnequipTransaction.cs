using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

/// **QuickUnequipTransaction** - Double-click to unequip item from equipment slot
/// 
/// What it does:
/// - Finds first available inventory slot
/// - Moves equipped item from equipment slot to inventory
/// 
/// This transaction is triggered by double-clicking an equipped item.
/// It automatically finds an empty spot in your inventory and puts the item there.
public class QuickUnequipTransaction : DragDropTransaction
{
    private readonly EquipmentSlot sourceEquipmentSlot;
    private readonly Inventory_Item equippedItem;
    private readonly List<Slot> inventorySlots;
    
    private int targetInventoryIndex = -1;
    private Slot targetInventorySlot;

    public QuickUnequipTransaction(
        EquipmentSlot sourceEquipmentSlot,
        Inventory_Item equippedItem,
        List<Slot> inventorySlots,
        InventoryController inventoryController,
        EquipmentController equipmentController,
        bool debugMode = false)
        : base(inventoryController, equipmentController, debugMode)
    {
        this.sourceEquipmentSlot = sourceEquipmentSlot;
        this.equippedItem = equippedItem;
        this.inventorySlots = inventorySlots;
    }

    public override bool CanExecute()
    {
        if (equippedItem == null || equippedItem.itemData == null) {
            LogError("No item to unequip");
            return false;
        }

        if (sourceEquipmentSlot == null) {
            LogError("No source equipment slot");
            return false;
        }

        targetInventoryIndex = FindFirstEmptyInventorySlot();

        if (targetInventoryIndex < 0) {
            Log("No empty inventory slot available for unequipping");
            return false;
        }

        targetInventorySlot = inventorySlots[targetInventoryIndex];
        Log($"Quick unequip {equippedItem.itemData.itemName} from {sourceEquipmentSlot.SlotType} to slot {targetInventoryIndex}");
        return true;
    }

    public override IEnumerator Execute()
    {
        yield return null;

        Log($"Unequipping {equippedItem.itemData.itemName} from {sourceEquipmentSlot.SlotType} to inventory slot {targetInventoryIndex}");

        sourceEquipmentSlot.ClearItem();
        sourceEquipmentSlot.RefreshVisualState();
        equipmentController?.UnequipSlot(sourceEquipmentSlot.SlotType);

        if (targetInventorySlot != null) {
            targetInventorySlot.SetItem(equippedItem);
        }
        inventoryController?.AddItemToSlot(equippedItem, targetInventoryIndex);

        targetInventorySlot?.AddToClassList("inventorySlots--drop-target");
        yield return null;
        targetInventorySlot?.RemoveFromClassList("inventorySlots--drop-target");
    }

    public override VisualElement GetTargetVisual() => targetInventorySlot;

    /// Finds the first available inventory slot.
    /// Returns the index of the first empty slot, or -1 if inventory is full.
    private int FindFirstEmptyInventorySlot()
    {
        if (inventorySlots == null) return -1;

        for (int i = 0; i < inventorySlots.Count; i++) {
            if (inventorySlots[i] != null && !inventorySlots[i].HasItem) {
                return i;
            }
        }

        return -1;
    }
}
