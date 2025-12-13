using UnityEngine;

/// Encapsulates the state of a drag operation.
/// This makes drag state management cleaner and easier to reason about.
public class DragState
{
    public bool IsDragging { get; set; }
    public Vector2 StartPosition { get; set; }
    
    // Source can be either inventory slot OR equipment slot
    public Slot SourceInventorySlot { get; set; }
    public EquipmentSlot SourceEquipmentSlot { get; set; }
    
    // Helper properties
    public bool IsFromInventory => SourceInventorySlot != null;
    public bool IsFromEquipment => SourceEquipmentSlot != null;
    public bool HasValidSource => IsFromInventory || IsFromEquipment;
    
    /// Gets the item being dragged (works for both inventory and equipment sources)
    public Inventory_Item GetDraggedItem(InventoryController inventoryController)
    {
        if (IsFromEquipment)
            return SourceEquipmentSlot.GetEquippedItem();
            
        if (IsFromInventory && inventoryController != null)
            return inventoryController.GetItemAtSlot(SourceInventorySlot.SlotIndex);
            
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
