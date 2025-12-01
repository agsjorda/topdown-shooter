using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class InventorySlotElement : VisualElement
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
        indexLabel.pickingMode = PickingMode.Ignore;     // important
        hierarchy.Add(indexLabel);

        iconImage = new Image();
        iconImage.pickingMode = PickingMode.Ignore;      // important
        iconImage.scaleMode = ScaleMode.ScaleToFit;
        hierarchy.Add(iconImage);

        qtyLabel = new Label();
        qtyLabel.style.unityTextAlign = TextAnchor.LowerRight;
        qtyLabel.style.color = Color.white;
        qtyLabel.pickingMode = PickingMode.Ignore;       // important
        hierarchy.Add(qtyLabel);

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
        iconImage.image = item.icon ? item.icon.texture : null;
        qtyLabel.text = item.stackable ? qty.ToString() : "";
    }

    public void ClearItem()
    {
        iconImage.image = null;
        qtyLabel.text = "";
    }

    public void ApplyVisuals(bool useBackground, Texture2D backgroundTexture, Sprite backgroundSprite, Color backgroundTint, float borderRadius)
    {
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
        }
    }

    // Inspector-set static border color (does not fight hover)
    public void SetStaticBorderColor(Color color)
    {
        style.borderLeftColor = color;
        style.borderRightColor = color;
        style.borderTopColor = color;
        style.borderBottomColor = color;
    }
}