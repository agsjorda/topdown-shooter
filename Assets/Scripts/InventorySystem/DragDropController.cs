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
    [SerializeField] private bool highlightEmptySlots = true; // Optional feature

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

        // Wait for inventory to initialize
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

        // Create drag ghost AFTER slots are available
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

        // Create new ghost - SIMPLIFIED: no USS classes initially
        dragGhost = new VisualElement();
        dragGhost.name = "dragGhost";
        dragGhost.style.position = Position.Absolute;
        dragGhost.style.width = 250;
        dragGhost.style.height = 250;
        dragGhost.style.visibility = Visibility.Hidden;
        dragGhost.style.opacity = 0.8f;
        // dragGhost.style.borderWidth = 2;
        dragGhost.style.borderTopWidth = 2;
        dragGhost.style.borderRightWidth = 2;
        dragGhost.style.borderBottomWidth = 2;
        dragGhost.style.borderLeftWidth = 2;
        // dragGhost.style.borderColor = new Color(0.97f, 0.64f, 0.18f, 0.7f);
        dragGhost.style.borderTopColor = new Color(0.97f, 0.64f, 0.18f, 0.7f);
        dragGhost.style.borderRightColor = new Color(0.97f, 0.64f, 0.18f, 0.7f);
        dragGhost.style.borderBottomColor = new Color(0.97f, 0.64f, 0.18f, 0.7f);
        dragGhost.style.borderLeftColor = new Color(0.97f, 0.64f, 0.18f, 0.7f);
        dragGhost.style.borderTopLeftRadius = 5;
        dragGhost.style.borderTopRightRadius = 5;
        dragGhost.style.borderBottomLeftRadius = 5;
        dragGhost.style.borderBottomRightRadius = 5;
        dragGhost.style.backgroundColor = new Color(0.13f, 0.13f, 0.13f, 0.8f);
        dragGhost.pickingMode = PickingMode.Ignore;

        root.Add(dragGhost);
    }

    private void RegisterSlotEvents()
    {
        if (slots == null) return;

        foreach (var slot in slots) {
            if (slot != null) {
                // Clear existing events first
                slot.UnregisterCallback<PointerDownEvent>(OnSlotPointerDown);
                slot.UnregisterCallback<PointerMoveEvent>(OnSlotPointerMove);
                slot.UnregisterCallback<PointerUpEvent>(OnSlotPointerUp);

                // Register events
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

        // Remove ghost
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

        // Show ghost
        if (dragGhost != null && draggedSlot.Icon != null) {
            // Get texture
            Texture2D texture = null;
            if (draggedSlot.Icon.image != null)
                texture = draggedSlot.Icon.image as Texture2D;
            else if (draggedSlot.Icon.sprite != null)
                texture = draggedSlot.Icon.sprite.texture;

            if (texture != null) {
                dragGhost.style.backgroundImage = new StyleBackground(texture);
                dragGhost.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                dragGhost.style.visibility = Visibility.Visible;
                dragGhost.BringToFront();

                // Fade original icon
                draggedSlot.Icon.style.opacity = 0.3f;

                // Optional: Highlight empty slots
                if (highlightEmptySlots) {
                    HighlightEmptySlots();
                }

                UpdateDragPosition(position);

                if (debugMode) Debug.Log($"Ghost visible with texture: {texture.name}");
            }
        }
    }

    // Optional: Highlight empty slots during drag
    private void HighlightEmptySlots()
    {
        if (slots == null) return;

        foreach (var slot in slots) {
            if (slot != null && slot != draggedSlot && !slot.HasItem) {
                // Add visual indicator for empty slots
                slot.style.borderTopColor = new Color(0, 1, 0, 0.5f);
                slot.style.borderRightColor = new Color(0, 1, 0, 0.5f);
                slot.style.borderBottomColor = new Color(0, 1, 0, 0.5f);
                slot.style.borderLeftColor = new Color(0, 1, 0, 0.5f);
                slot.style.borderTopWidth = 3;
                slot.style.borderRightWidth = 3;
                slot.style.borderBottomWidth = 3;
                slot.style.borderLeftWidth = 3;
            }
        }
    }

    // Optional: Remove empty slot highlighting
    private void RemoveEmptySlotHighlighting()
    {
        if (slots == null) return;

        foreach (var slot in slots) {
            if (slot != null) {
                // Reset border to default (let USS handle it)
                slot.style.borderTopColor = new StyleColor(StyleKeyword.Null);
                slot.style.borderRightColor = new StyleColor(StyleKeyword.Null);
                slot.style.borderBottomColor = new StyleColor(StyleKeyword.Null);
                slot.style.borderLeftColor = new StyleColor(StyleKeyword.Null);
                slot.style.borderTopWidth = new StyleFloat(StyleKeyword.Null);
                slot.style.borderRightWidth = new StyleFloat(StyleKeyword.Null);
                slot.style.borderBottomWidth = new StyleFloat(StyleKeyword.Null);
                slot.style.borderLeftWidth = new StyleFloat(StyleKeyword.Null);
            }
        }
    }

    private void UpdateDragPosition(Vector2 position)
    {
        if (dragGhost != null) {
            // Center the ghost on cursor (half of 250px slot size)
            dragGhost.style.left = position.x - 125f;
            dragGhost.style.top = position.y - 125f;
        }
    }

    // METHOD 1: RECOMMENDED - Simple and clean
    private void HandleDrop(Vector2 dropPosition)
    {
        if (draggedSlot == null || inventoryController == null) return;

        Slot targetSlot = FindDropTarget(dropPosition);

        if (targetSlot != null && targetSlot != draggedSlot) {
            if (debugMode) {
                string action = targetSlot.HasItem ? "swapping with" : "moving to empty";
                Debug.Log($"Dropping from slot {draggedSlot.SlotIndex} to slot {targetSlot.SlotIndex} ({action})");
            }

            // BEST APPROACH: Always use MoveItem - let inventory handle the logic
            inventoryController.MoveItem(draggedSlot.SlotIndex, targetSlot.SlotIndex);
        } else if (targetSlot == draggedSlot) {
            if (debugMode) Debug.Log("Dropped back on original slot - cancelled");
        } else {
            if (debugMode) Debug.Log("Dropped outside any slot - cancelled");
        }
    }

    /*
    // METHOD 2: Alternative - Explicit swap vs move (more code)
    private void HandleDrop(Vector2 dropPosition)
    {
        if (draggedSlot == null || inventoryController == null) return;
        
        Slot targetSlot = FindDropTarget(dropPosition);
        
        if (targetSlot != null && targetSlot != draggedSlot)
        {
            if (debugMode) Debug.Log($"Dropping from slot {draggedSlot.SlotIndex} to slot {targetSlot.SlotIndex}");
            
            // Check if target slot has an item
            if (targetSlot.HasItem)
            {
                // Swap items
                if (debugMode) Debug.Log("Swapping items between slots");
                inventoryController.SwapSlots(draggedSlot.SlotIndex, targetSlot.SlotIndex);
            }
            else
            {
                // Move to empty slot
                if (debugMode) Debug.Log("Moving item to empty slot");
                inventoryController.MoveItem(draggedSlot.SlotIndex, targetSlot.SlotIndex);
            }
        }
        else
        {
            if (debugMode) Debug.Log("Dropped outside valid target - cancelled");
        }
    }
    */

    private Slot FindDropTarget(Vector2 position)
    {
        if (slots == null) return null;

        foreach (var slot in slots) {
            if (slot != null && slot != draggedSlot) {
                var bounds = slot.worldBound;
                // Add a little padding for easier dropping
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
        // Restore original icon opacity
        if (draggedSlot != null && draggedSlot.Icon != null) {
            draggedSlot.Icon.style.opacity = 1f;
        }

        // Optional: Remove empty slot highlighting
        if (highlightEmptySlots) {
            RemoveEmptySlotHighlighting();
        }

        // Hide ghost
        if (dragGhost != null) {
            dragGhost.style.visibility = Visibility.Hidden;
            dragGhost.style.backgroundImage = null;
        }

        // Reset state
        isDragging = false;
        draggedSlot = null;

        if (debugMode) Debug.Log("Drag reset");
    }

    // Public method to check if currently dragging (for other scripts)
    public bool IsDragging() => isDragging;

    // Public method to get currently dragged slot (for other scripts)
    public Slot GetDraggedSlot() => draggedSlot;
}