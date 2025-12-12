using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class DragDropController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private EquipmentController equipmentController;
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

    // Equipment slot dragging
    private EquipmentSlotWrapper _clickedEquipmentSlot;
    private EquipmentSlotWrapper _draggedFromEquipment;

    // Cache
    private List<Slot> slots;
    private List<EquipmentSlotWrapper> equipmentSlots = new List<EquipmentSlotWrapper>();
    private bool isInitialized = false;

    void Awake()
    {
        inventoryController ??= GetComponent<InventoryController>();
        equipmentController ??= GetComponent<EquipmentController>();
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

        // Clear existing equipment slots
        equipmentSlots.Clear();

        // Initialize equipment slots
        InitializeEquipmentSlot("equipSlotWeapon", EquipmentSlotType.Weapon);
        InitializeEquipmentSlot("equipSlotHeadgear", EquipmentSlotType.Headgear);
        InitializeEquipmentSlot("equipSlotArmor", EquipmentSlotType.Armor);
        InitializeEquipmentSlot("equipSlotBoots", EquipmentSlotType.Boots);

        CreateDragGhost();
        RegisterSlotEvents();
        RegisterEquipmentSlotEvents();

        isInitialized = true;

        if (debugMode) Debug.Log($"DragDropController initialized with {slots.Count} inventory slots and {equipmentSlots.Count} equipment slots");
    }

    private void InitializeEquipmentSlot(string slotName, EquipmentSlotType slotType)
    {
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        if (root == null) return;

        var element = root.Q<VisualElement>(slotName);
        if (element != null) {
            // Check if this element already has a wrapper
            var existingWrapper = equipmentSlots.Find(w => w.VisualElement == element);
            if (existingWrapper != null) {
                equipmentSlots.Remove(existingWrapper);
            }

            var wrapper = new EquipmentSlotWrapper(element, slotType);
            equipmentSlots.Add(wrapper);

            // Refresh visual state to ensure proper initial display
            wrapper.RefreshVisualState();

            if (debugMode) Debug.Log($"Initialized equipment slot: {slotName} ({slotType})");
        } else {
            if (debugMode) Debug.LogWarning($"Equipment slot not found: {slotName}");
        }
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
        dragGhost.style.position = Position.Absolute;
        dragGhost.style.width = 128;
        dragGhost.style.height = 128;

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

    private void RegisterEquipmentSlotEvents()
    {
        foreach (var eqWrapper in equipmentSlots) {
            if (eqWrapper.VisualElement != null) {
                eqWrapper.VisualElement.UnregisterCallback<PointerDownEvent>(OnEquipmentPointerDown);
                eqWrapper.VisualElement.UnregisterCallback<PointerMoveEvent>(OnSlotPointerMove);
                eqWrapper.VisualElement.UnregisterCallback<PointerUpEvent>(OnEquipmentPointerUp);

                eqWrapper.VisualElement.RegisterCallback<PointerDownEvent>(OnEquipmentPointerDown);
                eqWrapper.VisualElement.RegisterCallback<PointerMoveEvent>(OnSlotPointerMove);
                eqWrapper.VisualElement.RegisterCallback<PointerUpEvent>(OnEquipmentPointerUp);
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

        foreach (var eqWrapper in equipmentSlots) {
            if (eqWrapper.VisualElement != null) {
                eqWrapper.VisualElement.UnregisterCallback<PointerDownEvent>(OnEquipmentPointerDown);
                eqWrapper.VisualElement.UnregisterCallback<PointerMoveEvent>(OnSlotPointerMove);
                eqWrapper.VisualElement.UnregisterCallback<PointerUpEvent>(OnEquipmentPointerUp);
            }
        }
        equipmentSlots.Clear();

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

        if (debugMode) Debug.Log($"Pointer down on inventory slot {slot.SlotIndex}");

        dragStartPosition = evt.position;
        draggedSlot = slot;
        _clickedEquipmentSlot = null;
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

    private void OnEquipmentPointerDown(PointerDownEvent evt)
    {
        if (evt.button != 0 || isDragging) return;

        var element = evt.target as VisualElement;
        if (element == null) return;

        EquipmentSlotWrapper eqWrapper = null;
        foreach (var wrapper in equipmentSlots) {
            if (wrapper.VisualElement == element || wrapper.VisualElement.Contains(element)) {
                eqWrapper = wrapper;
                break;
            }
        }

        if (eqWrapper == null) return;

        if (debugMode) Debug.Log($"Pointer down on equipment slot: {eqWrapper.SlotType} (HasItem: {eqWrapper.HasItem})");

        dragStartPosition = evt.position;
        _clickedEquipmentSlot = eqWrapper;
        draggedSlot = null;

        eqWrapper.VisualElement.CapturePointer(evt.pointerId);
        evt.StopPropagation();
    }

    private void OnSlotPointerMove(PointerMoveEvent evt)
    {
        if (_clickedEquipmentSlot != null && _clickedEquipmentSlot.VisualElement.HasPointerCapture(evt.pointerId)) {
            float distance = Vector2.Distance(dragStartPosition, evt.position);

            if (!isDragging && distance > dragThreshold) {
                StartDrag(evt.position);
            }

            if (isDragging && dragGhost != null) {
                UpdateDragPosition(evt.position);
                evt.StopPropagation();
            }
        } else if (draggedSlot != null && draggedSlot.HasPointerCapture(evt.pointerId)) {
            float distance = Vector2.Distance(dragStartPosition, evt.position);

            if (!isDragging && distance > dragThreshold) {
                StartDrag(evt.position);
            }

            if (isDragging && dragGhost != null) {
                UpdateDragPosition(evt.position);
                evt.StopPropagation();
            }
        }
    }

    private void OnSlotPointerUp(PointerUpEvent evt)
    {
        if (draggedSlot != null && draggedSlot.HasPointerCapture(evt.pointerId)) {
            if (isDragging) {
                HandleDrop(evt.position);
            }

            draggedSlot.ReleasePointer(evt.pointerId);
            ResetDrag();
        }
    }

    private void OnEquipmentPointerUp(PointerUpEvent evt)
    {
        if (_clickedEquipmentSlot != null && _clickedEquipmentSlot.VisualElement.HasPointerCapture(evt.pointerId)) {
            if (isDragging) {
                HandleDrop(evt.position);
            }

            _clickedEquipmentSlot.VisualElement.ReleasePointer(evt.pointerId);
            ResetDrag();
        }
    }

    private void StartDrag(Vector2 position)
    {
        if (_clickedEquipmentSlot != null && _clickedEquipmentSlot.HasItem) {
            isDragging = true;

            if (debugMode) Debug.Log($"Starting drag FROM equipment slot: {_clickedEquipmentSlot.SlotType}");

            var equippedItem = _clickedEquipmentSlot.GetEquippedItem();
            if (equippedItem == null) {
                ResetDrag();
                return;
            }

            draggedSlot = new Slot();
            draggedSlot.SetItem(equippedItem);

            _draggedFromEquipment = _clickedEquipmentSlot;

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

                    if (highlightEmptySlots) {
                        HighlightEmptySlots();
                    }

                    UpdateDragPosition(position);
                }
            }
        } else if (draggedSlot != null && draggedSlot.HasItem) {
            isDragging = true;

            if (debugMode) Debug.Log($"Starting drag from inventory slot {draggedSlot.SlotIndex}");

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

                    draggedSlot.AddToClassList("inventorySlots--dragging");

                    if (highlightEmptySlots) {
                        HighlightEmptySlots();
                    }

                    UpdateDragPosition(position);
                }
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

            Slot targetSlot = null;
            EquipmentSlotWrapper targetEquipment = null;

            foreach (var eqWrapper in equipmentSlots) {
                if (eqWrapper.VisualElement != null) {
                    var bounds = eqWrapper.VisualElement.worldBound;
                    var paddedBounds = new Rect(
                        bounds.x - 20,
                        bounds.y - 20,
                        bounds.width + 40,
                        bounds.height + 40
                    );

                    if (paddedBounds.Contains(position)) {
                        targetEquipment = eqWrapper;
                        break;
                    }
                }
            }

            if (targetEquipment == null) {
                targetSlot = FindDropTarget(position);
            }

            if (targetEquipment != null) {
                if (_draggedFromEquipment == targetEquipment) {
                    dragGhost.AddToClassList("drag-ghost--over-occupied");
                    dragGhost.RemoveFromClassList("drag-ghost--over-empty");
                } else {
                    bool canEquip = CanEquipItemInSlot(draggedSlot, targetEquipment);

                    if (canEquip) {
                        dragGhost.AddToClassList("drag-ghost--over-empty");
                        dragGhost.RemoveFromClassList("drag-ghost--over-occupied");
                    } else {
                        dragGhost.AddToClassList("drag-ghost--over-occupied");
                        dragGhost.RemoveFromClassList("drag-ghost--over-empty");
                    }
                }
            } else if (targetSlot != null) {
                if (_draggedFromEquipment != null) {
                    dragGhost.AddToClassList("drag-ghost--over-empty");
                    dragGhost.RemoveFromClassList("drag-ghost--over-occupied");
                } else {
                    if (targetSlot.HasItem) {
                        dragGhost.AddToClassList("drag-ghost--over-occupied");
                        dragGhost.RemoveFromClassList("drag-ghost--over-empty");
                    } else {
                        dragGhost.AddToClassList("drag-ghost--over-empty");
                        dragGhost.RemoveFromClassList("drag-ghost--over-occupied");
                    }
                }
            } else {
                dragGhost.RemoveFromClassList("drag-ghost--over-empty");
                dragGhost.RemoveFromClassList("drag-ghost--over-occupied");
            }
        }
    }

    private bool CanEquipItemInSlot(Slot sourceSlot, EquipmentSlotWrapper targetEquipment)
    {
        if (sourceSlot == null || targetEquipment == null || !sourceSlot.HasItem) return false;

        Inventory_Item inventoryItem = null;

        if (_draggedFromEquipment != null) {
            inventoryItem = _draggedFromEquipment.GetEquippedItem();
        } else {
            inventoryItem = inventoryController?.GetItemAtSlot(sourceSlot.SlotIndex);
        }

        if (inventoryItem?.itemData == null) return false;

        return targetEquipment.CanAcceptItem(inventoryItem.itemData);
    }

    private void HandleDrop(Vector2 dropPosition)
    {
        if (draggedSlot == null) return;

        if (debugMode) Debug.Log($"HandleDrop at position: {dropPosition}");

        EquipmentSlotWrapper targetEquipment = null;
        foreach (var eqWrapper in equipmentSlots) {
            if (eqWrapper.VisualElement != null) {
                var bounds = eqWrapper.VisualElement.worldBound;
                var paddedBounds = new Rect(
                    bounds.x - 20,
                    bounds.y - 20,
                    bounds.width + 40,
                    bounds.height + 40
                );

                if (paddedBounds.Contains(dropPosition)) {
                    targetEquipment = eqWrapper;
                    if (debugMode) Debug.Log($"Dropping on equipment slot: {eqWrapper.SlotType}");
                    break;
                }
            }
        }

        if (targetEquipment != null) {
            HandleEquipmentDrop(draggedSlot, targetEquipment);
        } else {
            Slot targetSlot = FindDropTarget(dropPosition);
            if (targetSlot != null) {
                if (debugMode) Debug.Log($"Dropping on inventory slot (HasItem: {targetSlot.HasItem})");

                if (_draggedFromEquipment != null) {
                    HandleUnequipToInventory(_draggedFromEquipment, targetSlot);
                } else if (targetSlot != draggedSlot) {
                    if (debugMode) {
                        string action = targetSlot.HasItem ? "swapping with" : "moving to empty";
                        Debug.Log($"Dropping from slot {draggedSlot.SlotIndex} to slot {targetSlot.SlotIndex} ({action})");
                    }

                    targetSlot.AddToClassList("inventorySlots--drop-target");
                    StartCoroutine(DelayedMoveItem(draggedSlot.SlotIndex, targetSlot.SlotIndex, targetSlot));
                } else if (targetSlot == draggedSlot) {
                    if (debugMode) Debug.Log("Dropped back on original slot - cancelled");
                }
            } else {
                if (debugMode) Debug.Log("Dropped outside any slot - cancelled");
            }
        }
    }

    private void HandleUnequipToInventory(EquipmentSlotWrapper fromEquipment, Slot toInventorySlot)
    {
        if (fromEquipment == null || toInventorySlot == null || equipmentController == null || inventoryController == null) {
            if (debugMode) Debug.LogError("HandleUnequipToInventory: Missing references");
            return;
        }

        var equippedItem = fromEquipment.GetEquippedItem();
        if (equippedItem == null) {
            if (debugMode) Debug.LogError("HandleUnequipToInventory: No equipped item");
            return;
        }

        if (debugMode) Debug.Log($"Unequipping {equippedItem.itemData.itemName} to inventory");

        int targetSlotIndex = -1;
        for (int i = 0; i < slots.Count; i++) {
            if (slots[i] == toInventorySlot) {
                targetSlotIndex = i;
                break;
            }
        }

        if (targetSlotIndex >= 0) {
            if (debugMode) Debug.Log($"Found target inventory slot index: {targetSlotIndex}");

            toInventorySlot.AddToClassList("inventorySlots--drop-target");

            var inventoryItemInSlot = inventoryController.GetItemAtSlot(targetSlotIndex);

            if (inventoryItemInSlot != null) {
                if (debugMode) Debug.Log($"Target slot has item: {inventoryItemInSlot.itemData.itemName}");

                if (fromEquipment.CanAcceptItem(inventoryItemInSlot.itemData)) {
                    if (debugMode) Debug.Log($"Can swap: {inventoryItemInSlot.itemData.itemName} can be equipped in {fromEquipment.SlotType}");
                    StartCoroutine(PerformSwapTransaction(fromEquipment, equippedItem, targetSlotIndex, inventoryItemInSlot, toInventorySlot));
                } else {
                    if (debugMode) Debug.Log($"Cannot swap: {inventoryItemInSlot.itemData.itemName} cannot be equipped in {fromEquipment.SlotType}");
                    toInventorySlot.RemoveFromClassList("inventorySlots--drop-target");
                }
            } else {
                if (debugMode) Debug.Log("Target slot is empty - simple unequip");
                StartCoroutine(PerformUnequipTransaction(fromEquipment, targetSlotIndex, toInventorySlot));
            }
        } else {
            if (debugMode) Debug.LogError("Could not find target inventory slot index");
        }
    }

    private System.Collections.IEnumerator PerformUnequipTransaction(EquipmentSlotWrapper fromEquipment, int targetSlotIndex, Slot toInventorySlot)
    {
        yield return null;

        if (debugMode) Debug.Log($"Performing unequip to slot {targetSlotIndex}");

        var equippedItem = fromEquipment.GetEquippedItem();

        if (equippedItem != null) {
            // Clear equipment slot first
            fromEquipment.ClearItem();
            fromEquipment.RefreshVisualState();
            equipmentController?.UnequipSlot(fromEquipment.SlotType);

            // Get the visual slot
            if (targetSlotIndex >= 0 && targetSlotIndex < slots.Count) {
                var visualSlot = slots[targetSlotIndex];
                if (visualSlot != null) {
                    visualSlot.SetItem(equippedItem);
                }
            }

            // Update data
            inventoryController?.AddItemToSlot(equippedItem, targetSlotIndex);

            if (debugMode) Debug.Log($"Unequipped {equippedItem.itemData.itemName} to inventory slot {targetSlotIndex}");
        } else {
            if (debugMode) Debug.LogError("No equipped item to unequip");
        }

        if (toInventorySlot != null) {
            toInventorySlot.RemoveFromClassList("inventorySlots--drop-target");
        }
    }

    private System.Collections.IEnumerator PerformSwapTransaction(
        EquipmentSlotWrapper equipmentSlot,
        Inventory_Item equippedItem,
        int inventorySlotIndex,
        Inventory_Item inventoryItem,
        Slot inventorySlotVisual)
    {
        yield return null;

        if (debugMode) Debug.Log($"Performing swap: {equippedItem.itemData.itemName} ↔ {inventoryItem.itemData.itemName} at slot {inventorySlotIndex}");

        // Get the target inventory slot
        if (inventorySlotIndex < 0 || inventorySlotIndex >= slots.Count) {
            if (debugMode) Debug.LogError($"Invalid inventory slot index: {inventorySlotIndex}");
            inventorySlotVisual.RemoveFromClassList("inventorySlots--drop-target");
            yield break;
        }

        var targetInventorySlot = slots[inventorySlotIndex];
        if (targetInventorySlot == null) {
            if (debugMode) Debug.LogError($"No slot found at index: {inventorySlotIndex}");
            inventorySlotVisual.RemoveFromClassList("inventorySlots--drop-target");
            yield break;
        }

        // Update visual slot FIRST
        targetInventorySlot.ClearItem();
        targetInventorySlot.SetItem(equippedItem);

        // Update inventory data
        inventoryController?.RemoveItemAtSlot(inventorySlotIndex);
        inventoryController?.AddItemToSlot(equippedItem, inventorySlotIndex);

        // Update equipment slot
        equipmentSlot.ClearItem();
        equipmentSlot.RefreshVisualState();
        equipmentController?.UnequipSlot(equipmentSlot.SlotType);

        equipmentController?.EquipItem(inventoryItem, equipmentSlot.SlotType);
        equipmentSlot.SetItem(inventoryItem);
        equipmentSlot.RefreshVisualState();

        if (debugMode) Debug.Log($"Swap complete: {equippedItem.itemData.itemName} moved to inventory slot {inventorySlotIndex}, {inventoryItem.itemData.itemName} equipped");

        inventorySlotVisual.RemoveFromClassList("inventorySlots--drop-target");
    }

    private void HandleEquipmentDrop(Slot fromSlot, EquipmentSlotWrapper toEquipment)
    {
        if (fromSlot == null || toEquipment == null || inventoryController == null) return;

        if (_draggedFromEquipment == toEquipment) {
            if (debugMode) Debug.Log("Cannot drop on same equipment slot");
            return;
        }

        if (!CanEquipItemInSlot(fromSlot, toEquipment)) {
            if (debugMode) Debug.Log($"Cannot equip this item in {toEquipment.SlotType} slot");
            return;
        }

        toEquipment.VisualElement.AddToClassList("inventorySlots--drop-target");
        StartCoroutine(PerformEquipmentTransaction(fromSlot, toEquipment));
    }

    private System.Collections.IEnumerator PerformEquipmentTransaction(Slot fromSlot, EquipmentSlotWrapper toEquipment)
    {
        yield return null;

        Inventory_Item itemToEquip = null;

        if (_draggedFromEquipment != null) {
            itemToEquip = _draggedFromEquipment.GetEquippedItem();
            if (debugMode) Debug.Log($"Dragging from equipment slot: {itemToEquip?.itemData?.itemName}");
        } else {
            itemToEquip = inventoryController?.GetItemAtSlot(fromSlot.SlotIndex);
            if (debugMode) Debug.Log($"Dragging from inventory slot: {itemToEquip?.itemData?.itemName}");
        }

        if (itemToEquip != null) {
            var currentlyEquipped = equipmentController?.GetEquippedItem(toEquipment.SlotType);

            if (currentlyEquipped != null) {
                if (debugMode) Debug.Log($"Target slot already has item: {currentlyEquipped.itemData.itemName}");

                if (_draggedFromEquipment != null) {
                    // Clear both slots first
                    _draggedFromEquipment.ClearItem();
                    _draggedFromEquipment.RefreshVisualState();
                    toEquipment.ClearItem();
                    toEquipment.RefreshVisualState();

                    // Unequip from both slots
                    equipmentController?.UnequipSlot(_draggedFromEquipment.SlotType);
                    equipmentController?.UnequipSlot(toEquipment.SlotType);

                    // Equip items in swapped positions
                    equipmentController?.EquipItem(itemToEquip, toEquipment.SlotType);
                    toEquipment.SetItem(itemToEquip);
                    toEquipment.RefreshVisualState();

                    equipmentController?.EquipItem(currentlyEquipped, _draggedFromEquipment.SlotType);
                    _draggedFromEquipment.SetItem(currentlyEquipped);
                    _draggedFromEquipment.RefreshVisualState();

                    if (debugMode) Debug.Log($"Swapped {itemToEquip.itemData.itemName} with {currentlyEquipped.itemData.itemName} between equipment slots");
                } else {
                    int emptySlotIndex = FindEmptyInventorySlot();
                    if (emptySlotIndex >= 0) {
                        // Update visual slot for the item being moved to inventory
                        if (emptySlotIndex >= 0 && emptySlotIndex < slots.Count) {
                            var emptySlot = slots[emptySlotIndex];
                            if (emptySlot != null) {
                                emptySlot.SetItem(currentlyEquipped);
                            }
                        }

                        inventoryController?.AddItemToSlot(currentlyEquipped, emptySlotIndex);
                        inventoryController?.RemoveItemAtSlot(fromSlot.SlotIndex);

                        // Clear and set equipment
                        toEquipment.ClearItem();
                        toEquipment.RefreshVisualState();

                        equipmentController?.EquipItem(itemToEquip, toEquipment.SlotType);
                        toEquipment.SetItem(itemToEquip);
                        toEquipment.RefreshVisualState();

                        if (debugMode) Debug.Log($"Equipped {itemToEquip.itemData.itemName}, unequipped {currentlyEquipped.itemData.itemName} to inventory slot {emptySlotIndex}");
                    } else {
                        if (debugMode) Debug.LogWarning("No empty inventory slots to unequip item!");
                        toEquipment.VisualElement.RemoveFromClassList("inventorySlots--drop-target");
                        yield break;
                    }
                }
            } else {
                if (_draggedFromEquipment != null) {
                    _draggedFromEquipment.ClearItem();
                    _draggedFromEquipment.RefreshVisualState();
                    equipmentController?.UnequipSlot(_draggedFromEquipment.SlotType);

                    toEquipment.ClearItem();
                    toEquipment.RefreshVisualState();

                    equipmentController?.EquipItem(itemToEquip, toEquipment.SlotType);
                    toEquipment.SetItem(itemToEquip);
                    toEquipment.RefreshVisualState();

                    if (debugMode) Debug.Log($"Moved {itemToEquip.itemData.itemName} from {_draggedFromEquipment.SlotType} to {toEquipment.SlotType}");
                } else {
                    inventoryController?.RemoveItemAtSlot(fromSlot.SlotIndex);

                    toEquipment.ClearItem();
                    toEquipment.RefreshVisualState();

                    equipmentController?.EquipItem(itemToEquip, toEquipment.SlotType);
                    toEquipment.SetItem(itemToEquip);
                    toEquipment.RefreshVisualState();

                    if (debugMode) Debug.Log($"Equipped {itemToEquip.itemData.itemName} in {toEquipment.SlotType} slot");
                }
            }
        }

        toEquipment.VisualElement.RemoveFromClassList("inventorySlots--drop-target");
    }

    private int FindEmptyInventorySlot()
    {
        if (inventoryController == null) return -1;

        for (int i = 0; i < inventoryController.Slots.Count; i++) {
            if (!inventoryController.Slots[i].HasItem) {
                return i;
            }
        }
        return -1;
    }

    private System.Collections.IEnumerator DelayedMoveItem(int fromIndex, int toIndex, Slot targetSlot)
    {
        yield return null;
        inventoryController.MoveItem(fromIndex, toIndex);

        if (targetSlot != null) {
            targetSlot.RemoveFromClassList("inventorySlots--drop-target");
        }
    }

    private Slot FindDropTarget(Vector2 position)
    {
        if (slots == null) return null;

        foreach (var slot in slots) {
            if (slot != null) {
                var bounds = slot.worldBound;
                var paddedBounds = new Rect(
                    bounds.x - 10,
                    bounds.y - 10,
                    bounds.width + 20,
                    bounds.height + 20
                );

                if (paddedBounds.Contains(position)) {
                    if (debugMode) Debug.Log($"Found drop target slot at position {position}");
                    return slot;
                }
            }
        }

        if (debugMode) Debug.Log($"No drop target found at position {position}");
        return null;
    }

    private void ResetDrag()
    {
        if (draggedSlot != null) {
            draggedSlot.RemoveFromClassList("inventorySlots--dragging");
        }

        if (highlightEmptySlots) {
            RemoveEmptySlotHighlighting();
        }

        if (dragGhost != null) {
            dragGhost.RemoveFromClassList("drag-ghost--visible");
            dragGhost.RemoveFromClassList("drag-ghost--over-empty");
            dragGhost.RemoveFromClassList("drag-ghost--over-occupied");
            dragGhost.style.backgroundImage = null;
        }

        isDragging = false;
        draggedSlot = null;
        _clickedEquipmentSlot = null;
        _draggedFromEquipment = null;

        if (debugMode) Debug.Log("Drag reset");
    }

    // Debug method to check equipment slot states
    private void DebugAllEquipmentSlots(string context)
    {
        if (!debugMode) return;

        Debug.Log($"=== {context} ===");
        foreach (var eqSlot in equipmentSlots) {
            var defaultIcon = eqSlot.VisualElement.Q<VisualElement>("default-icon-container");
            var itemIcon = eqSlot.VisualElement.Q<Image>("equipment-icon");

            Debug.Log($"{eqSlot.SlotType}: HasItem={eqSlot.HasItem}, DefaultIcon={(defaultIcon != null ? $"Display:{defaultIcon.style.display}" : "null")}, ItemIcon={(itemIcon != null ? $"Display:{itemIcon.style.display}" : "null")}");
        }
    }

    public bool IsDragging() => isDragging;
    public Slot GetDraggedSlot() => draggedSlot;
}