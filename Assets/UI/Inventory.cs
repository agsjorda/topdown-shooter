using UnityEngine;
using UnityEngine.UIElements;

public class Inventory : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private int slotSize = 100;
    [SerializeField] private int numberOfslots = 30;

    private VisualElement allTabButton, weaponsTabButton, armorTabButton, consumableTabButton, miscTabButton;
    private VisualElement currentActiveTab;

    private const int Rows = 4;
    private const float CellMargin = 8f;

    private void Start()
    {
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        //Initialize Visual Elements
        var root = uiDocument?.rootVisualElement;
        if (root == null) return;

        var inventoryContainer = root.Q<VisualElement>("inventory-container");
        if (inventoryContainer == null) return;

        var tabAndContentContainer = root.Q<VisualElement>("tabAndContentContainer");
        var tabContentContainer = root.Q<VisualElement>("tabContentContainer");

        // Tab buttons
        allTabButton = root.Q<VisualElement>("allTabButton");
        weaponsTabButton = root.Q<VisualElement>("weaponsTabButton");
        armorTabButton = root.Q<VisualElement>("armorTabButton");
        consumableTabButton = root.Q<VisualElement>("consumableTabButton");
        miscTabButton = root.Q<VisualElement>("miscTabButton");

        SetActiveWhenPressed();

        // Create content and attach
        var tabContent = CreateTabContent();
        tabContentContainer.Add(tabContent);
        tabAndContentContainer.Add(tabContentContainer);

        inventoryContainer.Add(tabAndContentContainer);

        SetActiveTab(allTabButton);
    }

    private void SetActiveWhenPressed()
    {
        allTabButton.RegisterCallback<ClickEvent>(ev => SetActiveTab(allTabButton));
        weaponsTabButton.RegisterCallback<ClickEvent>(ev => SetActiveTab(weaponsTabButton));
        armorTabButton.RegisterCallback<ClickEvent>(ev => SetActiveTab(armorTabButton));
        consumableTabButton.RegisterCallback<ClickEvent>(ev => SetActiveTab(consumableTabButton));
        miscTabButton.RegisterCallback<ClickEvent>(ev => SetActiveTab(miscTabButton));
    }

    private void SetActiveTab(VisualElement tabButton)
    {
        // Remove active state from all tabs
        allTabButton?.RemoveFromClassList("inventoryTab--active");
        weaponsTabButton?.RemoveFromClassList("inventoryTab--active");
        armorTabButton?.RemoveFromClassList("inventoryTab--active");
        consumableTabButton?.RemoveFromClassList("inventoryTab--active");
        miscTabButton?.RemoveFromClassList("inventoryTab--active");

        tabButton.AddToClassList("inventoryTab--active");
        currentActiveTab = tabButton;

        // (Add tab switching content here later if needed)
    }

    private VisualElement CreateTabContent()
    {
        // Horizontal scroll view
        var scrollView = new ScrollView(ScrollViewMode.Horizontal) {
            style =
            {
                width = Length.Percent(100),
                height = Length.Percent(100),
                overflow = Overflow.Hidden // hides scrollbars
            }
        };

        scrollView.AddToClassList("inventoryScrollView");

        // Container that holds exactly 4 rows
        var gridContainer = new VisualElement {
            style =
            {
                flexDirection = FlexDirection.Column,
                flexGrow = 1,
                flexShrink = 0,
                flexWrap = Wrap.NoWrap
            }
        };

        gridContainer.AddToClassList("inventoryGridContainer");

        // Create rows
        var rows = new VisualElement[Rows];

        for (int r = 0; r < Rows; r++) {
            var row = new VisualElement {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = slotSize,
                    flexGrow = 0,
                    flexShrink = 0
                }
            };

            rows[r] = row;
            gridContainer.Add(row);
        }

        // Fill slots across rows
        int currentRow = 0;
        int itemsPerRow = Mathf.CeilToInt(numberOfslots / (float)Rows);

        for (int i = 0; i < numberOfslots; i++) {
            if (i > 0 && i % itemsPerRow == 0)
                currentRow++;

            currentRow = Mathf.Clamp(currentRow, 0, Rows - 1);

            rows[currentRow].Add(CreateInventorySlot(i + 1));
        }

        scrollView.Add(gridContainer);
        return scrollView;
    }

    private VisualElement CreateInventorySlot(int number)
    {
        var slot = new VisualElement();
        slot.AddToClassList("inventorySlots");

        slot.style.width = slotSize;
        slot.style.height = slotSize;
        slot.style.marginLeft = CellMargin;
        slot.style.marginTop = CellMargin;

        var label = new Label(number.ToString()) {
            style =
            {
                unityTextAlign = TextAnchor.MiddleCenter,
                color = Color.white
            }
        };

        slot.Add(label);

        return slot;
    }
}
