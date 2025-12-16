using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using InventorySystem;

namespace InventorySystem.DragDrop.Transactions
{
    /// <summary>
    /// Handles quick-unequip (double-click) from equipment to inventory. Uses ViewModel/Controller for state changes.
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
            InventoryViewModel inventoryViewModel,
            EquipmentController equipmentController,
            bool debugMode = false)
            : base(inventoryViewModel, equipmentController, debugMode)
        {
            this.sourceEquipmentSlot = sourceEquipmentSlot;
            this.equippedItem = equippedItem;
            this.inventorySlots = inventorySlots;
        }

        /// <inheritdoc/>
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
            // Find first empty slot in ViewModel
            targetInventoryIndex = -1;
            for (int i = 0; i < inventoryViewModel.MaxInventorySize; i++) {
                if (inventoryViewModel.GetItemAt(i) == null) {
                    targetInventoryIndex = i;
                    break;
                }
            }
            if (targetInventoryIndex < 0) {
                Log("No empty inventory slot available for unequipping");
                return false;
            }
            if (targetInventoryIndex < inventorySlots.Count)
                targetInventorySlot = inventorySlots[targetInventoryIndex];
            else
                targetInventorySlot = null;
            Log($"Quick unequip {equippedItem.itemData.itemName} from {sourceEquipmentSlot.SlotType} to slot {targetInventoryIndex}");
            return true;
        }

        /// <inheritdoc/>
        public override IEnumerator Execute()
        {
            yield return null;
            Log($"Unequipping {equippedItem.itemData.itemName} from {sourceEquipmentSlot.SlotType} to inventory slot {targetInventoryIndex}");
            sourceEquipmentSlot.ClearItem();
            sourceEquipmentSlot.RefreshVisualState();
            equipmentController?.UnequipSlot(sourceEquipmentSlot.SlotType);
            inventoryViewModel.SetItemAt(targetInventoryIndex, equippedItem);
            if (targetInventorySlot != null) {
                targetInventorySlot.SetItem(equippedItem);
                targetInventorySlot.AddToClassList("inventorySlots--drop-target");
                yield return null;
                targetInventorySlot.RemoveFromClassList("inventorySlots--drop-target");
            }
        }

        /// <inheritdoc/>
        public override VisualElement GetTargetVisual() => targetInventorySlot as VisualElement;
    }
}
