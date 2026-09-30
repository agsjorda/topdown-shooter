using System.Collections;
using UnityEngine.UIElements;

namespace InventorySystem
{
    /// <summary>
    /// Unequips an item into an inventory slot, swapping when that slot is occupied.
    /// </summary>
    public class EquipmentToInventoryTransaction : DragDropTransaction
    {
        private readonly EquipmentSlotView sourceEquipment;
        private readonly int targetSlotIndex;
        private readonly SlotView targetSlot;

        public EquipmentToInventoryTransaction(
            EquipmentSlotView sourceEquipment,
            int targetSlotIndex,
            SlotView targetSlot,
            IInventory inventory,
            IEquipmentSystem equipmentSystem,
            bool debugMode = false)
            : base(inventory, equipmentSystem, debugMode)
        {
            this.sourceEquipment = sourceEquipment;
            this.targetSlotIndex = targetSlotIndex;
            this.targetSlot = targetSlot;
        }

        public override bool CanExecute()
        {
            if (sourceEquipment == null || targetSlot == null) {
                LogError("Source equipment or target slot is null");
                return false;
            }
            if (inventory == null || !inventory.IsValidSlotIndex(targetSlotIndex)) {
                LogError($"Invalid inventory slot index: {targetSlotIndex}");
                return false;
            }
            if (equipmentSystem == null) {
                LogError("Equipment system is null");
                return false;
            }
            var equippedItem = equipmentSystem.GetEquippedItem(sourceEquipment.SlotType);
            if (equippedItem == null) {
                LogError("No equipped item to unequip");
                return false;
            }
            var inventoryItemInSlot = inventory.GetItemAt(targetSlotIndex);
            if (inventoryItemInSlot != null && !equipmentSystem.CanEquip(inventoryItemInSlot, sourceEquipment.SlotType)) {
                Log($"Cannot swap: {inventoryItemInSlot.itemData.itemName} cannot be equipped in {sourceEquipment.SlotType}");
                return false;
            }
            return true;
        }

        public override IEnumerator Execute()
        {
            yield return null;
            var equippedItem = equipmentSystem.GetEquippedItem(sourceEquipment.SlotType);
            var inventoryItemInSlot = inventory.GetItemAt(targetSlotIndex);
            if (inventoryItemInSlot != null) {
                Log($"Swapping: Unequipping {equippedItem.itemData.itemName}, equipping {inventoryItemInSlot.itemData.itemName}");
                inventory.SetItemAt(targetSlotIndex, equippedItem);
                equipmentSystem.UnequipSlot(sourceEquipment.SlotType);
                equipmentSystem.EquipItem(inventoryItemInSlot, sourceEquipment.SlotType);
            } else {
                Log($"Unequipping {equippedItem.itemData.itemName} to inventory slot {targetSlotIndex}");
                equipmentSystem.UnequipSlot(sourceEquipment.SlotType);
                inventory.SetItemAt(targetSlotIndex, equippedItem);
            }
            targetSlot?.RemoveFromClassList("inventorySlots--drop-target");
        }

        public override VisualElement GetTargetVisual() => targetSlot;
    }
}
