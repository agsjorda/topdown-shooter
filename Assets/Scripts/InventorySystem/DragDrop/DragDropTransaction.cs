using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Base class for all drag/drop transactions.
/// Each transaction type (inventory-to-inventory, inventory-to-equipment, etc.) inherits from this.
/// This uses the Strategy pattern to eliminate duplicate transaction code.
/// </summary>
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
    
    /// <summary>
    /// Validates if this transaction can be performed
    /// </summary>
    public abstract bool CanExecute();
    
    /// <summary>
    /// Executes the transaction (as a coroutine for visual feedback timing)
    /// </summary>
    public abstract IEnumerator Execute();
    
    /// <summary>
    /// Gets the target slot for visual feedback (can be null)
    /// </summary>
    public abstract VisualElement GetTargetVisual();
    
    /// <summary>
    /// Helper to log debug messages if debug mode is enabled
    /// </summary>
    protected void Log(string message)
    {
        if (debugMode) Debug.Log($"[Transaction] {message}");
    }
    
    /// <summary>
    /// Helper to log errors
    /// </summary>
    protected void LogError(string message)
    {
        Debug.LogError($"[Transaction] {message}");
    }
}
