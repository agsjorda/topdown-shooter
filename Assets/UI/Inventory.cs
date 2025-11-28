using UnityEngine;
using UnityEngine.UIElements;

public class Inventory : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private int slotSize = 100;
    [SerializeField] private Sprite weaponIcon, armorIcon, miscIcon;

    private VisualElement weaponsTabButton, armorTabButton, miscTabButton;
    private VisualElement currentActiveTab;
    private VisualElement tabButtonsContainer;

    private void Start()
    {
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        var root = uiDocument?.rootVisualElement;
        if (root == null) return;

        var inventoryContainer = root.Q<VisualElement>("inventory-container");
        if (inventoryContainer == null) return;


        // Create main layout
        var tabAndContentContainer = root.Q<VisualElement>("tabAndContentContainer");
        tabButtonsContainer = root.Q<VisualElement>("tabButtonsContainer");
        // Create tab buttons row
        CreateTabButtons();
        tabAndContentContainer.Add(tabButtonsContainer);

        // Create tab content
        var tabContent = CreateTabContent();
        tabAndContentContainer.Add(tabContent);

        inventoryContainer.Add(tabAndContentContainer);

        // Set first tab as active by default
        SetActiveTab(weaponsTabButton);
    }

    private void CreateTabButtons()
    {
        // Create three tab buttons
        weaponsTabButton = CreateIconTabButton("weapons", weaponIcon);
        armorTabButton = CreateIconTabButton("armor", armorIcon);
        miscTabButton = CreateIconTabButton("misc", miscIcon);

        tabButtonsContainer.Add(weaponsTabButton);
        tabButtonsContainer.Add(armorTabButton);
        tabButtonsContainer.Add(miscTabButton);

    }

    private VisualElement CreateIconTabButton(string name, Sprite icon)
    {
        var button = new VisualElement();
        button.name = $"{name}-tab";
        button.AddToClassList("inventoryTab"); // Use the new USS class

        // Set fixed size
        button.style.width = slotSize;
        button.style.height = slotSize;
        button.style.marginLeft = 15;
        button.style.marginRight = 15;

        // Add icon container
        var iconContainer = new VisualElement();
        iconContainer.name = $"{name}-icon";
        iconContainer.AddToClassList("inventoryTab-icon");

        if (icon != null) {
            iconContainer.style.backgroundImage = new StyleBackground(icon);
        }

        button.Add(iconContainer);

        // Add click event
        button.RegisterCallback<ClickEvent>(evt => SetActiveTab(button));

        return button;
    }

    private void SetActiveTab(VisualElement tabButton)
    {
        // Remove active state from all tabs
        weaponsTabButton?.RemoveFromClassList("inventoryTab--active");
        armorTabButton?.RemoveFromClassList("inventoryTab--active");
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
        for (int i = 0; i < 10; i++) {
            content.Add(CreateInventorySlot(i + 1));
        }

        return content;
    }

    private VisualElement CreateInventorySlot(int number)
    {
        var slot = new VisualElement();
        slot.AddToClassList("inventorySlots");
        slot.style.width = slotSize;
        slot.style.height = slotSize;

        var label = new Label(number.ToString());
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.color = Color.white;
        slot.Add(label);

        return slot;
    }
}