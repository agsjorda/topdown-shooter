using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
    [UxmlElement]
    public partial class InventoryTabElement : VisualElement
    {
        public const string ActiveClass = "inventoryTab--active";

        public string TabId { get; private set; }

        private readonly VisualElement _background;
        private readonly VisualElement _activeBackground;
        private readonly VisualElement _icon;
        private readonly Label _label;

        public event System.Action<InventoryTabElement> Clicked;

        public VisualElement BackgroundLayer => _background;
        public VisualElement ActiveBackgroundLayer => _activeBackground;

        public InventoryTabElement()
        {
            AddToClassList("inventoryTab");
            name = "inventory-tab";

            _background = new VisualElement { name = "inventoryTab-bg", pickingMode = PickingMode.Ignore };
            _background.AddToClassList("inventoryTab-bg");
            hierarchy.Add(_background);

            _activeBackground = new VisualElement { name = "inventoryTab-bg-active", pickingMode = PickingMode.Ignore };
            _activeBackground.AddToClassList("inventoryTab-bg--active");
            hierarchy.Add(_activeBackground);

            _icon = new VisualElement { pickingMode = PickingMode.Ignore };
            _icon.AddToClassList("inventoryTab-icon");
            hierarchy.Add(_icon);

            _label = new Label { pickingMode = PickingMode.Ignore };
            _label.AddToClassList("inventoryTab-label");
            hierarchy.Add(_label);

            RegisterCallback<ClickEvent>(evt =>
            {
                Clicked?.Invoke(this);
                evt.StopPropagation();
            });
        }

        public void SetId(string id) => TabId = id;

        // Pixel sizes come from InventoryUIConfig and are per tab instance.
        public void SetSize(int widthPx, int heightPx)
        {
            style.width = widthPx > 0 ? (Length)widthPx : new StyleLength(StyleKeyword.Null);
            style.height = heightPx > 0 ? (Length)heightPx : new StyleLength(StyleKeyword.Null);
        }

        public void SetIconSize(int widthPx, int heightPx)
        {
            _icon.style.width = widthPx > 0 ? (Length)widthPx : new StyleLength(StyleKeyword.Null);
            _icon.style.height = heightPx > 0 ? (Length)heightPx : new StyleLength(StyleKeyword.Null);
        }

        public void SetIcon(Texture2D texture, Color? tint = null)
        {
            if (texture != null) {
                _icon.style.backgroundImage = new StyleBackground(texture);
                _icon.style.unityBackgroundImageTintColor = tint ?? Color.white;
            } else {
                _icon.style.backgroundImage = StyleKeyword.Null;
            }
        }

        public void SetLabel(string text)
        {
            bool hasLabel = !string.IsNullOrEmpty(text);
            _label.text = hasLabel ? text : string.Empty;
            EnableInClassList("inventoryTab--has-label", hasLabel);
        }

        public void SetActive(bool active)
        {
            EnableInClassList(ActiveClass, active);
        }
    }
}
