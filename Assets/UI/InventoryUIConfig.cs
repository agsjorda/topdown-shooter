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

    [Header("Scroll Wrapper")]
    public bool autoCreateScrollWrapper = true;
    public int scrollWidthPx = 0;
    public int scrollHeightPx = 350;
    public bool centerWrapper = true;
    public ScrollerVisibility verticalVisibility = ScrollerVisibility.Auto;
    public ScrollerVisibility horizontalVisibility = ScrollerVisibility.Hidden;

    [Header("Wrapper Background")]
    public bool wrapperUseBackground = false;
    public Texture2D wrapperBackgroundTexture;
    public Sprite wrapperBackgroundSprite;
    public Color wrapperBackgroundTint = Color.white;

    #region Tabs
    [System.Serializable]
    public class TabDescriptor
    {
        public string id = "all";
        public string label = "";
        public Texture2D iconTexture; // Sprite removed
        public Color iconTint = Color.white;
    }

    [Header("Tabs")]
    public List<TabDescriptor> tabs = new List<TabDescriptor>();
    public int activeTabIndex = 0;

    [Header("Tab Size (applies to all tabs)")]
    [Range(20, 400)] public int tabWidthPx = 0;   // 0 = use USS
    [Range(20, 400)] public int tabHeightPx = 0;  // 0 = use USS

    [Header("Tab Icon Size (applies to all tabs)")]
    [Range(16, 512)] public int tabIconWidthPx = 0;   // 0 = use USS
    [Range(16, 512)] public int tabIconHeightPx = 0;  // 0 = use USS 
    #endregion

    // cache for change detection (removed _cBorderPreset)
    int _cSlotSize, _cSlotCount;
    float _cCellMargin, _cBorderRadius;
    bool _cUseBg, _cUseCustomBorder;
    Color _cBgTint, _cBorderColor;
    Texture2D _cBgTex;
    Sprite _cBgSprite;
    int _cTabW, _cTabH, _cTabIconW, _cTabIconH;

    void Awake()
    {
        if (targetDocument == null)
            targetDocument = GetComponent<UIDocument>();
        SyncSlotCountFromInventory();
    }

    void OnEnable()
    {
        if (targetDocument == null) targetDocument = GetComponent<UIDocument>();
        if (inventorySource != null) inventorySource.InventoryChanged += OnInventoryChanged;

        SyncSlotCountFromInventory();
        BuildOrUpdate();
        Cache();
    }

    void OnDisable()
    {
        if (inventorySource != null) inventorySource.InventoryChanged -= OnInventoryChanged;
    }

    void OnValidate()
    {
        if (!Application.isPlaying) {
            SyncSlotCountFromInventory();
            BuildOrUpdate();
            Cache();
        }
    }

    void Update()
    {
        if (!Application.isPlaying) return;

        // Keep slotCount in sync at runtime too
        if (inventorySource != null && slotCount != Mathf.Max(1, inventorySource.maxInventorySize)) {
            slotCount = Mathf.Max(1, inventorySource.maxInventorySize);
        }

        if (HasChanged()) {
            BuildOrUpdate();
            Cache();
        }
    }

    private void OnInventoryChanged()
    {
        SyncSlotCountFromInventory();
        BuildOrUpdate();
        Cache();
    }

    private void SyncSlotCountFromInventory()
    {
        if (inventorySource != null)
            slotCount = Mathf.Max(1, inventorySource.maxInventorySize);
    }

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

    private void BuildOrUpdate()
    {
        if (targetDocument == null || targetDocument.rootVisualElement == null) return;

        var root = targetDocument.rootVisualElement;

        // Build tabs first (reads/writes tabButtonsContainer)
        BuildOrUpdateTabs(root);

        // Build slots inside scroll wrapper
        BuildOrUpdateSlots(root);
    }

    private void BuildOrUpdateTabs(VisualElement root)
    {
        var tabButtonsContainer = root.Q<VisualElement>("tabButtonsContainer");
        if (tabButtonsContainer == null) return;

        // Apply global sizes from inspector sliders
        InventoryTabElement.SetGlobalSize(tabWidthPx, tabHeightPx);
        InventoryTabElement.SetGlobalIconSize(tabIconWidthPx, tabIconHeightPx);

        tabButtonsContainer.Clear();

        for (int i = 0; i < tabs.Count; i++) {
            var def = tabs[i];
            var tab = new InventoryTabElement();
            tab.SetId(def.id);
            tab.SetIcon(def.iconTexture, def.iconTint); // Texture2D only
            tab.SetLabel(def.label);
            tab.SetActive(i == Mathf.Clamp(activeTabIndex, 0, Mathf.Max(0, tabs.Count - 1)));

            int capturedIndex = i;
            tab.Clicked += _ =>
            {
                SetActiveTab(tabButtonsContainer, capturedIndex);
                // TODO: filter content by TabId if needed
            };

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
        var tabContentContainer = root.Q<VisualElement>("tabContentContainer");
        if (tabContentContainer == null) return;

        var scrollWrapper = tabContentContainer.Q<InventoryScrollElement>();
        if (autoCreateScrollWrapper && scrollWrapper == null) {
            scrollWrapper = new InventoryScrollElement();
            tabContentContainer.Add(scrollWrapper);
        }

        VisualElement slotsContainer = scrollWrapper != null
            ? scrollWrapper.SlotsContainer
            : tabContentContainer.Q<VisualElement>(className: "inventory-slots-container");

        if (scrollWrapper != null) {
            scrollWrapper.SetBackground(wrapperBackgroundTexture, wrapperBackgroundSprite, wrapperBackgroundTint, wrapperUseBackground);
            scrollWrapper.SetSize(scrollWidthPx, scrollHeightPx);
            scrollWrapper.SetScrollerVisibility(verticalVisibility, horizontalVisibility);
            if (centerWrapper) scrollWrapper.CenterInParent();
        }

        if (slotsContainer == null) return;

        slotsContainer.Clear();

        int itemCount = inventorySource != null ? inventorySource.itemList.Count : 0;

        for (int i = 0; i < slotCount; i++) {
            var slot = new InventorySlotElement();
            slot.SetIndex(i + 1);
            slot.SetSlotSize(slotSize);
            slot.SetCellMargin(cellMargin);

            slot.ApplyVisuals(
                slotUseBackground,
                slotBackgroundTexture,
                slotBackgroundSprite,
                slotBackgroundTint,
                slotBorderRadius
            );

            // Default: let USS control borders (null inline colors)
            void ClearInlineBorderColors()
            {
                slot.style.borderLeftColor = new StyleColor(StyleKeyword.Null);
                slot.style.borderRightColor = new StyleColor(StyleKeyword.Null);
                slot.style.borderTopColor = new StyleColor(StyleKeyword.Null);
                slot.style.borderBottomColor = new StyleColor(StyleKeyword.Null);
            }
            ClearInlineBorderColors();

            // If custom color is enabled, override hover color using inline style on hover
            if (useCustomBorderColor) {
                var hoverColor = slotBorderColor; // capture

                slot.RegisterCallback<MouseEnterEvent>(_ =>
                {
                    slot.SetStaticBorderColor(hoverColor); // inline color beats USS
                });

                slot.RegisterCallback<MouseLeaveEvent>(_ =>
                {
                    ClearInlineBorderColors(); // restore USS default color when not hovering
                });
            }

            if (i < itemCount) {
                var invItem = inventorySource.itemList[i];
                slot.SetItem(invItem != null ? invItem.itemData : null, 1);
            } else {
                slot.ClearItem();
            }

            slotsContainer.Add(slot);
        }

        root.MarkDirtyRepaint();
    }
}
