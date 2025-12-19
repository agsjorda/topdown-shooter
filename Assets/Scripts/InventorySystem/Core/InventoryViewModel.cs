using System;
using System.Collections.Generic;

namespace InventorySystem
{
    // ViewModel for inventory operations. All inventory logic should go through this class.
    public class InventoryViewModel
    {
        private readonly InventoryModel _inventory;
        public event Action OnInventoryChanged;

        // Construct a new InventoryViewModel for the given InventoryModel.
        public InventoryViewModel(InventoryModel inventory)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _inventory.OnInventoryChanged += () => OnInventoryChanged?.Invoke();
        }

        // Read-only list of inventory items.
        public IReadOnlyList<InventoryItem> Items => _inventory.itemList;
        public int MaxInventorySize => _inventory.maxInventorySize;

        // Get a filtered list of inventory items.
        public List<InventoryItem> GetFilteredItems(Func<InventoryItem, bool> filter)
        {
            var result = new List<InventoryItem>();
            foreach (var item in _inventory.itemList) {
                if (item != null && (filter == null || filter(item)))
                    result.Add(item);
            }
            return result;
        }

        // Add an item to the inventory.
        public void AddItem(InventoryItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            _inventory.AddItem(item);
        }

        // Remove the item at the given index.
        public void RemoveItemAt(int index)
        {
            if (!IsValidSlotIndex(index)) return;
            _inventory.itemList[index] = null;
            _inventory.NotifyInventoryChanged();
        }

        // Move an item from one slot to another.
        public void MoveItem(int fromIndex, int toIndex)
        {
            if (!IsValidSlotIndex(fromIndex) || !IsValidSlotIndex(toIndex)) return;
            _inventory.SmartMoveItem(fromIndex, toIndex);
        }

        // Swap two items in the inventory.
        public void SwapItems(int indexA, int indexB)
        {
            if (!IsValidSlotIndex(indexA) || !IsValidSlotIndex(indexB)) return;
            _inventory.SwapItems(indexA, indexB);
        }

        // Get the item at the given index.
        public InventoryItem GetItemAt(int index)
        {
            if (index < 0 || index >= _inventory.maxInventorySize)
                return null;
            // Ensure the list is large enough
            while (_inventory.itemList.Count <= index)
                _inventory.itemList.Add(null);
            return _inventory.itemList[index];
        }

        // Check if an item can be added to the inventory.
        public bool CanAddItem() => _inventory.CanAddItem();

        // Set an item at a specific slot index.
        public void SetItemAt(int index, InventoryItem item)
        {
            if (!IsValidSlotIndex(index)) return;
            _inventory.SetItemAt(index, item);
        }

        // Helper to check if a slot index is valid.
        public bool IsValidSlotIndex(int index)
        {
            return index >= 0 && index < _inventory.maxInventorySize;
        }
    }
}
