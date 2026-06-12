using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
    [UxmlElement]
    public partial class EquipmentSlotView : SlotView
    {
        private VisualElement _equipmentIcon;
        private Texture2D _originalBackgroundTexture;
        private Sprite _originalBackgroundSprite;

        public EquipmentSlotTypeSO SlotType { get; private set; }
        public InventoryItem CurrentItem { get; private set; }

        public override bool IsEquipmentSlot => true;
        public override EquipmentSlotTypeSO EquipmentType => SlotType;

        public EquipmentSlotView() : base()
        {
            // Remove inventory slot class, we use equipment slot classes
            RemoveFromClassList("inventorySlots");
        }

        public void Initialize(EquipmentSlotTypeSO slotType)
        {
            SlotType = slotType;

            // Store original background from USS
            var bgImage = style.backgroundImage;

            if (bgImage.keyword == StyleKeyword.Undefined && bgImage.value != null) {
                if (bgImage.value.texture != null) {
                    _originalBackgroundTexture = bgImage.value.texture as Texture2D;
                } else if (bgImage.value.sprite != null) {
                    _originalBackgroundSprite = bgImage.value.sprite;
                    _originalBackgroundTexture = _originalBackgroundSprite.texture;
                }
            }

            // Fallback to resolvedStyle for USS-computed values
            if (_originalBackgroundTexture == null && _originalBackgroundSprite == null) {
                var resolvedBg = resolvedStyle.backgroundImage;

                if (resolvedBg.texture != null) {
                    _originalBackgroundTexture = resolvedBg.texture as Texture2D;
                } else if (resolvedBg.sprite != null) {
                    _originalBackgroundSprite = resolvedBg.sprite;
                    _originalBackgroundTexture = _originalBackgroundSprite.texture;
                }
            }

            // Create equipment icon element (for equipped items)
            _equipmentIcon = new VisualElement {
                name = "equipment-icon"
            };
            _equipmentIcon.AddToClassList("equipment-icon");

            _equipmentIcon.style.width = Length.Percent(100);
            _equipmentIcon.style.height = Length.Percent(100);
            _equipmentIcon.style.position = Position.Absolute;
            _equipmentIcon.style.top = 0;
            _equipmentIcon.style.left = 0;
            _equipmentIcon.style.display = DisplayStyle.None;
            _equipmentIcon.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            _equipmentIcon.style.scale = new Scale(new Vector2(0.8f, 0.8f));

            Add(_equipmentIcon);

            // Add equipment slot type classes
            AddToClassList("equipment-slot");
            string suffix = string.IsNullOrEmpty(slotType.ussClassSuffix) ? slotType.id : slotType.ussClassSuffix;
            if (!string.IsNullOrEmpty(suffix)) {
                AddToClassList($"equipment-slot--{suffix.ToLower()}");
            }
        }

        // Slot fit is item-driven: the item's compatibleSlots list is the single source
        // of truth, so any game's slot taxonomy works without changes here.
        public override bool CanAcceptItem(Item_DataSO itemData)
        {
            return itemData != null && SlotType != null && itemData.CanEquipTo(SlotType);
        }

        public override void SetItem(InventoryItem item)
        {
            if (item == null || item.itemData == null) {
                ClearItem();
                return;
            }

            CurrentItem = item;
            _hasItem = true;
            _itemId = item.itemData.itemId ?? string.Empty;

            // Hide default background
            style.backgroundImage = StyleKeyword.None;

            // Show equipped item icon
            if (_equipmentIcon != null && item.itemData.icon != null) {
                _equipmentIcon.style.backgroundImage = new StyleBackground(item.itemData.icon);
                _equipmentIcon.style.display = DisplayStyle.Flex;
            }

            AddToClassList("has-item");
        }

        public override void ClearItem()
        {
            CurrentItem = null;
            _hasItem = false;
            _itemId = string.Empty;

            // Restore default background
            if (_originalBackgroundSprite != null) {
                style.backgroundImage = new StyleBackground(_originalBackgroundSprite);
            } else if (_originalBackgroundTexture != null) {
                style.backgroundImage = new StyleBackground(_originalBackgroundTexture);
            }

            // Hide equipment icon
            if (_equipmentIcon != null) {
                _equipmentIcon.style.backgroundImage = StyleKeyword.None;
                _equipmentIcon.style.display = DisplayStyle.None;
            }

            RemoveFromClassList("has-item");
        }

        public InventoryItem GetEquippedItem()
        {
            return CurrentItem;
        }

        public void RefreshVisualState()
        {
            if (HasItem && CurrentItem != null && CurrentItem.itemData?.icon != null) {
                style.backgroundImage = StyleKeyword.None;

                if (_equipmentIcon != null) {
                    _equipmentIcon.style.backgroundImage = new StyleBackground(CurrentItem.itemData.icon);
                    _equipmentIcon.style.display = DisplayStyle.Flex;
                }

                AddToClassList("has-item");
            } else {
                if (_originalBackgroundSprite != null) {
                    style.backgroundImage = new StyleBackground(_originalBackgroundSprite);
                } else if (_originalBackgroundTexture != null) {
                    style.backgroundImage = new StyleBackground(_originalBackgroundTexture);
                }

                if (_equipmentIcon != null) {
                    _equipmentIcon.style.backgroundImage = StyleKeyword.None;
                    _equipmentIcon.style.display = DisplayStyle.None;
                }

                RemoveFromClassList("has-item");
            }
        }
    }
}
