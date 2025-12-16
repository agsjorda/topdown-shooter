using InventorySystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TabFilterManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public InventoryViewModel inventoryViewModel;
    [SerializeField] private InventoryUIConfig uiConfig;

    [Header("Tab Filtering")]
    public bool enableTabFiltering = true;
    public string defaultTabId = "all";

    private Dictionary<string, List<InventoryItem>> _filteredItems = new Dictionary<string, List<InventoryItem>>();
    private string _currentTabId = "all";

    void Awake()
    {
        // Ensure InventoryViewModel is assigned
        if (inventoryViewModel == null) {
            var inventoryBase = Object.FindFirstObjectByType<InventoryModel>();
            if (inventoryBase != null) {
                inventoryViewModel = new InventoryViewModel(inventoryBase);
                Debug.Log("[TabFilterManager] InventoryViewModel auto-created from Inventory_Base");
            } else {
                Debug.LogError("[TabFilterManager] No Inventory_Base found in scene. InventoryViewModel cannot be created.");
            }
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
            var tabDef = uiConfig.tabs[i];
            var tab = new InventoryTabElement();
            tab.SetId(tabDef.id);
            tab.SetIcon(tabDef.iconTexture, tabDef.iconTint);
            tab.SetLabel(tabDef.label);
            bool isActive = (tabDef.id == _currentTabId) ||
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
            tabIndex < 0 || tabIndex >= uiConfig.tabs.Count) return;
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
                bool isActive = (i < uiConfig.tabs.Count && uiConfig.tabs[i].id == _currentTabId);
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

    private void RefreshFilteredItems()
    {
        if (inventoryViewModel == null) return;
        _filteredItems.Clear();
        if (uiConfig != null) {
            foreach (var tab in uiConfig.tabs) {
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

    private string GetTabIdForItem(InventoryItem item)
    {
        if (item?.itemData != null) {
            switch (item.itemData.itemType) {
                case ItemType.Weapon: return "weapon";
                case ItemType.Armor: return "armor";
                case ItemType.Consumable: return "consumable";
                case ItemType.Material: return "materials";
                case ItemType.Quest: return "quest";
                case ItemType.Miscellaneous: return "misc";
                default: return null;
            }
        }
        return null;
    }

    private void RefreshDisplayForCurrentTab()
    {
        if (!enableTabFiltering || inventoryViewModel == null || uiConfig?.Slots == null) return;
        var slots = uiConfig.Slots;
        int maxSlotsToFill = Mathf.Min(slots.Count, inventoryViewModel.MaxInventorySize);
        for (int i = 0; i < slots.Count; i++)
            slots[i].ClearItem();
        for (int i = 0; i < maxSlotsToFill; i++) {
            InventoryItem item = (i < inventoryViewModel.Items.Count) ? inventoryViewModel.Items[i] : null;
            if (item != null) {
                if (_currentTabId == "all") {
                    slots[i].SetItem(item);
                } else {
                    string tabId = GetTabIdForItem(item);
                    if (!string.IsNullOrEmpty(tabId) && tabId == _currentTabId) {
                        slots[i].SetItem(item);
                    }
                }
            }
        }
    }
}