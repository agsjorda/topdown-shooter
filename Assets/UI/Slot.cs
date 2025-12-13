using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class Slot : VisualElement
{
    private Image _icon;
    protected bool _hasItem;
    protected string _itemId = string.Empty;
    private int _slotIndex;

    public Image Icon {
        get {
            if (_icon == null) {
                _icon = this.Q<Image>("inventorySlots-icon");
                if (_icon == null) {
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

    public int SlotIndex {
        get => _slotIndex;
        protected set => _slotIndex = value;
    }

    public bool HasItem => _hasItem;
    public string ItemId => _itemId;

    // NEW: Property to identify if this is an equipment slot
    public virtual bool IsEquipmentSlot => false;

    // NEW: Property for equipment type
    public virtual EquipmentSlotType EquipmentType => EquipmentSlotType.Weapon;

    protected int slotSize = 100;
    protected float cellMargin = 8f;

    public Slot()
    {
        AddToClassList("inventorySlots");
        focusable = true;
        pickingMode = PickingMode.Position;
    }

    // NEW: Method to check if item can be accepted
    public virtual bool CanAcceptItem(Item_DataSO itemData)
    {
        return true; // Default: accept all items
    }

    public virtual void SetItem(Inventory_Item item, int qty = 1)
    {
        if (item == null || item.itemData == null) {
            ClearItem();
            return;
        }

        _hasItem = true;
        _itemId = item.itemData.itemId ?? string.Empty;

        if (item.itemData.icon != null) {
            if (Icon.image != item.itemData.icon.texture) {
                Icon.image = item.itemData.icon.texture;
                Icon.sprite = item.itemData.icon;
            }

            if (Icon.style.display != DisplayStyle.Flex) {
                Icon.style.display = DisplayStyle.Flex;
            }
        } else {
            ClearItem();
        }
    }

    public virtual void ClearItem()
    {
        _hasItem = false;
        _itemId = string.Empty;

        if (Icon.image != null) {
            Icon.image = null;
            Icon.sprite = null;
        }

        if (Icon.style.display != DisplayStyle.None) {
            Icon.style.display = DisplayStyle.None;
        }
    }

    public virtual void SetSlotIndex(int index) => SlotIndex = index;

    #region UI settings
    public virtual void SetSlotSize(int size)
    {
        if (slotSize != size) {
            slotSize = Mathf.Max(1, size);
            style.width = slotSize;
            style.height = slotSize;
        }
    }

    public virtual void SetCellMargin(float margin)
    {
        if (!Mathf.Approximately(cellMargin, margin)) {
            cellMargin = Mathf.Max(0f, margin);
            style.marginRight = cellMargin;
            style.marginBottom = cellMargin;
        }
    }
    #endregion
}