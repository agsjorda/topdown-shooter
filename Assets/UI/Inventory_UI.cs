using UnityEngine;
using UnityEngine.UIElements;

public class Inventory_UI : UIBaseComponent
{
    public bool IsOpen => IsVisible;

    public Inventory_UI(VisualElement root) : base(root)
    {
        if (!IsValid) return;
        SetupSlots();
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

    private void SetupSlots()
    {
        // Inventory setup logic
        Debug.Log("Inventory slots initialized");
    }
}