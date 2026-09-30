using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
    [DisallowMultipleComponent]
    public class InventoryUIConfig : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Panel Renderer whose UXML contains the inventory. Auto-filled from the same GameObject.")]
        public PanelRenderer targetPanel;

        [Tooltip("Element name of the panel the theme's panel background is applied to")]
        public string panelElementName = "inventory-panel";

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

        [Tooltip("Equipment data this panel displays. Falls back to InventoryService when empty.")]
        [SerializeField] private EquipmentController equipmentController;

        [Header("Theme")]
        [Tooltip("Optional textures for the panel, slot grid, empty slots, tabs, and equipment slots. Empty sprites fall back to USS.")]
        public InventoryThemeSO theme;

        [Header("Styling")]
        [Tooltip("Functional styles the inventory code toggles (drag states, ghost, qty label). Auto-loaded from the module's Resources when null; the host game's own stylesheets override these")]
        public StyleSheet coreStyles;
        [Tooltip("Add the core stylesheet to the panel root at startup")]
        [SerializeField] private bool addCoreStyles = true;

        private VisualElement _tabContentContainer;
        private InventoryScrollElement _scrollWrapper;
        private VisualElement _slotsContainer;
        private readonly List<SlotView> _createdSlots = new List<SlotView>();
        private readonly List<EquipmentSlotView> _equipmentSlots = new List<EquipmentSlotView>();
        private IEquipmentSystem _equipment;
        private bool _equipmentSubscribed;
        private VisualElement _root;
        private int _uiVersion;
        private PanelRenderer _registeredPanel;

        public InventoryViewModel ViewModel { get; set; }

        /// <summary>Root of the Panel Renderer's tree; null until the panel has loaded.</summary>
        public VisualElement Root => _root;

        /// <summary>True once slots and equipment views exist for the current root.</summary>
        public bool IsBuilt { get; private set; }

        /// <summary>
        /// Fired after the slot grid and equipment views are (re)built for a newly loaded root.
        /// Slot and container instances are replaced, so listeners must re-query them.
        /// </summary>
        public event Action UIBuilt;

        public List<SlotView> Slots => _createdSlots;
        public List<EquipmentSlotView> EquipmentSlots => _equipmentSlots;
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

        public TabFilterManager TabFilterManager { get; set; }

        void Awake()
        {
            ResolvePanel();
            if (equipmentController == null) equipmentController = GetComponent<EquipmentController>();
            if (ViewModel == null) {
                ViewModel = InventoryService.GetPlayerInventoryViewModel();
                if (ViewModel == null) {
                    Debug.LogError("[InventoryUIConfig] No InventoryModel found in scene. InventoryViewModel cannot be created.");
                }
            }
        }

        void OnEnable()
        {
            ResolvePanel();
            RegisterPanel();
        }

        void OnDisable()
        {
            UnregisterPanel();
            UnsubscribeEquipment();
        }

        // Edit-mode preview: registering replays the current root, which rebuilds the grid
        // with the new Inspector values. Rebuilding also runs on every UXML reload.
        void OnValidate()
        {
            if (Application.isPlaying) return;
            ResolvePanel();
            if (_registeredPanel != null && _registeredPanel == targetPanel && _root != null) {
                BuildForRoot(_root);
            } else {
                RegisterPanel();
            }
        }

        private void ResolvePanel()
        {
            if (targetPanel == null) targetPanel = GetComponent<PanelRenderer>();
        }

        private void RegisterPanel()
        {
            if (_registeredPanel == targetPanel && _registeredPanel != null) return;
            UnregisterPanel();
            if (targetPanel == null) return;
            _registeredPanel = targetPanel;
            // Replays immediately when the UI is already loaded
            _registeredPanel.RegisterUIReloadCallback(OnUIReloaded);
        }

        private void UnregisterPanel()
        {
            if (_registeredPanel != null) _registeredPanel.UnregisterUIReloadCallback(OnUIReloaded);
            _registeredPanel = null;
        }

        private void OnUIReloaded(PanelRenderer renderer, VisualElement root, int version)
        {
            if (IsBuilt && root == _root && version == _uiVersion) return;
            _uiVersion = version;
            BuildForRoot(root);
        }

        private void BuildForRoot(VisualElement root)
        {
            if (root != _root) {
                // Containers belong to the previous tree
                _tabContentContainer = null;
                _scrollWrapper = null;
                _slotsContainer = null;
                _root = root;
            }
            CacheContainers();
            BuildOrUpdateSlotsOnly();
            InitializeEquipmentSlots();
            SubscribeEquipment();
            SyncEquipmentFromModel();
            ApplyTheme();
            IsBuilt = _root != null;
            if (IsBuilt && Application.isPlaying) UIBuilt?.Invoke();
        }

        // Recreates the slot views for layout changes (slot size/count/margin).
        // Call explicitly after changing layout fields at runtime; routine content
        // updates go through TabFilterManager instead and never recreate slots.
        public void RebuildUI()
        {
            CacheContainers();
            BuildOrUpdateSlotsOnly();
            InitializeEquipmentSlots();
            if (TabFilterManager != null) {
                TabFilterManager.RefreshDisplayForCurrentTab();
            }
            SyncEquipmentFromModel();
            ApplyTheme();
        }

        /// <summary>Swap the theme at runtime and repaint every themed element.</summary>
        public void SetTheme(InventoryThemeSO newTheme)
        {
            theme = newTheme;
            ApplyTheme();
        }

        /// <summary>Paints the current theme onto the panel, grid, slots, equipment layers, and tabs.</summary>
        public void ApplyTheme()
        {
            var root = _root;
            if (root == null) return;

            string panelName = string.IsNullOrEmpty(panelElementName) ? "inventory-panel" : panelElementName;
            VisualElement panel = root.Q<VisualElement>(panelName);

            InventoryThemeApplier.Apply(
                theme,
                root,
                panel,
                _slotsContainer,
                _createdSlots,
                _equipmentSlots,
                CollectTabs(root));
        }

        private void InitializeEquipmentSlots()
        {
            _equipmentSlots.Clear();
            var root = _root;
            if (root == null || equipmentSlotBindings == null) return;

            foreach (var binding in equipmentSlotBindings) {
                if (binding == null || string.IsNullOrEmpty(binding.elementName) || binding.slotType == null) continue;
                var slot = root.Q<EquipmentSlotView>(binding.elementName);
                if (slot == null) continue;
                slot.Initialize(binding.slotType);
                _equipmentSlots.Add(slot);
            }
        }

        private void SubscribeEquipment()
        {
            if (!Application.isPlaying || _equipmentSubscribed) return;
            _equipment = equipmentController != null
                ? equipmentController
                : InventoryService.GetPlayerEquipment();
            if (_equipment == null) return;
            _equipment.OnEquipmentChanged += HandleEquipmentChanged;
            _equipmentSubscribed = true;
            SyncEquipmentFromModel();
        }

        private void UnsubscribeEquipment()
        {
            if (!_equipmentSubscribed || _equipment == null) {
                _equipmentSubscribed = false;
                return;
            }
            _equipment.OnEquipmentChanged -= HandleEquipmentChanged;
            _equipmentSubscribed = false;
        }

        private void HandleEquipmentChanged(EquipmentSlotTypeSO slotType, InventoryItem item)
        {
            var slot = FindEquipmentSlot(slotType);
            if (slot == null) return;
            if (item == null) slot.ClearItem();
            else slot.SetItem(item);
        }

        private void SyncEquipmentFromModel()
        {
            if (_equipment == null) return;
            for (int i = 0; i < _equipmentSlots.Count; i++) {
                var slot = _equipmentSlots[i];
                if (slot == null || slot.SlotType == null) continue;
                var item = _equipment.GetEquippedItem(slot.SlotType);
                if (item == null) slot.ClearItem();
                else slot.SetItem(item);
            }
        }

        private EquipmentSlotView FindEquipmentSlot(EquipmentSlotTypeSO slotType)
        {
            for (int i = 0; i < _equipmentSlots.Count; i++) {
                var slot = _equipmentSlots[i];
                if (slot != null && slot.SlotType == slotType) return slot;
            }
            return null;
        }

        private static List<InventoryTabElement> CollectTabs(VisualElement root)
        {
            var tabs = new List<InventoryTabElement>();
            var tabButtons = root.Q<VisualElement>("tabButtonsContainer");
            if (tabButtons == null) return tabs;
            foreach (var child in tabButtons.Children()) {
                if (child is InventoryTabElement tab) tabs.Add(tab);
            }
            return tabs;
        }

        private void CacheContainers()
        {
            var root = _root;
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
    }
}
