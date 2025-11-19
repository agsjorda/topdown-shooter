
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class HealthBar_UI : UIInterfaceSubComponent
{
    private VisualElement healthBarFill;
    private bool isValid = false;

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
        Debug.Log("HealthBar_UI: Initialized successfully");
    }

    public void SetHealthPercent(float percent)
    {
        // defensive: check validity and presence of element
        if (!isValid || healthBarFill == null) {
            Debug.LogWarning("HealthBar_UI: Attempted to set health but health bar is not valid or not found");
            return;
        }

        percent = Mathf.Clamp01(percent);
        healthBarFill.style.width = new Length(percent * 100f, LengthUnit.Percent);
        UpdateHealthVisuals(percent);
    }

    private void UpdateHealthVisuals(float percent)
    {
        // Change color based on health percentage
        Color healthColor = percent switch {
            > 0.6f => Color.green,
            > 0.3f => Color.yellow,
            _ => Color.red
        };

        healthBarFill.style.backgroundColor = new StyleColor(healthColor);
    }

    public void Show() => SetVisibility(true);
    public void Hide() => SetVisibility(false);

    private void SetVisibility(bool visible)
    {
        if (isValid && healthBarFill != null) {
            healthBarFill.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    public bool IsValid => isValid;
}