using System.Collections;
using UnityEngine.UIElements;

namespace InventorySystem
{
    public class EquipmentToEquipmentTransaction : DragDropTransaction
    {
        private readonly EquipmentSlotView sourceEquipment;
        private readonly EquipmentSlotView targetEquipment;

        public EquipmentToEquipmentTransaction(
            EquipmentSlotView sourceEquipment,
            EquipmentSlotView targetEquipment,
            IInventory inventory,
            IEquipmentSystem equipmentSystem,
            bool debugMode = false)
            : base(inventory, equipmentSystem, debugMode)
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
            if (equipmentSystem == null) {
                LogError("Equipment system is null");
                return false;
            }

            var itemToMove = equipmentSystem.GetEquippedItem(sourceEquipment.SlotType);
            if (itemToMove == null) {
                LogError("No item to move from source equipment");
                return false;
            }
            if (!equipmentSystem.CanEquip(itemToMove, targetEquipment.SlotType)) {
                Log($"Target equipment slot {targetEquipment.SlotType} cannot accept {itemToMove.itemData.itemName}");
                return false;
            }

            var currentlyEquipped = equipmentSystem.GetEquippedItem(targetEquipment.SlotType);
            if (currentlyEquipped != null && !equipmentSystem.CanEquip(currentlyEquipped, sourceEquipment.SlotType)) {
                Log($"Cannot swap: {currentlyEquipped.itemData.itemName} cannot be equipped in {sourceEquipment.SlotType}");
                return false;
            }
            return true;
        }

        public override IEnumerator Execute()
        {
            yield return null;

            var itemToMove = equipmentSystem.GetEquippedItem(sourceEquipment.SlotType);
            var currentlyEquipped = equipmentSystem.GetEquippedItem(targetEquipment.SlotType);

            if (currentlyEquipped != null) {
                Log($"Swapping {itemToMove.itemData.itemName} with {currentlyEquipped.itemData.itemName} between equipment slots");
                equipmentSystem.UnequipSlot(sourceEquipment.SlotType);
                equipmentSystem.UnequipSlot(targetEquipment.SlotType);
                equipmentSystem.EquipItem(itemToMove, targetEquipment.SlotType);
                equipmentSystem.EquipItem(currentlyEquipped, sourceEquipment.SlotType);
            } else {
                Log($"Moving {itemToMove.itemData.itemName} from {sourceEquipment.SlotType} to {targetEquipment.SlotType}");
                equipmentSystem.UnequipSlot(sourceEquipment.SlotType);
                equipmentSystem.EquipItem(itemToMove, targetEquipment.SlotType);
            }

            targetEquipment?.RemoveFromClassList("inventorySlots--drop-target");
        }

        public override VisualElement GetTargetVisual() => targetEquipment;
    }
}
