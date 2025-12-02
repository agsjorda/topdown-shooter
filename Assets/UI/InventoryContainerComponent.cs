using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

[UxmlElement]
public partial class InventoryContainerComponent : VisualElement
{
    private const string UXML_PATH = "Assets/UI/uxml/InventoryContainer.uxml";

    [UxmlAttribute("template")]
    public VisualTreeAsset Template { get; set; }

    private VisualTreeAsset _uxmlTemplate;

    public VisualElement TabButtonsContainer => this.Q<VisualElement>("tabButtonsContainer");
    public VisualElement TabContentContainer => this.Q<VisualElement>("tabContentContainer");

    public InventoryContainerComponent()
    {
        _uxmlTemplate = Template;
#if UNITY_EDITOR
        if (_uxmlTemplate == null)
            _uxmlTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
#endif
        if (_uxmlTemplate == null)
            _uxmlTemplate = Resources.Load<VisualTreeAsset>("UI/uxml/InventoryContainer");

        if (_uxmlTemplate != null) {
            _uxmlTemplate.CloneTree(this);

            AddToClassList("inventory-container-component");

            // Let UXML/USS drive sizing. Clear any defaults that could override UXML.
            style.flexGrow = StyleKeyword.Null;
            style.flexShrink = StyleKeyword.Null;
            style.width = StyleKeyword.Null;
            style.alignItems = StyleKeyword.Null;
            style.flexWrap = StyleKeyword.Null;
        } else {
            Debug.LogError($"InventoryContainerComponent: Could not load UXML at {UXML_PATH}");
            CreateFallbackUI();
        }
    }

    private void CreateFallbackUI()
    {
        Clear();

        var tabAndContentContainer = new VisualElement { name = "tabAndContentContainer" };
        var tabButtonsContainer = new VisualElement { name = "tabButtonsContainer" };
        var tabContentContainer = new VisualElement { name = "tabContentContainer" };

        tabAndContentContainer.Add(tabButtonsContainer);
        tabAndContentContainer.Add(tabContentContainer);
        Add(tabAndContentContainer);
    }

    public void SetWidth(Length width) => style.width = width;
    public void SetHeight(Length height) => style.height = height;
    public void SetAlignment(Align alignItems, Justify justifyContent)
    {
        style.alignItems = alignItems;
        style.justifyContent = justifyContent;
    }

    public VisualElement AddTabButton(string name, string text = "", Texture2D icon = null)
    {
        var button = new Button { name = name, text = text };
        if (icon != null) button.style.backgroundImage = new StyleBackground(icon);
        TabButtonsContainer.Add(button);
        return button;
    }

    public void ClearTabs() => TabButtonsContainer.Clear();
    public void ClearContent() => TabContentContainer.Clear();
}