using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameHUD_UI
{
    private VisualElement rootElement;
    private HealthBar_UI healthBar;
    private bool isValid = false;
    private List<UIInterfaceSubComponent> uiComponents = new List<UIInterfaceSubComponent>();

    public GameHUD_UI(VisualElement root,
                   string healthBarFillName = "healthBar_fill",
                   string miniMapElementName = "mini-map")
    {
        this.rootElement = root;

        if (root == null) {
            Debug.LogError("GameHUD: Root element is null!");
            isValid = false;
            return;
        }

        // Initialize health bar
        healthBar = new HealthBar_UI(root, healthBarFillName);
        if (healthBar != null && healthBar.IsValid) {
            uiComponents.Add(healthBar);
            isValid = true; // only valid when a required subcomponent is valid
        } else {
            isValid = false;
            Debug.LogWarning("GameHUD: healthBar not found — HUD not marked valid");
            return;
        }

        Debug.Log("GameHUD: Initialized successfully");
    }

    public void SetHealthPercent(float percent)
    {
        if (!isValid) return;
        healthBar?.SetHealthPercent(percent);
    }

    public void SetHealth(float current, float max)
    {
        if (!isValid) return;
        healthBar?.SetHealthPercent(current / max);
    }

    // Global visibility control
    public void Show() => SetVisibility(true);
    public void Hide() => SetVisibility(false);

    private void SetVisibility(bool visible)
    {
        if (isValid && rootElement != null) {
            rootElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    // Individual component visibility
    public void ShowHealthBar() => healthBar?.Show();
    public void HideHealthBar() => healthBar?.Hide();

    public bool IsValid => isValid;
}