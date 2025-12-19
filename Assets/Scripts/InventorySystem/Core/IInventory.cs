using System;
using System.Collections.Generic;


public interface IInventory
{
    IReadOnlyList<InventoryItem> Items { get; }
    int MaxInventorySize { get; }
    void AddItem(InventoryItem item);
    void RemoveItemAt(int index);
    void MoveItem(int fromIndex, int toIndex);
    void SwapItems(int indexA, int indexB);
    InventoryItem GetItemAt(int index);
    bool CanAddItem();
    void SetItemAt(int index, InventoryItem item);
    bool IsValidSlotIndex(int index);
    event Action OnInventoryChanged;
}
