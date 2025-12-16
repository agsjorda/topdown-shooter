using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using InventorySystem;

/// Base class for all drag/drop transactions.
/// Each transaction type (inventory-to-inventory, inventory-to-equipment, etc.) inherits from this.
/// This uses the Strategy pattern to eliminate duplicate transaction code.
public abstract class DragDropTransaction
{
    protected readonly InventoryViewModel inventoryViewModel;
    protected readonly EquipmentController equipmentController;
    protected readonly bool debugMode;
    
    protected DragDropTransaction(
        InventoryViewModel inventoryViewModel,
        EquipmentController equipmentController,
        bool debugMode = false)
    {
        this.inventoryViewModel = inventoryViewModel;
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
