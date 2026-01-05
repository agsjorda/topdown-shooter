using InventorySystem;
using InventorySystem.DragDrop.Transactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class DragDropController : MonoBehaviour
{
    // Inspector-exposed fields for references and settings
    [Header("References")]
    [Tooltip("Controls the inventory ViewModel and data")]
    [SerializeField] private InventoryViewModel inventoryViewModel;

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

    // Internal state and references
    private DragDropService dragDropService;
    private DragVisualHandler visualHandler;
    private DoubleClickHandler doubleClickHandler;
    private List<SlotView> inventorySlots;
    private List<EquipmentSlotView> equipmentSlots = new List<EquipmentSlotView>();
    private bool isInitialized;
    private InventoryUIConfig uiConfig;

    // Unity lifecycle method for initialization
    private void Awake()
    {
        equipmentController ??= GetComponent<EquipmentController>();
        uiDocument ??= GetComponent<UIDocument>();
        doubleClickHandler = new DoubleClickHandler(0.3f);
        
        // Use InventoryService instead of FindFirstObjectByType
        if (inventoryViewModel == null) {
            inventoryViewModel = InventoryService.GetPlayerInventoryViewModel();
            if (inventoryViewModel != null) {
                if (debugMode) Debug.Log("[DragDrop] InventoryViewModel obtained from InventoryService");
            } else if (debugMode) {
                Debug.LogError("[DragDrop] No InventoryModel found via InventoryService.");
            }
        }
    }

    // Register event handlers and initialize on enable
    private void OnEnable()
    {
        if (!isInitialized) {
            StartCoroutine(Initialize());
        }
    }

    // Cleanup on disable
    private void OnDisable()
    {
        Cleanup();
    }

    // Coroutine to initialize UI and drag-drop system
    private IEnumerator Initialize()
    {
        yield return null;
        yield return null;
        int maxWait = 60;
        int waited = 0;
        while ((inventoryViewModel == null || inventoryViewModel.Items == null) && waited < maxWait) {
            yield return null;
            waited++;
        }
        if (inventoryViewModel == null || inventoryViewModel.Items == null) {
            if (debugMode) Debug.LogError("[DragDrop] InventoryViewModel or Items not found");
            yield break;
        }
        uiConfig = Object.FindFirstObjectByType<InventoryUIConfig>();
        inventorySlots = uiConfig != null ? new List<SlotView>(uiConfig.Slots) : new List<SlotView>();
        equipmentSlots.Clear();
        InitializeEquipmentSlots();
        visualHandler = new DragVisualHandler(
            uiDocument.rootVisualElement,
            inventorySlots,
            highlightEmptySlots
        );
        
        // Use IEquipmentSystem interface
        IEquipmentSystem equipmentSystem = equipmentController;
        dragDropService = new DragDropService(
            inventoryViewModel,
            equipmentSystem,
            inventorySlots,
            equipmentSlots,
            visualHandler,
            debugMode
        );
        RegisterEventHandlers();
        RegisterServiceEvents();
        isInitialized = true;
        if (debugMode) Debug.Log($"[DragDrop] Initialized with {inventorySlots.Count} inventory slots and {equipmentSlots.Count} equipment slots");
    }

    // Find and initialize all equipment slots in the UI
    private void InitializeEquipmentSlots()
    {
        InitializeEquipmentSlot("equipSlotWeapon", EquipmentSlotType.Weapon);
        InitializeEquipmentSlot("equipSlotHeadgear", EquipmentSlotType.Headgear);
        InitializeEquipmentSlot("equipSlotArmor", EquipmentSlotType.Vest);
        InitializeEquipmentSlot("equipSlotBoots", EquipmentSlotType.Boots);
    }

    // Helper to initialize a single equipment slot
    private void InitializeEquipmentSlot(string slotName, EquipmentSlotType slotType)
    {
        if (uiDocument?.rootVisualElement == null) return;
        var equipmentSlot = uiDocument.rootVisualElement.Q<EquipmentSlotView>(slotName);
        if (equipmentSlot != null) {
            equipmentSlots.RemoveAll(s => s == equipmentSlot);
            equipmentSlot.Initialize(slotType);
            equipmentSlots.Add(equipmentSlot);
            equipmentSlot.RefreshVisualState();
            if (debugMode) Debug.Log($"[DragDrop] Initialized equipment slot: {slotName} ({slotType})");
        } else {
            if (debugMode) Debug.LogWarning($"[DragDrop] Equipment slot not found: {slotName}");
        }
    }

    // Register UI event handlers for drag and drop
    private void RegisterEventHandlers()
    {
        RegisterInventorySlotEvents();
        RegisterEquipmentSlotEvents();
    }

    // Register mouse events for inventory slots
    private void RegisterInventorySlotEvents()
    {
        if (inventorySlots == null) return;
        foreach (var slot in inventorySlots) {
            if (slot == null) continue;
            slot.UnregisterCallback<PointerDownEvent>(OnInventorySlotPointerDown);
            slot.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            slot.UnregisterCallback<PointerUpEvent>(OnInventorySlotPointerUp);
            slot.RegisterCallback<PointerDownEvent>(OnInventorySlotPointerDown);
            slot.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            slot.RegisterCallback<PointerUpEvent>(OnInventorySlotPointerUp);
        }
    }

    // Register mouse events for equipment slots
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

    // Subscribe to events from the drag-drop service for UI updates
    private void RegisterServiceEvents()
    {
        dragDropService.OnDragStarted += (position, draggedItem) =>
        {
            if (dragDropService.DragState.IsFromInventory)
                visualHandler?.AddDraggingClass(dragDropService.DragState.SourceInventorySlot);
            visualHandler?.ShowGhost(draggedItem.itemData.icon, position);
        };
        dragDropService.OnDragUpdated += (position) =>
        {
            visualHandler?.UpdatePosition(position);
        };
        dragDropService.OnDragEnded += () =>
        {
            if (dragDropService.DragState.IsFromInventory)
                visualHandler?.RemoveDraggingClass(dragDropService.DragState.SourceInventorySlot);
            visualHandler?.HideGhost();
        };
        dragDropService.OnTransactionReady += (transaction) =>
        {
            var targetVisual = transaction.GetTargetVisual();
            targetVisual?.AddToClassList("inventorySlots--drop-target");
            StartCoroutine(ExecuteTransaction(transaction));
        };
        dragDropService.OnDebugLog += (msg) => { if (debugMode) Debug.Log(msg); };
    }

    // Cleanup event handlers and state
    private void Cleanup()
    {
        UnregisterInventorySlotEvents();
        UnregisterEquipmentSlotEvents();
        equipmentSlots.Clear();
        visualHandler?.Cleanup();
        dragDropService?.ResetDrag();
        isInitialized = false;
    }

    // Unregister inventory slot events
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

    // Unregister equipment slot events
    private void UnregisterEquipmentSlotEvents()
    {
        foreach (var slot in equipmentSlots) {
            if (slot == null) continue;
            slot.UnregisterCallback<PointerDownEvent>(OnEquipmentSlotPointerDown);
            slot.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            slot.UnregisterCallback<PointerUpEvent>(OnEquipmentSlotPointerUp);
        }
    }

    // Shared handler for pointer down events
    private void HandleSlotPointerDown<TSlot>(PointerDownEvent evt, TSlot slot, bool isEquipmentSlot)
        where TSlot : VisualElement
    {
        if (evt.button != 0 || dragDropService.DragState.IsDragging || slot == null) return;
        if (isEquipmentSlot && slot is EquipmentSlotView equipmentSlot)
        {
            if (debugMode) Debug.Log($"[DragDrop] Pointer down on equipment slot: {equipmentSlot.SlotType}");
            bool isDoubleClick = doubleClickHandler.RegisterClick(-1, isEquipmentSlot: true, equipmentSlot.SlotType);
            if (isDoubleClick && equipmentSlot.HasItem) {
                HandleQuickUnequip(equipmentSlot);
                evt.StopPropagation();
                return;
            }
            dragDropService.DragState.StartPosition = evt.position;
            dragDropService.DragState.SourceEquipmentSlot = equipmentSlot;
            dragDropService.DragState.SourceInventorySlot = null;
            equipmentSlot.CapturePointer(evt.pointerId);
        }
        else if (!isEquipmentSlot && slot is SlotView inventorySlot && inventorySlot.HasItem)
        {
            if (debugMode) Debug.Log($"[DragDrop] Pointer down on inventory slot {inventorySlot.SlotIndex}");
            bool isDoubleClick = doubleClickHandler.RegisterClick(inventorySlot.SlotIndex, isEquipmentSlot: false);
            if (isDoubleClick) {
                HandleQuickEquip(inventorySlot);
                evt.StopPropagation();
                return;
            }
            dragDropService.DragState.StartPosition = evt.position;
            dragDropService.DragState.SourceInventorySlot = inventorySlot;
            dragDropService.DragState.SourceEquipmentSlot = null;
            inventorySlot.CapturePointer(evt.pointerId);
        }
        evt.StopPropagation();
    }

    // Shared handler for pointer up events
    private void HandleSlotPointerUp<TSlot>(PointerUpEvent evt, TSlot slot, bool isEquipmentSlot)
        where TSlot : VisualElement
    {
        if (slot == null) return;
        if (isEquipmentSlot && slot is EquipmentSlotView equipmentSlot)
        {
            if (dragDropService.DragState.SourceEquipmentSlot != null && equipmentSlot.HasPointerCapture(evt.pointerId)) {
                if (dragDropService.DragState.IsDragging) dragDropService.HandleDrop(evt.position, uiConfig);
                equipmentSlot.ReleasePointer(evt.pointerId);
                dragDropService.EndDrag();
            }
        }
        else if (!isEquipmentSlot && slot is SlotView inventorySlot)
        {
            if (dragDropService.DragState.SourceInventorySlot != null && inventorySlot.HasPointerCapture(evt.pointerId)) {
                if (dragDropService.DragState.IsDragging) dragDropService.HandleDrop(evt.position, uiConfig);
                inventorySlot.ReleasePointer(evt.pointerId);
                dragDropService.EndDrag();
            }
        }
    }

    // Handle pointer down on inventory slot
    private void OnInventorySlotPointerDown(PointerDownEvent evt)
    {
        var slot = evt.target as SlotView ?? GetSlotFromParent(evt.target as VisualElement);
        HandleSlotPointerDown(evt, slot, false);
    }

    // Handle pointer down on equipment slot
    private void OnEquipmentSlotPointerDown(PointerDownEvent evt)
    {
        var equipmentSlot = GetEquipmentSlotFromTarget(evt.target as VisualElement);
        HandleSlotPointerDown(evt, equipmentSlot, true);
    }

    // Handle pointer up on inventory slot
    private void OnInventorySlotPointerUp(PointerUpEvent evt)
    {
        var slot = evt.target as SlotView ?? GetSlotFromParent(evt.target as VisualElement);
        HandleSlotPointerUp(evt, slot, false);
    }

    // Handle pointer up on equipment slot
    private void OnEquipmentSlotPointerUp(PointerUpEvent evt)
    {
        var equipmentSlot = GetEquipmentSlotFromTarget(evt.target as VisualElement);
        HandleSlotPointerUp(evt, equipmentSlot, true);
    }

    // Handle pointer move for drag operation
    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!dragDropService.DragState.HasValidSource) return;
        float distance = Vector2.Distance(dragDropService.DragState.StartPosition, evt.position);
        if (!dragDropService.DragState.IsDragging && distance > dragThreshold) {
            dragDropService.StartDrag(evt.position);
        }
        if (dragDropService.DragState.IsDragging && visualHandler != null) {
            dragDropService.UpdateDrag(evt.position);
            evt.StopPropagation();
        }
    }

    // Helper to find SlotView from a UI element
    private SlotView GetSlotFromParent(VisualElement element)
    {
        while (element != null) {
            if (element is SlotView slot) return slot;
            element = element.parent;
        }
        return null;
    }

    // Helper to find EquipmentSlotView from a UI element
    private EquipmentSlotView GetEquipmentSlotFromTarget(VisualElement element)
    {
        if (element == null) return null;
        if (element is EquipmentSlotView slot) return slot;
        var parent = element.parent;
        while (parent != null) {
            if (parent is EquipmentSlotView parentSlot) return parentSlot;
            parent = parent.parent;
        }
        return null;
    }

    // Public API to check drag state
    public bool IsDragging() => dragDropService?.DragState?.IsDragging ?? false;
    public SlotView GetDraggedSlot() => dragDropService?.DragState?.SourceInventorySlot;

    // Handle double-click to quick-equip an item
    private void HandleQuickEquip(SlotView slot)
    {
        if (slot == null || !slot.HasItem) return;
        var item = inventoryViewModel != null ? inventoryViewModel.GetItemAt(slot.SlotIndex) : null;
        if (item == null) return;
        if (debugMode) Debug.Log($"[DragDrop] Quick equip: {item.itemData.itemName} from slot {slot.SlotIndex}");
        
        IEquipmentSystem equipmentSystem = equipmentController;
        var transaction = new QuickEquipTransaction(
            item,
            slot.SlotIndex,
            slot,
            equipmentSlots,
            inventorySlots,
            inventoryViewModel,
            equipmentSystem,
            debugMode
        );
        if (transaction.CanExecute()) {
            StartCoroutine(ExecuteTransaction(transaction));
        } else {
            if (debugMode) Debug.Log($"[DragDrop] Cannot quick equip {item.itemData.itemName}");
            if (uiConfig != null && uiConfig.TabFilterManager != null)
                uiConfig.TabFilterManager.RefreshAndRebuildUI();
        }
    }

    // Handle double-click to quick-unequip an item
    private void HandleQuickUnequip(EquipmentSlotView equipmentSlot)
    {
        if (equipmentSlot == null || !equipmentSlot.HasItem) return;
        var item = equipmentSlot.GetEquippedItem();
        if (item == null) return;
        if (debugMode) Debug.Log($"[DragDrop] Quick unequip: {item.itemData.itemName} from {equipmentSlot.SlotType} slot");
        
        IEquipmentSystem equipmentSystem = equipmentController;
        var transaction = new QuickUnequipTransaction(
            equipmentSlot,
            item,
            inventorySlots,
            inventoryViewModel,
            equipmentSystem,
            debugMode
        );
        if (transaction.CanExecute()) {
            StartCoroutine(ExecuteTransaction(transaction));
        } else {
            if (debugMode) Debug.Log($"[DragDrop] Cannot quick unequip {item.itemData.itemName} (inventory full?)");
            if (uiConfig != null && uiConfig.TabFilterManager != null) {
                uiConfig.TabFilterManager.RefreshFilteredItems();
                uiConfig.RebuildUI();
            }
        }
    }

    // Coroutine to execute a drag-drop transaction and update UI
    private IEnumerator ExecuteTransaction(DragDropTransaction transaction)
    {
        yield return transaction.Execute();
        dragDropService.ResetDrag();
        if (uiConfig != null && uiConfig.TabFilterManager != null) {
            uiConfig.TabFilterManager.RefreshFilteredItems();
            uiConfig.RebuildUI();
        }
    }
}
