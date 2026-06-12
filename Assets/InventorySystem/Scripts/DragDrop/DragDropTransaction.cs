using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
    /// <summary>
    /// Base class for all drag/drop transactions.
    /// Each transaction type (inventory-to-inventory, inventory-to-equipment, etc.) inherits from this.
    /// This uses the Strategy pattern to eliminate duplicate transaction code.
    /// </summary>
    public abstract class DragDropTransaction
    {
        protected readonly InventoryViewModel inventoryViewModel;
        protected readonly IEquipmentSystem equipmentSystem;
        protected readonly bool debugMode;

        protected DragDropTransaction(
            InventoryViewModel inventoryViewModel,
            IEquipmentSystem equipmentSystem,
            bool debugMode = false)
        {
            this.inventoryViewModel = inventoryViewModel;
            this.equipmentSystem = equipmentSystem;
            this.debugMode = debugMode;
        }

        /// <summary>
        /// Validates if this transaction can be performed.
        /// </summary>
        /// <returns>True if the transaction can execute, false otherwise</returns>
        public abstract bool CanExecute();

        /// <summary>
        /// Executes the transaction as a coroutine for visual feedback timing.
        /// </summary>
        /// <returns>Coroutine enumerator</returns>
        public abstract IEnumerator Execute();

        /// <summary>
        /// Gets the target slot for visual feedback (can be null).
        /// </summary>
        /// <returns>The target visual element or null</returns>
        public abstract VisualElement GetTargetVisual();

        /// <summary>
        /// Helper to log debug messages if debug mode is enabled.
        /// </summary>
        /// <param name="message">The message to log</param>
        protected void Log(string message)
        {
            if (debugMode) Debug.Log($"[Transaction] {message}");
        }

        /// <summary>
        /// Helper to log errors.
        /// </summary>
        /// <param name="message">The error message to log</param>
        protected void LogError(string message)
        {
            Debug.LogError($"[Transaction] {message}");
        }
    }
}
