using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameHUD_UI : UIBaseComponent
{
    private HealthBar_UI healthBar;
    private List<UIBaseComponent> childComponents = new List<UIBaseComponent>();

    public GameHUD_UI(VisualElement root) : base(root)
    {
        if (!IsValid) return;

        // Initialize child components
        healthBar = new HealthBar_UI(root);

        if (healthBar.IsValid) {
            childComponents.Add(healthBar);
        } else {
            Debug.LogError("GameHUD: Failed to initialize health bar");
            return;
        }
    }

    public void SetHealthPercent(float percent)
    {
        healthBar?.SetHealthPercent(percent);
    }

    public void SetHealth(float current, float max)
    {
        healthBar?.SetHealth(current, max);
    }

    public override void Show()
    {
        base.Show();
        childComponents.ForEach(component => component.Show());
    }

    public override void Hide()
    {
        base.Hide();
        childComponents.ForEach(component => component.Hide());
    }

    // Individual component control
    public void ShowHealthBar() => healthBar?.Show();
    public void HideHealthBar() => healthBar?.Hide();
}