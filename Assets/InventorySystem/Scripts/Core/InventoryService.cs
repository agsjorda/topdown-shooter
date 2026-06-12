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
        private static InventoryViewModel _playerInventoryViewModel;

        // Statics survive entering Play Mode when Domain Reload is disabled
        // (Project Settings > Editor > Enter Play Mode Options), which would leave
        // them pointing at destroyed scene objects. Reset explicitly on every play.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Clear();

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
            _playerInventoryViewModel = null;
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
            _playerInventoryViewModel = null;
        }

        /// <summary>
        /// Gets the shared InventoryViewModel wrapping the player's inventory.
        /// One instance per registered inventory — all UI and drag/drop code must
        /// consume this rather than constructing their own ViewModels.
        /// </summary>
        /// <returns>The shared InventoryViewModel, or null if no inventory exists</returns>
        public static InventoryViewModel GetPlayerInventoryViewModel()
        {
            if (_playerInventoryViewModel == null && GetPlayerInventory() is InventoryModel model)
            {
                _playerInventoryViewModel = new InventoryViewModel(model);
            }
            return _playerInventoryViewModel;
        }
    }
}
