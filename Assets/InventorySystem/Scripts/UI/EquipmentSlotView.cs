using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
    [UxmlElement]
    public partial class EquipmentSlotView : SlotView
    {
        private readonly VisualElement _background;
        private readonly VisualElement _equipmentIcon;

        public EquipmentSlotTypeSO SlotType { get; private set; }

        public override bool IsEquipmentSlot => true;
        public override EquipmentSlotTypeSO EquipmentType => SlotType;

        // Theme textures are applied to this layer so they don't fight the host's
        // empty-state background on the slot element itself.
        public VisualElement BackgroundLayer => _background;

        public EquipmentSlotView() : base()
        {
            RemoveFromClassList("inventorySlots");

            _background = new VisualElement { name = "equipment-slot-bg", pickingMode = PickingMode.Ignore };
            _background.AddToClassList("equipment-slot-bg");
            Add(_background);

            _equipmentIcon = new VisualElement { name = "equipment-icon", pickingMode = PickingMode.Ignore };
            _equipmentIcon.AddToClassList("equipment-icon");
            Add(_equipmentIcon);
        }

        public void Initialize(EquipmentSlotTypeSO slotType)
        {
            SlotType = slotType;
            AddToClassList("equipment-slot");
            if (slotType == null) return;

            string suffix = string.IsNullOrEmpty(slotType.ussClassSuffix) ? slotType.id : slotType.ussClassSuffix;
            if (!string.IsNullOrEmpty(suffix)) {
                AddToClassList($"equipment-slot--{suffix.ToLower()}");
            }
        }

        public override void SetItem(InventoryItem item)
        {
            if (item == null || item.itemData == null) {
                ClearItem();
                return;
            }

            _hasItem = true;
            _itemId = item.itemData.itemId ?? string.Empty;
            _equipmentIcon.style.backgroundImage = item.itemData.icon != null
                ? new StyleBackground(item.itemData.icon)
                : StyleKeyword.None;
            EnableInClassList("has-item", true);
        }

        public override void ClearItem()
        {
            _hasItem = false;
            _itemId = string.Empty;
            _equipmentIcon.style.backgroundImage = StyleKeyword.None;
            EnableInClassList("has-item", false);
        }
    }
}
