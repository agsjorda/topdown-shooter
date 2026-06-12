using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
    [UxmlElement]
    public partial class SlotView : VisualElement
    {
        private Image _icon;
        private Label _qtyLabel;
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

        // Stack-count label shown when an item's quantity is above 1.
        // Styleable via the "inventorySlots-qty" USS class.
        public Label QtyLabel {
            get {
                if (_qtyLabel == null) {
                    _qtyLabel = this.Q<Label>("inventorySlots-qty");
                    if (_qtyLabel == null) {
                        _qtyLabel = new Label {
                            name = "inventorySlots-qty",
                            pickingMode = PickingMode.Ignore
                        };
                        _qtyLabel.AddToClassList("inventorySlots-qty");
                        _qtyLabel.style.position = Position.Absolute;
                        _qtyLabel.style.right = 2;
                        _qtyLabel.style.bottom = 2;
                        _qtyLabel.style.display = DisplayStyle.None;
                        Add(_qtyLabel);
                    }
                }
                return _qtyLabel;
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

        // NEW: Property for equipment type (null for plain inventory slots)
        public virtual EquipmentSlotTypeSO EquipmentType => null;

        protected int slotSize = 100;
        protected float cellMargin = 8f;

        public SlotView()
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

        public virtual void SetItem(InventoryItem item)
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

                UpdateQuantity(item.quantity);
            } else {
                ClearItem();
            }
        }

        protected void UpdateQuantity(int quantity)
        {
            if (quantity > 1) {
                QtyLabel.text = quantity.ToString();
                QtyLabel.style.display = DisplayStyle.Flex;
            } else if (_qtyLabel != null) {
                _qtyLabel.style.display = DisplayStyle.None;
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

            if (_qtyLabel != null) {
                _qtyLabel.style.display = DisplayStyle.None;
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
}
