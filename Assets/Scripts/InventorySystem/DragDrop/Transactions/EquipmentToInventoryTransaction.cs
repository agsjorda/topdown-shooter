using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

/// Handles unequipping an item from equipment slot to inventory
public class EquipmentToInventoryTransaction : DragDropTransaction
{
    private readonly EquipmentSlot sourceEquipment;
    private readonly int targetSlotIndex;
    private readonly Slot targetSlot;
    private readonly List<Slot> inventorySlots;
    
    public EquipmentToInventoryTransaction(
        EquipmentSlot sourceEquipment,
        int targetSlotIndex,
        Slot targetSlot,
        List<Slot> inventorySlots,
        InventoryController inventoryController,
        EquipmentController equipmentController,
        bool debugMode = false)
        : base(inventoryController, equipmentController, debugMode)
    {
        this.sourceEquipment = sourceEquipment;
        this.targetSlotIndex = targetSlotIndex;
        this.targetSlot = targetSlot;
        this.inventorySlots = inventorySlots;
    }
    
    public override bool CanExecute()
    {
        if (sourceEquipment == null || targetSlot == null) {
            LogError("Source equipment or target slot is null");
            return false;
        }
        
        var equippedItem = sourceEquipment.GetEquippedItem();
        if (equippedItem == null) {
            LogError("No equipped item to unequip");
            return false;
        }
        
        return true;
    }
    
    public override IEnumerator Execute()
    {
        yield return null;
        
        var equippedItem = sourceEquipment.GetEquippedItem();
        var inventoryItemInSlot = inventoryController?.GetItemAtSlot(targetSlotIndex);
        
        if (inventoryItemInSlot != null) {
            // Swap: unequipped item goes to inventory, inventory item gets equipped
            if (!sourceEquipment.CanAcceptItem(inventoryItemInSlot.itemData)) {
                Log($"Cannot swap: {inventoryItemInSlot.itemData.itemName} cannot be equipped in {sourceEquipment.SlotType}");
                targetSlot?.RemoveFromClassList("inventorySlots--drop-target");
                yield break;
            }
            
            Log($"Swapping: Unequipping {equippedItem.itemData.itemName}, equipping {inventoryItemInSlot.itemData.itemName}");
            
            // Update inventory slot
            var targetInventorySlot = inventorySlots[targetSlotIndex];
            targetInventorySlot.ClearItem();
            targetInventorySlot.SetItem(equippedItem);
            inventoryController?.RemoveItemAtSlot(targetSlotIndex);
            inventoryController?.AddItemToSlot(equippedItem, targetSlotIndex);
            
            // Update equipment slot
            sourceEquipment.ClearItem();
            sourceEquipment.RefreshVisualState();
            equipmentController?.UnequipSlot(sourceEquipment.SlotType);
            equipmentController?.EquipItem(inventoryItemInSlot, sourceEquipment.SlotType);
            sourceEquipment.SetItem(inventoryItemInSlot);
            sourceEquipment.RefreshVisualState();
        } else {
            // Simple unequip: add to inventory
            Log($"Unequipping {equippedItem.itemData.itemName} to inventory slot {targetSlotIndex}");
            
            // Clear equipment slot
            sourceEquipment.ClearItem();
            sourceEquipment.RefreshVisualState();
            equipmentController?.UnequipSlot(sourceEquipment.SlotType);
            
            // Add to inventory
            if (targetSlotIndex >= 0 && targetSlotIndex < inventorySlots.Count) {
                inventorySlots[targetSlotIndex]?.SetItem(equippedItem);
            }
            inventoryController?.AddItemToSlot(equippedItem, targetSlotIndex);
        }
        
        targetSlot?.RemoveFromClassList("inventorySlots--drop-target");
    }
    
    public override VisualElement GetTargetVisual() => targetSlot;
}
