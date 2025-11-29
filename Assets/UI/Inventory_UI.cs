using UnityEngine;
using UnityEngine.UIElements;

public class Inventory_UI : UIBaseComponent
{
    private const string INVENTORY_TAB_ACTIVE_CLASS = "inventoryTab--active";
    private const string INVENTORY_SLOTS_CLASS = "inventorySlots";

    private Inventory_Base inventoryBase;

    private VisualElement[] tabButtons;
    private ScrollView scrollView;
    private VisualElement slotsContainer;

    private int slotSize = 250;
    private int numberOfSlots = 30;
    private float cellMargin = 8f;

    public Inventory_UI(VisualElement root) : base(root)
    {
        if (!IsValid) return;

        inventoryBase = Object.FindFirstObjectByType<Inventory_Base>();
        if (inventoryBase != null)
            numberOfSlots = inventoryBase.maxInventorySize;

        InitializeInventory();
    }

    private void InitializeInventory()
    {
        var inventoryContainer = FindElement("inventory-container");
        var tabContentContainer = FindElement("tabContentContainer");

        if (inventoryContainer == null || tabContentContainer == null) {
            Debug.LogError("InventoryUI: Required UI containers not found.");
            return;
        }

        InitializeTabButtons();
        InitializeScrollView(tabContentContainer);

        // Activate first tab by default
        if (tabButtons != null && tabButtons.Length > 0)
            SetActiveTab(tabButtons[0]);
    }

    private void InitializeTabButtons()
    {
        tabButtons = new VisualElement[]
        {
            FindElement("allTabButton"),
            FindElement("weaponsTabButton"),
            FindElement("armorTabButton"),
            FindElement("consumableTabButton"),
            FindElement("miscTabButton")
        };

        // Register tab click events
        for (int i = 0; i < tabButtons.Length; i++) {
            int index = i;
            tabButtons[i]?.RegisterCallback<ClickEvent>(_ => SetActiveTab(tabButtons[index]));
        }
    }

    private void InitializeScrollView(VisualElement parent)
    {
        parent.Clear();

        scrollView = new ScrollView() {
            verticalScrollerVisibility = ScrollerVisibility.Hidden,
            horizontalScrollerVisibility = ScrollerVisibility.Hidden
        };
        scrollView.AddToClassList("inventory-scroll-view");

        slotsContainer = new VisualElement();
        slotsContainer.AddToClassList("inventory-slots-container");

        CreateInventorySlots();

        scrollView.Add(slotsContainer);
        parent.Add(scrollView);
    }

    private void CreateInventorySlots()
    {
        for (int i = 0; i < numberOfSlots; i++) {
            slotsContainer.Add(CreateSlot(i + 1));
        }
    }

    private VisualElement CreateSlot(int slotNumber)
    {
        var slot = new VisualElement();
        slot.AddToClassList(INVENTORY_SLOTS_CLASS);

        slot.style.width = slotSize;
        slot.style.height = slotSize;
        slot.style.marginRight = cellMargin;
        slot.style.marginBottom = cellMargin;

        var label = new Label(slotNumber.ToString()) {
            style =
            {
                unityTextAlign = TextAnchor.MiddleCenter,
                color = Color.white
            }
        };

        slot.Add(label);
        return slot;
    }

    private void SetActiveTab(VisualElement activeTab)
    {
        if (activeTab == null) return;

        foreach (var tab in tabButtons)
            tab?.RemoveFromClassList(INVENTORY_TAB_ACTIVE_CLASS);

        activeTab.AddToClassList(INVENTORY_TAB_ACTIVE_CLASS);
    }

    //add logic to update slots based on inventory size
    public void UpdateSlots(int newSlotCount)
    {
        numberOfSlots = Mathf.Max(0, newSlotCount);

        slotsContainer?.Clear();
        CreateInventorySlots();
    }

    public override void Show()
    {
        base.Show();
        Debug.Log("Inventory opened");
    }

    public override void Hide()
    {
        base.Hide();
        Debug.Log("Inventory closed");
    }
}
