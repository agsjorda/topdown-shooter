using UnityEngine;
using UnityEngine.UIElements;

public class EquipmentSlotWrapper
{
    public VisualElement VisualElement { get; private set; }
    public EquipmentSlotType SlotType { get; private set; }
    public bool HasItem { get; private set; }
    public Inventory_Item CurrentItem { get; private set; }

    private Image _icon;
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
        
        Debug.Log($"[{slotType}] Init - Inline style keyword: {bgImage.keyword}, value: {bgImage.value}");
        
        if (bgImage.keyword == StyleKeyword.Undefined && bgImage.value != null) {
            // Has inline style
            if (bgImage.value.texture != null) {
                _originalBackgroundTexture = bgImage.value.texture as Texture2D;
                Debug.Log($"[{slotType}] Got texture from inline style: {_originalBackgroundTexture?.name}");
            } else if (bgImage.value.sprite != null) {
                _originalBackgroundSprite = bgImage.value.sprite;
                _originalBackgroundTexture = _originalBackgroundSprite.texture;
                Debug.Log($"[{slotType}] Got sprite from inline style: {_originalBackgroundSprite?.name}");
            }
        }
        
        // ALWAYS try resolvedStyle as it contains computed values from USS
        var resolvedBg = element.resolvedStyle.backgroundImage;
        Debug.Log($"[{slotType}] Resolved style - texture: {resolvedBg.texture != null}, sprite: {resolvedBg.sprite != null}");
        
        if (_originalBackgroundTexture == null && _originalBackgroundSprite == null) {
            // Fallback to resolvedStyle
            if (resolvedBg.texture != null) {
                _originalBackgroundTexture = resolvedBg.texture as Texture2D;
                Debug.Log($"[{slotType}] Got texture from resolved style: {_originalBackgroundTexture?.name}");
            } else if (resolvedBg.sprite != null) {
                _originalBackgroundSprite = resolvedBg.sprite;
                _originalBackgroundTexture = _originalBackgroundSprite.texture;
                Debug.Log($"[{slotType}] Got sprite from resolved style: {_originalBackgroundSprite?.name}");
            }
        }

        Debug.Log($"[{slotType}] Final - Has texture: {_originalBackgroundTexture != null}, Has sprite: {_originalBackgroundSprite != null}");

        // Create the item icon that will replace the background when equipped
        _icon = new Image {
            name = "equipment-icon",
            scaleMode = ScaleMode.ScaleToFit
        };
        _icon.AddToClassList("equipment-icon");
        _icon.style.width = Length.Percent(100);
        _icon.style.height = Length.Percent(100);
        _icon.style.position = Position.Absolute;
        _icon.style.top = 0;
        _icon.style.left = 0;
        _icon.style.display = DisplayStyle.None;

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

        // Show the item icon
        if (_icon != null && item.itemData.icon != null) {
            _icon.image = item.itemData.icon.texture;
            _icon.sprite = item.itemData.icon;
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

        // DON'T restore tint color - let USS handle it via .equipSlot-icon class

        // Hide the item icon
        if (_icon != null) {
            _icon.image = null;
            _icon.sprite = null;
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
                _icon.image = CurrentItem.itemData.icon.texture;
                _icon.sprite = CurrentItem.itemData.icon;
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

            // DON'T restore tint color - let USS handle it via .equipSlot-icon class

            if (_icon != null) {
                _icon.image = null;
                _icon.sprite = null;
                _icon.style.display = DisplayStyle.None;
            }

            VisualElement.RemoveFromClassList("has-item");
        }
    }
}