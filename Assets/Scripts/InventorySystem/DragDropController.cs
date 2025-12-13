using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// **DragDropController** - Main controller for inventory drag and drop functionality.
/// 
/// It uses a modular architecture with helper classes to keep the code organized:
/// - DragState: Tracks what's being dragged and from where
/// - DragVisualHandler: Manages the visual ghost that follows your mouse
/// - TransactionFactory: Creates the right type of operation (move/swap/equip/unequip)
/// - Transaction classes: Execute specific drag/drop operations
/// 
/// Think of this like a game of moving chess pieces:
/// 1. Player clicks on a piece (PointerDown)
/// 2. Player drags the piece (PointerMove)
/// 3. Player releases the piece (PointerUp)
/// 4. We check if the move is valid
/// 5. We execute the move (transaction)
/// 
/// **Architecture Benefits:**
/// - Easy to understand: Each class does ONE thing
/// - Easy to test: Can test each transaction type separately
/// - Easy to extend: Want a new slot type? Just add it to InitializeEquipmentSlots!
/// - Easy to debug: Clear separation makes finding bugs easier
[DisallowMultipleComponent]
public class DragDropController : MonoBehaviour
{
    #region Serialized Fields (Inspector-visible settings)
    [Header("References")]
    [Tooltip("Controls the inventory UI and data")]
    [SerializeField] private InventoryController inventoryController;

    [Tooltip("Controls equipped items (weapon, armor, etc.)")]
    [SerializeField] private EquipmentController equipmentController;

