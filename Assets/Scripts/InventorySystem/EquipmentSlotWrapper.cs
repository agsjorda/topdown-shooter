using UnityEngine;
using UnityEngine.UIElements;

public class EquipmentSlotWrapper
{
    public VisualElement VisualElement { get; private set; }
    public EquipmentSlotType SlotType { get; private set; }
    public bool HasItem { get; private set; }
    public Inventory_Item CurrentItem { get; private set; }

    private Image _icon;
    private VisualElement _defaultIconContainer;
    private StyleBackground _defaultBackground;

    public EquipmentSlotWrapper(VisualElement element, EquipmentSlotType slotType)
    {
        VisualElement = element;
        SlotType = slotType;
        CurrentItem = null;
        HasItem = false;

        // Store the original background image
        _defaultBackground = element.style.backgroundImage;

        // Clear the background image from the main element
        element.style.backgroundImage = StyleKeyword.None;

        // Create a container for the default icon
        _defaultIconContainer = new VisualElement();
        _defaultIconContainer.name = "default-icon-container";
        _defaultIconContainer.AddToClassList("equipSlot-icon");
        _defaultIconContainer.style.width = Length.Percent(100);
        _defaultIconContainer.style.height = Length.Percent(100);

        // Set the background image from the original element
        if (_defaultBackground != null && _defaultBackground != StyleKeyword.Null) {
            _defaultIconContainer.style.backgroundImage = _defaultBackground;
        }

        // Make it visible initially
        _defaultIconContainer.style.display = DisplayStyle.Flex;
        _defaultIconContainer.style.opacity = 1f;

        // Add the default icon container to the element
        element.Add(_defaultIconContainer);

        // Create or find the item icon
        _icon = element.Q<Image>("equipment-icon");
        if (_icon == null) {
            _icon = new Image {
                name = "equipment-icon",
                scaleMode = ScaleMode.ScaleToFit
            };
            _icon.AddToClassList("equipment-icon");
            _icon.style.width = Length.Percent(100);
            _icon.style.height = Length.Percent(100);
            _icon.style.display = DisplayStyle.None;
            _icon.style.opacity = 0f;
            element.Add(_icon);
        }

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

        // Hide default icon container
        if (_defaultIconContainer != null) {
            _defaultIconContainer.style.display = DisplayStyle.None;
            _defaultIconContainer.style.opacity = 0f;
        }

        // Show the item icon
        if (_icon != null && item.itemData.icon != null) {
            _icon.image = item.itemData.icon.texture;
            _icon.sprite = item.itemData.icon;
            _icon.style.display = DisplayStyle.Flex;
            _icon.style.opacity = 1f;
        }

        // Add class to indicate slot has item
        VisualElement.AddToClassList("has-item");
    }

    public void ClearItem()
    {
        HasItem = false;
        CurrentItem = null;

        // Hide the item icon
        if (_icon != null) {
            _icon.image = null;
            _icon.sprite = null;
            _icon.style.display = DisplayStyle.None;
            _icon.style.opacity = 0f;
        }

        // Show default icon container
        if (_defaultIconContainer != null) {
            _defaultIconContainer.style.display = DisplayStyle.Flex;
            _defaultIconContainer.style.opacity = 1f;
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
        if (HasItem && CurrentItem != null && _icon != null && CurrentItem.itemData?.icon != null) {
            // Ensure item icon is showing
            _icon.image = CurrentItem.itemData.icon.texture;
            _icon.sprite = CurrentItem.itemData.icon;
            _icon.style.display = DisplayStyle.Flex;
            _icon.style.opacity = 1f;

            // Ensure default icon is hidden
            if (_defaultIconContainer != null) {
                _defaultIconContainer.style.display = DisplayStyle.None;
                _defaultIconContainer.style.opacity = 0f;
            }

            VisualElement.AddToClassList("has-item");
        } else {
            // Ensure default icon is showing
            if (_defaultIconContainer != null) {
                _defaultIconContainer.style.display = DisplayStyle.Flex;
                _defaultIconContainer.style.opacity = 1f;
            }

            // Ensure item icon is hidden
            if (_icon != null) {
                _icon.style.display = DisplayStyle.None;
                _icon.style.opacity = 0f;
            }

            VisualElement.RemoveFromClassList("has-item");
        }
    }
}