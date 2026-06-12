using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    /// <summary>
    /// Factory for creating the appropriate DragDropTransaction based on drag/drop context.
    /// This centralizes transaction creation logic and makes it easy to add new transaction types.
    /// </summary>
    public static class TransactionFactory
    {
        /// <summary>
        /// Creates the appropriate transaction based on drag source and drop target.
        /// </summary>
        /// <param name="dragState">The current drag state</param>
        /// <param name="targetInventorySlot">Target inventory slot (if dropping to inventory)</param>
        /// <param name="targetEquipmentSlot">Target equipment slot (if dropping to equipment)</param>
        /// <param name="inventorySlots">List of all inventory slots</param>
        /// <param name="inventoryViewModel">The inventory view model</param>
        /// <param name="equipmentSystem">The equipment system interface</param>
        /// <param name="debugMode">Enable debug logging</param>
        /// <returns>The appropriate transaction or null if invalid</returns>
        public static DragDropTransaction CreateTransaction(
            DragState dragState,
            SlotView targetInventorySlot,
            EquipmentSlotView targetEquipmentSlot,
            List<SlotView> inventorySlots,
            InventoryViewModel inventoryViewModel,
            IEquipmentSystem equipmentSystem,
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
                    equipmentSystem,
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
                    equipmentSystem,
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
                    equipmentSystem,
                    debugMode
                );
            }

            if (fromEquipment && toEquipment) {
                return new EquipmentToEquipmentTransaction(
                    dragState.SourceEquipmentSlot,
                    targetEquipmentSlot,
                    inventoryViewModel,
                    equipmentSystem,
                    debugMode
                );
            }

            if (debugMode) {
                Debug.LogWarning($"[TransactionFactory] Cannot create transaction: fromInventory={fromInventory}, fromEquipment={fromEquipment}, toInventory={toInventory}, toEquipment={toEquipment}");
            }

            return null;
        }
    }
}
