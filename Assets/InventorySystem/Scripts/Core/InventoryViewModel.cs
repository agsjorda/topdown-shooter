using System;
using System.Collections.Generic;

namespace InventorySystem
{
    // ViewModel for inventory operations. All inventory logic should go through this class.
    public class InventoryViewModel : IInventory
    {
        private readonly InventoryModel _inventory;
        public event Action OnInventoryChanged;
        public event Action<int> OnSlotChanged;

        // Construct a new InventoryViewModel for the given InventoryModel.
        public InventoryViewModel(InventoryModel inventory)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _inventory.OnInventoryChanged += () => OnInventoryChanged?.Invoke();
            _inventory.OnSlotChanged += index => OnSlotChanged?.Invoke(index);
        }

        // Read-only list of inventory items.
        public IReadOnlyList<InventoryItem> Items => _inventory.itemList;
        public int MaxInventorySize => _inventory.maxInventorySize;

        // Add an item to the inventory. Returns false if the item is null or the inventory is full.
        public bool AddItem(InventoryItem item)
        {
            return item != null && _inventory.AddItem(item);
        }

        // Remove the item at the given index. Returns false if the slot was already empty or invalid.
        public bool RemoveItemAt(int index)
        {
            return IsValidSlotIndex(index) && _inventory.RemoveItemAt(index);
        }

        // Move an item from one slot to another (swaps when the target is occupied).
        public bool MoveItem(int fromIndex, int toIndex)
        {
            if (!IsValidSlotIndex(fromIndex) || !IsValidSlotIndex(toIndex)) return false;
            return _inventory.SmartMoveItem(fromIndex, toIndex);
        }

        // Swap two items in the inventory.
        public bool SwapItems(int indexA, int indexB)
        {
            if (!IsValidSlotIndex(indexA) || !IsValidSlotIndex(indexB)) return false;
            return _inventory.SwapItems(indexA, indexB);
        }

        // Get the item at the given index.
        public InventoryItem GetItemAt(int index) => _inventory.GetItemAt(index);

        // Check if an item can be added to the inventory.
        public bool CanAddItem() => _inventory.CanAddItem();

        // Set an item at a specific slot index.
        public bool SetItemAt(int index, InventoryItem item)
        {
            return IsValidSlotIndex(index) && _inventory.SetItemAt(index, item);
        }

        // Helper to check if a slot index is valid.
        public bool IsValidSlotIndex(int index)
        {
            return index >= 0 && index < _inventory.maxInventorySize;
        }
    }
}
