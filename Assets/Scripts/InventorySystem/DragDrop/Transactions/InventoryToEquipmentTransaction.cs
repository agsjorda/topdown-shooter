using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

/// Handles equipping an item from inventory to an equipment slot
public class InventoryToEquipmentTransaction : DragDropTransaction
{
    private readonly Inventory_Item itemToEquip;
    private readonly int sourceSlotIndex;
    private readonly EquipmentSlot targetEquipment;
    private readonly List<Slot> inventorySlots;
    
    public InventoryToEquipmentTransaction(
        Inventory_Item itemToEquip,
        int sourceSlotIndex,
        EquipmentSlot targetEquipment,
        List<Slot> inventorySlots,
        InventoryController inventoryController,
        EquipmentController equipmentController,
        bool debugMode = false)
        : base(inventoryController, equipmentController, debugMode)
    {
        this.itemToEquip = itemToEquip;
        this.sourceSlotIndex = sourceSlotIndex;
        this.targetEquipment = targetEquipment;
        this.inventorySlots = inventorySlots;
    }
    
    public override bool CanExecute()
    {
        if (itemToEquip == null || targetEquipment == null) {
            LogError("Item or target equipment is null");
            return false;
        }
        
        if (!targetEquipment.CanAcceptItem(itemToEquip.itemData)) {
            Log($"Equipment slot {targetEquipment.SlotType} cannot accept {itemToEquip.itemData.itemName}");
            return false;
        }
        
        return true;
    }
    
    public override IEnumerator Execute()
    {
        yield return null;
        
        var currentlyEquipped = equipmentController?.GetEquippedItem(targetEquipment.SlotType);
        
        if (currentlyEquipped != null) {
            // Swap: equipped item goes to inventory, inventory item gets equipped
            Log($"Swapping: Equipping {itemToEquip.itemData.itemName}, returning {currentlyEquipped.itemData.itemName} to slot {sourceSlotIndex}");
            
            var sourceSlot = inventorySlots[sourceSlotIndex];
            sourceSlot.ClearItem();
            sourceSlot.SetItem(currentlyEquipped);
            
            inventoryController?.RemoveItemAtSlot(sourceSlotIndex);
            inventoryController?.AddItemToSlot(currentlyEquipped, sourceSlotIndex);
        } else {
            // Simple equip: remove from inventory
            Log($"Equipping {itemToEquip.itemData.itemName} from slot {sourceSlotIndex}");
            inventoryController?.RemoveItemAtSlot(sourceSlotIndex);
        }
        
        // Equip the new item
        targetEquipment.ClearItem();
        targetEquipment.RefreshVisualState();
        equipmentController?.EquipItem(itemToEquip, targetEquipment.SlotType);
        targetEquipment.SetItem(itemToEquip);
        targetEquipment.RefreshVisualState();
        
        targetEquipment?.RemoveFromClassList("inventorySlots--drop-target");
    }
    
    public override VisualElement GetTargetVisual() => targetEquipment;
}
