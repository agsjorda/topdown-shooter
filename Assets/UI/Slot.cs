using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class Slot : VisualElement
{
    private Image _icon;
    private bool _hasItem;
    private string _itemId = string.Empty;

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

    public int SlotIndex { get; private set; }
    public bool HasItem => _hasItem;
    public string ItemId => _itemId;

    protected int slotSize = 100;
    protected float cellMargin = 8f;

    // REMOVED: Event for drag start - we'll handle it differently
    // public event Action<Vector2, Slot> OnBeginDrag;

    public Slot()
    {
        AddToClassList("inventorySlots");
        focusable = true;
        pickingMode = PickingMode.Position;
    }

    // REMOVED: Method to trigger drag
    // public void BeginDrag(Vector2 position) { }

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

    // REMOVED: Visual feedback during drag (handled in DragDropController)
    // public void SetDragState(bool isDragging) { }

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