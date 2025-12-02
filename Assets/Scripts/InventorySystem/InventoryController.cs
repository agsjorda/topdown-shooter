using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class InventoryController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument targetDocument;
    [SerializeField] private Inventory_Base inventory;
    [SerializeField] private InventoryUIConfig ui;

    public BaseSlot[] Slots;
    private IReadOnlyList<BaseSlot> slots;
    private VisualElement ghostIcon;
    private bool isDragging;
    private BaseSlot originalSlot;
    private Vector2 ghostIconOffset = Vector2.zero;

    public event System.Action<BaseSlot, BaseSlot> OnDrop;

    void Awake()
    {
        if (inventory == null) inventory = Object.FindFirstObjectByType<Inventory_Base>();
        if (ui == null) ui = Object.FindFirstObjectByType<InventoryUIConfig>();

        var root = targetDocument?.rootVisualElement;
        if (root != null) {
            ghostIcon = root.Q("ghostIcon");
            if (ghostIcon == null) {
                // Create ghost icon if it doesn't exist
                ghostIcon = new VisualElement();
                ghostIcon.name = "ghostIcon";
                ghostIcon.style.position = Position.Absolute;
                ghostIcon.style.width = 50;
                ghostIcon.style.height = 50;
                ghostIcon.style.visibility = Visibility.Hidden;
                ghostIcon.style.opacity = 0.8f;
                root.Add(ghostIcon);
            }
        }

        Debug.Log($"Ghost icon found: {ghostIcon != null}");
    }

    void OnEnable()
    {
        // Force UI rebuild first
        if (ui != null) {
            ui.RebuildUI();
        }

        StartCoroutine(InitSlots());

        if (inventory != null) {
            inventory.OnInventoryChanged -= OnInventoryChanged;
            inventory.OnInventoryChanged += OnInventoryChanged;
        }
    }

    void OnDisable()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= OnInventoryChanged;

        // Clean up event handlers
        if (ghostIcon != null) {
            ghostIcon.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            ghostIcon.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }
    }

    private IEnumerator InitSlots()
    {
        yield return null; // Wait for UI Toolkit to build

        // Get slots from UI Config
        if (ui != null && ui.Slots != null) {
            slots = ui.Slots;
            Slots = slots.ToArray();
            Debug.Log($"Found {Slots.Length} slots from UI Config");
        } else {
            Debug.LogError("UI Config or slots not found!");
            yield break;
        }

        // Initialize ghost icon
        if (ghostIcon != null) {
            ghostIcon.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            ghostIcon.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        // Register drag events for each slot
        foreach (var slot in Slots) {
            if (slot != null) {
                slot.OnStartDrag += OnPointerDown;
            }
        }

        // Initial sync
        SyncAllSlotsFromInventory();
    }

    private void OnPointerDown(Vector2 position, BaseSlot slot)
    {
        if (slot == null || !slot.HasItem) return;
        if (ghostIcon == null) return;

        Debug.Log($"Starting drag from slot {slot.SlotIndex}");

        isDragging = true;
        originalSlot = slot;

        // Calculate offset for smoother dragging
        ghostIconOffset = new Vector2(
            ghostIcon.layout.width / 2,
            ghostIcon.layout.height / 2
        );

        // Set ghost icon position
        SetGhostIconPosition(position);

        // Set ghost icon image - use the slot's icon
        if (slot.Icon?.image != null) {
            // ghostIcon.style.backgroundImage = new StyleBackground(slot.Icon.image);
            ghostIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(slot.Icon.image as Texture2D));
        } else if (originalSlot.Icon?.sprite != null && originalSlot.Icon.sprite.texture != null) {
            // ghostIcon.style.backgroundImage = new StyleBackground(originalSlot.Icon.sprite.texture);
            ghostIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(originalSlot.Icon.sprite.texture));
        }

        // Hide the original slot's icon while dragging
        originalSlot.Icon.style.visibility = Visibility.Hidden;

        // Show the ghost icon
        ghostIcon.style.visibility = Visibility.Visible;
        ghostIcon.BringToFront();
    }

    private void SetGhostIconPosition(Vector2 position)
    {
        if (ghostIcon != null) {
            ghostIcon.style.left = position.x - ghostIconOffset.x;
            ghostIcon.style.top = position.y - ghostIconOffset.y;
        }
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!isDragging || ghostIcon == null) return;

        // Update ghost icon position
        SetGhostIconPosition(evt.position);
        evt.StopPropagation();
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!isDragging || ghostIcon == null) return;

        Debug.Log($"Ending drag at position {evt.position}");

        // Find the slot under the cursor
        BaseSlot closestSlot = null;
        float closestDistance = float.MaxValue;

        foreach (var slot in Slots) {
            if (slot == null) continue;

            // Check if pointer is within slot bounds
            if (slot.worldBound.Contains(evt.position)) {
                closestSlot = slot;
                break;
            }

            // Fallback: find closest slot if not exactly inside
            float distance = Vector2.Distance(
                slot.worldBound.center,
                evt.position
            );

            if (distance < closestDistance) {
                closestDistance = distance;
                closestSlot = slot;
            }
        }

        // Handle the drop
        if (closestSlot != null && closestSlot != originalSlot) {
            Debug.Log($"Dropping from slot {originalSlot.SlotIndex} to slot {closestSlot.SlotIndex}");
            OnDrop?.Invoke(originalSlot, closestSlot);

            // Swap items in inventory
            if (inventory != null) {
                inventory.SwapItems(originalSlot.SlotIndex, closestSlot.SlotIndex);
            }
        } else if (closestSlot == originalSlot) {
            // Dropped back on original slot
            Debug.Log("Dropped back on original slot");
        }

        // Reset dragging state
        EndDrag();
    }

    private void EndDrag()
    {
        // Show the original slot's icon again
        if (originalSlot != null) {
            originalSlot.Icon.style.visibility = Visibility.Visible;
        }

        // Hide ghost icon
        if (ghostIcon != null) {
            ghostIcon.style.visibility = Visibility.Hidden;
            ghostIcon.style.backgroundImage = null;
        }

        isDragging = false;
        originalSlot = null;
    }

    private void OnInventoryChanged()
    {
        SyncAllSlotsFromInventory();
    }

    private void SyncAllSlotsFromInventory()
    {
        if (slots == null || inventory == null) {
            Debug.LogWarning("Slots or inventory is null");
            return;
        }

        // Clear all slots first
        for (int i = 0; i < slots.Count; i++) {
            slots[i].ClearItem();
        }

        // Fill slots with inventory items
        int max = Mathf.Min(slots.Count, inventory.itemList.Count);
        for (int i = 0; i < max; i++) {
            var invItem = inventory.itemList[i];
            if (invItem != null && invItem.itemData != null) {
                slots[i].SetItem(invItem);
                Debug.Log($"Slot {i} set to: {invItem.itemData.itemName}");
            }
        }
    }
}