using System;
using System.Collections;
using System.Collections.Generic;
using InventorySystem;
using InventorySystem.DragDrop.Transactions;
using UnityEngine;

public class DragDropService
{
    public event Action<Vector2, InventoryItem> OnDragStarted;
    public event Action<Vector2> OnDragUpdated;
    public event Action OnDragEnded;
    public event Action<DragDropTransaction> OnTransactionReady;
    public event Action<string> OnDebugLog;

    private DragState dragState;
    private InventoryViewModel inventoryViewModel;
    private EquipmentController equipmentController;
    private List<SlotView> inventorySlots;
    private List<EquipmentSlotView> equipmentSlots;
    private bool debugMode;
    private readonly DragVisualHandler dragVisualHandler;

    public DragDropService(
        InventoryViewModel inventoryViewModel,
        EquipmentController equipmentController,
        List<SlotView> inventorySlots,
        List<EquipmentSlotView> equipmentSlots,
        DragVisualHandler dragVisualHandler,
        bool debugMode = false)
    {
        this.inventoryViewModel = inventoryViewModel;
        this.equipmentController = equipmentController;
        this.inventorySlots = inventorySlots;
        this.equipmentSlots = equipmentSlots;
        this.dragVisualHandler = dragVisualHandler;
        this.debugMode = debugMode;
        dragState = new DragState();
    }

    public DragState DragState => dragState;

    public void StartDrag(Vector2 position)
    {
        dragState.IsDragging = true;
        var draggedItem = dragState.GetDraggedItem(inventoryViewModel);
        if (draggedItem == null)
        {
            ResetDrag();
            return;
        }
        OnDebugLog?.Invoke($"[DragDropService] Drag started at {position}");
        OnDragStarted?.Invoke(position, draggedItem);
    }

    public void UpdateDrag(Vector2 position)
    {
        if (!dragState.IsDragging) return;
        OnDragUpdated?.Invoke(position);
    }

    private void CleanupDragVisualsAndState()
    {
        dragVisualHandler?.CleanupDragVisuals(dragState.SourceInventorySlot, dragState.SourceEquipmentSlot);
        dragState.Reset();
        OnDragEnded?.Invoke();
    }

    public void EndDrag()
    {
        CleanupDragVisualsAndState();
    }

    public void HandleDrop(Vector2 dropPosition, InventoryUIConfig uiConfig)
    {
        var targetEquipment = FindEquipmentSlotAtPosition(dropPosition);
        var targetInventory = targetEquipment == null ? FindInventorySlotAtPosition(dropPosition) : null;
        if (targetEquipment == null && targetInventory == null)
        {
            OnDebugLog?.Invoke("[DragDropService] Dropped outside any slot - cancelled");
            EndDrag();
            return;
        }
        var transaction = TransactionFactory.CreateTransaction(
            dragState,
            targetInventory,
            targetEquipment,
            inventorySlots,
            inventoryViewModel,
            equipmentController,
            debugMode
        );
        if (transaction != null && transaction.CanExecute())
        {
            OnTransactionReady?.Invoke(transaction);
        }
        else
        {
            OnDebugLog?.Invoke("[DragDropService] Transaction cannot be executed");
            EndDrag();
        }
    }

    private EquipmentSlotView FindEquipmentSlotAtPosition(Vector2 position)
    {
        foreach (var slot in equipmentSlots)
        {
            if (slot == null) continue;
            var paddedBounds = slot.worldBound.Padded(20);
            if (paddedBounds.Contains(position)) return slot;
        }
        return null;
    }
    private SlotView FindInventorySlotAtPosition(Vector2 position)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot == null) continue;
            var paddedBounds = slot.worldBound.Padded(10);
            if (paddedBounds.Contains(position)) return slot;
        }
        return null;
    }

    public void ResetDrag()
    {
        CleanupDragVisualsAndState();
    }
}
