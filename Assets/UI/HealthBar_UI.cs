using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class HealthBar_UI : UIInterfaceSubComponent
{
    private VisualElement healthBarFill;
    private bool isValid = false;
    private bool isVisible = true;

    public bool IsVisible => isVisible;

    public HealthBar_UI(VisualElement root, string healthBarFillName = "healthBar_fill")
    {
        if (root == null) {
            Debug.LogError("HealthBar_UI: Root element is null!");
            return;
        }

        healthBarFill = root.Q<VisualElement>(healthBarFillName);
        if (healthBarFill == null) {
            Debug.LogError($"HealthBar_UI: Could not find health bar fill with name '{healthBarFillName}'");
            return;
        }

        isValid = true;
        // Start visible by default
        Show();
    }

    public void SetHealthPercent(float percent)
    {
        if (!isValid || healthBarFill == null) {
#if UNITY_EDITOR
            Debug.LogWarning("HealthBar_UI: Attempted to set health but health bar is not valid");
#endif
            return;
        }

        percent = Mathf.Clamp01(percent);
        healthBarFill.style.width = new Length(percent * 100f, LengthUnit.Percent);
        UpdateHealthVisuals(percent);
    }

    private void UpdateHealthVisuals(float percent)
    {
        Color healthColor = percent switch {
            > 0.6f => Color.green,
            > 0.3f => Color.yellow,
            _ => Color.red
        };

        healthBarFill.style.backgroundColor = new StyleColor(healthColor);
    }

    public void Show()
    {
        if (!isValid || healthBarFill == null) return;
        isVisible = true;
        healthBarFill.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        if (!isValid || healthBarFill == null) return;
        isVisible = false;
        healthBarFill.style.display = DisplayStyle.None;
    }

    public bool IsValid => isValid;
}