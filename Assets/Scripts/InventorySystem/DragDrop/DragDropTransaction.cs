using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// Base class for all drag/drop transactions.
/// Each transaction type (inventory-to-inventory, inventory-to-equipment, etc.) inherits from this.
/// This uses the Strategy pattern to eliminate duplicate transaction code.
public abstract class DragDropTransaction
{
    protected readonly InventoryController inventoryController;
    protected readonly EquipmentController equipmentController;
    protected readonly bool debugMode;
    
    protected DragDropTransaction(
        InventoryController inventoryController,
        EquipmentController equipmentController,
        bool debugMode = false)
    {
        this.inventoryController = inventoryController;
        this.equipmentController = equipmentController;
        this.debugMode = debugMode;
    }
    
    /// Validates if this transaction can be performed
    public abstract bool CanExecute();
    
    /// Executes the transaction (as a coroutine for visual feedback timing)
    public abstract IEnumerator Execute();
    
    /// Gets the target slot for visual feedback (can be null)
    public abstract VisualElement GetTargetVisual();
    
    /// Helper to log debug messages if debug mode is enabled
    protected void Log(string message)
    {
        if (debugMode) Debug.Log($"[Transaction] {message}");
    }
    
    /// Helper to log errors
    protected void LogError(string message)
    {
        Debug.LogError($"[Transaction] {message}");
    }
}
