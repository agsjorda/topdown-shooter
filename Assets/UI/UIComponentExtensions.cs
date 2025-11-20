using UnityEngine;

public static class UIComponentExtensions
{
    public static void Toggle(this UIBaseComponent component)
    {
        if (component == null || !component.IsValid) {
            Debug.LogWarning("Attempted to toggle null or invalid component");
            return;
        }

        if (component.IsVisible)
            component.Hide();
        else
            component.Show();
    }
}