using UnityEngine;
using UnityEngine.UIElements;

public class Inventory : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private int slotSize = 100;
    [SerializeField] private Sprite weaponIcon, armorIcon, miscIcon;

    private VisualElement weaponsTabButton, armorTabButton, miscTabButton;
    private VisualElement currentActiveTab;

    private void Start()
    {
        CreateInventoryInContainer();
    }

    private void CreateInventoryInContainer()
    {
        var root = uiDocument?.rootVisualElement;
        if (root == null) return;

        var inventoryContainer = root.Q<VisualElement>("inventory-container");
        if (inventoryContainer == null) return;

        inventoryContainer.Clear();

        // Create main layout
        var mainContainer = new VisualElement();
        mainContainer.style.flexDirection = FlexDirection.Column;
        mainContainer.style.justifyContent = Justify.Center;
        mainContainer.style.alignItems = Align.Center;
        mainContainer.style.flexGrow = 1;

        // Create tab buttons row
        var tabButtons = CreateTabButtons();
        mainContainer.Add(tabButtons);

        // Create tab content
        var tabContent = CreateTabContent();
        mainContainer.Add(tabContent);

        inventoryContainer.Add(mainContainer);

        // Set first tab as active by default
        SetActiveTab(weaponsTabButton);
    }

    private VisualElement CreateTabButtons()
    {
        var container = new VisualElement();
        container.name = "tab-buttons-container";
        container.style.flexDirection = FlexDirection.Row;
        container.style.justifyContent = Justify.Center;
        container.style.alignItems = Align.FlexEnd; // Align to bottom for border
        container.style.marginBottom = 10; // Reduced margin
        container.style.height = slotSize + 10; // Extra space for border

        // Create three tab buttons
        weaponsTabButton = CreateIconTabButton("weapons", weaponIcon);
        armorTabButton = CreateIconTabButton("armor", armorIcon);
        miscTabButton = CreateIconTabButton("misc", miscIcon);

        container.Add(weaponsTabButton);
        container.Add(armorTabButton);
        container.Add(miscTabButton);

        return container;
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
        content.AddToClassList("inventoryBox");

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