using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
    [UxmlElement]
    public partial class InventoryTabElement : VisualElement
    {
        public const string ActiveClass = "inventoryTab--active";

        // Global size shared by all tabs (0 => use USS/default)
        private static int s_GlobalWidthPx = 0;
        private static int s_GlobalHeightPx = 0;

        // Global icon size shared by all tabs (0 => use USS/default)
        private static int s_GlobalIconWidthPx = 0;
        private static int s_GlobalIconHeightPx = 0;

        private static readonly HashSet<InventoryTabElement> s_Instances = new HashSet<InventoryTabElement>();

        public string TabId { get; private set; }

        private VisualElement _icon;
        private Label _label;

        public event System.Action<InventoryTabElement> Clicked;

        public InventoryTabElement()
        {
            AddToClassList("inventoryTab");
            name = "inventory-tab";

            _icon = new VisualElement();
            _icon.AddToClassList("inventoryTab-icon");
            _icon.pickingMode = PickingMode.Ignore;
            hierarchy.Add(_icon);

            _label = new Label();
            _label.style.display = DisplayStyle.None;
            _label.pickingMode = PickingMode.Ignore;
            hierarchy.Add(_label);

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                s_Instances.Add(this);
                ApplyGlobalSizeTo(this);
                ApplyGlobalIconSizeTo(this);
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                s_Instances.Remove(this);
            });

            RegisterCallback<ClickEvent>(evt =>
            {
                Clicked?.Invoke(this);
                evt.StopPropagation();
            });
        }

        public void SetId(string id) => TabId = id;

        // Size is controlled globally; this forwards to the global setter.
        public void SetSize(int widthPx, int heightPx) => SetGlobalSize(widthPx, heightPx);

        public static void SetGlobalSize(int widthPx, int heightPx)
        {
            s_GlobalWidthPx = Mathf.Max(0, widthPx);
            s_GlobalHeightPx = Mathf.Max(0, heightPx);

            foreach (var tab in s_Instances)
                ApplyGlobalSizeTo(tab);
        }

        private static void ApplyGlobalSizeTo(InventoryTabElement tab)
        {
            if (tab == null) return;

            tab.style.width = s_GlobalWidthPx > 0 ? (Length)s_GlobalWidthPx : new StyleLength(StyleKeyword.Null);
            tab.style.height = s_GlobalHeightPx > 0 ? (Length)s_GlobalHeightPx : new StyleLength(StyleKeyword.Null);
        }

        // New: Global icon size for all tabs
        public static void SetGlobalIconSize(int widthPx, int heightPx)
        {
            s_GlobalIconWidthPx = Mathf.Max(0, widthPx);
            s_GlobalIconHeightPx = Mathf.Max(0, heightPx);

            foreach (var tab in s_Instances)
                ApplyGlobalIconSizeTo(tab);
        }

        private static void ApplyGlobalIconSizeTo(InventoryTabElement tab)
        {
            if (tab == null || tab._icon == null) return;

            tab._icon.style.width = s_GlobalIconWidthPx > 0 ? (Length)s_GlobalIconWidthPx : new StyleLength(StyleKeyword.Null);
            tab._icon.style.height = s_GlobalIconHeightPx > 0 ? (Length)s_GlobalIconHeightPx : new StyleLength(StyleKeyword.Null);
        }

        // Texture2D-only icon API
        public void SetIcon(Texture2D texture, Color? tint = null)
        {
            if (texture != null) {
                _icon.style.backgroundImage = new StyleBackground(texture);
                _icon.style.unityBackgroundImageTintColor = tint ?? Color.white;
            } else {
                _icon.style.backgroundImage = null;
            }
        }

        public void SetLabel(string text)
        {
            if (string.IsNullOrEmpty(text)) {
                _label.text = "";
                _label.style.display = DisplayStyle.None;
            } else {
                _label.text = text;
                _label.style.display = DisplayStyle.Flex;
            }
        }

        public void SetActive(bool active)
        {
            if (active) AddToClassList(ActiveClass);
            else RemoveFromClassList(ActiveClass);
        }
    }
}
