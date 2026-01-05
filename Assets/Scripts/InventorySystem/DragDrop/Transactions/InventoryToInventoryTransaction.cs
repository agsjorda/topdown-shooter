using System.Collections;
using UnityEngine.UIElements;
using InventorySystem;

/// <summary>
/// Handles moving or swapping items within inventory slots using the ViewModel.
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
        InventoryViewModel inventoryViewModel,
        IEquipmentSystem equipmentSystem,
        bool debugMode = false)
        : base(inventoryViewModel, equipmentSystem, debugMode)
    {
        this.fromIndex = fromIndex;
        this.toIndex = toIndex;
        this.targetSlot = targetSlot;
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        if (fromIndex == toIndex) {
            Log("Source and target are the same slot");
            return false;
        }
        if (inventoryViewModel == null) {
            LogError("InventoryViewModel is null");
            return false;
        }
        if (!inventoryViewModel.IsValidSlotIndex(fromIndex) || !inventoryViewModel.IsValidSlotIndex(toIndex)) {
            LogError($"Invalid slot index: from {fromIndex}, to {toIndex}");
            return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public override IEnumerator Execute()
    {
        yield return null; // Wait one frame for visual feedback

        var fromItem = inventoryViewModel.GetItemAt(fromIndex);
        var toItem = inventoryViewModel.GetItemAt(toIndex);
        if (fromItem != null && toItem != null) {
            Log($"Inventory swap between slot {fromIndex} and slot {toIndex}");
            inventoryViewModel.SwapItems(fromIndex, toIndex);
        } else {
            Log($"Inventory move from slot {fromIndex} to slot {toIndex} (moving to empty)");
            inventoryViewModel.MoveItem(fromIndex, toIndex);
        }
        // Only update visuals on the slot
        targetSlot?.RemoveFromClassList("inventorySlots--drop-target");
    }

    /// <inheritdoc/>
    public override VisualElement GetTargetVisual() => targetSlot as VisualElement;
}
