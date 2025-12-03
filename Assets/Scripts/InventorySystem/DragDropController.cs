using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class DragDropController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private UIDocument uiDocument;

    [Header("Drag Settings")]
    [SerializeField] private float dragThreshold = 5f;
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool highlightEmptySlots = true;

    // Drag state
    private bool isDragging = false;
    private Slot draggedSlot;
    private Vector2 dragStartPosition;
    private VisualElement dragGhost;

    // Cache
    private List<Slot> slots;
    private bool isInitialized = false;

    void Awake()
    {
        inventoryController ??= GetComponent<InventoryController>();
        uiDocument ??= GetComponent<UIDocument>();
    }

    void OnEnable()
    {
        if (!isInitialized) {
            StartCoroutine(Initialize());
        }
    }

    void OnDisable()
    {
        Cleanup();
    }

    private System.Collections.IEnumerator Initialize()
    {
        yield return null;

        int maxWait = 60;
        int waited = 0;
        while ((inventoryController?.Slots == null) && waited < maxWait) {
            yield return null;
            waited++;
        }

        if (inventoryController?.Slots == null) {
            if (debugMode) Debug.LogError("Inventory slots not found");
            yield break;
        }

        slots = new List<Slot>(inventoryController.Slots);
        CreateDragGhost();
        RegisterSlotEvents();

        isInitialized = true;

        if (debugMode) Debug.Log($"DragDropController initialized with {slots.Count} slots");
    }

    private void CreateDragGhost()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null) return;

        var root = uiDocument.rootVisualElement;

        // Remove existing ghost if any
        var existingGhost = root.Q<VisualElement>("dragGhost");
        if (existingGhost != null) {
            existingGhost.RemoveFromHierarchy();
        }

        // Create new ghost with USS classes only
        dragGhost = new VisualElement();
        dragGhost.name = "dragGhost";
        dragGhost.AddToClassList("drag-ghost");
        dragGhost.pickingMode = PickingMode.Ignore;

        root.Add(dragGhost);
    }

    private void RegisterSlotEvents()
    {
        if (slots == null) return;

        foreach (var slot in slots) {
            if (slot != null) {
                slot.UnregisterCallback<PointerDownEvent>(OnSlotPointerDown);
                slot.UnregisterCallback<PointerMoveEvent>(OnSlotPointerMove);
                slot.UnregisterCallback<PointerUpEvent>(OnSlotPointerUp);

                slot.RegisterCallback<PointerDownEvent>(OnSlotPointerDown);
                slot.RegisterCallback<PointerMoveEvent>(OnSlotPointerMove);
                slot.RegisterCallback<PointerUpEvent>(OnSlotPointerUp);
            }
        }
    }

    private void Cleanup()
    {
        if (slots != null) {
            foreach (var slot in slots) {
                if (slot != null) {
                    slot.UnregisterCallback<PointerDownEvent>(OnSlotPointerDown);
                    slot.UnregisterCallback<PointerMoveEvent>(OnSlotPointerMove);
                    slot.UnregisterCallback<PointerUpEvent>(OnSlotPointerUp);
                }
            }
        }

        if (dragGhost != null && dragGhost.parent != null) {
            dragGhost.RemoveFromHierarchy();
            dragGhost = null;
        }

        isInitialized = false;
    }

    private void OnSlotPointerDown(PointerDownEvent evt)
    {
        if (evt.button != 0 || isDragging) return;

        var slot = evt.target as Slot;
        if (slot == null || !slot.HasItem) {
            slot = GetSlotFromParent(evt.target as VisualElement);
            if (slot == null || !slot.HasItem) return;
        }

        if (debugMode) Debug.Log($"Pointer down on slot {slot.SlotIndex}");

        dragStartPosition = evt.position;
        draggedSlot = slot;
        slot.CapturePointer(evt.pointerId);
        evt.StopPropagation();
    }

    private Slot GetSlotFromParent(VisualElement element)
    {
        while (element != null) {
            if (element is Slot slot) return slot;
            element = element.parent;
        }
        return null;
    }

    private void OnSlotPointerMove(PointerMoveEvent evt)
    {
        if (draggedSlot == null || !draggedSlot.HasPointerCapture(evt.pointerId)) return;

        float distance = Vector2.Distance(dragStartPosition, evt.position);

        if (!isDragging && distance > dragThreshold) {
            StartDrag(evt.position);
        }

        if (isDragging && dragGhost != null) {
            UpdateDragPosition(evt.position);
            evt.StopPropagation();
        }
    }

    private void OnSlotPointerUp(PointerUpEvent evt)
    {
        if (draggedSlot == null || !draggedSlot.HasPointerCapture(evt.pointerId)) return;

        if (isDragging) {
            HandleDrop(evt.position);
        }

        draggedSlot.ReleasePointer(evt.pointerId);
        ResetDrag();
    }

    private void StartDrag(Vector2 position)
    {
        if (draggedSlot == null || !draggedSlot.HasItem) return;

        isDragging = true;

        if (debugMode) Debug.Log($"Starting drag from slot {draggedSlot.SlotIndex}");

        // Show ghost with USS class
        if (dragGhost != null && draggedSlot.Icon != null) {
            Texture2D texture = null;
            if (draggedSlot.Icon.image != null)
                texture = draggedSlot.Icon.image as Texture2D;
            else if (draggedSlot.Icon.sprite != null)
                texture = draggedSlot.Icon.sprite.texture;

            if (texture != null) {
                dragGhost.style.backgroundImage = new StyleBackground(texture);
                dragGhost.AddToClassList("drag-ghost--visible");
                dragGhost.BringToFront();

                // Add dragging class to original slot
                draggedSlot.AddToClassList("inventorySlots--dragging");

                // Optional: Highlight empty slots
                if (highlightEmptySlots) {
                    HighlightEmptySlots();
                }

                UpdateDragPosition(position);
            }
        }
    }

    private void HighlightEmptySlots()
    {
        if (slots == null) return;

        foreach (var slot in slots) {
            if (slot != null && slot != draggedSlot && !slot.HasItem) {
                slot.AddToClassList("inventorySlots--empty-highlight");
            }
        }
    }

    private void RemoveEmptySlotHighlighting()
    {
        if (slots == null) return;

        foreach (var slot in slots) {
            if (slot != null) {
                slot.RemoveFromClassList("inventorySlots--empty-highlight");
            }
        }
    }

    private void UpdateDragPosition(Vector2 position)
    {
        if (dragGhost != null) {
            dragGhost.style.left = position.x - (dragGhost.resolvedStyle.width / 2);
            dragGhost.style.top = position.y - (dragGhost.resolvedStyle.height / 2);

            // Optional: Change ghost style based on what's under cursor
            Slot target = FindDropTarget(position);
            if (target != null) {
                if (target.HasItem) {
                    dragGhost.RemoveFromClassList("drag-ghost--over-empty");
                    dragGhost.AddToClassList("drag-ghost--over-occupied");
                } else {
                    dragGhost.RemoveFromClassList("drag-ghost--over-occupied");
                    dragGhost.AddToClassList("drag-ghost--over-empty");
                }
            } else {
                dragGhost.RemoveFromClassList("drag-ghost--over-empty");
                dragGhost.RemoveFromClassList("drag-ghost--over-occupied");
            }
        }
    }

    private void HandleDrop(Vector2 dropPosition)
    {
        if (draggedSlot == null || inventoryController == null) return;

        Slot targetSlot = FindDropTarget(dropPosition);

        if (targetSlot != null && targetSlot != draggedSlot) {
            if (debugMode) {
                string action = targetSlot.HasItem ? "swapping with" : "moving to empty";
                Debug.Log($"Dropping from slot {draggedSlot.SlotIndex} to slot {targetSlot.SlotIndex} ({action})");
            }

            // Add drop target highlight briefly
            targetSlot.AddToClassList("inventorySlots--drop-target");

            // Wait one frame for visual feedback, then move
            StartCoroutine(DelayedMoveItem(draggedSlot.SlotIndex, targetSlot.SlotIndex, targetSlot));
        } else if (targetSlot == draggedSlot) {
            if (debugMode) Debug.Log("Dropped back on original slot - cancelled");
        } else {
            if (debugMode) Debug.Log("Dropped outside any slot - cancelled");
        }
    }

    private System.Collections.IEnumerator DelayedMoveItem(int fromIndex, int toIndex, Slot targetSlot)
    {
        yield return null; // Wait one frame for visual feedback
        inventoryController.MoveItem(fromIndex, toIndex);

        // Remove highlight after move
        if (targetSlot != null) {
            targetSlot.RemoveFromClassList("inventorySlots--drop-target");
        }
    }

    private Slot FindDropTarget(Vector2 position)
    {
        if (slots == null) return null;

        foreach (var slot in slots) {
            if (slot != null && slot != draggedSlot) {
                var bounds = slot.worldBound;
                var paddedBounds = new Rect(
                    bounds.x - 10,
                    bounds.y - 10,
                    bounds.width + 20,
                    bounds.height + 20
                );

                if (paddedBounds.Contains(position)) {
                    return slot;
                }
            }
        }
        return null;
    }

    private void ResetDrag()
    {
        // Remove dragging class from original slot
        if (draggedSlot != null) {
            draggedSlot.RemoveFromClassList("inventorySlots--dragging");
        }

        // Remove empty slot highlighting
        if (highlightEmptySlots) {
            RemoveEmptySlotHighlighting();
        }

        // Hide ghost by removing visible class
        if (dragGhost != null) {
            dragGhost.RemoveFromClassList("drag-ghost--visible");
            dragGhost.style.backgroundImage = null;
        }

        // Reset state
        isDragging = false;
        draggedSlot = null;

        if (debugMode) Debug.Log("Drag reset");
    }

    public bool IsDragging() => isDragging;
    public Slot GetDraggedSlot() => draggedSlot;
}