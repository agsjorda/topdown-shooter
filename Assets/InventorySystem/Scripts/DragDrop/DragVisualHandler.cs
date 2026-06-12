using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
    /// Handles all visual feedback during drag operations (ghost, highlighting, etc.)
    /// Separates visual concerns from drag logic.
    public class DragVisualHandler
    {
        private readonly VisualElement dragGhost;
        // Resolved on use so slot rebuilds can never leave this handler pointing at stale views
        private readonly Func<IReadOnlyList<SlotView>> getInventorySlots;
        private readonly bool highlightEmptySlots;

        public DragVisualHandler(VisualElement root, Func<IReadOnlyList<SlotView>> getInventorySlots, bool highlightEmptySlots)
        {
            this.getInventorySlots = getInventorySlots;
            this.highlightEmptySlots = highlightEmptySlots;
            this.dragGhost = CreateDragGhost(root);
        }

        private VisualElement CreateDragGhost(VisualElement root)
        {
            // Remove existing ghost if any
            root.Q<VisualElement>("dragGhost")?.RemoveFromHierarchy();

            // Create new ghost with fluent API using extensions
            return root.CreateChild("drag-ghost")
                .WithName("dragGhost")
                .WithPickingMode(PickingMode.Ignore)
                .WithAbsolutePosition()
                .WithSize(128, 128);
        }

        public void ShowGhost(Sprite icon, Vector2 position)
        {
            if (dragGhost == null || icon == null) return;

            Texture2D texture = icon.texture;
            if (texture == null) return;

            // Use fluent API for cleaner code
            dragGhost
                .WithBackgroundImage(texture)
                .AddClass("drag-ghost--visible")
                .BringToFront();

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
            // Use extension to remove multiple classes at once, then conditionally add
            dragGhost
                .RemoveClass("drag-ghost--over-empty", "drag-ghost--over-occupied")
                .AddClassIf(isValidDrop, "drag-ghost--over-empty", "drag-ghost--over-occupied");
        }

        public void HideGhost()
        {
            // Use null-conditional operator with fluent API
            dragGhost?
                .RemoveClass("drag-ghost--visible", "drag-ghost--over-empty", "drag-ghost--over-occupied")
                .WithBackgroundImage(null);

            if (highlightEmptySlots) RemoveEmptySlotHighlighting();
        }

        public void AddDraggingClass(SlotView slot)
        {
            slot?.AddClass("inventorySlots--dragging");
        }

        public void RemoveDraggingClass(SlotView slot)
        {
            slot?.RemoveClass("inventorySlots--dragging");
        }

        private void HighlightEmptySlots()
        {
            // Use LINQ + batch extension for cleaner code
            getInventorySlots?.Invoke()?
                .Where(slot => slot != null && !slot.HasItem)
                .AddClassToAll("inventorySlots--empty-highlight");
        }

        private void RemoveEmptySlotHighlighting()
        {
            // Use batch extension to remove class from all slots
            getInventorySlots?.Invoke()?.RemoveClassFromAll("inventorySlots--empty-highlight");
        }

        public void Cleanup()
        {
            if (dragGhost?.parent != null) {
                dragGhost.RemoveFromHierarchy();
            }
        }

        /// Cleans up all drag-related visuals for a given source slot (inventory or equipment)
        public void CleanupDragVisuals(SlotView sourceInventorySlot = null, EquipmentSlotView sourceEquipmentSlot = null)
        {
            if (sourceInventorySlot != null)
            {
                sourceInventorySlot.RemoveFromClassList("inventorySlots--dragging");
                sourceInventorySlot.RemoveFromClassList("inventorySlots--empty-highlight");
            }
            if (sourceEquipmentSlot != null)
            {
                sourceEquipmentSlot.RemoveFromClassList("inventorySlots--dragging");
                sourceEquipmentSlot.RemoveFromClassList("inventorySlots--empty-highlight");
            }
            HideGhost();
        }
    }
}
