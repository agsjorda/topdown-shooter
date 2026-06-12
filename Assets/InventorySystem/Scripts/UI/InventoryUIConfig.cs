using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
    [DisallowMultipleComponent]
    public class InventoryUIConfig : MonoBehaviour
    {
        [Header("Target")]
        public UIDocument targetDocument;

        [Header("Slot Layout")]
        [Range(40, 300)] public int slotSize = 100;
        [Range(1, 200)] public int slotCount = 30;
        [Range(0f, 50f)] public float cellMargin = 8f;

        [Header("Scroll Wrapper")]
        public bool autoCreateScrollWrapper = true;
        public int scrollWidthPx = 0;
        public int scrollHeightPx = 350;
        public ScrollerVisibility verticalVisibility = ScrollerVisibility.Auto;

        [System.Serializable]
        public class EquipmentSlotBinding
        {
            [Tooltip("UXML element name of the EquipmentSlotView, e.g. \"equipSlotWeapon\"")]
            public string elementName;
            public EquipmentSlotTypeSO slotType;
        }

        [Header("Tabs")]
        [Tooltip("Categories shown as tabs, in order. Author them via Create > Inventory System > Item Category; include one with id \"all\" for the show-everything tab")]
        public List<ItemCategorySO> tabs = new List<ItemCategorySO>();
        public int activeTabIndex = 0;
        [Range(20, 400)] public int tabWidthPx = 0;
        [Range(20, 400)] public int tabHeightPx = 0;
        [Range(16, 512)] public int tabIconWidthPx = 0;
        [Range(16, 512)] public int tabIconHeightPx = 0;

        [Header("Equipment Slots")]
        [Tooltip("Maps EquipmentSlotView elements in the UXML (by element name) to slot type assets. Defines which equipment slots exist for this game")]
        public List<EquipmentSlotBinding> equipmentSlotBindings = new List<EquipmentSlotBinding>();

        [Header("Styling")]
        [Tooltip("Functional styles the inventory code toggles (drag states, ghost, qty label). Auto-loaded from the module's Resources when null; the host game's own stylesheets override these")]
        public StyleSheet coreStyles;
        [Tooltip("Add the core stylesheet to the UIDocument root at startup")]
        [SerializeField] private bool addCoreStyles = true;

        private VisualElement _tabContentContainer;
        private InventoryScrollElement _scrollWrapper;
        private VisualElement _slotsContainer;
        private readonly List<SlotView> _createdSlots = new List<SlotView>();

        // MVVM: ViewModel property (set externally or via inspector)
        public InventoryViewModel ViewModel { get; set; }

        public List<SlotView> Slots => _createdSlots;
        public int CreatedSlotCount => _createdSlots.Count;

        // The element holding all SlotViews. Stable across rebuilds (only its children are
        // recreated), so pointer-event delegation can be registered here exactly once.
        public VisualElement SlotsContainer => _slotsContainer;

        public SlotView GetSlot(int index)
        {
            return (index >= 0 && index < _createdSlots.Count) ? _createdSlots[index] : null;
        }

        public bool TryGetSlot(int index, out SlotView slot)
        {
            if (index >= 0 && index < _createdSlots.Count) {
                slot = _createdSlots[index];
                return true;
            }
            slot = null;
            return false;
        }

        // Added reference to TabFilterManager
        public TabFilterManager TabFilterManager { get; set; }

        void Awake()
        {
            targetDocument ??= GetComponent<UIDocument>();
            // Use the shared ViewModel so all systems observe the same instance
            if (ViewModel == null) {
                ViewModel = InventoryService.GetPlayerInventoryViewModel();
                if (ViewModel == null) {
                    Debug.LogError("[InventoryUIConfig] No InventoryModel found in scene. InventoryViewModel cannot be created.");
                }
            }
        }

        void OnEnable()
        {
            targetDocument ??= GetComponent<UIDocument>();
            CacheContainers();
            BuildOrUpdateSlotsOnly();
        }

        void OnValidate()
        {
            if (!Application.isPlaying) {
                CacheContainers();
                BuildOrUpdateSlotsOnly();
            }
        }

        // Recreates the slot views for layout changes (slot size/count/margin).
        // Call explicitly after changing layout fields at runtime; routine content
        // updates go through TabFilterManager instead and never recreate slots.
        public void RebuildUI()
        {
            CacheContainers();
            BuildOrUpdateSlotsOnly();
            // Only let TabFilterManager handle slot filling to ensure correct filtering
            if (TabFilterManager != null)
            {
                TabFilterManager.RefreshDisplayForCurrentTab();
            }
        }

        #region Private Methods
        private void CacheContainers()
        {
            var root = targetDocument?.rootVisualElement;
            if (root == null) return;

            ApplyCoreStyles(root);

            _tabContentContainer ??= root.Q<VisualElement>("tabContentContainer");
            if (_tabContentContainer == null) return;

            _scrollWrapper ??= _tabContentContainer.Q<InventoryScrollElement>();
            if (autoCreateScrollWrapper && _scrollWrapper == null) {
                _scrollWrapper = new InventoryScrollElement();
                _tabContentContainer.Add(_scrollWrapper);
            }

            _slotsContainer = _scrollWrapper?.SlotsContainer
                ?? _tabContentContainer.Q<VisualElement>(className: "inventory-slots-container");

            if (_scrollWrapper != null) {
                _scrollWrapper.SetSize(scrollWidthPx, scrollHeightPx);
                _scrollWrapper.SetScrollerVisibility(verticalVisibility);
            }
        }

        private void ApplyCoreStyles(VisualElement root)
        {
            if (!addCoreStyles) return;
            if (coreStyles == null) {
                coreStyles = Resources.Load<StyleSheet>("InventorySystem/InventoryCore");
            }
            if (coreStyles != null && !root.styleSheets.Contains(coreStyles)) {
                root.styleSheets.Add(coreStyles);
            }
        }

        // Only build slots, not tabs. TabFilterManager will handle tab UI and filtering.
        private void BuildOrUpdateSlotsOnly()
        {
            if (_tabContentContainer == null || _slotsContainer == null)
                CacheContainers();

            if (_slotsContainer == null) return;

            _slotsContainer.Clear();
            _createdSlots.Clear();

            _createdSlots.Capacity = slotCount;

            for (int i = 0; i < slotCount; i++) {
                var slot = new SlotView();
                slot.SetSlotIndex(i);
                slot.SetSlotSize(slotSize);
                slot.SetCellMargin(cellMargin);

                _slotsContainer.Add(slot);
                _createdSlots.Add(slot);
            }

            _tabContentContainer.MarkDirtyRepaint();
        }
        #endregion
    }
}
