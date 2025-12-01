using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Reusable inventory slot VisualElement. Configurable at runtime.
/// Keep visuals CSS-driven where possible so USS :hover rules work.
/// </summary>
[UxmlElement] // This attribute replaces the need for UxmlFactory/UxmlTraits
public class InventorySlotElement : VisualElement
{
    private const string HOVER_CLASS = "inventorySlots--hover";

    private Label indexLabel;
    private int index = 0;
    private int slotSize = 250;
    private float cellMargin = 8f;

    public int SlotIndex { get; private set; }
    private Image iconImage;
    private Label qtyLabel;

    // drag events
    public event System.Action<int> BeginDrag;
    public event System.Action<int> EndDrop;

    public InventorySlotElement()
    {
        AddToClassList("inventorySlots");
        name = "inventory-slot-element";

        indexLabel = new Label();
        indexLabel.style.unityTextAlign = TextAnchor.LowerRight;
        indexLabel.style.color = Color.white;
        hierarchy.Add(indexLabel);

        iconImage = new Image();
        iconImage.pickingMode = PickingMode.Ignore;
        iconImage.scaleMode = ScaleMode.ScaleToFit;
        hierarchy.Add(iconImage);

        qtyLabel = new Label();
        qtyLabel.style.unityTextAlign = TextAnchor.LowerRight;
        qtyLabel.style.color = Color.white;
        hierarchy.Add(qtyLabel);

        // Only toggle a class on enter/leave — let USS :hover handle visuals
        RegisterCallback<MouseEnterEvent>(_ => AddToClassList(HOVER_CLASS));
        RegisterCallback<MouseLeaveEvent>(_ => RemoveFromClassList(HOVER_CLASS));

        RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.button == (int)MouseButton.LeftMouse) {
                BeginDrag?.Invoke(SlotIndex);
                evt.StopPropagation();
            }
        });
        RegisterCallback<PointerUpEvent>(evt =>
        {
            if (evt.button == (int)MouseButton.LeftMouse) {
                EndDrop?.Invoke(SlotIndex);
                evt.StopPropagation();
            }
        });
    }

    public void SetIndex(int oneBasedIndex)
    {
        index = oneBasedIndex;
        if (indexLabel != null)
            indexLabel.text = oneBasedIndex > 0 ? oneBasedIndex.ToString() : "";
    }

    public void SetSlotIndex(int index) { SlotIndex = index; SetIndex(index); }

    public void SetSlotSize(int size)
    {
        slotSize = Mathf.Max(1, size);
        style.width = slotSize;
        style.height = slotSize;
    }

    public void SetCellMargin(float margin)
    {
        cellMargin = Mathf.Max(0f, margin);
        style.marginRight = cellMargin;
        style.marginBottom = cellMargin;
    }

    public void SetItem(Item_DataSO item, int qty)
    {
        if (item == null) { ClearItem(); return; }
        if (iconImage != null) iconImage.image = item.icon ? item.icon.texture : null;
        if (qtyLabel != null) qtyLabel.text = item.stackable ? qty.ToString() : "";
    }

    public void ClearItem()
    {
        if (iconImage != null) iconImage.image = null;
        if (qtyLabel != null) qtyLabel.text = "";
    }

    /// <summary>
    /// Apply visual options but do NOT set inline border color/width so USS :hover can override.
    /// Signature matches how Inventory_UI calls it.
    /// </summary>
    public void ApplyVisuals(bool useBackground, Texture2D backgroundTexture, Sprite backgroundSprite, Color backgroundTint, float borderRadius)
    {
        // only set radius and optional background; avoid inline border color/width
        style.borderTopLeftRadius = borderRadius;
        style.borderTopRightRadius = borderRadius;
        style.borderBottomLeftRadius = borderRadius;
        style.borderBottomRightRadius = borderRadius;

        if (useBackground) {
            Texture2D tex = backgroundTexture;
            if (tex == null && backgroundSprite != null)
                tex = backgroundSprite.texture;

            if (tex != null) {
                style.backgroundImage = new StyleBackground(tex);
                style.unityBackgroundImageTintColor = backgroundTint;
            } else {
                style.backgroundImage = null;
                style.backgroundColor = backgroundTint;
            }
        } else {
            style.backgroundImage = null;
            // do not override backgroundColor so USS :hover/background remains effective
        }
    }

    public void Build()
    {
        SetSlotSize(slotSize);
        SetCellMargin(cellMargin);
        SetIndex(index);
    }
}