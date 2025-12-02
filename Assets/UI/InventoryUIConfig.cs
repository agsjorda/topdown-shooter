using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class InventoryUIConfig : MonoBehaviour
{
    [Header("Target")]
    public UIDocument targetDocument;

    [Header("Inventory Source")]
    public Inventory_Base inventorySource;

    #region Slots Properties
    [Header("Slot Layout")]
    [Range(40, 300)] public int slotSize = 100;
    [Range(1, 200)] public int slotCount = 30;
    [Range(0f, 50f)] public float cellMargin = 8f;

    [Header("Slot Visuals")]
    public bool slotUseBackground = false;
    public Texture2D slotBackgroundTexture;
    public Sprite slotBackgroundSprite;
    public Color slotBackgroundTint = Color.white;
    [Range(0f, 50f)] public float slotBorderRadius = 5f;

    [Header("Slot Border Color")]
    public bool useCustomBorderColor = false;
    public Color slotBorderColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    #endregion

    #region Scroll Wrapper
    [Header("Scroll Wrapper")]
    public bool autoCreateScrollWrapper = true;
    public int scrollWidthPx = 0;
    public int scrollHeightPx = 350;
    public ScrollerVisibility verticalVisibility = ScrollerVisibility.Auto;

    [Header("Wrapper Background")]
    public bool wrapperUseBackground = false;
    public Texture2D wrapperBackgroundTexture;
    public Sprite wrapperBackgroundSprite;
    public Color wrapperBackgroundTint = Color.white;
    #endregion

    #region Tab Properties
    [System.Serializable]
    public class TabDescriptor
    {
        public string id = "all";
        public string label = "";
        public Texture2D iconTexture;
        public Color iconTint = Color.white;
    }

    [Header("Tabs")]
    public List<TabDescriptor> tabs = new List<TabDescriptor>();
    public int activeTabIndex = 0;
    [Range(20, 400)] public int tabWidthPx = 0;
    [Range(20, 400)] public int tabHeightPx = 0;
    [Range(16, 512)] public int tabIconWidthPx = 0;
    [Range(16, 512)] public int tabIconHeightPx = 0;
    #endregion

    // cache for change detection
    int _cSlotSize, _cSlotCount;
    float _cCellMargin, _cBorderRadius;
    bool _cUseBg, _cUseCustomBorder;
    Color _cBgTint, _cBorderColor;
    Texture2D _cBgTex;
    Sprite _cBgSprite;
    int _cTabW, _cTabH, _cTabIconW, _cTabIconH;

    // cached UI containers and slots
    VisualElement _tabContentContainer;
    InventoryScrollElement _scrollWrapper;
    VisualElement _slotsContainer;
    List<BaseSlot> _createdSlots = new List<BaseSlot>();

    void Awake()
    {
        if (targetDocument == null)
            targetDocument = GetComponent<UIDocument>();
    }

    void OnEnable()
    {
        if (targetDocument == null) targetDocument = GetComponent<UIDocument>();

        // Subscribe to inventory changes
        if (inventorySource != null) {
            inventorySource.InventoryChanged -= OnInventoryChanged;
            inventorySource.InventoryChanged += OnInventoryChanged;
            Debug.Log("Subscribed to inventory changes");
        }

        CacheContainers();
        BuildOrUpdate();
        Cache();

        // Initial inventory sync
        SyncItemsToSlots();
    }

    void OnDisable()
    {
        // Unsubscribe from inventory changes
        if (inventorySource != null) {
            inventorySource.InventoryChanged -= OnInventoryChanged;
            Debug.Log("Unsubscribed from inventory changes");
        }
    }

    void OnValidate()
    {
        if (!Application.isPlaying) {
            CacheContainers();
            BuildOrUpdate();
            Cache();
        }
    }

    void Update()
    {
        if (!Application.isPlaying) return;

        if (HasChanged()) {
            BuildOrUpdate();
            Cache();
        }
    }

    // This is called when inventory changes
    void OnInventoryChanged()
    {
        Debug.Log("Inventory changed - updating UI");
        SyncItemsToSlots();
    }

    #region Caching
    bool HasChanged()
    {
        return _cSlotSize != slotSize
            || _cSlotCount != slotCount
            || !Mathf.Approximately(_cCellMargin, cellMargin)
            || !Mathf.Approximately(_cBorderRadius, slotBorderRadius)
            || _cUseBg != slotUseBackground
            || _cBgTex != slotBackgroundTexture
            || _cBgSprite != slotBackgroundSprite
            || _cBgTint != slotBackgroundTint
            || _cUseCustomBorder != useCustomBorderColor
            || _cBorderColor != slotBorderColor
            || _cTabW != tabWidthPx
            || _cTabH != tabHeightPx
            || _cTabIconW != tabIconWidthPx
            || _cTabIconH != tabIconHeightPx;
    }

    void Cache()
    {
        _cSlotSize = slotSize;
        _cSlotCount = slotCount;
        _cCellMargin = cellMargin;
        _cBorderRadius = slotBorderRadius;
        _cUseBg = slotUseBackground;
        _cBgTex = slotBackgroundTexture;
        _cBgSprite = slotBackgroundSprite;
        _cBgTint = slotBackgroundTint;
        _cUseCustomBorder = useCustomBorderColor;
        _cBorderColor = slotBorderColor;
        _cTabW = tabWidthPx;
        _cTabH = tabHeightPx;
        _cTabIconW = tabIconWidthPx;
        _cTabIconH = tabIconHeightPx;
    }

    void CacheContainers()
    {
        var root = targetDocument != null ? targetDocument.rootVisualElement : null;
        if (root == null) return;

        _tabContentContainer = _tabContentContainer ?? root.Q<VisualElement>("tabContentContainer");
        if (_tabContentContainer == null) return;

        _scrollWrapper = _scrollWrapper ?? _tabContentContainer.Q<InventoryScrollElement>();
        if (autoCreateScrollWrapper && _scrollWrapper == null) {
            _scrollWrapper = new InventoryScrollElement();
            _tabContentContainer.Add(_scrollWrapper);
        }

        _slotsContainer = _scrollWrapper != null
            ? _scrollWrapper.SlotsContainer
            : _tabContentContainer.Q<VisualElement>(className: "inventory-slots-container");

        if (_scrollWrapper != null) {
            _scrollWrapper.SetBackground(wrapperBackgroundTexture, wrapperBackgroundSprite, wrapperBackgroundTint, wrapperUseBackground);
            _scrollWrapper.SetSize(scrollWidthPx, scrollHeightPx);
            _scrollWrapper.SetScrollerVisibility(verticalVisibility);
        }
    }
    #endregion

    void BuildOrUpdate()
    {
        var root = targetDocument != null ? targetDocument.rootVisualElement : null;
        if (root == null) return;

        BuildOrUpdateTabs(root);
        BuildOrUpdateSlots(root);

        // Sync items after creating slots
        SyncItemsToSlots();
    }

    void BuildOrUpdateTabs(VisualElement root)
    {
        var tabButtonsContainer = root.Q<VisualElement>("tabButtonsContainer");
        if (tabButtonsContainer == null) return;

        InventoryTabElement.SetGlobalSize(tabWidthPx, tabHeightPx);
        InventoryTabElement.SetGlobalIconSize(tabIconWidthPx, tabIconHeightPx);

        tabButtonsContainer.Clear();

        for (int i = 0; i < tabs.Count; i++) {
            var def = tabs[i];
            var tab = new InventoryTabElement();
            tab.SetId(def.id);
            tab.SetIcon(def.iconTexture, def.iconTint);
            tab.SetLabel(def.label);
            tab.SetActive(i == Mathf.Clamp(activeTabIndex, 0, Mathf.Max(0, tabs.Count - 1)));

            int capturedIndex = i;
            tab.Clicked += _ => SetActiveTab(tabButtonsContainer, capturedIndex);
            tabButtonsContainer.Add(tab);
        }
    }

    void SetActiveTab(VisualElement tabButtonsContainer, int index)
    {
        activeTabIndex = Mathf.Clamp(index, 0, Mathf.Max(0, tabs.Count - 1));
        int i = 0;
        foreach (var child in tabButtonsContainer.Children()) {
            if (child is InventoryTabElement tab)
                tab.SetActive(i == activeTabIndex);
            i++;
        }
    }

    void BuildOrUpdateSlots(VisualElement root)
    {
        if (_tabContentContainer == null || _slotsContainer == null)
            CacheContainers();
        if (_slotsContainer == null) return;

        _slotsContainer.Clear();
        _createdSlots.Clear();

        for (int i = 0; i < slotCount; i++) {
            var slot = new BaseSlot();
            slot.SetSlotIndex(i);
            slot.SetSlotSize(slotSize);
            slot.SetCellMargin(cellMargin);

            slot.ApplyBackground(
                slotUseBackground,
                slotBackgroundTexture,
                slotBackgroundSprite,
                slotBackgroundTint,
                slotBorderRadius
            );

            ClearInlineBorderColors(slot);

            if (useCustomBorderColor) {
                var hoverColor = slotBorderColor;
                slot.RegisterCallback<MouseEnterEvent>(_ => slot.SetStaticBorderColor(hoverColor));
                slot.RegisterCallback<MouseLeaveEvent>(_ => ClearInlineBorderColors(slot));
            }

            _slotsContainer.Add(slot);
            _createdSlots.Add(slot);
        }

        root.MarkDirtyRepaint();
    }

    // ✅ CRITICAL: This method syncs inventory items to UI slots
    void SyncItemsToSlots()
    {
        if (inventorySource == null) {
            Debug.LogWarning("No inventory source to sync with");
            return;
        }

        if (_createdSlots == null || _createdSlots.Count == 0) {
            Debug.LogWarning("No slots created yet");
            return;
        }

        Debug.Log($"Syncing {inventorySource.itemList.Count} items to {_createdSlots.Count} slots");

        // Clear all slots first
        foreach (var slot in _createdSlots) {
            slot.ClearItem();
        }

        // Assign items to slots
        for (int i = 0; i < inventorySource.itemList.Count && i < _createdSlots.Count; i++) {
            var item = inventorySource.itemList[i];
            if (item != null && item.itemData != null) {
                Debug.Log($"Setting item {item.itemData.itemName} at slot {i}");
                _createdSlots[i].SetItem(item, 1);
            }
        }

        // Force UI refresh
        if (targetDocument != null && targetDocument.rootVisualElement != null)
            targetDocument.rootVisualElement.MarkDirtyRepaint();
    }

    static void ClearInlineBorderColors(VisualElement el)
    {
        el.style.borderLeftColor = new StyleColor(StyleKeyword.Null);
        el.style.borderRightColor = new StyleColor(StyleKeyword.Null);
        el.style.borderTopColor = new StyleColor(StyleKeyword.Null);
        el.style.borderBottomColor = new StyleColor(StyleKeyword.Null);
    }
}