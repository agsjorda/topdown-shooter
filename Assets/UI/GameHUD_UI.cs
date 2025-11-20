using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameHUD_UI : UIInterfaceSubComponent
{
    private VisualElement rootElement;
    private HealthBar_UI healthBar;
    private bool isValid = false;
    private bool isVisible = true;
    private List<UIInterfaceSubComponent> uiComponents = new List<UIInterfaceSubComponent>();

    public bool IsVisible => isVisible;

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
            isValid = true;
        } else {
            isValid = false;
            Debug.LogWarning("GameHUD: healthBar not found — HUD not marked valid");
            return;
        }

        // Initialize other components as needed
        // var miniMap = new MiniMap_UI(root, miniMapElementName);
        // if (miniMap != null && miniMap.IsValid) uiComponents.Add(miniMap);

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

    public void Show()
    {
        if (!isValid || rootElement == null) return;

        isVisible = true;
        rootElement.style.display = DisplayStyle.Flex;

        // Show all child components
        foreach (var component in uiComponents) {
            component.Show();
        }
    }

    public void Hide()
    {
        if (!isValid || rootElement == null) return;

        isVisible = false;
        rootElement.style.display = DisplayStyle.None;

        // Hide all child components
        foreach (var component in uiComponents) {
            component.Hide();
        }
    }

    // Individual component visibility
    public void ShowHealthBar() => healthBar?.Show();
    public void HideHealthBar() => healthBar?.Hide();

    public bool IsValid => isValid;
}