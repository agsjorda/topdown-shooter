using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
    [UxmlElement]
    public partial class InventoryContainerComponent : VisualElement
    {
        // Lives in the module's Resources folder so it loads in any project the
        // module is dropped into — no hard-coded project paths.
        private const string RESOURCES_PATH = "InventorySystem/InventoryContainer";

        [UxmlAttribute("template")]
        public VisualTreeAsset Template { get; set; }

        private VisualTreeAsset _uxmlTemplate;

        public InventoryContainerComponent()
        {
            _uxmlTemplate = Template;
            if (_uxmlTemplate == null)
                _uxmlTemplate = Resources.Load<VisualTreeAsset>(RESOURCES_PATH);

            if (_uxmlTemplate != null) {
                _uxmlTemplate.CloneTree(this);
            } else {
                Debug.LogError($"InventoryContainerComponent: Could not load UXML from Resources/{RESOURCES_PATH}");
            }
        }
    }
}
