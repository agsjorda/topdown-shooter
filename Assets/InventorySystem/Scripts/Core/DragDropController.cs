using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
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

        [Tooltip("Maximum seconds between clicks to count as a double-click")]
        [SerializeField] private float doubleClickWindow = 0.3f;

        [Tooltip("Enable detailed logging to Console (helpful for debugging)")]
        [SerializeField] private bool debugMode = false;

        [Tooltip("Show highlighting on empty slots during drag")]
        [SerializeField] private bool highlightEmptySlots = true;

        // Internal state and references
        private DragDropService dragDropService;
        private DragVisualHandler visualHandler;
        private DoubleClickHandler doubleClickHandler;
        private readonly List<EquipmentSlotView> equipmentSlots = new List<EquipmentSlotView>();
        private bool isInitialized;
        private InventoryUIConfig uiConfig;
        private VisualElement slotsContainer;

        private static readonly List<SlotView> EmptySlots = new List<SlotView>();

        // Inventory slots are always resolved through the config: it owns the live list,
        // which is recreated whenever the slot layout is rebuilt.
        private List<SlotView> InventorySlots => uiConfig != null ? uiConfig.Slots : EmptySlots;

        // Unity lifecycle method for initialization
        private void Awake()
        {
            equipmentController ??= GetComponent<EquipmentController>();
            uiDocument ??= GetComponent<UIDocument>();
            doubleClickHandler = new DoubleClickHandler(doubleClickWindow);

            // Use the shared ViewModel so all systems observe the same instance
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
                inventoryViewModel ??= InventoryService.GetPlayerInventoryViewModel();
                yield return null;
                waited++;
            }
            if (inventoryViewModel == null || inventoryViewModel.Items == null) {
                if (debugMode) Debug.LogError("[DragDrop] InventoryViewModel or Items not found");
                yield break;
            }
            uiConfig = Object.FindFirstObjectByType<InventoryUIConfig>();
            if (uiConfig == null) {
                if (debugMode) Debug.LogError("[DragDrop] No InventoryUIConfig found in scene");
                yield break;
            }
            equipmentSlots.Clear();
            InitializeEquipmentSlots();
            visualHandler = new DragVisualHandler(
                uiDocument.rootVisualElement,
                () => InventorySlots,
                highlightEmptySlots
            );

            // Use IEquipmentSystem interface
            IEquipmentSystem equipmentSystem = equipmentController;
            dragDropService = new DragDropService(
                inventoryViewModel,
                equipmentSystem,
                equipmentSlots,
                visualHandler,
                debugMode
            );
            RegisterEventHandlers();
            RegisterServiceEvents();
            isInitialized = true;
            if (debugMode) Debug.Log($"[DragDrop] Initialized with {InventorySlots.Count} inventory slots and {equipmentSlots.Count} equipment slots");
        }

        // Find and initialize all equipment slots declared by the config's bindings.
        // Which slots exist (and their types) is per-game data, not code.
        private void InitializeEquipmentSlots()
        {
            if (uiConfig == null || uiConfig.equipmentSlotBindings == null) return;
            foreach (var binding in uiConfig.equipmentSlotBindings) {
                if (binding == null || string.IsNullOrEmpty(binding.elementName) || binding.slotType == null) {
                    if (debugMode) Debug.LogWarning("[DragDrop] Skipping incomplete equipment slot binding");
                    continue;
                }
                InitializeEquipmentSlot(binding.elementName, binding.slotType);
            }
        }

        // Helper to initialize a single equipment slot
        private void InitializeEquipmentSlot(string slotName, EquipmentSlotTypeSO slotType)
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

        // Register UI event handlers for drag and drop.
        // Inventory slot events are delegated to the slots CONTAINER (pointer events bubble
        // up from the slots), so rebuilding/recreating SlotViews can never orphan handlers.
        // Equipment slots are fixed UXML elements, so per-slot registration is safe for them.
        private void RegisterEventHandlers()
        {
            RegisterSlotsContainerEvents();
            RegisterEquipmentSlotEvents();
        }

        private void RegisterSlotsContainerEvents()
        {
            slotsContainer = uiConfig != null ? uiConfig.SlotsContainer : null;
            if (slotsContainer == null) {
                if (debugMode) Debug.LogWarning("[DragDrop] Slots container not found - inventory drag/drop disabled");
                return;
            }
            UnregisterSlotsContainerEvents();
            slotsContainer.RegisterCallback<PointerDownEvent>(OnInventorySlotPointerDown);
            slotsContainer.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            slotsContainer.RegisterCallback<PointerUpEvent>(OnInventorySlotPointerUp);
        }

        private void UnregisterSlotsContainerEvents()
        {
            if (slotsContainer == null) return;
            slotsContainer.UnregisterCallback<PointerDownEvent>(OnInventorySlotPointerDown);
            slotsContainer.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            slotsContainer.UnregisterCallback<PointerUpEvent>(OnInventorySlotPointerUp);
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
            // Never leave a stale capture behind (it would swallow all panel input)
            if (slotsContainer != null && slotsContainer.HasPointerCapture(PointerId.mousePointerId)) {
                slotsContainer.ReleasePointer(PointerId.mousePointerId);
            }
            UnregisterSlotsContainerEvents();
            UnregisterEquipmentSlotEvents();
            equipmentSlots.Clear();
            visualHandler?.Cleanup();
            dragDropService?.ResetDrag();
            isInitialized = false;
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
                // Capture on the CONTAINER, not the slot: UI Toolkit delivers a captured
                // pointer's events exclusively to the capturing element, and the Move/Up
                // handlers live on the container. Capturing the slot would starve them and
                // leak the capture (nothing would ever call ReleasePointer), freezing all
                // pointer input in the panel.
                slotsContainer?.CapturePointer(evt.pointerId);
            }
            evt.StopPropagation();
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

        // Handle pointer up for inventory-originated drags. The container holds the pointer
        // capture, so this handler receives the Up wherever the pointer is released —
        // including over equipment slots or outside any slot (= cancel).
        private void OnInventorySlotPointerUp(PointerUpEvent evt)
        {
            if (slotsContainer == null || dragDropService == null) return;
            if (dragDropService.DragState.SourceInventorySlot != null && slotsContainer.HasPointerCapture(evt.pointerId)) {
                if (dragDropService.DragState.IsDragging) dragDropService.HandleDrop(evt.position, uiConfig);
                slotsContainer.ReleasePointer(evt.pointerId);
                dragDropService.EndDrag();
            }
        }

        // Handle pointer up on equipment slot (equipment slots capture themselves and
        // have their own handlers, so captured events reach them directly)
        private void OnEquipmentSlotPointerUp(PointerUpEvent evt)
        {
            var equipmentSlot = GetEquipmentSlotFromTarget(evt.target as VisualElement);
            if (equipmentSlot == null || dragDropService == null) return;
            if (dragDropService.DragState.SourceEquipmentSlot != null && equipmentSlot.HasPointerCapture(evt.pointerId)) {
                if (dragDropService.DragState.IsDragging) dragDropService.HandleDrop(evt.position, uiConfig);
                equipmentSlot.ReleasePointer(evt.pointerId);
                dragDropService.EndDrag();
            }
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
                InventorySlots,
                inventoryViewModel,
                equipmentSystem,
                debugMode
            );
            if (transaction.CanExecute()) {
                StartCoroutine(ExecuteTransaction(transaction));
            } else {
                if (debugMode) Debug.Log($"[DragDrop] Cannot quick equip {item.itemData.itemName}");
                RefreshSlotContents();
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
                InventorySlots,
                inventoryViewModel,
                equipmentSystem,
                debugMode
            );
            if (transaction.CanExecute()) {
                StartCoroutine(ExecuteTransaction(transaction));
            } else {
                if (debugMode) Debug.Log($"[DragDrop] Cannot quick unequip {item.itemData.itemName} (inventory full?)");
                RefreshSlotContents();
            }
        }

        // Coroutine to execute a drag-drop transaction and update UI.
        // Slot contents refresh via TabFilterManager (it listens to the model's
        // OnInventoryChanged) — slot views are intentionally NOT rebuilt here.
        private IEnumerator ExecuteTransaction(DragDropTransaction transaction)
        {
            var targetVisual = transaction.GetTargetVisual();
            yield return transaction.Execute();
            targetVisual?.RemoveFromClassList("inventorySlots--drop-target");
            dragDropService.ResetDrag();
            RefreshSlotContents();
        }

        // Refresh what each slot displays without recreating the slot views
        private void RefreshSlotContents()
        {
            if (uiConfig != null && uiConfig.TabFilterManager != null) {
                uiConfig.TabFilterManager.RefreshAndRebuildUI();
            }
        }
    }
}
