using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace InventorySystem
{
    /// <summary>
    /// Double-click unequip into the first empty inventory slot.
    /// </summary>
    public class QuickUnequipTransaction : DragDropTransaction
    {
        private readonly EquipmentSlotView sourceEquipmentSlot;
        private readonly InventoryItem equippedItem;
        private readonly List<SlotView> inventorySlots;
        private int targetInventoryIndex = -1;
        private SlotView targetInventorySlot;

        public QuickUnequipTransaction(
            EquipmentSlotView sourceEquipmentSlot,
            InventoryItem equippedItem,
            List<SlotView> inventorySlots,
            IInventory inventory,
            IEquipmentSystem equipmentSystem,
            bool debugMode = false)
            : base(inventory, equipmentSystem, debugMode)
        {
            this.sourceEquipmentSlot = sourceEquipmentSlot;
            this.equippedItem = equippedItem;
            this.inventorySlots = inventorySlots;
        }

        public override bool CanExecute()
        {
            if (equippedItem == null || equippedItem.itemData == null) {
                LogError("No item to unequip");
                return false;
            }
            if (sourceEquipmentSlot == null) {
                LogError("No source equipment slot");
                return false;
            }
            if (inventory == null) {
                LogError("Inventory is null");
                return false;
            }

            targetInventoryIndex = -1;
            for (int i = 0; i < inventory.MaxInventorySize; i++) {
                if (inventory.GetItemAt(i) == null) {
                    targetInventoryIndex = i;
                    break;
                }
            }
            if (targetInventoryIndex < 0) {
                Log("No empty inventory slot available for unequipping");
                return false;
            }
            targetInventorySlot = targetInventoryIndex < inventorySlots.Count
                ? inventorySlots[targetInventoryIndex]
                : null;
            Log($"Quick unequip {equippedItem.itemData.itemName} from {sourceEquipmentSlot.SlotType} to slot {targetInventoryIndex}");
            return true;
        }

        public override IEnumerator Execute()
        {
            yield return null;
            Log($"Unequipping {equippedItem.itemData.itemName} from {sourceEquipmentSlot.SlotType} to inventory slot {targetInventoryIndex}");
            equipmentSystem?.UnequipSlot(sourceEquipmentSlot.SlotType);
            inventory.SetItemAt(targetInventoryIndex, equippedItem);
            if (targetInventorySlot != null) {
                targetInventorySlot.AddToClassList("inventorySlots--drop-target");
                yield return null;
                targetInventorySlot.RemoveFromClassList("inventorySlots--drop-target");
            }
        }

        public override VisualElement GetTargetVisual() => targetInventorySlot;
    }
}
