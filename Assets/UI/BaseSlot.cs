using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class BaseSlot : VisualElement
{
    public Image Icon;
    public int SlotIndex { get; private set; }
    public bool HasItem { get; protected set; }

    protected int slotSize = 200;
    protected float cellMargin = 4f;

    // Drag & drop event properties (easy to assign from controller)
    public System.Action<BaseSlot> OnBeginDrag { get; set; }
    public System.Action<BaseSlot> OnEndDrag { get; set; }
    public System.Action<BaseSlot> OnDrop { get; set; }

    public BaseSlot()
    {
        AddToClassList("inventorySlots");

        Icon = new Image { pickingMode = PickingMode.Ignore };
        Icon.AddToClassList("inventorySlots-icon");
        Icon.scaleMode = ScaleMode.ScaleToFit;
        Icon.style.width = Length.Percent(100);
        Icon.style.height = Length.Percent(100);
        Add(Icon);

        SetupInteractionEvents();
    }

    private void SetupInteractionEvents()
    {
        // Click to start drag
        RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.button != 0 || !HasItem) return;

            AddToClassList("dragging");
            OnBeginDrag?.Invoke(this);
            evt.StopPropagation();
        });

        // Release to end drag
        RegisterCallback<PointerUpEvent>(evt =>
        {
            if (evt.button != 0) return;

            RemoveFromClassList("dragging");
            OnEndDrag?.Invoke(this);
            OnDrop?.Invoke(this);
            evt.StopPropagation();
        });

        // Hover for drop target
        RegisterCallback<PointerEnterEvent>(evt =>
        {
            AddToClassList("drop-target");
        });

        RegisterCallback<PointerLeaveEvent>(evt =>
        {
            RemoveFromClassList("drop-target");
        });
    }

    public virtual void SetItem(Inventory_Item item, int qty = 1)
    {
        if (item == null || item.itemData == null) {
            ClearItem();
            return;
        }

        HasItem = true;
        Icon.sprite = item.itemData.icon;
        Icon.style.display = DisplayStyle.Flex;
    }

    public virtual void ClearItem()
    {
        HasItem = false;
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

        // Icon should always appear ABOVE the background
        Icon.style.position = Position.Absolute;
        Icon.style.left = 0;
        Icon.style.top = 0;


        if (!useBackground) {
            style.backgroundImage = null;
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