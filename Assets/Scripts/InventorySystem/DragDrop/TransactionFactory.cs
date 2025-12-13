using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Factory for creating the appropriate DragDropTransaction based on drag/drop context.
/// This centralizes transaction creation logic and makes it easy to add new transaction types.
/// </summary>
public static class TransactionFactory
{
    /// <summary>
    /// Creates the appropriate transaction based on drag source and drop target
    /// </summary>
    public static DragDropTransaction CreateTransaction(
        DragState dragState,
        Slot targetInventorySlot,
        EquipmentSlot targetEquipmentSlot,
        List<Slot> inventorySlots,
        InventoryController inventoryController,
        EquipmentController equipmentController,
        bool debugMode = false)
    {
        // Determine transaction type based on source and target
        bool fromInventory = dragState.IsFromInventory;
        bool fromEquipment = dragState.IsFromEquipment;
        bool toInventory = targetInventorySlot != null;
        bool toEquipment = targetEquipmentSlot != null;
        
        // Inventory to Inventory (move/swap)
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
        
        // Inventory to Equipment (equip)
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
        
        // Equipment to Inventory (unequip)
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
        
        // Equipment to Equipment (move/swap)
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
        
        // Invalid transaction
        if (debugMode)
        {
            Debug.LogWarning($"[TransactionFactory] Cannot create transaction: fromInventory={fromInventory}, fromEquipment={fromEquipment}, toInventory={toInventory}, toEquipment={toEquipment}");
        }
        
        return null;
    }
}