    [Tooltip("The UI Document that contains all UI elements")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Drag Settings")]
    [Tooltip("How many pixels to move before drag starts (prevents accidental drags)")]
    [SerializeField] private float dragThreshold = 5f;

    [Tooltip("Enable detailed logging to Console (helpful for debugging)")]
    [SerializeField] private bool debugMode = false;

    [Tooltip("Show highlighting on empty slots during drag")]
    [SerializeField] private bool highlightEmptySlots = true;
    #endregion

    #region Private Fields (Internal state - not visible in Inspector)
    // DragState tracks what we're dragging (inventory item or equipment)
    private DragState dragState;

    // Visual handler shows the "ghost" image that follows your mouse
    private DragVisualHandler visualHandler;

    // Double-click handler detects quick equip/unequip actions
    private DoubleClickHandler doubleClickHandler;

    // Cached references to slots (for performance - avoids constant lookups)
    private List<Slot> inventorySlots;
    private List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>();

    // Flag to ensure we only initialize once
    private bool isInitialized;
    #endregion

    #region Unity Lifecycle Methods
    /// Called when the script instance is being loaded.
    /// This happens BEFORE Start, good for setting up references.
    private void Awake()
    {
        inventoryController ??= GetComponent<InventoryController>();
        equipmentController ??= GetComponent<EquipmentController>();
        uiDocument ??= GetComponent<UIDocument>();

        dragState = new DragState();
        doubleClickHandler = new DoubleClickHandler(0.3f);
    }

    /// Called when the GameObject becomes active.
    /// Perfect for registering event listeners.
    private void OnEnable()
    {
        if (!isInitialized) {
            StartCoroutine(Initialize());
        }
    }

    /// Called when the GameObject becomes inactive.
    /// IMPORTANT: Always clean up event listeners to prevent memory leaks!
    private void OnDisable()
    {
        Cleanup();
    }
    #endregion

    #region Initialization
    /// Sets up all the drag/drop system components.
    /// 
    /// Why use a Coroutine (IEnumerator)?
    /// - UI Elements need time to load and be ready
    /// - "yield return null" waits one frame, giving Unity time to set things up
    /// - This prevents trying to access UI before it's ready (would cause errors!)
    private IEnumerator Initialize()
    {
        // Wait 2 frames for UI to fully load and USS styles to be applied
        yield return null;
        yield return null;

        // Wait up to 60 frames (about 1 second) for inventory slots to be ready
        int maxWait = 60;
        int waited = 0;
        while (inventoryController?.Slots == null && waited < maxWait) {
            yield return null; // Wait one frame
            waited++;
        }

        // If slots still aren't ready after waiting, something's wrong!
        if (inventoryController?.Slots == null) {
            if (debugMode) Debug.LogError("[DragDrop] Inventory slots not found");
            yield break; // Exit coroutine early
        }

        // Cache the slots (storing them now is faster than looking them up every frame)
        inventorySlots = new List<Slot>(inventoryController.Slots);
        equipmentSlots.Clear();

        // Set up equipment slots (weapon, headgear, vest, boots)
        InitializeEquipmentSlots();

        // Create the visual handler that shows the drag ghost
        visualHandler = new DragVisualHandler(
            uiDocument.rootVisualElement,
            inventorySlots,
            highlightEmptySlots
        );

        // Hook up all our event listeners (click, drag, release)
        RegisterEventHandlers();

        isInitialized = true;

        if (debugMode) Debug.Log($"[DragDrop] Initialized with {inventorySlots.Count} inventory slots and {equipmentSlots.Count} equipment slots");
    }

    /// Finds and sets up all equipment slots in the UI.
    /// Equipment slots are: Weapon, Headgear, Vest (armor), Boots.
    /// Each one needs to know what type it is so it can validate items.
    private void InitializeEquipmentSlots()
    {
        // The first string is the UI element name in the .uxml file
        // The second parameter is the slot type enum
        InitializeEquipmentSlot("equipSlotWeapon", EquipmentSlotType.Weapon);
        InitializeEquipmentSlot("equipSlotHeadgear", EquipmentSlotType.Headgear);
        InitializeEquipmentSlot("equipSlotArmor", EquipmentSlotType.Vest);
        InitializeEquipmentSlot("equipSlotBoots", EquipmentSlotType.Boots);
    }

    /// Helper method to set up one equipment slot.
    /// This method:
    /// 1. Finds the slot by name in the UI
    /// 2. Tells it what type it is (Weapon, Headgear, etc.)
    /// 3. Adds it to our list for easy access later
    private void InitializeEquipmentSlot(string slotName, EquipmentSlotType slotType)
    {
        if (uiDocument?.rootVisualElement == null) return;

        // Q<T>() is like GameObject.Find but for UI Elements
        var equipmentSlot = uiDocument.rootVisualElement.Q<EquipmentSlot>(slotName);

        if (equipmentSlot != null) {
            // Safety check: remove if already in list (prevents duplicates)
            equipmentSlots.RemoveAll(s => s == equipmentSlot);

            // Tell the slot what type it is
            equipmentSlot.Initialize(slotType);

            // Add to our list
            equipmentSlots.Add(equipmentSlot);

            // Update its visual appearance
            equipmentSlot.RefreshVisualState();

            if (debugMode) Debug.Log($"[DragDrop] Initialized equipment slot: {slotName} ({slotType})");
        } else {
            if (debugMode) Debug.LogWarning($"[DragDrop] Equipment slot not found: {slotName}");
        }
    }
    #endregion

    #region Event Registration
    /// Hook up all mouse event listeners.
    /// 
    /// UI Toolkit uses callback-based events (like JavaScript):
    /// - RegisterCallback<EventType>(method) = "When this event happens, call this method"
    /// - UnregisterCallback<EventType>(method) = "Stop listening for this event"
    private void RegisterEventHandlers()
    {
        RegisterInventorySlotEvents();
        RegisterEquipmentSlotEvents();
    }

    /// Listen for mouse events on inventory slots.
    /// 
    /// We listen for 3 events:
    /// 1. PointerDown = Mouse button pressed (start of potential drag)
    /// 2. PointerMove = Mouse moved (dragging)
    /// 3. PointerUp = Mouse button released (end of drag, perform action)
    private void RegisterInventorySlotEvents()
    {
        if (inventorySlots == null) return;

        foreach (var slot in inventorySlots) {
            if (slot == null) continue;

            // Always unregister first to prevent double-registration
            slot.UnregisterCallback<PointerDownEvent>(OnInventorySlotPointerDown);
            slot.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            slot.UnregisterCallback<PointerUpEvent>(OnInventorySlotPointerUp);

            // Now register our methods
            slot.RegisterCallback<PointerDownEvent>(OnInventorySlotPointerDown);
            slot.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            slot.RegisterCallback<PointerUpEvent>(OnInventorySlotPointerUp);
        }
    }

    /// Listen for mouse events on equipment slots.
    /// Same as inventory, but for equipment (weapon, armor, etc.)
    private void RegisterEquipmentSlotEvents()
    {
        foreach (var slot in equipmentSlots) {
            if (slot == null) continue;

            slot.UnregisterCallback<PointerDownEvent>(OnEquipmentSlotPointerDown);
            slot.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            slot.UnregisterCallback<PointerUpEvent>(OnEquipmentSlotPointerUp);

            slot.RegisterCallback<PointerDownEvent>(OnEquipmentSlotPointerDown);
            slot.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            slot.RegisterCallback<PointerUpEvent>(OnEquipmentSlotPointerUp);
        }
    }

    /// Remove all event listeners and reset state.
    /// 
    /// CRITICAL: If you don't unregister callbacks, they keep running even after
    /// the GameObject is destroyed, causing memory leaks and errors!
    private void Cleanup()
    {
        UnregisterInventorySlotEvents();
        UnregisterEquipmentSlotEvents();

        equipmentSlots.Clear();
        visualHandler?.Cleanup();
        dragState?.Reset();

        isInitialized = false;
    }

    private void UnregisterInventorySlotEvents()
    {
        if (inventorySlots == null) return;

        foreach (var slot in inventorySlots) {
            if (slot == null) continue;

            slot.UnregisterCallback<PointerDownEvent>(OnInventorySlotPointerDown);
            slot.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            slot.UnregisterCallback<PointerUpEvent>(OnInventorySlotPointerUp);
        }
    }

    private void UnregisterEquipmentSlotEvents()
    {
        foreach (var slot in equipmentSlots) {
            if (slot == null) continue;

            slot.UnregisterCallback<PointerDownEvent>(OnEquipmentSlotPointerDown);
            slot.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            slot.UnregisterCallback<PointerUpEvent>(OnEquipmentSlotPointerUp);
        }
    }
    #endregion

    #region Pointer Event Handlers
    /// Called when mouse button is pressed on inventory slot.
    /// 
    /// This is the START of a potential drag operation.
    /// We don't start dragging yet - user might just be clicking!
    /// We wait until they move the mouse a bit (see OnPointerMove).
    /// Also detects double-clicks for quick equip!
    private void OnInventorySlotPointerDown(PointerDownEvent evt)
    {
        // evt.button 0 = left mouse button (1 = right, 2 = middle)
        if (evt.button != 0 || dragState.IsDragging) return;

        // Get the actual Slot component (might click on child element)
        var slot = evt.target as Slot ?? GetSlotFromParent(evt.target as VisualElement);
        if (slot == null || !slot.HasItem) return;

        if (debugMode) Debug.Log($"[DragDrop] Pointer down on inventory slot {slot.SlotIndex}");

        // Check for double-click BEFORE starting drag tracking
        bool isDoubleClick = doubleClickHandler.RegisterClick(slot.SlotIndex, isEquipmentSlot: false);

        if (isDoubleClick) {
            // Double-click detected! Quick equip the item
            HandleQuickEquip(slot);
            evt.StopPropagation();
            return; // Don't start drag operation
        }

        // Remember where drag started and what slot
        dragState.StartPosition = evt.position;
        dragState.SourceInventorySlot = slot;
        dragState.SourceEquipmentSlot = null; // Not from equipment

        // "Capture" the pointer - all pointer events now go to this slot until released
        // This ensures we don't lose the drag if mouse moves fast
        slot.CapturePointer(evt.pointerId);

        // Stop this event from bubbling up to parent elements
        evt.StopPropagation();
    }

    /// Called when mouse button pressed on equipment slot.
    /// Same as inventory, but for equipment slots (weapon, armor, etc.)
    /// Also detects double-clicks for quick unequip!
    private void OnEquipmentSlotPointerDown(PointerDownEvent evt)
    {
        if (evt.button != 0 || dragState.IsDragging) return;

        var equipmentSlot = GetEquipmentSlotFromTarget(evt.target as VisualElement);
        if (equipmentSlot == null) return;

        if (debugMode) Debug.Log($"[DragDrop] Pointer down on equipment slot: {equipmentSlot.SlotType}");

        // Check for double-click BEFORE starting drag tracking
        bool isDoubleClick = doubleClickHandler.RegisterClick(-1, isEquipmentSlot: true, equipmentSlot.SlotType);

        if (isDoubleClick && equipmentSlot.HasItem) {
            // Double-click detected on equipped item! Quick unequip it
            HandleQuickUnequip(equipmentSlot);
            evt.StopPropagation();
            return; // Don't start drag operation
        }

        dragState.StartPosition = evt.position;
        dragState.SourceEquipmentSlot = equipmentSlot;
        dragState.SourceInventorySlot = null; // Not from inventory

        equipmentSlot.CapturePointer(evt.pointerId);
        evt.StopPropagation();
    }

    /// Called when mouse moves (while pointer is captured).
    /// 
    /// This is where we:
    /// 1. Check if user moved far enough to start dragging
    /// 2. Show the drag ghost image
    /// 3. Update visual feedback (green = valid drop, red = invalid)
    private void OnPointerMove(PointerMoveEvent evt)
    {
        // Make sure we have a valid source (either inventory or equipment)
        if (!dragState.HasValidSource) return;

        // Calculate how far mouse has moved from start position
        float distance = Vector2.Distance(dragState.StartPosition, evt.position);

        // Did they move far enough to start dragging?
        // (prevents accidental drags from small mouse movements)
        if (!dragState.IsDragging && distance > dragThreshold) {
            StartDrag(evt.position);
        }

        // If currently dragging, update visuals
        if (dragState.IsDragging && visualHandler != null) {
            UpdateDragVisuals(evt.position);
            evt.StopPropagation();
        }
    }

    /// Mouse button released on inventory slot.
    /// 
    /// This is the END of the drag operation - time to perform the action!
    private void OnInventorySlotPointerUp(PointerUpEvent evt)
    {
        if (dragState.SourceInventorySlot != null && dragState.SourceInventorySlot.HasPointerCapture(evt.pointerId)) {
            // If we were dragging, handle the drop
            if (dragState.IsDragging) HandleDrop(evt.position);

            // Release pointer capture (stop receiving events)
            dragState.SourceInventorySlot.ReleasePointer(evt.pointerId);

            // Clean up drag state
            ResetDrag();
        }
    }

    /// Mouse button released on equipment slot.
    /// Same as inventory but for equipment.
    private void OnEquipmentSlotPointerUp(PointerUpEvent evt)
    {
        if (dragState.SourceEquipmentSlot != null && dragState.SourceEquipmentSlot.HasPointerCapture(evt.pointerId)) {
            if (dragState.IsDragging) HandleDrop(evt.position);
            dragState.SourceEquipmentSlot.ReleasePointer(evt.pointerId);
            ResetDrag();
        }
    }
    #endregion

    #region Drag Operations
    /// Begin dragging (user moved mouse far enough).
    /// This shows the "ghost" image that follows your mouse.
    private void StartDrag(Vector2 position)
    {
        dragState.IsDragging = true;

        // Get the item being dragged
        var draggedItem = dragState.GetDraggedItem(inventoryController);
        if (draggedItem == null) {
            ResetDrag();
            return;
        }

        if (debugMode) {
            string source = dragState.IsFromInventory
                ? $"inventory slot {dragState.SourceInventorySlot.SlotIndex}"
                : $"equipment slot {dragState.SourceEquipmentSlot.SlotType}";
            Debug.Log($"[DragDrop] Starting drag from {source}");
        }

        // Add "dragging" visual effect to source slot
        if (dragState.IsFromInventory) {
            visualHandler?.AddDraggingClass(dragState.SourceInventorySlot);
        }

        // Show the ghost image (item icon following mouse)
        visualHandler?.ShowGhost(draggedItem.itemData.icon, position);
    }

    /// Update ghost position and show drop feedback.
    /// 
    /// As you drag, this updates:
    /// - Ghost image position (follows mouse)
    /// - Visual feedback (green = can drop here, red = can't drop here)
    private void UpdateDragVisuals(Vector2 position)
    {
        // Move ghost to current mouse position
        visualHandler?.UpdatePosition(position);

        // Find what slot the mouse is currently over
        var targetEquipment = FindEquipmentSlotAtPosition(position);
        var targetInventory = targetEquipment == null ? FindInventorySlotAtPosition(position) : null;

        // Check if this is a valid drop location
        bool isValidDrop = ValidateDropTarget(targetInventory, targetEquipment);

        // Update ghost appearance (green border = valid, red = invalid)
        visualHandler?.UpdateAppearance(isValidDrop);
    }

    /// Clean up after drag ends.
    /// This removes all visual effects and resets state.
    private void ResetDrag()
    {
        // Remove "dragging" class from source slot
        if (dragState.IsFromInventory) {
            visualHandler?.RemoveDraggingClass(dragState.SourceInventorySlot);
        }

        // Hide ghost image
        visualHandler?.HideGhost();

        // Reset drag state
        dragState.Reset();

        if (debugMode) Debug.Log("[DragDrop] Drag reset");
    }
    #endregion

    #region Drop Handling
    /// Execute the drag/drop action.
    /// 
    /// Based on where you dragged FROM and TO, we create the right type of "transaction":
    /// - Inventory → Inventory = Move or swap items
    /// - Inventory → Equipment = Equip item
    /// - Equipment → Inventory = Unequip item
    /// - Equipment → Equipment = Swap equipped items
    /// 
    /// We use a "Transaction" pattern (like database transactions):
    /// 1. Create a transaction object describing the operation
    /// 2. Validate it can be executed (CanExecute)
    /// 3. Execute it if valid
    private void HandleDrop(Vector2 dropPosition)
    {
        if (debugMode) Debug.Log($"[DragDrop] HandleDrop at position: {dropPosition}");

        // Find what slot we dropped on
        var targetEquipment = FindEquipmentSlotAtPosition(dropPosition);
        var targetInventory = targetEquipment == null ? FindInventorySlotAtPosition(dropPosition) : null;

        // Dropped outside any valid slot? Cancel!
        if (targetEquipment == null && targetInventory == null) {
            if (debugMode) Debug.Log("[DragDrop] Dropped outside any slot - cancelled");
            return;
        }

        // Create the appropriate transaction using the Factory pattern
        // The factory looks at source + target and creates the right transaction type
        var transaction = TransactionFactory.CreateTransaction(
            dragState,              // What we're dragging (source)
            targetInventory,        // Where we're dropping (inventory)
            targetEquipment,        // Where we're dropping (equipment)
            inventorySlots,         // All inventory slots (for swapping)
            inventoryController,    // To modify inventory data
            equipmentController,    // To modify equipment data
            debugMode              // Debug logging
        );

        // Can we execute this transaction?
        if (transaction != null && transaction.CanExecute()) {
            // Add visual feedback to target slot
            var targetVisual = transaction.GetTargetVisual();
            targetVisual?.AddToClassList("inventorySlots--drop-target");

            // Execute the transaction (as a coroutine for smooth animation)
            StartCoroutine(ExecuteTransaction(transaction));
        } else {
            if (debugMode) Debug.Log("[DragDrop] Transaction cannot be executed");
        }
    }

    /// Run the transaction as a coroutine.
    /// Allows for smooth animations and visual feedback before performing data changes.
    private IEnumerator ExecuteTransaction(DragDropTransaction transaction)
    {
        yield return transaction.Execute();
    }
    #endregion

    #region Validation
    /// Check if we can drop here.
    /// 
    /// Provides real-time feedback while dragging:
    /// - Green = Valid drop location
    /// - Red = Invalid drop location
    /// 
    /// Rules:
    /// - Can't drop on the same slot you're dragging from
    /// - Equipment slots validate item types (e.g., only armor in armor slot)
    /// - Inventory slots always accept items (will swap if occupied)
    private bool ValidateDropTarget(Slot targetInventory, EquipmentSlot targetEquipment)
    {
        // Validating equipment slot
        if (targetEquipment != null) {
            var draggedItem = dragState.GetDraggedItem(inventoryController);
            if (draggedItem == null) return false;

            // Can't drop on same equipment slot we're dragging from
            if (dragState.IsFromEquipment && dragState.SourceEquipmentSlot == targetEquipment) {
                return false;
            }

            // Does the equipment slot accept this item type?
            // (e.g., only Armor items in armor slot, only Weapons in weapon slot)
            return targetEquipment.CanAcceptItem(draggedItem.itemData);
        }

        // Validating inventory slot
        if (targetInventory != null) {
            // Can't drop on same inventory slot we're dragging from
            if (dragState.IsFromInventory && dragState.SourceInventorySlot == targetInventory) {
                return false;
            }

            // Inventory always accepts items (will swap if occupied)
            return true;
        }

        return false;
    }
    #endregion

    #region Helper Methods
    /// Find Slot component by walking up the UI hierarchy.
    /// 
    /// Sometimes you click on a child element (like an icon) instead of the slot itself.
    /// This walks up the parent chain to find the actual Slot component.
    private Slot GetSlotFromParent(VisualElement element)
    {
        while (element != null) {
            if (element is Slot slot) return slot;
            element = element.parent;
        }
        return null;
    }

    /// Find EquipmentSlot by walking up UI hierarchy.
    /// Same as GetSlotFromParent but for equipment slots.
    private EquipmentSlot GetEquipmentSlotFromTarget(VisualElement element)
    {
        if (element == null) return null;
        if (element is EquipmentSlot slot) return slot;

        var parent = element.parent;
        while (parent != null) {
            if (parent is EquipmentSlot parentSlot) return parentSlot;
            parent = parent.parent;
        }

        return null;
    }

    /// Find which equipment slot is at mouse position.
    /// 
    /// Uses worldBound (the slot's screen rectangle) to check if mouse is inside.
    /// Added padding makes it easier to drop - don't need pixel-perfect accuracy!
    private EquipmentSlot FindEquipmentSlotAtPosition(Vector2 position)
    {
        foreach (var slot in equipmentSlots) {
            if (slot == null) continue;

            // Add 20 pixel padding on all sides for easier dropping
            // Using extension method for cleaner code
            var paddedBounds = slot.worldBound.Padded(20);

            if (paddedBounds.Contains(position)) return slot;
        }

        return null;
    }

    /// Find which inventory slot is at mouse position.
    /// Same as equipment but with smaller padding (10 pixels instead of 20).
    private Slot FindInventorySlotAtPosition(Vector2 position)
    {
        if (inventorySlots == null) return null;

        foreach (var slot in inventorySlots) {
            if (slot == null) continue;

            // Add 10 pixel padding using extension method
            var paddedBounds = slot.worldBound.Padded(10);

            if (paddedBounds.Contains(position)) {
                if (debugMode) Debug.Log($"[DragDrop] Found drop target slot at position {position}");
                return slot;
            }
        }

        if (debugMode) Debug.Log($"[DragDrop] No drop target found at position {position}");
        return null;
    }
    #endregion

    #region Public API
    /// Check if currently dragging something.
    /// Useful for other systems (e.g., disable other UI while dragging).
    public bool IsDragging() => dragState?.IsDragging ?? false;

    /// Get the inventory slot being dragged (if any).
    /// Returns null if dragging from equipment or not dragging at all.
    public Slot GetDraggedSlot() => dragState?.SourceInventorySlot;
    #endregion

    #region Double-Click Handlers
    /// Double-clicked an inventory item, equip it automatically.
    /// 
    /// Creates a QuickEquipTransaction which:
    /// 1. Finds the right equipment slot for the item
    /// 2. If occupied, swaps items
    /// 3. If empty, just equips
    /// 
    /// It's like a shortcut - no dragging needed!
    private void HandleQuickEquip(Slot slot)
    {
        if (slot == null || !slot.HasItem) return;

        var item = inventoryController?.GetItemAtSlot(slot.SlotIndex);
        if (item == null) return;

        if (debugMode) Debug.Log($"[DragDrop] Quick equip: {item.itemData.itemName} from slot {slot.SlotIndex}");

        var transaction = new QuickEquipTransaction(
            item,
            slot.SlotIndex,
            slot,
            equipmentSlots,
            inventorySlots,
            inventoryController,
            equipmentController,
            debugMode
        );

        if (transaction.CanExecute()) {
            StartCoroutine(ExecuteTransaction(transaction));
        } else {
            if (debugMode) Debug.Log($"[DragDrop] Cannot quick equip {item.itemData.itemName}");
        }
    }

    /// Double-clicked an equipped item, unequip it automatically.
    /// 
    /// Creates a QuickUnequipTransaction which:
    /// 1. Finds the first empty inventory slot
    /// 2. Moves the equipped item there
    /// 
    /// If inventory is full, nothing happens (item stays equipped).
    private void HandleQuickUnequip(EquipmentSlot equipmentSlot)
    {
        if (equipmentSlot == null || !equipmentSlot.HasItem) return;

        var item = equipmentSlot.GetEquippedItem();
        if (item == null) return;

        if (debugMode) Debug.Log($"[DragDrop] Quick unequip: {item.itemData.itemName} from {equipmentSlot.SlotType} slot");

        var transaction = new QuickUnequipTransaction(
            equipmentSlot,
            item,
            inventorySlots,
            inventoryController,
            equipmentController,
            debugMode
        );

        if (transaction.CanExecute()) {
            StartCoroutine(ExecuteTransaction(transaction));
        } else {
            if (debugMode) Debug.Log($"[DragDrop] Cannot quick unequip {item.itemData.itemName} (inventory full?)");
        }
    }
    #endregion
}
