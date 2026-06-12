using System;
using System.Collections.Generic;

namespace InventorySystem
{
    public interface IInventory
    {
        IReadOnlyList<InventoryItem> Items { get; }
        int MaxInventorySize { get; }
        bool AddItem(InventoryItem item);
        bool RemoveItemAt(int index);
        bool MoveItem(int fromIndex, int toIndex);
        bool SwapItems(int indexA, int indexB);
        InventoryItem GetItemAt(int index);
        bool CanAddItem();
        bool SetItemAt(int index, InventoryItem item);
        bool IsValidSlotIndex(int index);
        event Action OnInventoryChanged;
        event Action<int> OnSlotChanged;
    }
}
