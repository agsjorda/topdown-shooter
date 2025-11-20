using UnityEngine;
using UnityEngine.UIElements;

public class HealthBar_UI : UIBaseComponent
{
    private VisualElement healthBarFill;

    public HealthBar_UI(VisualElement root, string healthBarFillName = "healthBar_fill") : base(root)
    {
        if (!IsValid) return;

        healthBarFill = FindElement(healthBarFillName);

        if (healthBarFill == null) {
            Debug.LogError($"HealthBar_UI: Could not find '{healthBarFillName}'");
            return;
        }
    }

    public void SetHealthPercent(float percent)
    {
        if (!IsValid || healthBarFill == null) return;

        percent = Mathf.Clamp01(percent);
        SetElementWidthPercent(healthBarFill, percent);
        UpdateHealthColor(percent);
    }

    public void SetHealth(float current, float max)
    {
        SetHealthPercent(current / max);
    }

    private void UpdateHealthColor(float percent)
    {
        Color healthColor = percent switch {
            > 0.6f => Color.green,
            > 0.3f => Color.yellow,
            _ => Color.red
        };

        healthBarFill.style.backgroundColor = new StyleColor(healthColor);
    }
}