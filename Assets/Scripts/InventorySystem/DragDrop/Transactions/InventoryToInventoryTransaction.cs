using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;

/// <summary>
/// Handles moving/swapping items within inventory slots
/// </summary>
public class InventoryToInventoryTransaction : DragDropTransaction
{
    private readonly int fromIndex;
    private readonly int toIndex;
    private readonly Slot targetSlot;
    
    public InventoryToInventoryTransaction(
        int fromIndex,
        int toIndex,
        Slot targetSlot,
        InventoryController inventoryController,
        EquipmentController equipmentController,
        bool debugMode = false)
        : base(inventoryController, equipmentController, debugMode)
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
        
        if (inventoryController == null) {
            LogError("InventoryController is null");
            return false;
        }
        
        return true;
    }
    
    public override IEnumerator Execute()
    {
        yield return null; // Wait one frame for visual feedback
        
        string action = targetSlot?.HasItem ?? false ? "swapping with" : "moving to empty";
        Log($"Inventory move from slot {fromIndex} to slot {toIndex} ({action})");
        
        inventoryController.MoveItem(fromIndex, toIndex);
        targetSlot?.RemoveFromClassList("inventorySlots--drop-target");
    }
    
    public override VisualElement GetTargetVisual() => targetSlot;
}
