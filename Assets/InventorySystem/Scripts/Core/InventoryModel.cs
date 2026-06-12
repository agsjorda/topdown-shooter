using System;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    public class InventoryModel : MonoBehaviour, IInventory
    {
        public event Action OnInventoryChanged;
        // Fired with the affected slot index on single-slot mutations; OnInventoryChanged still
        // fires once per operation for listeners that refresh everything.
        public event Action<int> OnSlotChanged;

        public int maxInventorySize = 24;
        //Item list can contain nulls representing empty slots
        public List<InventoryItem> itemList = new List<InventoryItem>();

        [Tooltip("Log inventory operations to the Console")]
        [SerializeField] private bool debugLogging = false;

        public IReadOnlyList<InventoryItem> Items => itemList;
        public int MaxInventorySize => maxInventorySize;

        public bool CanAddItem() => FindFirstEmptySlot() >= 0;

        public bool AddItem(InventoryItem itemToAdd)
        {
            if (itemToAdd == null || itemToAdd.itemData == null) {
                Debug.LogError("Cannot add null item");
                return false;
            }

            // Stackable items merge into an existing stack with enough room first
            if (itemToAdd.itemData.stackable && TryStackItem(itemToAdd)) {
                return true;
            }

            int emptySlotIndex = FindFirstEmptySlot();
            if (emptySlotIndex < 0) {
                Log("Inventory full");
                return false;
            }

            EnsureSize(emptySlotIndex);
            itemList[emptySlotIndex] = itemToAdd;
            OnSlotChanged?.Invoke(emptySlotIndex);
            NotifyInventoryChanged();
            Log($"Added item to slot {emptySlotIndex}: {itemToAdd.itemData.itemName}");
            return true;
        }

        // Merges the incoming item into the first existing stack of the same item
        // that can hold its full quantity. Partial splits intentionally aren't done:
        // either the whole stack merges, or the item takes its own slot.
        private bool TryStackItem(InventoryItem itemToAdd)
        {
            for (int i = 0; i < itemList.Count; i++) {
                var existing = itemList[i];
                if (existing == null || existing.itemData == null) continue;
                if (existing.itemData.itemId != itemToAdd.itemData.itemId) continue;
                if (existing.quantity + itemToAdd.quantity > itemToAdd.itemData.maxStack) continue;

                existing.quantity += itemToAdd.quantity;
                OnSlotChanged?.Invoke(i);
                NotifyInventoryChanged();
                Log($"Stacked {itemToAdd.quantity}x {itemToAdd.itemData.itemName} into slot {i} (now {existing.quantity})");
                return true;
            }
            return false;
        }

        public bool RemoveItemAt(int index)
        {
            if (!IsValidSlotIndex(index)) return false;
            if (index >= itemList.Count || itemList[index] == null) return false;
            itemList[index] = null;
            OnSlotChanged?.Invoke(index);
            NotifyInventoryChanged();
            return true;
        }

        public bool MoveItem(int fromIndex, int toIndex)
        {
            return SmartMoveItem(fromIndex, toIndex);
        }

        public bool SwapItems(int indexA, int indexB)
        {
            if (itemList == null ||
                indexA < 0 || indexA >= itemList.Count ||
                indexB < 0 || indexB >= itemList.Count ||
                indexA == indexB)
                return false;

            (itemList[indexA], itemList[indexB]) = (itemList[indexB], itemList[indexA]);
            OnSlotChanged?.Invoke(indexA);
            OnSlotChanged?.Invoke(indexB);
            NotifyInventoryChanged();
            return true;
        }

        public InventoryItem GetItemAt(int index)
        {
            if (index < 0 || index >= maxInventorySize)
                return null;
            // Indices beyond the backing list are valid empty slots; do not grow the list here.
            return index < itemList.Count ? itemList[index] : null;
        }

        public bool SetItemAt(int index, InventoryItem item)
        {
            if (!IsValidSlotIndex(index)) return false;
            EnsureSize(index);
            itemList[index] = item;
            OnSlotChanged?.Invoke(index);
            NotifyInventoryChanged();
            return true;
        }

        public bool IsValidSlotIndex(int index)
        {
            return index >= 0 && index < maxInventorySize;
        }

        private int FindFirstEmptySlot()
        {
            for (int i = 0; i < itemList.Count; i++) {
                if (itemList[i] == null) {
                    return i;
                }
            }
            if (itemList.Count < maxInventorySize) {
                return itemList.Count;
            }
            return -1;
        }

        // Grow the backing list with empty slots so the index becomes addressable.
        private void EnsureSize(int index)
        {
            while (itemList.Count <= index) {
                itemList.Add(null);
            }
        }

        public bool SmartMoveItem(int fromIndex, int toIndex)
        {
            if (itemList == null || fromIndex == toIndex) return false;
            if (fromIndex < 0 || fromIndex >= itemList.Count) return false;
            if (toIndex < 0 || toIndex >= maxInventorySize) return false;
            EnsureSize(toIndex);
            var itemToMove = itemList[fromIndex];
            var targetItem = itemList[toIndex];
            if (targetItem == null) {
                itemList[fromIndex] = null;
                itemList[toIndex] = itemToMove;
            } else {
                itemList[fromIndex] = targetItem;
                itemList[toIndex] = itemToMove;
            }
            OnSlotChanged?.Invoke(fromIndex);
            OnSlotChanged?.Invoke(toIndex);
            NotifyInventoryChanged();
            Log($"Smart move: {fromIndex} -> {toIndex}");
            return true;
        }

        public void NotifyInventoryChanged()
        {
            OnInventoryChanged?.Invoke();
        }

        private void Log(string message)
        {
            if (debugLogging) Debug.Log(message);
        }
    }
}
