using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace InventorySystem
{
    /// <summary>
    /// Handles unequipping an item from equipment slot to inventory. Uses ViewModel/Controller for state changes.
    /// </summary>
    public class EquipmentToInventoryTransaction : DragDropTransaction
    {
        private readonly EquipmentSlotView sourceEquipment;
        private readonly int targetSlotIndex;
        private readonly SlotView targetSlot;
        private readonly List<SlotView> inventorySlots;

        public EquipmentToInventoryTransaction(
            EquipmentSlotView sourceEquipment,
            int targetSlotIndex,
            SlotView targetSlot,
            List<SlotView> inventorySlots,
            InventoryViewModel inventoryViewModel,
            IEquipmentSystem equipmentSystem,
            bool debugMode = false)
            : base(inventoryViewModel, equipmentSystem, debugMode)
        {
            this.sourceEquipment = sourceEquipment;
            this.targetSlotIndex = targetSlotIndex;
            this.targetSlot = targetSlot;
            this.inventorySlots = inventorySlots;
        }

        /// <inheritdoc/>
        public override bool CanExecute()
        {
            if (sourceEquipment == null || targetSlot == null) {
                LogError("Source equipment or target slot is null");
                return false;
            }
            if (!inventoryViewModel.IsValidSlotIndex(targetSlotIndex)) {
                LogError($"Invalid inventory slot index: {targetSlotIndex}");
                return false;
            }
            var equippedItem = sourceEquipment.GetEquippedItem();
            if (equippedItem == null) {
                LogError("No equipped item to unequip");
                return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public override IEnumerator Execute()
        {
            yield return null;
            var equippedItem = sourceEquipment.GetEquippedItem();
            var inventoryItemInSlot = inventoryViewModel.GetItemAt(targetSlotIndex);
            if (inventoryItemInSlot != null) {
                // Swap: unequipped item goes to inventory, inventory item gets equipped
                if (!sourceEquipment.CanAcceptItem(inventoryItemInSlot.itemData)) {
                    Log($"Cannot swap: {inventoryItemInSlot.itemData.itemName} cannot be equipped in {sourceEquipment.SlotType}");
                    targetSlot?.RemoveFromClassList("inventorySlots--drop-target");
                    yield break;
                }
                Log($"Swapping: Unequipping {equippedItem.itemData.itemName}, equipping {inventoryItemInSlot.itemData.itemName}");
                inventoryViewModel.SetItemAt(targetSlotIndex, equippedItem);
                sourceEquipment.ClearItem();
                sourceEquipment.RefreshVisualState();
                equipmentSystem?.UnequipSlot(sourceEquipment.SlotType);
                equipmentSystem?.EquipItem(inventoryItemInSlot, sourceEquipment.SlotType);
                sourceEquipment.SetItem(inventoryItemInSlot);
                sourceEquipment.RefreshVisualState();
            } else {
                // Simple unequip: add to inventory
                Log($"Unequipping {equippedItem.itemData.itemName} to inventory slot {targetSlotIndex}");
                sourceEquipment.ClearItem();
                sourceEquipment.RefreshVisualState();
                equipmentSystem?.UnequipSlot(sourceEquipment.SlotType);
                inventoryViewModel.SetItemAt(targetSlotIndex, equippedItem);
            }
            targetSlot?.RemoveFromClassList("inventorySlots--drop-target");
        }

        /// <inheritdoc/>
        public override VisualElement GetTargetVisual() => targetSlot as VisualElement;
    }
}
