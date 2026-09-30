using System.Collections;
using UnityEngine.UIElements;

namespace InventorySystem
{
    /// <summary>
    /// Equips an inventory item into an equipment slot. View updates come from OnEquipmentChanged.
    /// </summary>
    public class InventoryToEquipmentTransaction : DragDropTransaction
    {
        private readonly InventoryItem itemToEquip;
        private readonly int sourceSlotIndex;
        private readonly EquipmentSlotView targetEquipment;

        public InventoryToEquipmentTransaction(
            InventoryItem itemToEquip,
            int sourceSlotIndex,
            EquipmentSlotView targetEquipment,
            IInventory inventory,
            IEquipmentSystem equipmentSystem,
            bool debugMode = false)
            : base(inventory, equipmentSystem, debugMode)
        {
            this.itemToEquip = itemToEquip;
            this.sourceSlotIndex = sourceSlotIndex;
            this.targetEquipment = targetEquipment;
        }

        public override bool CanExecute()
        {
            if (itemToEquip == null || targetEquipment == null) {
                LogError("Item or target equipment is null");
                return false;
            }
            if (inventory == null || !inventory.IsValidSlotIndex(sourceSlotIndex)) {
                LogError($"Invalid inventory slot index: {sourceSlotIndex}");
                return false;
            }
            if (equipmentSystem == null || !equipmentSystem.CanEquip(itemToEquip, targetEquipment.SlotType)) {
                Log($"Equipment slot {targetEquipment.SlotType} cannot accept {itemToEquip.itemData.itemName}");
                return false;
            }
            return true;
        }

        public override IEnumerator Execute()
        {
            yield return null;
            var currentlyEquipped = equipmentSystem.GetEquippedItem(targetEquipment.SlotType);
            if (currentlyEquipped != null) {
                Log($"Swapping: Equipping {itemToEquip.itemData.itemName}, returning {currentlyEquipped.itemData.itemName} to slot {sourceSlotIndex}");
                inventory.RemoveItemAt(sourceSlotIndex);
                inventory.SetItemAt(sourceSlotIndex, currentlyEquipped);
            } else {
                Log($"Equipping {itemToEquip.itemData.itemName} from slot {sourceSlotIndex}");
                inventory.RemoveItemAt(sourceSlotIndex);
            }
            equipmentSystem.EquipItem(itemToEquip, targetEquipment.SlotType);
            targetEquipment.RemoveFromClassList("inventorySlots--drop-target");
        }

        public override VisualElement GetTargetVisual() => targetEquipment;
    }
}
