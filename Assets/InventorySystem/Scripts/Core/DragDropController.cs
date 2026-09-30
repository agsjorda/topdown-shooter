using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
    [DisallowMultipleComponent]
    public class DragDropController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Controls equipped items (weapon, armor, etc.)")]
        [SerializeField] private EquipmentController equipmentController;

        [Tooltip("Inventory panel config whose slots this drags between. Auto-filled from the same GameObject, then the scene.")]
        [SerializeField] private InventoryUIConfig uiConfig;

        [Header("Drag Settings")]
        [Tooltip("How many pixels to move before drag starts (prevents accidental drags)")]
        [SerializeField] private float dragThreshold = 5f;

        [Tooltip("Maximum seconds between clicks to count as a double-click")]
        [SerializeField] private float doubleClickWindow = 0.3f;

        [Tooltip("Enable detailed logging to Console (helpful for debugging)")]
        [SerializeField] private bool debugMode = false;

        [Tooltip("Show highlighting on empty slots during drag")]
        [SerializeField] private bool highlightEmptySlots = true;

        private IInventory inventory;
        private DragDropService dragDropService;
        private DragVisualHandler visualHandler;
        private DoubleClickHandler doubleClickHandler;
        private bool isInitialized;
        private Coroutine initializeRoutine;
        private VisualElement slotsContainer;

        private static readonly List<SlotView> EmptySlots = new List<SlotView>();
        private static readonly List<EquipmentSlotView> EmptyEquipmentSlots = new List<EquipmentSlotView>();

        private List<SlotView> InventorySlots => uiConfig != null ? uiConfig.Slots : EmptySlots;
        private List<EquipmentSlotView> EquipmentSlots => uiConfig != null ? uiConfig.EquipmentSlots : EmptyEquipmentSlots;

        private void Awake()
        {
            if (equipmentController == null) equipmentController = GetComponent<EquipmentController>();
            if (uiConfig == null) uiConfig = GetComponent<InventoryUIConfig>();
            if (uiConfig == null) uiConfig = Object.FindAnyObjectByType<InventoryUIConfig>();
            doubleClickHandler = new DoubleClickHandler(doubleClickWindow);
            inventory ??= InventoryService.GetPlayerInventoryViewModel();
        }

        private void OnEnable()
        {
            if (uiConfig != null) uiConfig.UIBuilt += OnUIBuilt;
            if (!isInitialized) {
                initializeRoutine = StartCoroutine(Initialize());
            }
        }

        private void OnDisable()
        {
            if (uiConfig != null) uiConfig.UIBuilt -= OnUIBuilt;
            if (initializeRoutine != null) {
                StopCoroutine(initializeRoutine);
                initializeRoutine = null;
            }
            Cleanup();
        }

        // A reload replaces every slot element and the root the drag ghost lives in
        private void OnUIBuilt()
        {
            if (!isInitialized) return;
            Cleanup();
            initializeRoutine = StartCoroutine(Initialize());
        }

        private IEnumerator Initialize()
        {
            if (uiConfig == null) {
                if (debugMode) Debug.LogError("[DragDrop] No InventoryUIConfig found in scene");
                yield break;
            }
            int maxWait = 60;
            int waited = 0;
            while ((inventory == null || inventory.Items == null) && waited < maxWait) {
                inventory ??= InventoryService.GetPlayerInventoryViewModel();
                yield return null;
                waited++;
            }
            if (inventory == null || inventory.Items == null) {
                if (debugMode) Debug.LogError("[DragDrop] Inventory or Items not found");
                yield break;
            }
            // The Panel Renderer loads its tree asynchronously; slots exist once the config builds
            while (!uiConfig.IsBuilt) {
                yield return null;
            }
            initializeRoutine = null;
            visualHandler = new DragVisualHandler(
                uiConfig.Root,
                () => InventorySlots,
                highlightEmptySlots
            );

            IEquipmentSystem equipmentSystem = equipmentController != null
                ? equipmentController
                : InventoryService.GetPlayerEquipment();
            dragDropService = new DragDropService(
                inventory,
                equipmentSystem,
                EquipmentSlots,
                visualHandler,
                debugMode
            );
            RegisterEventHandlers();
            RegisterServiceEvents();
            isInitialized = true;
            if (debugMode) Debug.Log($"[DragDrop] Initialized with {InventorySlots.Count} inventory slots and {EquipmentSlots.Count} equipment slots");
        }

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

        private void RegisterEquipmentSlotEvents()
        {
            foreach (var slot in EquipmentSlots) {
                if (slot == null) continue;
                slot.UnregisterCallback<PointerDownEvent>(OnEquipmentSlotPointerDown);
                slot.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                slot.UnregisterCallback<PointerUpEvent>(OnEquipmentSlotPointerUp);
                slot.RegisterCallback<PointerDownEvent>(OnEquipmentSlotPointerDown);
                slot.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                slot.RegisterCallback<PointerUpEvent>(OnEquipmentSlotPointerUp);
            }
        }

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

        private void Cleanup()
        {
            if (slotsContainer != null && slotsContainer.HasPointerCapture(PointerId.mousePointerId)) {
                slotsContainer.ReleasePointer(PointerId.mousePointerId);
            }
            UnregisterSlotsContainerEvents();
            UnregisterEquipmentSlotEvents();
            visualHandler?.Cleanup();
            dragDropService?.ResetDrag();
            isInitialized = false;
        }

        private void UnregisterEquipmentSlotEvents()
        {
            foreach (var slot in EquipmentSlots) {
                if (slot == null) continue;
                slot.UnregisterCallback<PointerDownEvent>(OnEquipmentSlotPointerDown);
                slot.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                slot.UnregisterCallback<PointerUpEvent>(OnEquipmentSlotPointerUp);
            }
        }

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
                // Capture on the container: Move/Up handlers live there, and UI Toolkit
                // delivers captured events only to the capturing element.
                slotsContainer?.CapturePointer(evt.pointerId);
            }
            evt.StopPropagation();
        }

        private void OnInventorySlotPointerDown(PointerDownEvent evt)
        {
            var slot = evt.target as SlotView ?? GetSlotFromParent(evt.target as VisualElement);
            HandleSlotPointerDown(evt, slot, false);
        }

        private void OnEquipmentSlotPointerDown(PointerDownEvent evt)
        {
            var equipmentSlot = GetEquipmentSlotFromTarget(evt.target as VisualElement);
            HandleSlotPointerDown(evt, equipmentSlot, true);
        }

        private void OnInventorySlotPointerUp(PointerUpEvent evt)
        {
            if (slotsContainer == null || dragDropService == null) return;
            if (dragDropService.DragState.SourceInventorySlot != null && slotsContainer.HasPointerCapture(evt.pointerId)) {
                if (dragDropService.DragState.IsDragging) dragDropService.HandleDrop(evt.position, uiConfig);
                slotsContainer.ReleasePointer(evt.pointerId);
                dragDropService.EndDrag();
            }
        }

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

        private SlotView GetSlotFromParent(VisualElement element)
        {
            while (element != null) {
                if (element is SlotView slot) return slot;
                element = element.parent;
            }
            return null;
        }

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

        public bool IsDragging() => dragDropService?.DragState?.IsDragging ?? false;
        public SlotView GetDraggedSlot() => dragDropService?.DragState?.SourceInventorySlot;

        private IEquipmentSystem ResolveEquipment()
        {
            return equipmentController != null
                ? equipmentController
                : InventoryService.GetPlayerEquipment();
        }

        private void HandleQuickEquip(SlotView slot)
        {
            if (slot == null || !slot.HasItem || inventory == null) return;
            var item = inventory.GetItemAt(slot.SlotIndex);
            if (item == null) return;
            if (debugMode) Debug.Log($"[DragDrop] Quick equip: {item.itemData.itemName} from slot {slot.SlotIndex}");

            var transaction = new QuickEquipTransaction(
                item,
                slot.SlotIndex,
                EquipmentSlots,
                inventory,
                ResolveEquipment(),
                debugMode
            );
            if (transaction.CanExecute()) {
                StartCoroutine(ExecuteTransaction(transaction));
            } else if (debugMode) {
                Debug.Log($"[DragDrop] Cannot quick equip {item.itemData.itemName}");
            }
        }

        private void HandleQuickUnequip(EquipmentSlotView equipmentSlot)
        {
            if (equipmentSlot == null || !equipmentSlot.HasItem) return;
            var equipmentSystem = ResolveEquipment();
            var item = equipmentSystem != null ? equipmentSystem.GetEquippedItem(equipmentSlot.SlotType) : null;
            if (item == null) return;
            if (debugMode) Debug.Log($"[DragDrop] Quick unequip: {item.itemData.itemName} from {equipmentSlot.SlotType} slot");

            var transaction = new QuickUnequipTransaction(
                equipmentSlot,
                item,
                InventorySlots,
                inventory,
                equipmentSystem,
                debugMode
            );
            if (transaction.CanExecute()) {
                StartCoroutine(ExecuteTransaction(transaction));
            } else if (debugMode) {
                Debug.Log($"[DragDrop] Cannot quick unequip {item.itemData.itemName} (inventory full?)");
            }
        }

        // Slot contents refresh via TabFilterManager (OnInventoryChanged) and
        // InventoryUIConfig (OnEquipmentChanged). Slot views are not rebuilt here.
        private IEnumerator ExecuteTransaction(DragDropTransaction transaction)
        {
            var targetVisual = transaction.GetTargetVisual();
            yield return transaction.Execute();
            targetVisual?.RemoveFromClassList("inventorySlots--drop-target");
            dragDropService.ResetDrag();
            if (uiConfig != null && uiConfig.TabFilterManager != null) {
                uiConfig.TabFilterManager.RefreshAndRebuildUI();
            }
        }
    }
}
