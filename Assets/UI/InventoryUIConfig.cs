using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class InventoryUIConfig : MonoBehaviour
{
    [Header("Target")]
    public UIDocument targetDocument;

    #region Slots Properties (Only size, count, and margin remain)
    [Header("Slot Layout")]
    [Range(40, 300)] public int slotSize = 100;
    [Range(1, 200)] public int slotCount = 30;
    [Range(0f, 50f)] public float cellMargin = 8f;
    #endregion

    #region Scroll Wrapper
    [Header("Scroll Wrapper")]
    public bool autoCreateScrollWrapper = true;
    public int scrollWidthPx = 0;
    public int scrollHeightPx = 350;
    public ScrollerVisibility verticalVisibility = ScrollerVisibility.Auto;
    #endregion

    #region Tab Properties (Tabs remain unchanged)
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

    // Cache for change detection (only for size, count, margin)
    int _cSlotSize, _cSlotCount;
    float _cCellMargin;
    int _cTabW, _cTabH, _cTabIconW, _cTabIconH;

    // Cached UI containers and slots
    VisualElement _tabContentContainer;
    InventoryScrollElement _scrollWrapper;
    VisualElement _slotsContainer;

    // Store the actual created slots
    private readonly List<BaseSlot> _createdSlots = new List<BaseSlot>();

    // Expose read-only access
    public IReadOnlyList<BaseSlot> Slots => _createdSlots;
    public int CreatedSlotCount => _createdSlots.Count;
    public BaseSlot GetSlot(int index) => (index >= 0 && index < _createdSlots.Count) ? _createdSlots[index] : null;

    public bool TryGetSlot(int index, out BaseSlot slot)
    {
        if (index >= 0 && index < _createdSlots.Count) {
            slot = _createdSlots[index];
            return true;
        }
        slot = null;
        return false;
    }

    void Awake()
    {
        if (targetDocument == null)
            targetDocument = GetComponent<UIDocument>();
    }

    void OnEnable()
    {
        if (targetDocument == null) targetDocument = GetComponent<UIDocument>();
        CacheContainers();
        BuildOrUpdate();
        Cache();
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

    // Public entry point for external rebuilds
    public void RebuildUI()
    {
        CacheContainers();
        BuildOrUpdate();
        Cache();
    }

    #region Caching
    bool HasChanged()
    {
        return _cSlotSize != slotSize
            || _cSlotCount != slotCount
            || !Mathf.Approximately(_cCellMargin, cellMargin)
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
        _createdSlots.Capacity = slotCount;

        for (int i = 0; i < slotCount; i++) {
            var slot = new BaseSlot();
            slot.SetSlotIndex(i);
            slot.SetSlotSize(slotSize);
            slot.SetCellMargin(cellMargin);

            _slotsContainer.Add(slot);
            _createdSlots.Add(slot);
        }

        root.MarkDirtyRepaint();
    }
}