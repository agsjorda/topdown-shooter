using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class BaseSlot : VisualElement
{
    // Use backing field pattern correctly
    private Image _icon;
    private bool _hasItem;

    public Image Icon {
        get {
            // Lazy initialization with proper null check
            if (_icon == null) {
                _icon = this.Q<Image>("inventorySlots-icon");
                if (_icon == null) {
                    // Create if doesn't exist
                    _icon = new Image {
                        name = "inventorySlots-icon",
                        scaleMode = ScaleMode.ScaleToFit
                    };
                    _icon.AddToClassList("inventorySlots-icon");
                    _icon.style.width = Length.Percent(100);
                    _icon.style.height = Length.Percent(100);
                    Add(_icon);
                }
            }
            return _icon;
        }
    }

    public int SlotIndex { get; private set; }
    public bool HasItem => _hasItem;  // Use expression-bodied property

    protected int slotSize = 100;
    protected float cellMargin = 8f;

    public BaseSlot()
    {
        AddToClassList("inventorySlots");
        // Icon will be created lazily when accessed
    }

    public virtual void SetItem(Inventory_Item item, int qty = 1)
    {
        if (item == null || item.itemData == null) {
            ClearItem();
            return;
        }

        _hasItem = true;

        if (item.itemData.icon != null) {
            // Only assign if different to avoid unnecessary redraws
            if (Icon.image != item.itemData.icon.texture) {
                Icon.image = item.itemData.icon.texture;
                Icon.sprite = item.itemData.icon;
            }

            // Only change display if currently hidden
            if (Icon.style.display != DisplayStyle.Flex) {
                Icon.style.display = DisplayStyle.Flex;
            }
        } else {
            // Handle missing icon gracefully
            ClearItem();
        }
    }

    public virtual void ClearItem()
    {
        _hasItem = false;

        // Only clear if there's something to clear
        if (Icon.image != null) {
            Icon.image = null;
            Icon.sprite = null;
        }

        // Only hide if currently visible
        if (Icon.style.display != DisplayStyle.None) {
            Icon.style.display = DisplayStyle.None;
        }
    }

    public virtual void SetSlotIndex(int index) => SlotIndex = index;

    #region UI settings
    public virtual void SetSlotSize(int size)
    {
        // Only update if changed
        if (slotSize != size) {
            slotSize = Mathf.Max(1, size);
            style.width = slotSize;
            style.height = slotSize;
        }
    }

    public virtual void SetCellMargin(float margin)
    {
        // Only update if changed
        if (!Mathf.Approximately(cellMargin, margin)) {
            cellMargin = Mathf.Max(0f, margin);
            style.marginRight = cellMargin;
            style.marginBottom = cellMargin;
        }
    }
    #endregion
}