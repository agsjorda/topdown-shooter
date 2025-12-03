using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

    private int _cachedSlotSize, _cachedSlotCount;
    private float _cachedCellMargin;
    private int _cachedTabW, _cachedTabH, _cachedTabIconW, _cachedTabIconH;

    private VisualElement _tabContentContainer;
    private InventoryScrollElement _scrollWrapper;
    private VisualElement _slotsContainer;
    private readonly List<Slot> _createdSlots = new List<Slot>();

    public IReadOnlyList<Slot> Slots => _createdSlots;
    public int CreatedSlotCount => _createdSlots.Count;

    public Slot GetSlot(int index)
    {
        return (index >= 0 && index < _createdSlots.Count) ? _createdSlots[index] : null;
    }

    public bool TryGetSlot(int index, out Slot slot)
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
        targetDocument ??= GetComponent<UIDocument>();
    }

    void OnEnable()
    {
        targetDocument ??= GetComponent<UIDocument>();
        CacheContainers();
        BuildOrUpdate();
        CacheValues();
    }

    void OnValidate()
    {
        if (!Application.isPlaying) {
            CacheContainers();
            BuildOrUpdate();
            CacheValues();
        }
    }

    void Update()
    {
        if (!Application.isPlaying) return;

        if (HasLayoutChanged()) {
            BuildOrUpdate();
            CacheValues();
        }
    }

    public void RebuildUI()
    {
        CacheContainers();
        BuildOrUpdate();
        CacheValues();
    }

    #region Private Methods
    private bool HasLayoutChanged()
    {
        return _cachedSlotSize != slotSize
            || _cachedSlotCount != slotCount
            || !Mathf.Approximately(_cachedCellMargin, cellMargin)
            || _cachedTabW != tabWidthPx
            || _cachedTabH != tabHeightPx
            || _cachedTabIconW != tabIconWidthPx
            || _cachedTabIconH != tabIconHeightPx;
    }

    private void CacheValues()
    {
        _cachedSlotSize = slotSize;
        _cachedSlotCount = slotCount;
        _cachedCellMargin = cellMargin;
        _cachedTabW = tabWidthPx;
        _cachedTabH = tabHeightPx;
        _cachedTabIconW = tabIconWidthPx;
        _cachedTabIconH = tabIconHeightPx;
    }

    private void CacheContainers()
    {
        var root = targetDocument?.rootVisualElement;
        if (root == null) return;

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

    private void BuildOrUpdate()
    {
        var root = targetDocument?.rootVisualElement;
        if (root == null) return;

        BuildOrUpdateTabs(root);
        BuildOrUpdateSlots(root);
    }

    private void BuildOrUpdateTabs(VisualElement root)
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
            // FIXED: Check what Clicked event signature expects
            // If it's Action<InventoryTabElement>, pass the tab parameter
            tab.Clicked += (clickedTab) => SetActiveTab(tabButtonsContainer, capturedIndex);
            tabButtonsContainer.Add(tab);
        }
    }

    private void SetActiveTab(VisualElement tabButtonsContainer, int index)
    {
        activeTabIndex = Mathf.Clamp(index, 0, Mathf.Max(0, tabs.Count - 1));
        int i = 0;
        foreach (var child in tabButtonsContainer.Children()) {
            if (child is InventoryTabElement tab)
                tab.SetActive(i == activeTabIndex);
            i++;
        }
    }

    private void BuildOrUpdateSlots(VisualElement root)
    {
        if (_tabContentContainer == null || _slotsContainer == null)
            CacheContainers();

        if (_slotsContainer == null) return;

        _slotsContainer.Clear();
        _createdSlots.Clear();

        _createdSlots.Capacity = slotCount;

        for (int i = 0; i < slotCount; i++) {
            var slot = new Slot();
            slot.SetSlotIndex(i);
            slot.SetSlotSize(slotSize);
            slot.SetCellMargin(cellMargin);

            _slotsContainer.Add(slot);
            _createdSlots.Add(slot);
        }

        root.MarkDirtyRepaint();
    }
    #endregion
}