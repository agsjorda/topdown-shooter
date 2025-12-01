using UnityEngine;
using UnityEngine.UIElements;

public class Inventory_UI : UIBaseComponent
{
    private const string INVENTORY_TAB_ACTIVE_CLASS = "inventoryTab--active";
    private const string INVENTORY_SLOTS_CLASS = "inventory-slots-container";

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

    private void InitializeScrollView(VisualElement parent)
    {
        parent.Clear();

        scrollView = new ScrollView {
            verticalScrollerVisibility = ScrollerVisibility.Hidden,
            horizontalScrollerVisibility = ScrollerVisibility.Hidden
        };
        scrollView.AddToClassList("inventory-scroll-view");

        // Create the container and add the class so InventoryUIConfig can find it by class/name
        slotsContainer = new VisualElement();
        slotsContainer.AddToClassList(INVENTORY_SLOTS_CLASS);

        // Add container to visual tree first
        scrollView.Add(slotsContainer);
        parent.Add(scrollView);

        // Populate using config if available, otherwise create defaults
        var config = Object.FindFirstObjectByType<InventoryUIConfig>();
        if (config != null) {
            // Ensure config has a UIDocument reference that points to the same root if possible
            if (config.targetDocument == null) {
                var docs = Object.FindObjectsOfType<UIDocument>();
                foreach (var d in docs) {
                    if (d.rootVisualElement == rootElement) {
                        config.targetDocument = d;
                        break;
                    }
                }
                if (config.targetDocument == null && docs.Length > 0)
                    config.targetDocument = docs[0]; // best-effort fallback
            }

            // Let config populate the container. Config will query the document root for the container class/name.
            config.RefreshSlotsInDocument();
        } else {
            CreateInventorySlots_Default();
        }
    }

    // fallback slot creation used when no InventoryUIConfig is present
    private void CreateInventorySlots_Default()
    {
        int count = numberOfSlots;
        int size = slotSize;
        float margin = cellMargin;

        // create and add slots directly; CreateSlot keeps logic isolated and testable
        for (int i = 0; i < count; i++) {
            slotsContainer.Add(CreateSlot(i + 1, size, margin, null));
        }
    }

    private VisualElement CreateSlot(int slotNumber, int size, float margin, InventoryUIConfig config)
    {
        var slot = new InventorySlotElement();
        slot.SetIndex(slotNumber);
        slot.SetSlotSize(size);
        slot.SetCellMargin(margin);

        if (config != null) {
            // apply single preset class (USS defines color/hover) and non-border visuals
            switch (config.borderPreset) {
                case InventoryUIConfig.BorderPreset.Yellow: slot.AddToClassList("border-yellow"); break;
                case InventoryUIConfig.BorderPreset.Red: slot.AddToClassList("border-red"); break;
                case InventoryUIConfig.BorderPreset.Green: slot.AddToClassList("border-green"); break;
                default: break;
            }

            slot.ApplyVisuals(
                config.useBackground,
                config.backgroundTexture,
                config.backgroundSprite,
                config.backgroundTint,
                config.borderRadius
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

    public void UpdateSlots(int newSlotCount)
    {
        numberOfSlots = Mathf.Max(0, newSlotCount);

        // reuse container but recreate children (simple, predictable)
        slotsContainer?.Clear();

        // if a config exists, prefer it to populate so visual presets are respected
        var config = Object.FindFirstObjectByType<InventoryUIConfig>();
        if (config != null) {
            config.RefreshSlotsInDocument();
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
