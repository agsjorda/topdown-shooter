using UnityEngine;
using UnityEngine.UIElements;

public class Inventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private int slotSize = 100;
    [SerializeField] private int numberOfSlots = 30;
    [SerializeField] private float cellMargin = 8f;
    private Inventory_Base inventoryBase;

    private VisualElement[] tabButtons;
    private ScrollView scrollView;
    private VisualElement slotsContainer;

    // Cache frequently used strings
    private const string INVENTORY_TAB_ACTIVE_CLASS = "inventoryTab--active";
    private const string INVENTORY_SLOTS_CLASS = "inventorySlots";

    private void Awake()
    {
        inventoryBase = Object.FindFirstObjectByType<Inventory_Base>();
        numberOfSlots = inventoryBase != null ? inventoryBase.maxInventorySize : numberOfSlots;
    }

    private void Start()
    {
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        var root = uiDocument?.rootVisualElement;
        if (root == null) {
            Debug.LogError("Inventory: Root VisualElement not found");
            return;
        }

        var inventoryContainer = root.Q<VisualElement>("inventory-container");
        var tabContentContainer = root.Q<VisualElement>("tabContentContainer");

        if (inventoryContainer == null || tabContentContainer == null) {
            Debug.LogError("Inventory: Required containers not found");
            return;
        }

        InitializeTabButtons(root);
        InitializeScrollView(tabContentContainer);
        SetActiveTab(tabButtons[0]); // Set first tab as active
    }

    private void InitializeTabButtons(VisualElement root)
    {
        // Store all tab buttons in an array for easier management
        tabButtons = new VisualElement[]
        {
            root.Q<VisualElement>("allTabButton"),
            root.Q<VisualElement>("weaponsTabButton"),
            root.Q<VisualElement>("armorTabButton"),
            root.Q<VisualElement>("consumableTabButton"),
            root.Q<VisualElement>("miscTabButton")
        };

        // Validate tab buttons
        foreach (var tab in tabButtons) {
            if (tab == null) {
                Debug.LogWarning("Inventory: One or more tab buttons not found");
                return;
            }
        }

        // Register click events for all tabs
        for (int i = 0; i < tabButtons.Length; i++) {
            int tabIndex = i; // Capture index for closure
            tabButtons[i].RegisterCallback<ClickEvent>(_ => SetActiveTab(tabButtons[tabIndex]));
        }
    }

    private void InitializeScrollView(VisualElement parentContainer)
    {
        parentContainer.Clear();

        // Create and configure scroll view
        scrollView = new ScrollView {
            verticalScrollerVisibility = ScrollerVisibility.Hidden,
            horizontalScrollerVisibility = ScrollerVisibility.Hidden
        };
        scrollView.AddToClassList("inventory-scroll-view");

        // Create slots container
        slotsContainer = new VisualElement();
        slotsContainer.AddToClassList("inventory-slots-container");

        // Pre-allocate and create slots
        CreateInventorySlots();

        // Build hierarchy
        scrollView.Add(slotsContainer);
        parentContainer.Add(scrollView);
    }

    private void CreateInventorySlots()
    {
        // Pre-allocate capacity if needed (VisualElements don't have capacity, but good practice for lists)
        for (int i = 0; i < numberOfSlots; i++) {
            slotsContainer.Add(CreateInventorySlot(i + 1));
        }
    }

    private VisualElement CreateInventorySlot(int slotNumber)
    {
        var slot = new VisualElement();
        slot.AddToClassList(INVENTORY_SLOTS_CLASS);

        // Set styles efficiently in one go
        slot.style.width = slotSize;
        slot.style.height = slotSize;
        slot.style.marginRight = cellMargin;
        slot.style.marginBottom = cellMargin;

        // Create label with minimal style changes
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
        // Validate input
        if (activeTab == null) return;

        // Remove active class from all tabs
        foreach (var tab in tabButtons) {
            tab?.RemoveFromClassList(INVENTORY_TAB_ACTIVE_CLASS);
        }

        // Add active class to selected tab
        activeTab.AddToClassList(INVENTORY_TAB_ACTIVE_CLASS);
    }

    // Optional:  Method to update slots dynamically
    public void UpdateSlots(int newSlotCount)
    {
        if (newSlotCount < 0) return;

        numberOfSlots = newSlotCount;
        slotsContainer.Clear();
        CreateInventorySlots();
    }

    // Cleanup on destroy
    private void OnDestroy()
    {
        if (tabButtons != null) {
            foreach (var tab in tabButtons) {
                tab?.UnregisterCallback<ClickEvent>(_ => SetActiveTab(tab));
            }
        }
    }
}