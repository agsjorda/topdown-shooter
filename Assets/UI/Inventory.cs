using UnityEngine;
using UnityEngine.UIElements;

public class Inventory : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private int slotSize = 100;
    [SerializeField] private int numberOfslots = 12;
    [SerializeField] private Sprite weaponIcon, armorIcon, miscIcon;

    private VisualElement allTabButton, weaponsTabButton, armorTabButton, consumableTabButton, miscTabButton;
    private VisualElement currentActiveTab;

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
        allTabButton = root.Q<VisualElement>("allTabButton");
        weaponsTabButton = root.Q<VisualElement>("weaponsTabButton");
        armorTabButton = root.Q<VisualElement>("armorTabButton");
        consumableTabButton = root.Q<VisualElement>("consumableTabButton");
        miscTabButton = root.Q<VisualElement>("miscTabButton");

        //setup tab button callbacks
        SetActiveWhenPressed();

        // Create tab content
        var tabContent = CreateTabContent();
        tabContentContainer.Add(tabContent);
        tabAndContentContainer.Add(tabContentContainer);

        inventoryContainer.Add(tabAndContentContainer);

        // Set first tab as active by default
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

        // Add active state to clicked tab
        tabButton.AddToClassList("inventoryTab--active");
        currentActiveTab = tabButton;

        // Here you would also switch the tab content
        // SwitchTabContent(tabButton.name);
    }

    private VisualElement CreateTabContent()
    {
        var content = new VisualElement();
        content.AddToClassList("tabContentContainer");

        // Create 10 inventory slots
        for (int i = 0; i < numberOfslots; i++) {
            content.Add(CreateInventorySlot(i + 1));
        }

        return content;
    }

    private VisualElement CreateInventorySlot(int number)
    {
        var slot = new VisualElement();
        slot.AddToClassList("inventorySlots");
        //slot.style.width = slotSize;
        //slot.style.height = slotSize;

        var label = new Label(number.ToString());
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.color = Color.white;
        slot.Add(label);

        return slot;
    }
}