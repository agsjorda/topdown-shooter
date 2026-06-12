using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
    // Fix for CS0104: Explicitly reference UnityEngine.Object to avoid ambiguity
    public class TabFilterManager : MonoBehaviour
    {
        [Header("References")]
        public InventoryViewModel inventoryViewModel;
        [SerializeField] private InventoryUIConfig uiConfig;

        [Header("Tab Filtering")]
        public bool enableTabFiltering = true;
        public string defaultTabId = "all";

        private Dictionary<string, List<InventoryItem>> _filteredItems = new Dictionary<string, List<InventoryItem>>();
        private string _currentTabId = "all";

        void Awake()
        {
            // Use the shared ViewModel so all systems observe the same instance
            if (inventoryViewModel == null) {
                inventoryViewModel = InventoryService.GetPlayerInventoryViewModel();
                if (inventoryViewModel == null) {
                    Debug.LogError("[TabFilterManager] No InventoryModel found in scene. InventoryViewModel cannot be created.");
                }
            }

            // Register on the config so DragDropController can request content refreshes
            if (uiConfig == null) {
                uiConfig = GetComponent<InventoryUIConfig>();
            }
            if (uiConfig != null) {
                uiConfig.TabFilterManager = this;
            }
        }

        void Start()
        {
            if (inventoryViewModel != null) {
                inventoryViewModel.OnInventoryChanged += OnInventoryChanged;
            }
            InitializeTabFiltering();
            RefreshFilteredItems();
            RefreshDisplayForCurrentTab();
        }

        void OnDestroy()
        {
            if (inventoryViewModel != null) {
                inventoryViewModel.OnInventoryChanged -= OnInventoryChanged;
            }
        }

        private void InitializeTabFiltering()
        {
            if (!enableTabFiltering || uiConfig == null) return;
            var root = uiConfig.targetDocument?.rootVisualElement;
            if (root == null) return;
            var tabButtonsContainer = root.Q<VisualElement>("tabButtonsContainer");
            if (tabButtonsContainer == null) return;
            RebuildTabsWithFiltering(tabButtonsContainer);
        }

        private void RebuildTabsWithFiltering(VisualElement tabButtonsContainer)
        {
            InventoryTabElement.SetGlobalSize(uiConfig.tabWidthPx, uiConfig.tabHeightPx);
            InventoryTabElement.SetGlobalIconSize(uiConfig.tabIconWidthPx, uiConfig.tabIconHeightPx);
            tabButtonsContainer.Clear();
            for (int i = 0; i < uiConfig.tabs.Count; i++) {
                var category = uiConfig.tabs[i];
                if (category == null) continue;
                var tab = new InventoryTabElement();
                tab.SetId(category.id);
                tab.SetIcon(category.tabIcon, category.tabIconTint);
                tab.SetLabel(category.displayName);
                bool isActive = (category.id == _currentTabId) ||
                               (string.IsNullOrEmpty(_currentTabId) && i == 0);
                tab.SetActive(isActive);
                int capturedIndex = i;
                tab.Clicked += (clickedTab) => OnTabClicked(capturedIndex);
                tabButtonsContainer.Add(tab);
            }
        }

        private void OnTabClicked(int tabIndex)
        {
            if (!enableTabFiltering || uiConfig == null ||
                tabIndex < 0 || tabIndex >= uiConfig.tabs.Count ||
                uiConfig.tabs[tabIndex] == null) return;
            _currentTabId = uiConfig.tabs[tabIndex].id;
            UpdateActiveTabVisual();
            RefreshDisplayForCurrentTab();
        }

        private void UpdateActiveTabVisual()
        {
            var root = uiConfig.targetDocument?.rootVisualElement;
            if (root == null) return;
            var tabButtonsContainer = root.Q<VisualElement>("tabButtonsContainer");
            if (tabButtonsContainer == null) return;
            int i = 0;
            foreach (var child in tabButtonsContainer.Children()) {
                if (child is InventoryTabElement tab) {
                    bool isActive = (i < uiConfig.tabs.Count && uiConfig.tabs[i] != null && uiConfig.tabs[i].id == _currentTabId);
                    tab.SetActive(isActive);
                }
                i++;
            }
        }

        private void OnInventoryChanged()
        {
            if (!enableTabFiltering) return;
            RefreshFilteredItems();
            RefreshDisplayForCurrentTab();
        }

        public void RefreshFilteredItems()
        {
            if (inventoryViewModel == null) return;
            _filteredItems.Clear();
            if (uiConfig != null) {
                foreach (var tab in uiConfig.tabs) {
                    if (tab == null || string.IsNullOrEmpty(tab.id)) continue;
                    _filteredItems[tab.id] = new List<InventoryItem>();
                }
            }
            if (!_filteredItems.ContainsKey("all")) {
                _filteredItems["all"] = new List<InventoryItem>();
            }
            for (int i = 0; i < inventoryViewModel.MaxInventorySize; i++) {
                InventoryItem item = (i < inventoryViewModel.Items.Count) ? inventoryViewModel.Items[i] : null;
                if (item == null) continue;
                _filteredItems["all"].Add(item);
                string tabId = GetTabIdForItem(item);
                if (!string.IsNullOrEmpty(tabId) && _filteredItems.ContainsKey(tabId)) {
                    _filteredItems[tabId].Add(item);
                }
            }
        }

        // The item's authored category IS its tab — no code mapping to maintain
        private string GetTabIdForItem(InventoryItem item)
        {
            var categoryId = item?.itemData?.category?.id;
            return string.IsNullOrEmpty(categoryId) ? null : categoryId;
        }

        public void RefreshDisplayForCurrentTab()
        {
            if (!enableTabFiltering || inventoryViewModel == null || uiConfig?.Slots == null) return;
            var slots = uiConfig.Slots;
            var items = inventoryViewModel.Items;
            string currentTab = _currentTabId;

            for (int i = 0; i < slots.Count; i++)
            {
                if (i < items.Count && items[i] != null)
                {
                    string tabId = GetTabIdForItem(items[i]);
                    if (currentTab == "all" || (tabId != null && tabId == currentTab))
                        slots[i].SetItem(items[i]);
                    else
                        slots[i].ClearItem();
                }
                else
                {
                    slots[i].ClearItem();
                }
            }
        }

        public List<InventoryItem> GetFilteredItemsForCurrentTab()
        {
            if (_filteredItems.TryGetValue(_currentTabId, out var items))
                return items;
            return new List<InventoryItem>();
        }

        public void RefreshAndRebuildUI()
        {
            RefreshFilteredItems();
            RefreshDisplayForCurrentTab();
        }
    }
}
