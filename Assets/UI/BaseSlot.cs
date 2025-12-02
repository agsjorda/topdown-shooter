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

    public BaseSlot()
    {
        AddToClassList("inventorySlots");

        Icon = new Image { pickingMode = PickingMode.Ignore };
        Icon.AddToClassList("inventorySlots-icon");
        Icon.scaleMode = ScaleMode.ScaleToFit;
        Icon.style.width = Length.Percent(100);
        Icon.style.height = Length.Percent(100);
        Add(Icon);
    }

    public virtual void SetItem(Inventory_Item item, int qty = 1)
    {
        if (item == null || item.itemData == null) {
            ClearItem();
            return;
        }

        HasItem = true;
        Icon.sprite = item.itemData.icon;
    }

    public virtual void ClearItem()
    {
        HasItem = false;
        Icon.sprite = null;
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

        if (!useBackground || HasItem) {
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