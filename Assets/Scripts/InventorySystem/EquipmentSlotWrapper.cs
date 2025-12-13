using UnityEngine;
using UnityEngine.UIElements;

public class EquipmentSlotWrapper
{
    public VisualElement VisualElement { get; private set; }
    public EquipmentSlotType SlotType { get; private set; }
    public bool HasItem { get; private set; }
    public Inventory_Item CurrentItem { get; private set; }

    private VisualElement _icon;
    private Texture2D _originalBackgroundTexture;
    private Sprite _originalBackgroundSprite;

    public EquipmentSlotWrapper(VisualElement element, EquipmentSlotType slotType)
    {
        VisualElement = element;
        SlotType = slotType;
        CurrentItem = null;
        HasItem = false;

        // Try to get background from inline style first
        var bgImage = element.style.backgroundImage;

        if (bgImage.keyword == StyleKeyword.Undefined && bgImage.value != null) {
            // Has inline style
            if (bgImage.value.texture != null) {
                _originalBackgroundTexture = bgImage.value.texture as Texture2D;
            } else if (bgImage.value.sprite != null) {
                _originalBackgroundSprite = bgImage.value.sprite;
                _originalBackgroundTexture = _originalBackgroundSprite.texture;
            }
        }

        // ALWAYS try resolvedStyle as it contains computed values from USS
        if (_originalBackgroundTexture == null && _originalBackgroundSprite == null) {
            var resolvedBg = element.resolvedStyle.backgroundImage;

            if (resolvedBg.texture != null) {
                _originalBackgroundTexture = resolvedBg.texture as Texture2D;
            } else if (resolvedBg.sprite != null) {
                _originalBackgroundSprite = resolvedBg.sprite;
                _originalBackgroundTexture = _originalBackgroundSprite.texture;
            }
        }

        // Create the item icon using VisualElement instead of Image
        // This ensures rotation works the same way as the default icon
        _icon = new VisualElement {
            name = "equipment-icon"
        };
        _icon.AddToClassList("equipment-icon");

        // Set all styling inline to ensure immediate application
        _icon.style.width = Length.Percent(100);
        _icon.style.height = Length.Percent(100);
        _icon.style.position = Position.Absolute;
        _icon.style.top = 0;
        _icon.style.left = 0;
        _icon.style.display = DisplayStyle.None;

        // Set background image scale mode
        _icon.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

        // REMOVED: DO NOT apply rotation inline! The USS class already has rotate: -45deg
        // The parent (equipSlots) has rotate: 45deg, and equipment-icon class has rotate: -45deg
        // This gives us 0deg net rotation for the item icon
        _icon.style.scale = new Scale(new Vector2(0.8f, 0.8f));

        // Add to parent
        element.Add(_icon);

        // Add equipment slot classes
        element.AddToClassList("equipment-slot");
        element.AddToClassList($"equipment-slot--{slotType.ToString().ToLower()}");
    }

    public bool CanAcceptItem(Item_DataSO itemData)
    {
        if (itemData == null) return false;

        switch (SlotType) {
            case EquipmentSlotType.Weapon:
                return itemData.itemType == ItemType.Weapon;
            case EquipmentSlotType.Armor:
                return itemData.itemType == ItemType.Armor;
            case EquipmentSlotType.Headgear:
                return itemData.itemType == ItemType.Armor;
            case EquipmentSlotType.Boots:
                return itemData.itemType == ItemType.Armor;
            default:
                return false;
        }
    }

    public void SetItem(Inventory_Item item)
    {
        if (item == null || item.itemData == null) {
            ClearItem();
            return;
        }

        HasItem = true;
        CurrentItem = item;

        // Hide the default background by setting it to null
        VisualElement.style.backgroundImage = StyleKeyword.None;

        // Show the item icon using background image (not Image.image property)
        if (_icon != null && item.itemData.icon != null) {
            _icon.style.backgroundImage = new StyleBackground(item.itemData.icon);
            _icon.style.display = DisplayStyle.Flex;
        }

        // Add class to indicate slot has item
        VisualElement.AddToClassList("has-item");
    }

    public void ClearItem()
    {
        HasItem = false;
        CurrentItem = null;

        // Restore the default background - prefer sprite if available
        if (_originalBackgroundSprite != null) {
            VisualElement.style.backgroundImage = new StyleBackground(_originalBackgroundSprite);
        } else if (_originalBackgroundTexture != null) {
            VisualElement.style.backgroundImage = new StyleBackground(_originalBackgroundTexture);
        }

        // Hide the item icon
        if (_icon != null) {
            _icon.style.backgroundImage = StyleKeyword.None;
            _icon.style.display = DisplayStyle.None;
        }

        // Remove class to indicate slot is empty
        VisualElement.RemoveFromClassList("has-item");
    }

    public Inventory_Item GetEquippedItem()
    {
        return CurrentItem;
    }

    // Method to refresh the visual state
    public void RefreshVisualState()
    {
        if (HasItem && CurrentItem != null && CurrentItem.itemData?.icon != null) {
            // Has item - hide background, show item icon
            VisualElement.style.backgroundImage = StyleKeyword.None;

            if (_icon != null) {
                _icon.style.backgroundImage = new StyleBackground(CurrentItem.itemData.icon);
                _icon.style.display = DisplayStyle.Flex;
            }

            VisualElement.AddToClassList("has-item");
        } else {
            // No item - restore background, hide item icon
            if (_originalBackgroundSprite != null) {
                VisualElement.style.backgroundImage = new StyleBackground(_originalBackgroundSprite);
            } else if (_originalBackgroundTexture != null) {
                VisualElement.style.backgroundImage = new StyleBackground(_originalBackgroundTexture);
            }

            if (_icon != null) {
                _icon.style.backgroundImage = StyleKeyword.None;
                _icon.style.display = DisplayStyle.None;
            }

            VisualElement.RemoveFromClassList("has-item");
        }
    }
}