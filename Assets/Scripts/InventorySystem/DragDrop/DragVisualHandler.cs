using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

/// Handles all visual feedback during drag operations (ghost, highlighting, etc.)
/// Separates visual concerns from drag logic.
public class DragVisualHandler
{
    private readonly VisualElement dragGhost;
    private readonly List<Slot> inventorySlots;
    private readonly bool highlightEmptySlots;
    
    public DragVisualHandler(VisualElement root, List<Slot> inventorySlots, bool highlightEmptySlots)
    {
        this.inventorySlots = inventorySlots;
        this.highlightEmptySlots = highlightEmptySlots;
        this.dragGhost = CreateDragGhost(root);
    }
    
    private VisualElement CreateDragGhost(VisualElement root)
    {
        // Remove existing ghost if any
        var existingGhost = root.Q<VisualElement>("dragGhost");
        existingGhost?.RemoveFromHierarchy();

        // Create new ghost
        var ghost = new VisualElement {
            name = "dragGhost",
            pickingMode = PickingMode.Ignore
        };

        ghost.AddToClassList("drag-ghost");
        ghost.style.position = Position.Absolute;
        ghost.style.width = 128;
        ghost.style.height = 128;

        root.Add(ghost);
        return ghost;
    }
    
    public void ShowGhost(Sprite icon, Vector2 position)
    {
        if (dragGhost == null || icon == null) return;

        Texture2D texture = icon.texture;
        if (texture == null) return;

        dragGhost.style.backgroundImage = new StyleBackground(texture);
        dragGhost.AddToClassList("drag-ghost--visible");
        dragGhost.BringToFront();

        if (highlightEmptySlots) HighlightEmptySlots();

        UpdatePosition(position);
    }
    
    public void UpdatePosition(Vector2 position)
    {
        if (dragGhost == null) return;

        dragGhost.style.left = position.x - (dragGhost.resolvedStyle.width / 2);
        dragGhost.style.top = position.y - (dragGhost.resolvedStyle.height / 2);
    }
    
    public void UpdateAppearance(bool isValidDrop)
    {
        dragGhost.RemoveFromClassList("drag-ghost--over-empty");
        dragGhost.RemoveFromClassList("drag-ghost--over-occupied");
        dragGhost.AddToClassList(isValidDrop ? "drag-ghost--over-empty" : "drag-ghost--over-occupied");
    }
    
    public void HideGhost()
    {
        if (dragGhost == null) return;

        dragGhost.RemoveFromClassList("drag-ghost--visible");
        dragGhost.RemoveFromClassList("drag-ghost--over-empty");
        dragGhost.RemoveFromClassList("drag-ghost--over-occupied");
        dragGhost.style.backgroundImage = null;

        if (highlightEmptySlots) RemoveEmptySlotHighlighting();
    }
    
    public void AddDraggingClass(Slot slot)
    {
        slot?.AddToClassList("inventorySlots--dragging");
    }
    
    public void RemoveDraggingClass(Slot slot)
    {
        slot?.RemoveFromClassList("inventorySlots--dragging");
    }
    
    private void HighlightEmptySlots()
    {
        if (inventorySlots == null) return;

        foreach (var slot in inventorySlots) {
            if (slot != null && !slot.HasItem) {
                slot.AddToClassList("inventorySlots--empty-highlight");
            }
        }
    }
    
    private void RemoveEmptySlotHighlighting()
    {
        if (inventorySlots == null) return;

        foreach (var slot in inventorySlots) {
            slot?.RemoveFromClassList("inventorySlots--empty-highlight");
        }
    }
    
    public void Cleanup()
    {
        if (dragGhost?.parent != null) {
            dragGhost.RemoveFromHierarchy();
        }
    }
}
