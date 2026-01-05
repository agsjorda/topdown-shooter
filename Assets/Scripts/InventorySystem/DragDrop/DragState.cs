using UnityEngine;

/// Encapsulates the state of a drag operation.
/// This makes drag state management cleaner and easier to reason about.
public class DragState
{
    public bool IsDragging { get; set; }
    public Vector2 StartPosition { get; set; }

    // Source can be either inventory slot OR equipment slot
    public SlotView SourceInventorySlot { get; set; }
    public EquipmentSlotView SourceEquipmentSlot { get; set; }

    // Helper properties
    public bool IsFromInventory => SourceInventorySlot != null;
    public bool IsFromEquipment => SourceEquipmentSlot != null;
    public bool HasValidSource => IsFromInventory || IsFromEquipment;

    /// Gets the item being dragged (works for both inventory and equipment sources)
    public InventoryItem GetDraggedItem(InventoryViewModel inventoryViewModel)
    {
        if (IsFromEquipment)
            return SourceEquipmentSlot.GetEquippedItem();

        if (IsFromInventory && inventoryViewModel != null)
            return inventoryViewModel.GetItemAt(SourceInventorySlot.SlotIndex);

        return null;
    }

    /// Resets the drag state to initial values
    public void Reset()
    {
        IsDragging = false;
        StartPosition = Vector2.zero;
        SourceInventorySlot = null;
        SourceEquipmentSlot = null;
    }
}
