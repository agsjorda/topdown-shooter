using System.Collections.Generic;
using UnityEngine;

/// Factory for creating the appropriate DragDropTransaction based on drag/drop context.
/// This centralizes transaction creation logic and makes it easy to add new transaction types.
public static class TransactionFactory
{
    /// Creates the appropriate transaction based on drag source and drop target
    public static DragDropTransaction CreateTransaction(
        DragState dragState,
        Slot targetInventorySlot,
        EquipmentSlot targetEquipmentSlot,
        List<Slot> inventorySlots,
        InventoryController inventoryController,
        EquipmentController equipmentController,
        bool debugMode = false)
    {
        bool fromInventory = dragState.IsFromInventory;
        bool fromEquipment = dragState.IsFromEquipment;
        bool toInventory = targetInventorySlot != null;
        bool toEquipment = targetEquipmentSlot != null;
        
        if (fromInventory && toInventory)
        {
            int fromIndex = dragState.SourceInventorySlot.SlotIndex;
            int toIndex = inventorySlots.IndexOf(targetInventorySlot);
            
            return new InventoryToInventoryTransaction(
                fromIndex,
                toIndex,
                targetInventorySlot,
                inventoryController,
                equipmentController,
                debugMode
            );
        }
        
        if (fromInventory && toEquipment)
        {
            var item = inventoryController?.GetItemAtSlot(dragState.SourceInventorySlot.SlotIndex);
            int sourceIndex = dragState.SourceInventorySlot.SlotIndex;
            
            return new InventoryToEquipmentTransaction(
                item,
                sourceIndex,
                targetEquipmentSlot,
                inventorySlots,
                inventoryController,
                equipmentController,
                debugMode
            );
        }
        
        if (fromEquipment && toInventory)
        {
            int targetIndex = inventorySlots.IndexOf(targetInventorySlot);
            
            return new EquipmentToInventoryTransaction(
                dragState.SourceEquipmentSlot,
                targetIndex,
                targetInventorySlot,
                inventorySlots,
                inventoryController,
                equipmentController,
                debugMode
            );
        }
        
        if (fromEquipment && toEquipment)
        {
            return new EquipmentToEquipmentTransaction(
                dragState.SourceEquipmentSlot,
                targetEquipmentSlot,
                inventoryController,
                equipmentController,
                debugMode
            );
        }
        
        if (debugMode)
        {
            Debug.LogWarning($"[TransactionFactory] Cannot create transaction: fromInventory={fromInventory}, fromEquipment={fromEquipment}, toInventory={toInventory}, toEquipment={toEquipment}");
        }
        
        return null;
    }
}
