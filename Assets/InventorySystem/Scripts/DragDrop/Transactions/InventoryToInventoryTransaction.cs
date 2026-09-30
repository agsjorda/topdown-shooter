using System.Collections;
using UnityEngine.UIElements;

namespace InventorySystem
{
    /// <summary>
    /// Handles moving or swapping items within inventory slots.
    /// </summary>
    public class InventoryToInventoryTransaction : DragDropTransaction
    {
        private readonly int fromIndex;
        private readonly int toIndex;
        private readonly SlotView targetSlot;

        public InventoryToInventoryTransaction(
            int fromIndex,
            int toIndex,
            SlotView targetSlot,
            IInventory inventory,
            IEquipmentSystem equipmentSystem,
            bool debugMode = false)
            : base(inventory, equipmentSystem, debugMode)
        {
            this.fromIndex = fromIndex;
            this.toIndex = toIndex;
            this.targetSlot = targetSlot;
        }

        public override bool CanExecute()
        {
            if (fromIndex == toIndex) {
                Log("Source and target are the same slot");
                return false;
            }
            if (inventory == null) {
                LogError("Inventory is null");
                return false;
            }
            if (!inventory.IsValidSlotIndex(fromIndex) || !inventory.IsValidSlotIndex(toIndex)) {
                LogError($"Invalid slot index: from {fromIndex}, to {toIndex}");
                return false;
            }
            return true;
        }

        public override IEnumerator Execute()
        {
            yield return null;

            var fromItem = inventory.GetItemAt(fromIndex);
            var toItem = inventory.GetItemAt(toIndex);
            if (fromItem != null && toItem != null) {
                Log($"Inventory swap between slot {fromIndex} and slot {toIndex}");
                inventory.SwapItems(fromIndex, toIndex);
            } else {
                Log($"Inventory move from slot {fromIndex} to slot {toIndex} (moving to empty)");
                inventory.MoveItem(fromIndex, toIndex);
            }
            targetSlot?.RemoveFromClassList("inventorySlots--drop-target");
        }

        public override VisualElement GetTargetVisual() => targetSlot;
    }
}
