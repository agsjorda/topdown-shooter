using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class BaseSlot : VisualElement
{
    private Image _icon;

    public Image Icon => _icon ??= this.Q<Image>("inventorySlots-icon");
    public int SlotIndex { get; private set; }
    public bool HasItem { get; protected set; }

    protected int slotSize = 100;
    protected float cellMargin = 8f;

    public BaseSlot()
    {
        AddToClassList("inventorySlots");

        // Create icon
        _icon = new Image {
            name = "inventorySlots-icon",
            scaleMode = ScaleMode.ScaleToFit
        };
        _icon.AddToClassList("inventorySlots-icon");
        _icon.style.width = Length.Percent(100);
        _icon.style.height = Length.Percent(100);
        Add(_icon);

        // Removed PointerDown event registration
    }

    public virtual void SetItem(Inventory_Item item, int qty = 1)
    {
        if (item == null || item.itemData == null) {
            ClearItem();
            return;
        }

        HasItem = true;

        if (item.itemData.icon != null) {
            Icon.image = item.itemData.icon.texture;
            Icon.sprite = item.itemData.icon;
            Icon.style.display = DisplayStyle.Flex;
        }
    }

    public virtual void ClearItem()
    {
        HasItem = false;
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
    }

    public virtual void SetCellMargin(float margin)
    {
        cellMargin = Mathf.Max(0f, margin);
        style.marginRight = cellMargin;
        style.marginBottom = cellMargin;
    }
    #endregion
}