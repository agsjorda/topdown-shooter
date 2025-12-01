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

    [Header("Slot Visuals")]
    public bool slotUseBackground = false;
    public Texture2D slotBackgroundTexture;
    public Sprite slotBackgroundSprite;
    public Color slotBackgroundTint = Color.white;
    [Range(0f, 50f)] public float slotBorderRadius = 5f;

    [Header("Slot Border Color")]
    public bool useCustomBorderColor = false;
    public Color slotBorderColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    public enum BorderPreset { Default, Yellow, Red, Green }
    public BorderPreset borderPreset = BorderPreset.Yellow;

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

    // cache for change detection
    int _cSlotSize, _cSlotCount;
    float _cCellMargin, _cBorderRadius;
    bool _cUseBg, _cUseCustomBorder;
    Color _cBgTint, _cBorderColor;
    Texture2D _cBgTex;
    Sprite _cBgSprite;
    BorderPreset _cBorderPreset; // <- track preset

    void Awake()
    {
        if (targetDocument == null)
            targetDocument = GetComponent<UIDocument>();
    }

    void OnEnable() { BuildOrUpdate(); Cache(); }
    void OnValidate() { if (!Application.isPlaying) { BuildOrUpdate(); Cache(); } }
    void Update()
    {
        if (!Application.isPlaying) return;
        if (HasChanged()) { BuildOrUpdate(); Cache(); }
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
            || _cBorderPreset != borderPreset; // <- detect preset change
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
        _cBorderPreset = borderPreset; // <- cache preset
    }

    private void BuildOrUpdate()
    {
        if (targetDocument == null || targetDocument.rootVisualElement == null) return;

        var root = targetDocument.rootVisualElement;
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

        // layout
        slotsContainer.style.flexDirection = FlexDirection.Row;
        slotsContainer.style.flexWrap = Wrap.Wrap;
        slotsContainer.style.justifyContent = Justify.Center;
        slotsContainer.style.alignContent = Align.FlexStart;
        slotsContainer.style.alignItems = Align.FlexStart;
        slotsContainer.style.height = StyleKeyword.Null;
        slotsContainer.style.width = new Length(100, LengthUnit.Percent);

        // reuse existing when only preset changes (faster, avoids flicker)
        var existingSlots = slotsContainer.Query<InventorySlotElement>().ToList();
        if (existingSlots.Count == slotCount
            && _cSlotSize == slotSize
            && Mathf.Approximately(_cCellMargin, cellMargin)
            && _cUseBg == slotUseBackground
            && _cBorderRadius == slotBorderRadius
            && _cBgTex == slotBackgroundTexture
            && _cBgSprite == slotBackgroundSprite
            && _cBgTint == slotBackgroundTint
            && _cUseCustomBorder == useCustomBorderColor
            && _cBorderColor == slotBorderColor) {
            // Only preset changed: update classes, no rebuild
            ApplyPresetToExistingSlots(existingSlots);
            root.MarkDirtyRepaint();
            return;
        }

        // full rebuild
        slotsContainer.Clear();

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

            // If presets are used, ensure inline border colors are cleared so USS hover can win
            if (!useCustomBorderColor) {
                slot.style.borderLeftColor = new StyleColor(StyleKeyword.Null);
                slot.style.borderRightColor = new StyleColor(StyleKeyword.Null);
                slot.style.borderTopColor = new StyleColor(StyleKeyword.Null);
                slot.style.borderBottomColor = new StyleColor(StyleKeyword.Null);
            } else {
                slot.SetStaticBorderColor(slotBorderColor); // static, hover won't override inline
            }

            ApplyPresetClass(slot);

            slotsContainer.Add(slot);
        }

        root.MarkDirtyRepaint();
    }

    private void ApplyPresetToExistingSlots(System.Collections.Generic.List<InventorySlotElement> slots)
    {
        foreach (var slot in slots) {
            // Clear any inline border colors when using presets so hover color applies
            if (!useCustomBorderColor) {
                slot.style.borderLeftColor = new StyleColor(StyleKeyword.Null);
                slot.style.borderRightColor = new StyleColor(StyleKeyword.Null);
                slot.style.borderTopColor = new StyleColor(StyleKeyword.Null);
                slot.style.borderBottomColor = new StyleColor(StyleKeyword.Null);
            } else {
                slot.SetStaticBorderColor(slotBorderColor);
            }
            ApplyPresetClass(slot);
        }
    }

    private void ApplyPresetClass(InventorySlotElement slot)
    {
        slot.RemoveFromClassList("border-yellow");
        slot.RemoveFromClassList("border-red");
        slot.RemoveFromClassList("border-green");
        switch (borderPreset) {
            case BorderPreset.Yellow: slot.AddToClassList("border-yellow"); break;
            case BorderPreset.Red: slot.AddToClassList("border-red"); break;
            case BorderPreset.Green: slot.AddToClassList("border-green"); break;
            default: break;
        }
    }
}
