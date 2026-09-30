using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace InventorySystem
{
    /// <summary>
    /// Double-click equip. Picks the first equipment slot the item is allowed in.
    /// </summary>
    public class QuickEquipTransaction : DragDropTransaction
    {
        private readonly InventoryItem itemToEquip;
        private readonly int sourceSlotIndex;
        private readonly List<EquipmentSlotView> equipmentSlots;
        private EquipmentSlotView targetEquipmentSlot;

        public QuickEquipTransaction(
            InventoryItem itemToEquip,
            int sourceSlotIndex,
            List<EquipmentSlotView> equipmentSlots,
            IInventory inventory,
            IEquipmentSystem equipmentSystem,
            bool debugMode = false)
            : base(inventory, equipmentSystem, debugMode)
        {
            this.itemToEquip = itemToEquip;
            this.sourceSlotIndex = sourceSlotIndex;
            this.equipmentSlots = equipmentSlots;
        }

        public override bool CanExecute()
        {
            if (itemToEquip == null || itemToEquip.itemData == null) {
                LogError("No item to equip");
                return false;
            }
            if (inventory == null || !inventory.IsValidSlotIndex(sourceSlotIndex)) {
                LogError($"Invalid inventory slot index: {sourceSlotIndex}");
                return false;
            }
            if (equipmentSystem == null) {
                LogError("Equipment system is null");
                return false;
            }
            targetEquipmentSlot = FindAppropriateEquipmentSlot();
            if (targetEquipmentSlot == null) {
                Log($"No appropriate equipment slot found for {itemToEquip.itemData.itemName} (Category: {itemToEquip.itemData.category?.id})");
                return false;
            }
            Log($"Quick equip {itemToEquip.itemData.itemName} to {targetEquipmentSlot.SlotType} slot");
            return true;
        }

        public override IEnumerator Execute()
        {
            yield return null;
            var currentlyEquipped = equipmentSystem.GetEquippedItem(targetEquipmentSlot.SlotType);
            if (currentlyEquipped != null) {
                Log($"Swapping: Equipping {itemToEquip.itemData.itemName}, returning {currentlyEquipped.itemData.itemName} to slot {sourceSlotIndex}");
                inventory.RemoveItemAt(sourceSlotIndex);
                inventory.SetItemAt(sourceSlotIndex, currentlyEquipped);
            } else {
                Log($"Equipping {itemToEquip.itemData.itemName} from slot {sourceSlotIndex}");
                inventory.RemoveItemAt(sourceSlotIndex);
            }
            equipmentSystem.EquipItem(itemToEquip, targetEquipmentSlot.SlotType);
            targetEquipmentSlot.AddToClassList("inventorySlots--drop-target");
            yield return null;
            targetEquipmentSlot.RemoveFromClassList("inventorySlots--drop-target");
        }

        public override VisualElement GetTargetVisual() => targetEquipmentSlot;

        private EquipmentSlotView FindAppropriateEquipmentSlot()
        {
            if (equipmentSlots == null) return null;
            for (int i = 0; i < equipmentSlots.Count; i++) {
                var equipSlot = equipmentSlots[i];
                if (equipSlot != null && equipmentSystem.CanEquip(itemToEquip, equipSlot.SlotType)) {
                    return equipSlot;
                }
            }
            return null;
        }
    }
}
