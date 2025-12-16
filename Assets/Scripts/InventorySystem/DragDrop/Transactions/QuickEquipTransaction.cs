using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using InventorySystem;

/// <summary>
/// Handles quick-equip (double-click) from inventory to equipment. Uses ViewModel/Controller for state changes.
/// </summary>
public class QuickEquipTransaction : DragDropTransaction
{
    private readonly InventoryItem itemToEquip;
    private readonly int sourceSlotIndex;
    private readonly SlotView sourceSlot;
    private readonly List<EquipmentSlotView> equipmentSlots;
    private readonly List<SlotView> inventorySlots;
    private EquipmentSlotView targetEquipmentSlot;

    /// <summary>
    /// Handles quick-equip (double-click) from inventory to equipment. Uses ViewModel/Controller for state changes.
    /// </summary>
    /// <param name="itemToEquip"></param>
    /// <param name="sourceSlotIndex"></param>
    /// <param name="sourceSlot"></param>
    /// <param name="equipmentSlots"></param>
    /// <param name="inventorySlots"></param>
    /// <param name="inventoryViewModel"></param>
    /// <param name="equipmentController"></param>
    /// <param name="debugMode"></param>
    public QuickEquipTransaction(
        InventoryItem itemToEquip,
        int sourceSlotIndex,
        SlotView sourceSlot,
        List<EquipmentSlotView> equipmentSlots,
        List<SlotView> inventorySlots,
        InventoryViewModel inventoryViewModel,
        EquipmentController equipmentController,
        bool debugMode = false)
        : base(inventoryViewModel, equipmentController, debugMode)
    {
        this.itemToEquip = itemToEquip;
        this.sourceSlotIndex = sourceSlotIndex;
        this.sourceSlot = sourceSlot;
        this.equipmentSlots = equipmentSlots;
        this.inventorySlots = inventorySlots;
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        if (itemToEquip == null || itemToEquip.itemData == null) {
            LogError("No item to equip");
            return false;
        }
        if (!inventoryViewModel.IsValidSlotIndex(sourceSlotIndex)) {
            LogError($"Invalid inventory slot index: {sourceSlotIndex}");
            return false;
        }
        targetEquipmentSlot = FindAppropriateEquipmentSlot();
        if (targetEquipmentSlot == null) {
            Log($"No appropriate equipment slot found for {itemToEquip.itemData.itemName} (Type: {itemToEquip.itemData.itemType})");
            return false;
        }
        if (!targetEquipmentSlot.CanAcceptItem(itemToEquip.itemData)) {
            Log($"Equipment slot {targetEquipmentSlot.SlotType} cannot accept {itemToEquip.itemData.itemName}");
            return false;
        }
        Log($"Quick equip {itemToEquip.itemData.itemName} to {targetEquipmentSlot.SlotType} slot");
        return true;
    }

    /// <inheritdoc/>
    public override IEnumerator Execute()
    {
        yield return null;
        var currentlyEquipped = equipmentController?.GetEquippedItem(targetEquipmentSlot.SlotType);
        if (currentlyEquipped != null) {
            Log($"Swapping: Equipping {itemToEquip.itemData.itemName}, returning {currentlyEquipped.itemData.itemName} to slot {sourceSlotIndex}");
            inventoryViewModel.RemoveItemAt(sourceSlotIndex);
            inventoryViewModel.SetItemAt(sourceSlotIndex, currentlyEquipped);
        } else {
            Log($"Equipping {itemToEquip.itemData.itemName} from slot {sourceSlotIndex}");
            inventoryViewModel.RemoveItemAt(sourceSlotIndex);
        }
        targetEquipmentSlot.ClearItem();
        targetEquipmentSlot.RefreshVisualState();
        equipmentController?.EquipItem(itemToEquip, targetEquipmentSlot.SlotType);
        targetEquipmentSlot.SetItem(itemToEquip);
        targetEquipmentSlot.RefreshVisualState();
        targetEquipmentSlot?.AddToClassList("inventorySlots--drop-target");
        yield return null;
        targetEquipmentSlot?.RemoveFromClassList("inventorySlots--drop-target");
    }

    /// <inheritdoc/>
    public override VisualElement GetTargetVisual() => targetEquipmentSlot as VisualElement;

    private EquipmentSlotView FindAppropriateEquipmentSlot()
    {
        if (itemToEquip == null || itemToEquip.itemData == null) return null;
        foreach (var equipSlot in equipmentSlots) {
            if (equipSlot == null) continue;
            if (equipSlot.CanAcceptItem(itemToEquip.itemData)) {
                return equipSlot;
            }
        }
        return null;
    }
}
