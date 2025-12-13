using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

/// <summary>
/// **QuickEquipTransaction** - Double-click to equip item from inventory
/// 
/// **What it does:**
/// - Finds the appropriate equipment slot based on item type
/// - If slot is empty: Equips the item
/// - If slot is occupied: Swaps items (equipped item goes back to inventory)
/// 
/// **For Junior Developers:**
/// This transaction is triggered by double-clicking an inventory item.
/// It's like a shortcut - instead of dragging, just double-click!
/// </summary>
public class QuickEquipTransaction : DragDropTransaction
{
    private readonly Inventory_Item itemToEquip;
    private readonly int sourceSlotIndex;
    private readonly Slot sourceSlot;
    private readonly List<EquipmentSlot> equipmentSlots;
    private readonly List<Slot> inventorySlots;
    
    private EquipmentSlot targetEquipmentSlot; // Determined during CanExecute

    public QuickEquipTransaction(
        Inventory_Item itemToEquip,
        int sourceSlotIndex,
        Slot sourceSlot,
        List<EquipmentSlot> equipmentSlots,
        List<Slot> inventorySlots,
        InventoryController inventoryController,
        EquipmentController equipmentController,
        bool debugMode = false)
        : base(inventoryController, equipmentController, debugMode)
    {
        this.itemToEquip = itemToEquip;
        this.sourceSlotIndex = sourceSlotIndex;
        this.sourceSlot = sourceSlot;
        this.equipmentSlots = equipmentSlots;
        this.inventorySlots = inventorySlots;
    }

    public override bool CanExecute()
    {
        if (itemToEquip == null || itemToEquip.itemData == null) {
            LogError("No item to equip");
            return false;
        }

        // Find the appropriate equipment slot for this item
        targetEquipmentSlot = FindAppropriateEquipmentSlot();

        if (targetEquipmentSlot == null) {
            Log($"No appropriate equipment slot found for {itemToEquip.itemData.itemName} (Type: {itemToEquip.itemData.itemType})");
            return false;
        }

        // Validate that the equipment slot can accept this item
        if (!targetEquipmentSlot.CanAcceptItem(itemToEquip.itemData)) {
            Log($"Equipment slot {targetEquipmentSlot.SlotType} cannot accept {itemToEquip.itemData.itemName}");
            return false;
        }

        Log($"Quick equip {itemToEquip.itemData.itemName} to {targetEquipmentSlot.SlotType} slot");
        return true;
    }

    public override IEnumerator Execute()
    {
        yield return null; // Wait one frame for visual feedback

        var currentlyEquipped = equipmentController?.GetEquippedItem(targetEquipmentSlot.SlotType);

        if (currentlyEquipped != null) {
            // SWAP: Equipment slot is occupied
            Log($"Swapping: Equipping {itemToEquip.itemData.itemName}, returning {currentlyEquipped.itemData.itemName} to slot {sourceSlotIndex}");

            // Put currently equipped item back in inventory
            sourceSlot.ClearItem();
            sourceSlot.SetItem(currentlyEquipped);
            inventoryController?.RemoveItemAtSlot(sourceSlotIndex);
            inventoryController?.AddItemToSlot(currentlyEquipped, sourceSlotIndex);
        } else {
            // SIMPLE EQUIP: Equipment slot is empty
            Log($"Equipping {itemToEquip.itemData.itemName} from slot {sourceSlotIndex}");

            // Remove from inventory
            inventoryController?.RemoveItemAtSlot(sourceSlotIndex);
        }

        // Equip the new item
        targetEquipmentSlot.ClearItem();
        targetEquipmentSlot.RefreshVisualState();
        equipmentController?.EquipItem(itemToEquip, targetEquipmentSlot.SlotType);
        targetEquipmentSlot.SetItem(itemToEquip);
        targetEquipmentSlot.RefreshVisualState();

        // Visual feedback
        targetEquipmentSlot?.AddToClassList("inventorySlots--drop-target");
        yield return null;
        targetEquipmentSlot?.RemoveFromClassList("inventorySlots--drop-target");
    }

    public override VisualElement GetTargetVisual() => targetEquipmentSlot;

    /// <summary>
    /// **FindAppropriateEquipmentSlot** - Determines which equipment slot this item belongs in
    /// 
    /// Logic:
    /// - Weapons ? Weapon slot
    /// - Armor (Headgear) ? Headgear slot
    /// - Armor (Vest) ? Vest slot
    /// - Armor (Boots) ? Boots slot
    /// </summary>
    private EquipmentSlot FindAppropriateEquipmentSlot()
    {
        if (itemToEquip == null || itemToEquip.itemData == null) return null;

        foreach (var equipSlot in equipmentSlots) {
            if (equipSlot == null) continue;

            // Check if this slot can accept the item
            if (equipSlot.CanAcceptItem(itemToEquip.itemData)) {
                return equipSlot;
            }
        }

        return null;
    }
}
