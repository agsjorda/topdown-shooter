using UnityEngine;
using UnityEngine.UIElements;

public class Inventory_UI : UIBaseComponent
{
    private const string INVENTORY_TAB_ACTIVE_CLASS = "inventoryTab--active";

    private VisualElement[] tabButtons;

    public Inventory_UI(VisualElement root) : base(root)
    {
        if (!IsValid) return;
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

    private void SetActiveTab(VisualElement activeTab)
    {
        if (activeTab == null) return;
        foreach (var tab in tabButtons)
            tab?.RemoveFromClassList(INVENTORY_TAB_ACTIVE_CLASS);
        activeTab.AddToClassList(INVENTORY_TAB_ACTIVE_CLASS);
    }

    public override void Show() => base.Show();
    public override void Hide() => base.Hide();
}
