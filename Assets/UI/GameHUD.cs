using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class GameHUD
{
    private VisualElement root;
    private VisualElement healthBarFill;

    public GameHUD(VisualElement root)
    {
        this.root = root;
        healthBarFill = root.Q<VisualElement>("healthBar_fill");
    }

    public void SetHealthPercent(float percent)
    {
        if (healthBarFill == null) return;

        percent = Mathf.Clamp01(percent);

        // Apply as percentage width
        healthBarFill.style.width = new Length(percent * 100f, LengthUnit.Percent);
    }
}
