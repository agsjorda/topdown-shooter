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
                            scaleMode = ScaleMode.ScaleToFit,
                            pickingMode = PickingMode.Ignore
                        };
                        _icon.AddToClassList("inventorySlots-icon");
                        Add(_icon);
                    }
                }
                return _icon;
            }
        }

        // Stack-count label shown when an item's quantity is above 1.
        // Visibility is the inventorySlots--stacked class; look is inventorySlots-qty in USS.
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

        public virtual bool IsEquipmentSlot => false;
        public virtual EquipmentSlotTypeSO EquipmentType => null;

        protected int slotSize = 100;
        protected float cellMargin = 8f;

        public SlotView()
        {
            AddToClassList("inventorySlots");
            focusable = true;
            pickingMode = PickingMode.Position;
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
                EnableInClassList("has-item", true);
                UpdateQuantity(item.quantity);
            } else {
                ClearItem();
            }
        }

        protected void UpdateQuantity(int quantity)
        {
            bool stacked = quantity > 1;
            EnableInClassList("inventorySlots--stacked", stacked);
            if (stacked) QtyLabel.text = quantity.ToString();
        }

        public virtual void ClearItem()
        {
            _hasItem = false;
            _itemId = string.Empty;

            if (_icon != null) {
                _icon.image = null;
                _icon.sprite = null;
            }

            EnableInClassList("has-item", false);
            EnableInClassList("inventorySlots--stacked", false);
        }

        public virtual void SetSlotIndex(int index) => SlotIndex = index;

        // Pixel sizes come from InventoryUIConfig, so they stay inline.
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
    }
}
