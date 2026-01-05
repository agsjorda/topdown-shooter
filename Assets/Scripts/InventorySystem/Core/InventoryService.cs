using UnityEngine;

namespace InventorySystem
{
    /// <summary>
    /// Central service for accessing inventory and equipment systems.
    /// Replaces scattered FindFirstObjectByType calls with a centralized registry.
    /// </summary>
    public static class InventoryService
    {
        private static IInventory _playerInventory;
        private static IEquipmentSystem _playerEquipment;

        /// <summary>
        /// Gets the player's inventory instance.
        /// If not registered, attempts to find InventoryModel in the scene.
        /// </summary>
        /// <returns>The player's inventory or null if not found</returns>
        public static IInventory GetPlayerInventory()
        {
            if (_playerInventory == null)
            {
                var inventoryModel = Object.FindFirstObjectByType<InventoryModel>();
                if (inventoryModel != null)
                {
                    _playerInventory = inventoryModel;
                }
            }
            return _playerInventory;
        }

        /// <summary>
        /// Gets the player's equipment system.
        /// If not registered, attempts to find EquipmentController in the scene.
        /// </summary>
        /// <returns>The player's equipment system or null if not found</returns>
        public static IEquipmentSystem GetPlayerEquipment()
        {
            if (_playerEquipment == null)
            {
                var equipmentController = Object.FindFirstObjectByType<EquipmentController>();
                if (equipmentController != null)
                {
                    _playerEquipment = equipmentController;
                }
            }
            return _playerEquipment;
        }

        /// <summary>
        /// Manually registers the player's inventory.
        /// Useful for dependency injection or custom initialization.
        /// </summary>
        /// <param name="inventory">The inventory instance to register</param>
        public static void RegisterPlayerInventory(IInventory inventory)
        {
            _playerInventory = inventory;
        }

        /// <summary>
        /// Manually registers the player's equipment system.
        /// Useful for dependency injection or custom initialization.
        /// </summary>
        /// <param name="equipmentSystem">The equipment system instance to register</param>
        public static void RegisterPlayerEquipment(IEquipmentSystem equipmentSystem)
        {
            _playerEquipment = equipmentSystem;
        }

        /// <summary>
        /// Clears all registered services.
        /// Call this when changing scenes or resetting the game state.
        /// </summary>
        public static void Clear()
        {
            _playerInventory = null;
            _playerEquipment = null;
        }

        /// <summary>
        /// Gets the InventoryViewModel for the player's inventory.
        /// Creates one if it doesn't exist.
        /// </summary>
        /// <returns>InventoryViewModel wrapping the player's inventory</returns>
        public static InventoryViewModel GetPlayerInventoryViewModel()
        {
            var inventory = GetPlayerInventory();
            if (inventory is InventoryModel model)
            {
                return new InventoryViewModel(model);
            }
            return null;
        }
    }
}
