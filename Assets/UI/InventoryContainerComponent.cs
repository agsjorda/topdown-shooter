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
        } else {
            Debug.LogError($"InventoryContainerComponent: Could not load UXML at {UXML_PATH}");
        }
    }
}