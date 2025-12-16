using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TabFilterManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory_Base inventory;
    [SerializeField] private InventoryUIConfig uiConfig;

    [Header("Tab Filtering")]
    public bool enableTabFiltering = true;
    public string defaultTabId = "all";

    private Dictionary<string, List<Inventory_Item>> _filteredItems = new Dictionary<string, List<Inventory_Item>>();
    private string _currentTabId = "all";

    void Start()
    {
        if (inventory != null) {
            inventory.OnInventoryChanged += OnInventoryChanged;
        }

        InitializeTabFiltering();

        // Initial population
        RefreshFilteredItems();
        RefreshDisplayForCurrentTab();
    }

    void OnDestroy()
    {
        if (inventory != null) {
            inventory.OnInventoryChanged -= OnInventoryChanged;
        }
    }

    private void InitializeTabFiltering()
    {
        if (!enableTabFiltering || uiConfig == null) return;

        // Hook into tab clicks
        var root = uiConfig.targetDocument?.rootVisualElement;
        if (root == null) return;

        var tabButtonsContainer = root.Q<VisualElement>("tabButtonsContainer");
        if (tabButtonsContainer == null) return;

        // Clear and rebuild tabs with filtering
        RebuildTabsWithFiltering(tabButtonsContainer);
    }

    private void RebuildTabsWithFiltering(VisualElement tabButtonsContainer)
    {
        // Set global tab and icon size from config so inspector changes are applied
        InventoryTabElement.SetGlobalSize(uiConfig.tabWidthPx, uiConfig.tabHeightPx);
        InventoryTabElement.SetGlobalIconSize(uiConfig.tabIconWidthPx, uiConfig.tabIconHeightPx);

        tabButtonsContainer.Clear();

        for (int i = 0; i < uiConfig.tabs.Count; i++) {
            var tabDef = uiConfig.tabs[i];
            var tab = new InventoryTabElement();
            tab.SetId(tabDef.id);
            tab.SetIcon(tabDef.iconTexture, tabDef.iconTint);
            tab.SetLabel(tabDef.label);

            // Set active state
            bool isActive = (tabDef.id == _currentTabId) ||
                           (string.IsNullOrEmpty(_currentTabId) && i == 0);
            tab.SetActive(isActive);

            // Add click handler
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

        // Update UI
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
        if (inventory == null) return;

        _filteredItems.Clear();

        // Initialize with all tabs from config
        if (uiConfig != null) {
            foreach (var tab in uiConfig.tabs) {
                _filteredItems[tab.id] = new List<Inventory_Item>();
            }
        }

        // Always ensure "all" tab exists
        if (!_filteredItems.ContainsKey("all")) {
            _filteredItems["all"] = new List<Inventory_Item>();
        }

        // Filter items: always check all slots up to maxInventorySize
        for (int i = 0; i < inventory.maxInventorySize; i++) {
            Inventory_Item item = (i < inventory.itemList.Count) ? inventory.itemList[i] : null;
            if (item == null) continue;

            // Always add to "all" tab
            _filteredItems["all"].Add(item);

            // Add to specific tab based on item type
            string tabId = GetTabIdForItem(item);
            if (!string.IsNullOrEmpty(tabId) && _filteredItems.ContainsKey(tabId)) {
                _filteredItems[tabId].Add(item);
            }
        }
    }

    private string GetTabIdForItem(Inventory_Item item)
    {
        if (item?.itemData != null) {
            // Map ItemType to tab IDs (must match tab ids in UI config exactly)
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
        if (!enableTabFiltering || inventory == null || uiConfig?.Slots == null) return;

        var slots = uiConfig.Slots;
        int maxSlotsToFill = Mathf.Min(slots.Count, inventory.maxInventorySize);

        for (int i = 0; i < slots.Count; i++)
            slots[i].ClearItem();

        for (int i = 0; i < maxSlotsToFill; i++)
        {
            Inventory_Item item = (i < inventory.itemList.Count) ? inventory.itemList[i] : null;
            if (item != null)
            {
                if (_currentTabId == "all")
                {
                    slots[i].SetItem(item);
                }
                else
                {
                    string tabId = GetTabIdForItem(item);
                    if (!string.IsNullOrEmpty(tabId) && tabId == _currentTabId)
                    {
                        slots[i].SetItem(item);
                    }
                    // else: leave slot empty (filtered out)
                }
            }
        }
    }
}