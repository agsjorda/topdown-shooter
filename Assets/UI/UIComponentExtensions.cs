using UnityEngine;

public static class UIComponentExtensions
{
    /// <summary>
    /// Toggles the visibility of any UI component that implements UIInterfaceSubComponent
    /// </summary>
    public static void Toggle(this UIInterfaceSubComponent component)
    {
        if (component == null || !component.IsValid) {
            Debug.LogWarning("UIComponentExtensions: Attempted to toggle null or invalid component");
            return;
        }

        if (component.IsVisible)
            component.Hide();
        else
            component.Show();
    }

    /// <summary>
    /// Toggles visibility and returns the new state
    /// </summary>
    public static bool ToggleWithFeedback(this UIInterfaceSubComponent component)
    {
        if (component == null || !component.IsValid) return false;

        if (component.IsVisible) {
            component.Hide();
            return false;
        } else {
            component.Show();
            return true;
        }
    }
}