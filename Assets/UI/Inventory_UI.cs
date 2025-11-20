using UnityEngine;
using UnityEngine.UIElements;

public class Inventory_UI : UIInterfaceSubComponent
{
    private VisualElement inventoryPanel;
    private bool isValid = false;
    private bool isOpen = false;

    public bool IsOpen => isOpen;
    public bool IsVisible => isOpen; // Map IsOpen to IsVisible for interface
    public bool IsValid => isValid;

    public Inventory_UI(VisualElement inventoryPanel)
    {
        this.inventoryPanel = inventoryPanel;

        if (inventoryPanel == null) {
            Debug.LogError("InventoryUI: Inventory panel is null!");
            isValid = false;
            return;
        }

        isValid = true;
        Hide(); // Start hidden by default
        SetupSlots();
    }

    public void Show()
    {
        if (!isValid) return;
        isOpen = true;
        inventoryPanel.style.display = DisplayStyle.Flex;
        Debug.Log("InventoryUI: Show");
    }

    public void Hide()
    {
        if (!isValid) return;
        isOpen = false;
        inventoryPanel.style.display = DisplayStyle.None;
        Debug.Log("InventoryUI: Hide");
    }

    private void SetupSlots()
    {
        // Inventory slot setup logic
        Debug.Log("InventoryUI: Slots setup complete");
    }

}