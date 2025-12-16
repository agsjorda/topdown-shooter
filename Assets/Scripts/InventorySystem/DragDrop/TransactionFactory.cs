using InventorySystem;
using System.Collections.Generic;
using UnityEngine;

/// Factory for creating the appropriate DragDropTransaction based on drag/drop context.
/// This centralizes transaction creation logic and makes it easy to add new transaction types.
public static class TransactionFactory
{
    /// Creates the appropriate transaction based on drag source and drop target
    public static DragDropTransaction CreateTransaction(
        DragState dragState,
        SlotView targetInventorySlot,
        EquipmentSlotView targetEquipmentSlot,
        List<SlotView> inventorySlots,
        InventoryViewModel inventoryViewModel,
        EquipmentController equipmentController,
        bool debugMode = false)
    {
        bool fromInventory = dragState.IsFromInventory;
        bool fromEquipment = dragState.IsFromEquipment;
        bool toInventory = targetInventorySlot != null;
        bool toEquipment = targetEquipmentSlot != null;

        if (fromInventory && toInventory) {
            int fromIndex = dragState.SourceInventorySlot.SlotIndex;
            int toIndex = inventorySlots.IndexOf(targetInventorySlot);

            return new InventoryToInventoryTransaction(
                fromIndex,
                toIndex,
                targetInventorySlot,
                inventoryViewModel,
                equipmentController,
                debugMode
            );
        }

        if (fromInventory && toEquipment) {
            var item = inventoryViewModel?.GetItemAt(dragState.SourceInventorySlot.SlotIndex);
            int sourceIndex = dragState.SourceInventorySlot.SlotIndex;

            return new InventoryToEquipmentTransaction(
                item,
                sourceIndex,
                targetEquipmentSlot,
                inventorySlots,
                inventoryViewModel,
                equipmentController,
                debugMode
            );
        }

        if (fromEquipment && toInventory) {
            int targetIndex = inventorySlots.IndexOf(targetInventorySlot);

            return new EquipmentToInventoryTransaction(
                dragState.SourceEquipmentSlot,
                targetIndex,
                targetInventorySlot,
                inventorySlots,
                inventoryViewModel,
                equipmentController,
                debugMode
            );
        }

        if (fromEquipment && toEquipment) {
            return new EquipmentToEquipmentTransaction(
                dragState.SourceEquipmentSlot,
                targetEquipmentSlot,
                inventoryViewModel,
                equipmentController,
                debugMode
            );
        }

        if (debugMode) {
            Debug.LogWarning($"[TransactionFactory] Cannot create transaction: fromInventory={fromInventory}, fromEquipment={fromEquipment}, toInventory={toInventory}, toEquipment={toEquipment}");
        }

        return null;
    }
}
