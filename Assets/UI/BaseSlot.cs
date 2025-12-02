using System;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class BaseSlot : VisualElement
{
    private Image _icon;
    private Label _stackLabel;
    private Sprite _baseSprite;

    public Image Icon => _icon ??= this.Q<Image>("inventorySlots-icon");
    public Label StackLabel => _stackLabel ??= this.Q<Label>("stackCount");
    public Sprite BaseSprite => _baseSprite;

    public int Index => parent?.IndexOf(this) ?? -1;
    public SerializableGuid ItemId { get; private set; } = SerializableGuid.Empty;

    public int SlotIndex { get; private set; }
    public bool HasItem { get; protected set; }

    protected int slotSize = 200;
    protected float cellMargin = 4f;

    public event Action<Vector2, BaseSlot> OnStartDrag = delegate { };

    public BaseSlot()
    {
        AddToClassList("inventorySlots");

        // Create icon if it doesn't exist
        _icon = new Image {
            name = "inventorySlots-icon",
            scaleMode = ScaleMode.ScaleToFit
        };
        _icon.AddToClassList("inventorySlots-icon");
        _icon.style.width = Length.Percent(100);
        _icon.style.height = Length.Percent(100);
        Add(_icon);

        RegisterCallback<PointerDownEvent>(OnPointerDown);
        RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        RegisterCallback<MouseLeaveEvent>(OnMouseLeave);

        // Add hover effect
        this.AddManipulator(new Clickable(() => { }));
    }

    private void OnMouseEnter(MouseEnterEvent evt)
    {
        if (HasItem) {
            style.borderTopColor = Color.yellow;
            style.borderBottomColor = Color.yellow;
            style.borderLeftColor = Color.yellow;
            style.borderRightColor = Color.yellow;
        }
    }

    private void OnMouseLeave(MouseLeaveEvent evt)
    {
        style.borderTopColor = Color.clear;
        style.borderBottomColor = Color.clear;
        style.borderLeftColor = Color.clear;
        style.borderRightColor = Color.clear;
    }

    public void OnPointerDown(PointerDownEvent evt)
    {
        if (evt.button != 0 || !HasItem) return;

        Debug.Log($"Pointer down on slot {SlotIndex}, HasItem: {HasItem}");
        OnStartDrag?.Invoke(evt.position, this);
        evt.StopPropagation();
    }

    public virtual void SetItem(Inventory_Item item, int qty = 1)
    {
        if (item == null || item.itemData == null) {
            ClearItem();
            return;
        }

        HasItem = true;
        _baseSprite = item.itemData.icon;

        if (item.itemData.icon != null) {
            Icon.image = item.itemData.icon.texture;
            Icon.sprite = item.itemData.icon;
            Icon.style.display = DisplayStyle.Flex;
            Icon.style.visibility = Visibility.Visible;
            Icon.style.opacity = 1f;

            Debug.Log($"Set item icon for slot {SlotIndex}: {item.itemData.itemName}");
        } else {
            Debug.LogWarning($"Item {item.itemData.itemName} has no icon!");
            Icon.image = null;
            Icon.sprite = null;
        }
    }

    public virtual void ClearItem()
    {
        HasItem = false;
        ItemId = SerializableGuid.Empty;
        _baseSprite = null;

        Icon.image = null;
        Icon.sprite = null;
        Icon.style.display = DisplayStyle.None;
    }

    public virtual void SetSlotIndex(int index) => SlotIndex = index;

    #region UI settings
    public virtual void SetSlotSize(int size)
    {
        slotSize = Mathf.Max(1, size);
        style.width = slotSize;
        style.height = slotSize;

        // Also update icon size
        if (Icon != null) {
            Icon.style.width = slotSize - 10;
            Icon.style.height = slotSize - 10;
        }
    }

    public virtual void SetCellMargin(float margin)
    {
        cellMargin = Mathf.Max(0f, margin);
        style.marginRight = cellMargin;
        style.marginBottom = cellMargin;
    }

    public void ApplyBackground(bool useBackground, Texture2D tex, Sprite sprite, Color tint, float radius)
    {
        style.borderTopLeftRadius = radius;
        style.borderTopRightRadius = radius;
        style.borderBottomLeftRadius = radius;
        style.borderBottomRightRadius = radius;

        // Set border
        style.borderTopWidth = 2;
        style.borderBottomWidth = 2;
        style.borderLeftWidth = 2;
        style.borderRightWidth = 2;
        style.borderTopColor = Color.clear;
        style.borderBottomColor = Color.clear;
        style.borderLeftColor = Color.clear;
        style.borderRightColor = Color.clear;

        if (!useBackground) {
            style.backgroundImage = null;
            style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f); // Default background
            return;
        }

        Texture2D resolved = tex;
        if (resolved == null && sprite != null)
            resolved = sprite.texture;

        if (resolved != null) {
            style.backgroundImage = new StyleBackground(resolved);
            style.unityBackgroundImageTintColor = tint;
        } else {
            style.backgroundImage = null;
            style.backgroundColor = tint;
        }
    }

    public void SetStaticBorderColor(Color c)
    {
        style.borderLeftColor = c;
        style.borderRightColor = c;
        style.borderTopColor = c;
        style.borderBottomColor = c;
    }
    #endregion
}