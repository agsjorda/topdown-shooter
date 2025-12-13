using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class DragDropController : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private EquipmentController equipmentController;
    [SerializeField] private UIDocument uiDocument;

    [Header("Drag Settings")]
    [SerializeField] private float dragThreshold = 5f;
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool highlightEmptySlots = true;
    #endregion

    #region Private Fields
    // Drag state
    private bool isDragging;
    private Slot draggedSlot;
    private Vector2 dragStartPosition;
    private VisualElement dragGhost;

    // Equipment slot dragging
    private EquipmentSlot clickedEquipmentSlot;
    private EquipmentSlot draggedFromEquipment;

    // Cache
    private List<Slot> inventorySlots;
    private List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>();
    private bool isInitialized;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        inventoryController ??= GetComponent<InventoryController>();
        equipmentController ??= GetComponent<EquipmentController>();
        uiDocument ??= GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        if (!isInitialized) {
            StartCoroutine(Initialize());
        }
    }

    private void OnDisable()
    {
        Cleanup();
    }
    #endregion

    #region Initialization
    private IEnumerator Initialize()
    {
        // Wait for UI to be fully loaded
        yield return null;
        yield return null; // Extra frame for USS to be applied

        // Wait for inventory controller to initialize
        int maxWait = 60;
        int waited = 0;
        while (inventoryController?.Slots == null && waited < maxWait) {
            yield return null;
            waited++;
        }

        if (inventoryController?.Slots == null) {
            if (debugMode) Debug.LogError("[DragDrop] Inventory slots not found");
            yield break;
        }

        inventorySlots = new List<Slot>(inventoryController.Slots);
        equipmentSlots.Clear();

        InitializeEquipmentSlots();
        CreateDragGhost();
        RegisterEventHandlers();

        isInitialized = true;

        if (debugMode) Debug.Log($"[DragDrop] Initialized with {inventorySlots.Count} inventory slots and {equipmentSlots.Count} equipment slots");
    }

    private void InitializeEquipmentSlots()
    {
        InitializeEquipmentSlot("equipSlotWeapon", EquipmentSlotType.Weapon);
        InitializeEquipmentSlot("equipSlotHeadgear", EquipmentSlotType.Headgear);
        InitializeEquipmentSlot("equipSlotArmor", EquipmentSlotType.Armor);
        InitializeEquipmentSlot("equipSlotBoots", EquipmentSlotType.Boots);
    }

    private void InitializeEquipmentSlot(string slotName, EquipmentSlotType slotType)
    {
        if (uiDocument?.rootVisualElement == null) return;

        var equipmentSlot = uiDocument.rootVisualElement.Q<EquipmentSlot>(slotName);
        if (equipmentSlot != null) {
            // Remove if already exists (safety check)
            equipmentSlots.RemoveAll(s => s == equipmentSlot);

            equipmentSlot.Initialize(slotType);
            equipmentSlots.Add(equipmentSlot);
            equipmentSlot.RefreshVisualState();

            if (debugMode) Debug.Log($"[DragDrop] Initialized equipment slot: {slotName} ({slotType})");
        } else {
            if (debugMode) Debug.LogWarning($"[DragDrop] Equipment slot not found: {slotName}");
        }
    }

    private void CreateDragGhost()
    {
        if (uiDocument?.rootVisualElement == null) return;

        var root = uiDocument.rootVisualElement;

        // Remove existing ghost if any
        var existingGhost = root.Q<VisualElement>("dragGhost");
        existingGhost?.RemoveFromHierarchy();

        // Create new ghost
        dragGhost = new VisualElement {
            name = "dragGhost",
            pickingMode = PickingMode.Ignore
        };

        dragGhost.AddToClassList("drag-ghost");
        dragGhost.style.position = Position.Absolute;
        dragGhost.style.width = 128;
        dragGhost.style.height = 128;

        root.Add(dragGhost);
    }
    #endregion

    #region Event Registration
    private void RegisterEventHandlers()
    {
        RegisterInventorySlotEvents();
        RegisterEquipmentSlotEvents();
    }

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

    private void Cleanup()
    {
        UnregisterInventorySlotEvents();
        UnregisterEquipmentSlotEvents();

        equipmentSlots.Clear();

        if (dragGhost?.parent != null) {
            dragGhost.RemoveFromHierarchy();
            dragGhost = null;
        }

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
    private void OnInventorySlotPointerDown(PointerDownEvent evt)
    {
        if (evt.button != 0 || isDragging) return;

        var slot = evt.target as Slot ?? GetSlotFromParent(evt.target as VisualElement);
        if (slot == null || !slot.HasItem) return;

        if (debugMode) Debug.Log($"[DragDrop] Pointer down on inventory slot {slot.SlotIndex}");

        dragStartPosition = evt.position;
        draggedSlot = slot;
        clickedEquipmentSlot = null;

        slot.CapturePointer(evt.pointerId);
        evt.StopPropagation();
    }

    private void OnEquipmentSlotPointerDown(PointerDownEvent evt)
    {
        if (evt.button != 0 || isDragging) return;

        var equipmentSlot = GetEquipmentSlotFromTarget(evt.target as VisualElement);
        if (equipmentSlot == null) return;

        if (debugMode) Debug.Log($"[DragDrop] Pointer down on equipment slot: {equipmentSlot.SlotType} (HasItem: {equipmentSlot.HasItem})");

        dragStartPosition = evt.position;
        clickedEquipmentSlot = equipmentSlot;
        draggedSlot = null;

        equipmentSlot.CapturePointer(evt.pointerId);
        evt.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (clickedEquipmentSlot != null && clickedEquipmentSlot.HasPointerCapture(evt.pointerId)) {
            HandlePointerMove(evt, clickedEquipmentSlot.HasPointerCapture(evt.pointerId));
        } else if (draggedSlot != null && draggedSlot.HasPointerCapture(evt.pointerId)) {
            HandlePointerMove(evt, draggedSlot.HasPointerCapture(evt.pointerId));
        }
    }

    private void HandlePointerMove(PointerMoveEvent evt, bool hasCapture)
    {
        if (!hasCapture) return;

        float distance = Vector2.Distance(dragStartPosition, evt.position);

        if (!isDragging && distance > dragThreshold) {
            StartDrag(evt.position);
        }

        if (isDragging && dragGhost != null) {
            UpdateDragPosition(evt.position);
            evt.StopPropagation();
        }
    }

    private void OnInventorySlotPointerUp(PointerUpEvent evt)
    {
        if (draggedSlot != null && draggedSlot.HasPointerCapture(evt.pointerId)) {
            if (isDragging) HandleDrop(evt.position);
            draggedSlot.ReleasePointer(evt.pointerId);
            ResetDrag();
        }
    }

    private void OnEquipmentSlotPointerUp(PointerUpEvent evt)
    {
        if (clickedEquipmentSlot != null && clickedEquipmentSlot.HasPointerCapture(evt.pointerId)) {
            if (isDragging) HandleDrop(evt.position);
            clickedEquipmentSlot.ReleasePointer(evt.pointerId);
            ResetDrag();
        }
    }
    #endregion

    #region Drag Operations
    private void StartDrag(Vector2 position)
    {
        if (clickedEquipmentSlot != null && clickedEquipmentSlot.HasItem) {
            StartEquipmentSlotDrag(position);
        } else if (draggedSlot != null && draggedSlot.HasItem) {
            StartInventorySlotDrag(position);
        }
    }

    private void StartEquipmentSlotDrag(Vector2 position)
    {
        isDragging = true;

        if (debugMode) Debug.Log($"[DragDrop] Starting drag FROM equipment slot: {clickedEquipmentSlot.SlotType}");

        var equippedItem = clickedEquipmentSlot.GetEquippedItem();
        if (equippedItem == null) {
            ResetDrag();
            return;
        }

        draggedSlot = new Slot();
        draggedSlot.SetItem(equippedItem);
        draggedFromEquipment = clickedEquipmentSlot;

        ShowDragGhost(draggedSlot, position);
    }

    private void StartInventorySlotDrag(Vector2 position)
    {
        isDragging = true;

        if (debugMode) Debug.Log($"[DragDrop] Starting drag from inventory slot {draggedSlot.SlotIndex}");

        draggedSlot.AddToClassList("inventorySlots--dragging");
        ShowDragGhost(draggedSlot, position);
    }

    private void ShowDragGhost(Slot slot, Vector2 position)
    {
        if (dragGhost == null || slot?.Icon == null) return;

        Texture2D texture = slot.Icon.image as Texture2D ?? slot.Icon.sprite?.texture;
        if (texture == null) return;

        dragGhost.style.backgroundImage = new StyleBackground(texture);
        dragGhost.AddToClassList("drag-ghost--visible");
        dragGhost.BringToFront();

        if (highlightEmptySlots) HighlightEmptySlots();

        UpdateDragPosition(position);
    }

    private void UpdateDragPosition(Vector2 position)
    {
        if (dragGhost == null) return;

        dragGhost.style.left = position.x - (dragGhost.resolvedStyle.width / 2);
        dragGhost.style.top = position.y - (dragGhost.resolvedStyle.height / 2);

        UpdateDragGhostAppearance(position);
    }

    private void UpdateDragGhostAppearance(Vector2 position)
    {
        var targetEquipment = FindEquipmentSlotAtPosition(position);
        var targetInventorySlot = targetEquipment == null ? FindInventorySlotAtPosition(position) : null;

        dragGhost.RemoveFromClassList("drag-ghost--over-empty");
        dragGhost.RemoveFromClassList("drag-ghost--over-occupied");

        if (targetEquipment != null) {
            bool isValid = draggedFromEquipment != targetEquipment &&
                          CanEquipItemInSlot(draggedSlot, targetEquipment);

            dragGhost.AddToClassList(isValid ? "drag-ghost--over-empty" : "drag-ghost--over-occupied");
        } else if (targetInventorySlot != null) {
            bool isOccupied = draggedFromEquipment == null && targetInventorySlot.HasItem;
            dragGhost.AddToClassList(isOccupied ? "drag-ghost--over-occupied" : "drag-ghost--over-empty");
        }
    }

    private void ResetDrag()
    {
        draggedSlot?.RemoveFromClassList("inventorySlots--dragging");

        if (highlightEmptySlots) RemoveEmptySlotHighlighting();

        if (dragGhost != null) {
            dragGhost.RemoveFromClassList("drag-ghost--visible");
            dragGhost.RemoveFromClassList("drag-ghost--over-empty");
            dragGhost.RemoveFromClassList("drag-ghost--over-occupied");
            dragGhost.style.backgroundImage = null;
        }

        isDragging = false;
        draggedSlot = null;
        clickedEquipmentSlot = null;
        draggedFromEquipment = null;

        if (debugMode) Debug.Log("[DragDrop] Drag reset");
    }
    #endregion

    #region Drop Handling
    private void HandleDrop(Vector2 dropPosition)
    {
        if (draggedSlot == null) return;

        if (debugMode) Debug.Log($"[DragDrop] HandleDrop at position: {dropPosition}");

        var targetEquipment = FindEquipmentSlotAtPosition(dropPosition);

        if (targetEquipment != null) {
            HandleEquipmentDrop(draggedSlot, targetEquipment);
        } else {
            HandleInventoryDrop(dropPosition);
        }
    }

    private void HandleInventoryDrop(Vector2 dropPosition)
    {
        var targetSlot = FindInventorySlotAtPosition(dropPosition);
        if (targetSlot == null) {
            if (debugMode) Debug.Log("[DragDrop] Dropped outside any slot - cancelled");
            return;
        }

        if (debugMode) Debug.Log($"[DragDrop] Dropping on inventory slot (HasItem: {targetSlot.HasItem})");

        if (draggedFromEquipment != null) {
            HandleUnequipToInventory(draggedFromEquipment, targetSlot);
        } else if (targetSlot != draggedSlot) {
            if (debugMode) {
                string action = targetSlot.HasItem ? "swapping with" : "moving to empty";
                Debug.Log($"[DragDrop] Dropping from slot {draggedSlot.SlotIndex} to slot {targetSlot.SlotIndex} ({action})");
            }

            targetSlot.AddToClassList("inventorySlots--drop-target");
            StartCoroutine(PerformInventoryMove(draggedSlot.SlotIndex, targetSlot.SlotIndex, targetSlot));
        } else {
            if (debugMode) Debug.Log("[DragDrop] Dropped back on original slot - cancelled");
        }
    }

    private void HandleEquipmentDrop(Slot fromSlot, EquipmentSlot toEquipment)
    {
        if (fromSlot == null || toEquipment == null || inventoryController == null) return;

        if (draggedFromEquipment == toEquipment) {
            if (debugMode) Debug.Log("[DragDrop] Cannot drop on same equipment slot");
            return;
        }

        if (!CanEquipItemInSlot(fromSlot, toEquipment)) {
            if (debugMode) Debug.Log($"[DragDrop] Cannot equip this item in {toEquipment.SlotType} slot");
            return;
        }

        toEquipment.AddToClassList("inventorySlots--drop-target");
        StartCoroutine(PerformEquipmentTransaction(fromSlot, toEquipment));
    }

    private void HandleUnequipToInventory(EquipmentSlot fromEquipment, Slot toInventorySlot)
    {
        if (fromEquipment == null || toInventorySlot == null ||
            equipmentController == null || inventoryController == null) {
            if (debugMode) Debug.LogError("[DragDrop] HandleUnequipToInventory: Missing references");
            return;
        }

        var equippedItem = fromEquipment.GetEquippedItem();
        if (equippedItem == null) {
            if (debugMode) Debug.LogError("[DragDrop] HandleUnequipToInventory: No equipped item");
            return;
        }

        if (debugMode) Debug.Log($"[DragDrop] Unequipping {equippedItem.itemData.itemName} to inventory");

        int targetSlotIndex = inventorySlots.IndexOf(toInventorySlot);
        if (targetSlotIndex < 0) {
            if (debugMode) Debug.LogError("[DragDrop] Could not find target inventory slot index");
            return;
        }

        if (debugMode) Debug.Log($"[DragDrop] Found target inventory slot index: {targetSlotIndex}");

        toInventorySlot.AddToClassList("inventorySlots--drop-target");

        var inventoryItemInSlot = inventoryController.GetItemAtSlot(targetSlotIndex);

        if (inventoryItemInSlot != null) {
            if (debugMode) Debug.Log($"[DragDrop] Target slot has item: {inventoryItemInSlot.itemData.itemName}");

            if (fromEquipment.CanAcceptItem(inventoryItemInSlot.itemData)) {
                if (debugMode) Debug.Log($"[DragDrop] Can swap: {inventoryItemInSlot.itemData.itemName} can be equipped in {fromEquipment.SlotType}");
                StartCoroutine(PerformSwapTransaction(fromEquipment, equippedItem, targetSlotIndex, inventoryItemInSlot, toInventorySlot));
            } else {
                if (debugMode) Debug.Log($"[DragDrop] Cannot swap: {inventoryItemInSlot.itemData.itemName} cannot be equipped in {fromEquipment.SlotType}");
                toInventorySlot.RemoveFromClassList("inventorySlots--drop-target");
            }
        } else {
            if (debugMode) Debug.Log("[DragDrop] Target slot is empty - simple unequip");
            StartCoroutine(PerformUnequipTransaction(fromEquipment, targetSlotIndex, toInventorySlot));
        }
    }
    #endregion

    #region Transactions
    private System.Collections.IEnumerator PerformInventoryMove(int fromIndex, int toIndex, Slot targetSlot)
    {
        yield return null;

        inventoryController.MoveItem(fromIndex, toIndex);
        targetSlot?.RemoveFromClassList("inventorySlots--drop-target");
    }

    private System.Collections.IEnumerator PerformUnequipTransaction(EquipmentSlot fromEquipment, int targetSlotIndex, Slot toInventorySlot)
    {
        yield return null;

        if (debugMode) Debug.Log($"[DragDrop] Performing unequip to slot {targetSlotIndex}");

        var equippedItem = fromEquipment.GetEquippedItem();
        if (equippedItem == null) {
            if (debugMode) Debug.LogError("[DragDrop] No equipped item to unequip");
            toInventorySlot?.RemoveFromClassList("inventorySlots--drop-target");
            yield break;
        }

        // Clear equipment slot
        fromEquipment.ClearItem();
        fromEquipment.RefreshVisualState();
        equipmentController?.UnequipSlot(fromEquipment.SlotType);

        // Update inventory visual
        if (targetSlotIndex >= 0 && targetSlotIndex < inventorySlots.Count) {
            inventorySlots[targetSlotIndex]?.SetItem(equippedItem);
        }

        // Update inventory data
        inventoryController?.AddItemToSlot(equippedItem, targetSlotIndex);

        if (debugMode) Debug.Log($"[DragDrop] Unequipped {equippedItem.itemData.itemName} to inventory slot {targetSlotIndex}");

        toInventorySlot?.RemoveFromClassList("inventorySlots--drop-target");
    }

    private System.Collections.IEnumerator PerformSwapTransaction(
        EquipmentSlot equipmentSlot,
        Inventory_Item equippedItem,
        int inventorySlotIndex,
        Inventory_Item inventoryItem,
        Slot inventorySlotVisual)
    {
        yield return null;

        if (debugMode) Debug.Log($"[DragDrop] Performing swap: {equippedItem.itemData.itemName} ↔ {inventoryItem.itemData.itemName} at slot {inventorySlotIndex}");

        if (inventorySlotIndex < 0 || inventorySlotIndex >= inventorySlots.Count) {
            if (debugMode) Debug.LogError($"[DragDrop] Invalid inventory slot index: {inventorySlotIndex}");
            inventorySlotVisual?.RemoveFromClassList("inventorySlots--drop-target");
            yield break;
        }

        var targetInventorySlot = inventorySlots[inventorySlotIndex];
        if (targetInventorySlot == null) {
            if (debugMode) Debug.LogError($"[DragDrop] No slot found at index: {inventorySlotIndex}");
            inventorySlotVisual?.RemoveFromClassList("inventorySlots--drop-target");
            yield break;
        }

        // Update inventory slot visual and data
        targetInventorySlot.ClearItem();
        targetInventorySlot.SetItem(equippedItem);
        inventoryController?.RemoveItemAtSlot(inventorySlotIndex);
        inventoryController?.AddItemToSlot(equippedItem, inventorySlotIndex);

        // Update equipment slot visual and data
        equipmentSlot.ClearItem();
        equipmentSlot.RefreshVisualState();
        equipmentController?.UnequipSlot(equipmentSlot.SlotType);
        equipmentController?.EquipItem(inventoryItem, equipmentSlot.SlotType);
        equipmentSlot.SetItem(inventoryItem);
        equipmentSlot.RefreshVisualState();

        if (debugMode) Debug.Log($"[DragDrop] Swap complete: {equippedItem.itemData.itemName} moved to inventory slot {inventorySlotIndex}, {inventoryItem.itemData.itemName} equipped");

        inventorySlotVisual?.RemoveFromClassList("inventorySlots--drop-target");
    }

    private System.Collections.IEnumerator PerformEquipmentTransaction(Slot fromSlot, EquipmentSlot toEquipment)
    {
        yield return null;

        Inventory_Item itemToEquip = null;
        int sourceSlotIndex = -1;

        if (draggedFromEquipment != null) {
            itemToEquip = draggedFromEquipment.GetEquippedItem();
            if (debugMode) Debug.Log($"[DragDrop] Dragging from equipment slot: {itemToEquip?.itemData?.itemName}");
        } else {
            itemToEquip = inventoryController?.GetItemAtSlot(fromSlot.SlotIndex);
            sourceSlotIndex = fromSlot.SlotIndex;
            if (debugMode) Debug.Log($"[DragDrop] Dragging from inventory slot {sourceSlotIndex}: {itemToEquip?.itemData?.itemName}");
        }

        if (itemToEquip == null) {
            toEquipment?.RemoveFromClassList("inventorySlots--drop-target");
            yield break;
        }

        var currentlyEquipped = equipmentController?.GetEquippedItem(toEquipment.SlotType);

        if (currentlyEquipped != null) {
            if (debugMode) Debug.Log($"[DragDrop] Target slot already has item: {currentlyEquipped.itemData.itemName}");

            if (draggedFromEquipment != null) {
                yield return PerformEquipmentSwap(itemToEquip, currentlyEquipped, toEquipment);
            } else {
                yield return PerformInventoryToEquipmentSwap(itemToEquip, currentlyEquipped, sourceSlotIndex, toEquipment);
            }
        } else {
            if (debugMode) Debug.Log("[DragDrop] Target equipment slot is empty");

            if (draggedFromEquipment != null) {
                yield return PerformEquipmentMove(itemToEquip, toEquipment);
            } else {
                yield return PerformInventoryToEquipment(itemToEquip, sourceSlotIndex, toEquipment);
            }
        }

        toEquipment?.RemoveFromClassList("inventorySlots--drop-target");
    }

    private System.Collections.IEnumerator PerformEquipmentSwap(Inventory_Item itemToEquip, Inventory_Item currentlyEquipped, EquipmentSlot toEquipment)
    {
        // Swapping between two equipment slots
        draggedFromEquipment.ClearItem();
        draggedFromEquipment.RefreshVisualState();
        toEquipment.ClearItem();
        toEquipment.RefreshVisualState();

        equipmentController?.UnequipSlot(draggedFromEquipment.SlotType);
        equipmentController?.UnequipSlot(toEquipment.SlotType);

        equipmentController?.EquipItem(itemToEquip, toEquipment.SlotType);
        toEquipment.SetItem(itemToEquip);
        toEquipment.RefreshVisualState();

        equipmentController?.EquipItem(currentlyEquipped, draggedFromEquipment.SlotType);
        draggedFromEquipment.SetItem(currentlyEquipped);
        draggedFromEquipment.RefreshVisualState();

        if (debugMode) Debug.Log($"[DragDrop] Swapped {itemToEquip.itemData.itemName} with {currentlyEquipped.itemData.itemName} between equipment slots");

        yield break;
    }

    private System.Collections.IEnumerator PerformInventoryToEquipmentSwap(Inventory_Item itemToEquip, Inventory_Item currentlyEquipped, int sourceSlotIndex, EquipmentSlot toEquipment)
    {
        if (sourceSlotIndex < 0 || sourceSlotIndex >= inventorySlots.Count) {
            if (debugMode) Debug.LogError($"[DragDrop] Invalid source slot index: {sourceSlotIndex}");
            yield break;
        }

        var sourceSlot = inventorySlots[sourceSlotIndex];
        if (sourceSlot == null) {
            if (debugMode) Debug.LogError($"[DragDrop] Source slot is null at index: {sourceSlotIndex}");
            yield break;
        }

        // Swap: equipped item goes to inventory, inventory item gets equipped
        sourceSlot.ClearItem();
        sourceSlot.SetItem(currentlyEquipped);

        inventoryController?.RemoveItemAtSlot(sourceSlotIndex);
        inventoryController?.AddItemToSlot(currentlyEquipped, sourceSlotIndex);

        toEquipment.ClearItem();
        toEquipment.RefreshVisualState();

        equipmentController?.EquipItem(itemToEquip, toEquipment.SlotType);
        toEquipment.SetItem(itemToEquip);
        toEquipment.RefreshVisualState();

        if (debugMode) Debug.Log($"[DragDrop] Swapped: Equipped {itemToEquip.itemData.itemName}, put {currentlyEquipped.itemData.itemName} back in slot {sourceSlotIndex}");
    }

    private System.Collections.IEnumerator PerformEquipmentMove(Inventory_Item itemToEquip, EquipmentSlot toEquipment)
    {
        draggedFromEquipment.ClearItem();
        draggedFromEquipment.RefreshVisualState();
        equipmentController?.UnequipSlot(draggedFromEquipment.SlotType);

        toEquipment.ClearItem();
        toEquipment.RefreshVisualState();

        equipmentController?.EquipItem(itemToEquip, toEquipment.SlotType);
        toEquipment.SetItem(itemToEquip);
        toEquipment.RefreshVisualState();

        if (debugMode) Debug.Log($"[DragDrop] Moved {itemToEquip.itemData.itemName} from {draggedFromEquipment.SlotType} to {toEquipment.SlotType}");

        yield break;
    }

    private System.Collections.IEnumerator PerformInventoryToEquipment(Inventory_Item itemToEquip, int sourceSlotIndex, EquipmentSlot toEquipment)
    {
        inventoryController?.RemoveItemAtSlot(sourceSlotIndex);

        toEquipment.ClearItem();
        toEquipment.RefreshVisualState();

        equipmentController?.EquipItem(itemToEquip, toEquipment.SlotType);
        toEquipment.SetItem(itemToEquip);
        toEquipment.RefreshVisualState();

        if (debugMode) Debug.Log($"[DragDrop] Equipped {itemToEquip.itemData.itemName} from slot {sourceSlotIndex} in {toEquipment.SlotType} slot");

        yield break;
    }
    #endregion

    #region Helper Methods
    private Slot GetSlotFromParent(VisualElement element)
    {
        while (element != null) {
            if (element is Slot slot) return slot;
            element = element.parent;
        }
        return null;
    }

    private EquipmentSlot GetEquipmentSlotFromTarget(VisualElement element)
    {
        if (element == null) return null;

        // Check if target is EquipmentSlot
        if (element is EquipmentSlot slot) return slot;

        // Walk up the tree to find EquipmentSlot parent
        var parent = element.parent;
        while (parent != null) {
            if (parent is EquipmentSlot parentSlot) return parentSlot;
            parent = parent.parent;
        }

        return null;
    }

    private EquipmentSlot FindEquipmentSlotAtPosition(Vector2 position)
    {
        foreach (var slot in equipmentSlots) {
            if (slot == null) continue;

            var bounds = slot.worldBound;
            var paddedBounds = new Rect(
                bounds.x - 20,
                bounds.y - 20,
                bounds.width + 40,
                bounds.height + 40
            );

            if (paddedBounds.Contains(position)) return slot;
        }

        return null;
    }

    private Slot FindInventorySlotAtPosition(Vector2 position)
    {
        if (inventorySlots == null) return null;

        foreach (var slot in inventorySlots) {
            if (slot == null) continue;

            var bounds = slot.worldBound;
            var paddedBounds = new Rect(
                bounds.x - 10,
                bounds.y - 10,
                bounds.width + 20,
                bounds.height + 20
            );

            if (paddedBounds.Contains(position)) {
                if (debugMode) Debug.Log($"[DragDrop] Found drop target slot at position {position}");
                return slot;
            }
        }

        if (debugMode) Debug.Log($"[DragDrop] No drop target found at position {position}");
        return null;
    }

    private bool CanEquipItemInSlot(Slot sourceSlot, EquipmentSlot targetEquipment)
    {
        if (sourceSlot == null || targetEquipment == null || !sourceSlot.HasItem) return false;

        var inventoryItem = draggedFromEquipment != null
            ? draggedFromEquipment.GetEquippedItem()
            : inventoryController?.GetItemAtSlot(sourceSlot.SlotIndex);

        if (inventoryItem?.itemData == null) return false;

        return targetEquipment.CanAcceptItem(inventoryItem.itemData);
    }

    private void HighlightEmptySlots()
    {
        if (inventorySlots == null) return;

        foreach (var slot in inventorySlots) {
            if (slot != null && slot != draggedSlot && !slot.HasItem) {
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
    #endregion

    #region Public API
    public bool IsDragging() => isDragging;
    public Slot GetDraggedSlot() => draggedSlot;
    #endregion
}