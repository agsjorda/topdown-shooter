using UnityEngine;
using UnityEngine.UIElements;

public class Inventory_UI : UIBaseComponent
{
    private const string INVENTORY_TAB_ACTIVE_CLASS = "inventoryTab--active";
    private const string INVENTORY_SLOTS_CLASS = "inventory-slots-container";

    private Inventory_Base inventoryBase;

    private VisualElement[] tabButtons;
    private InventoryScrollElement scrollWrapper;
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

        for (int i = 0; i < tabButtons.Length; i++) {
            int index = i;
            tabButtons[i]?.RegisterCallback<ClickEvent>(_ => SetActiveTab(tabButtons[index]));
        }
    }

    // Ensure InventoryScrollElement lives inside tabContentContainer (no extra wrappers)
    private void InitializeScrollView(VisualElement tabContentContainer)
    {
        // Find existing wrapper under tabContentContainer (do not create here)
        var wrapper = tabContentContainer.Q<InventoryScrollElement>();
        if (wrapper != null) {
            scrollWrapper = wrapper;
            slotsContainer = scrollWrapper.SlotsContainer;
        } else {
            // No wrapper yet. If no config is present, use a plain container fallback.
            var config = Object.FindFirstObjectByType<InventoryUIConfig>();
            if (config == null) {
                slotsContainer = tabContentContainer.Q<VisualElement>(className: INVENTORY_SLOTS_CLASS);
                if (slotsContainer == null) {
                    slotsContainer = new VisualElement();
                    slotsContainer.AddToClassList(INVENTORY_SLOTS_CLASS);
                    tabContentContainer.Add(slotsContainer);
                }
                slotsContainer.Clear();
                CreateInventorySlots_Default(); // only when no config
            }
        }
    }

    private void CreateInventorySlots_Default()
    {
        for (int i = 0; i < numberOfSlots; i++)
            slotsContainer.Add(CreateSlot(i + 1, slotSize, cellMargin, null));
    }

    private VisualElement CreateSlot(int slotNumber, int size, float margin, InventoryUIConfig config)
    {
        var slot = new InventorySlotElement();
        slot.SetIndex(slotNumber);
        slot.SetSlotSize(size);
        slot.SetCellMargin(margin);

        if (config != null) {
            // Removed preset border classes: rely on USS default hover rule

            if (config.useCustomBorderColor) {
                slot.SetStaticBorderColor(config.slotBorderColor);
            }

            slot.ApplyVisuals(
                config.slotUseBackground,
                config.slotBackgroundTexture,
                config.slotBackgroundSprite,
                config.slotBackgroundTint,
                config.slotBorderRadius
            );
        }

        return slot;
    }

    private void SetActiveTab(VisualElement activeTab)
    {
        if (activeTab == null) return;
        foreach (var tab in tabButtons)
            tab?.RemoveFromClassList(INVENTORY_TAB_ACTIVE_CLASS);
        activeTab.AddToClassList(INVENTORY_TAB_ACTIVE_CLASS);
    }

    // Only update slots when no InventoryUIConfig is managing the content.
    public void UpdateSlots(int newSlotCount)
    {
        numberOfSlots = Mathf.Max(0, newSlotCount);
        if (slotsContainer == null) return;

        slotsContainer.Clear();
        var config = Object.FindFirstObjectByType<InventoryUIConfig>();
        if (config != null) {
            int count = config.slotCount > 0 ? config.slotCount : numberOfSlots;
            int size = config.slotSize > 0 ? config.slotSize : slotSize;
            float margin = config.cellMargin > 0 ? config.cellMargin : cellMargin;

            for (int i = 0; i < count; i++)
                slotsContainer.Add(CreateSlot(i + 1, size, margin, config));
        } else {
            CreateInventorySlots_Default();
        }
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
