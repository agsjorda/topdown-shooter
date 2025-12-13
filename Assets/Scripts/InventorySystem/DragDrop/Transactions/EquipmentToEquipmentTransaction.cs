using System.Collections;
using UnityEngine.UIElements;

/// Handles moving/swapping items between equipment slots
public class EquipmentToEquipmentTransaction : DragDropTransaction
{
    private readonly EquipmentSlot sourceEquipment;
    private readonly EquipmentSlot targetEquipment;
    
    public EquipmentToEquipmentTransaction(
        EquipmentSlot sourceEquipment,
        EquipmentSlot targetEquipment,
        InventoryController inventoryController,
        EquipmentController equipmentController,
        bool debugMode = false)
        : base(inventoryController, equipmentController, debugMode)
    {
        this.sourceEquipment = sourceEquipment;
        this.targetEquipment = targetEquipment;
    }
    
    public override bool CanExecute()
    {
        if (sourceEquipment == null || targetEquipment == null) {
            LogError("Source or target equipment is null");
            return false;
        }
        
        if (sourceEquipment == targetEquipment) {
            Log("Cannot drop on same equipment slot");
            return false;
        }
        
        var itemToMove = sourceEquipment.GetEquippedItem();
        if (itemToMove == null) {
            LogError("No item to move from source equipment");
            return false;
        }
        
        if (!targetEquipment.CanAcceptItem(itemToMove.itemData)) {
            Log($"Target equipment slot {targetEquipment.SlotType} cannot accept {itemToMove.itemData.itemName}");
            return false;
        }
        
        return true;
    }
    
    public override IEnumerator Execute()
    {
        yield return null;
        
        var itemToMove = sourceEquipment.GetEquippedItem();
        var currentlyEquipped = equipmentController?.GetEquippedItem(targetEquipment.SlotType);
        
        if (currentlyEquipped != null) {
            Log($"Swapping {itemToMove.itemData.itemName} with {currentlyEquipped.itemData.itemName} between equipment slots");
            
            sourceEquipment.ClearItem();
            sourceEquipment.RefreshVisualState();
            targetEquipment.ClearItem();
            targetEquipment.RefreshVisualState();
            
            equipmentController?.UnequipSlot(sourceEquipment.SlotType);
            equipmentController?.UnequipSlot(targetEquipment.SlotType);
            
            equipmentController?.EquipItem(itemToMove, targetEquipment.SlotType);
            targetEquipment.SetItem(itemToMove);
            targetEquipment.RefreshVisualState();
            
            equipmentController?.EquipItem(currentlyEquipped, sourceEquipment.SlotType);
            sourceEquipment.SetItem(currentlyEquipped);
            sourceEquipment.RefreshVisualState();
        } else {
            Log($"Moving {itemToMove.itemData.itemName} from {sourceEquipment.SlotType} to {targetEquipment.SlotType}");
            
            sourceEquipment.ClearItem();
            sourceEquipment.RefreshVisualState();
            equipmentController?.UnequipSlot(sourceEquipment.SlotType);
            
            targetEquipment.ClearItem();
            targetEquipment.RefreshVisualState();
            
            equipmentController?.EquipItem(itemToMove, targetEquipment.SlotType);
            targetEquipment.SetItem(itemToMove);
            targetEquipment.RefreshVisualState();
        }
        
        targetEquipment?.RemoveFromClassList("inventorySlots--drop-target");
    }
    
    public override VisualElement GetTargetVisual() => targetEquipment;
}
