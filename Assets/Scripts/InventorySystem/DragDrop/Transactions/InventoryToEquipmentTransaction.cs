using InventorySystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

/// <summary>
/// Handles equipping an inventory item to an equipment slot. Uses ViewModel/Controller for state changes.
/// </summary>
public class InventoryToEquipmentTransaction : DragDropTransaction
{
    private readonly InventoryItem itemToEquip;
    private readonly int sourceSlotIndex;
    private readonly EquipmentSlotView targetEquipment;
    private readonly List<SlotView> inventorySlots;

    /// <summary>
    /// Handles equipping an inventory item to an equipment slot. Uses ViewModel/Controller for state changes.
    /// </summary>
    /// <param name="itemToEquip">The inventory item to equip.</param>
    /// <param name="sourceSlotIndex">The index of the source slot in the inventory.</param>
    /// <param name="targetEquipment">The target equipment slot.</param>
    /// <param name="inventorySlots">The list of inventory slots.</param>
    /// <param name="inventoryViewModel">The inventory view model.</param>
    /// <param name="equipmentSystem">The equipment system.</param>
    /// <param name="debugMode">Whether to enable debug mode.</param>
    public InventoryToEquipmentTransaction(
        InventoryItem itemToEquip,
        int sourceSlotIndex,
        EquipmentSlotView targetEquipment,
        List<SlotView> inventorySlots,
        InventoryViewModel inventoryViewModel,
        IEquipmentSystem equipmentSystem,
        bool debugMode = false)
        : base(inventoryViewModel, equipmentSystem, debugMode)
    {
        this.itemToEquip = itemToEquip;
        this.sourceSlotIndex = sourceSlotIndex;
        this.targetEquipment = targetEquipment;
        this.inventorySlots = inventorySlots;
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        if (itemToEquip == null || targetEquipment == null) {
            LogError("Item or target equipment is null");
            return false;
        }
        if (!inventoryViewModel.IsValidSlotIndex(sourceSlotIndex)) {
            LogError($"Invalid inventory slot index: {sourceSlotIndex}");
            return false;
        }
        if (!targetEquipment.CanAcceptItem(itemToEquip.itemData)) {
            Log($"Equipment slot {targetEquipment.SlotType} cannot accept {itemToEquip.itemData.itemName}");
            return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public override IEnumerator Execute()
    {
        yield return null;
        var currentlyEquipped = equipmentSystem != null ? equipmentSystem.GetEquippedItem(targetEquipment.SlotType) : null;
        if (currentlyEquipped != null) {
            Log($"Swapping: Equipping {itemToEquip.itemData.itemName}, returning {currentlyEquipped.itemData.itemName} to slot {sourceSlotIndex}");
            inventoryViewModel.RemoveItemAt(sourceSlotIndex);
            inventoryViewModel.SetItemAt(sourceSlotIndex, currentlyEquipped);
        } else {
            Log($"Equipping {itemToEquip.itemData.itemName} from slot {sourceSlotIndex}");
            inventoryViewModel.RemoveItemAt(sourceSlotIndex);
        }
        // Equipment state change
        targetEquipment.ClearItem();
        targetEquipment.RefreshVisualState();
        if (equipmentSystem != null) {
            equipmentSystem.EquipItem(itemToEquip, targetEquipment.SlotType);
        }
        targetEquipment.SetItem(itemToEquip);
        targetEquipment.RefreshVisualState();
        targetEquipment?.RemoveFromClassList("inventorySlots--drop-target");
    }

    /// <inheritdoc/>
    public override VisualElement GetTargetVisual() => targetEquipment as VisualElement;
}
