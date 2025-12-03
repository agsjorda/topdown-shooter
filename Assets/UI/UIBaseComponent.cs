using UnityEngine;
using UnityEngine.UIElements;

public abstract class UIBaseComponent
{
    protected VisualElement rootElement;
    protected bool isVisible = true;
    public bool IsValid => rootElement != null;
    public bool IsVisible => isVisible;

    protected UIBaseComponent(VisualElement root)
    {
        this.rootElement = root;
        if (!IsValid) {
            Debug.LogWarning($"{GetType().Name}: Root element is null");
            return;
        }
    }

    public virtual void Show()
    {
        if (!IsValid) return;
        isVisible = true;
        rootElement.style.display = DisplayStyle.Flex;
    }

    public virtual void Hide()
    {
        if (!IsValid) return;
        isVisible = false;
        rootElement.style.display = DisplayStyle.None;
    }

    public void Toggle()
    {
        if (!IsValid) {
            Debug.LogWarning("Attempted to toggle null or invalid component");
            return;
        }
        if (IsVisible)
            Hide();
        else
            Show();
    }

    // Common helper methods
    protected void SetElementVisibility(VisualElement element, bool visible)
    {
        if (element != null) {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    protected void SetElementWidthPercent(VisualElement element, float percent)
    {
        if (element != null) {
            percent = Mathf.Clamp01(percent);
            element.style.width = new Length(percent * 100f, LengthUnit.Percent);
        }
    }

    protected VisualElement FindElement(string elementName)
    {
        return rootElement?.Q<VisualElement>(elementName);
    }

    protected T FindElement<T>(string elementName) where T : VisualElement
    {
        return rootElement?.Q<T>(elementName);
    }
}