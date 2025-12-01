using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Lightweight runtime configurator for inventory slot visuals.
/// - Updates in Play mode when inspector values change (via Update change-detection).
/// - Supports manual refresh via context menu and edit-time preview via OnValidate.
/// - Keeps implementation small and deterministic (clear + recreate slots).
/// </summary>
[DisallowMultipleComponent]
public class InventoryUIConfig : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("UIDocument to target. If empty the first active UIDocument in the scene will be used.")]
    public UIDocument targetDocument;

    [Tooltip("Name or class of the VisualElement container where slots will be placed.")]
    public string slotsContainerNameOrClass = "inventory-slots-container";

    [Header("Slot Layout")]
    [Range(40, 300)] public int slotSize = 100;
    [Range(1, 200)] public int slotCount = 30;
    [Range(0f, 50f)] public float cellMargin = 8f;

    [Header("Slot Visuals")]
    public bool useBackground = false;
    public Texture2D backgroundTexture;
    public Sprite backgroundSprite;
    public Color backgroundTint = Color.white;

    //[Header("Border Preset (USS classes)")]
    public enum BorderPreset { Default, Yellow, Red, Green }
    public BorderPreset borderPreset = BorderPreset.Yellow;
    [Range(0f, 50f)] public float borderRadius = 5f;

    // cached values for change detection (Play mode)
    int _cacheSlotSize;
    int _cacheSlotCount;
    float _cacheCellMargin;
    bool _cacheUseBackground;
    Object _cacheBackgroundTexture;
    Object _cacheBackgroundSprite;
    Color _cacheBackgroundTint;
    BorderPreset _cacheBorderPreset;
    float _cacheBorderRadius;

    void Start()
    {
        // ensure a UIDocument is available at runtime if none assigned
        if (targetDocument == null)
            targetDocument = FindObjectOfType<UIDocument>();
        CacheAll();
        RefreshSlotsInDocument();
    }

    void Update()
    {
        if (!Application.isPlaying) return;

        if (HasChanged()) {
            CacheAll();
            RefreshSlotsInDocument();
        }
    }

    void OnValidate()
    {
        // quick editor preview when editing values outside Play mode
        if (!Application.isPlaying)
            RefreshSlotsInDocument();
    }

    [ContextMenu("Refresh Slots")]
    public void RefreshSlotsInDocument()
    {
        // find a UIDocument if none assigned
        if (targetDocument == null)
            targetDocument = FindObjectOfType<UIDocument>();

        if (targetDocument == null) return;

        var root = targetDocument.rootVisualElement;
        if (root == null) return;

        // locate container by name OR class
        VisualElement container = root.Q<VisualElement>(slotsContainerNameOrClass);
        if (container == null)
            container = root.Query<VisualElement>(className: slotsContainerNameOrClass).First();

        if (container == null) return;

        // simple and robust: clear and recreate
        container.Clear();

        for (int i = 0; i < slotCount; i++) {
            var slot = new InventorySlotElement();
            slot.SetIndex(i + 1);
            slot.SetSlotSize(slotSize);
            slot.SetCellMargin(cellMargin);

            // apply non-border visuals (border colors handled via USS classes)
            slot.ApplyVisuals(useBackground, backgroundTexture, backgroundSprite, backgroundTint, borderRadius);

            // toggle one preset class for border color (USS must define .border-yellow/.border-red/.border-green)
            slot.RemoveFromClassList("border-yellow");
            slot.RemoveFromClassList("border-red");
            slot.RemoveFromClassList("border-green");

            switch (borderPreset) {
                case BorderPreset.Yellow: slot.AddToClassList("border-yellow"); break;
                case BorderPreset.Red: slot.AddToClassList("border-red"); break;
                case BorderPreset.Green: slot.AddToClassList("border-green"); break;
                default: break;
            }

            container.Add(slot);
        }

        // request repaint so changes appear immediately
        root.MarkDirtyRepaint();
    }

    bool HasChanged()
    {
        if (_cacheSlotSize != slotSize) return true;
        if (_cacheSlotCount != slotCount) return true;
        if (!Mathf.Approximately(_cacheCellMargin, cellMargin)) return true;
        if (_cacheUseBackground != useBackground) return true;
        if (_cacheBackgroundTexture != backgroundTexture) return true;
        if (_cacheBackgroundSprite != backgroundSprite) return true;
        if (_cacheBackgroundTint != backgroundTint) return true;
        if (_cacheBorderPreset != borderPreset) return true;
        if (!Mathf.Approximately(_cacheBorderRadius, borderRadius)) return true;
        return false;
    }

    void CacheAll()
    {
        _cacheSlotSize = slotSize;
        _cacheSlotCount = slotCount;
        _cacheCellMargin = cellMargin;
        _cacheUseBackground = useBackground;
        _cacheBackgroundTexture = backgroundTexture;
        _cacheBackgroundSprite = backgroundSprite;
        _cacheBackgroundTint = backgroundTint;
        _cacheBorderPreset = borderPreset;
        _cacheBorderRadius = borderRadius;
    }
}
