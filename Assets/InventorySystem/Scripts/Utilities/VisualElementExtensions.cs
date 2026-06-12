using System;
using UnityEngine.UIElements;

namespace InventorySystem
{
    /// Extension methods for UnityEngine.UIElements.VisualElement
    /// Provides fluent API for creating, styling, and manipulating UI elements
    /// 
    /// Example usage:
    ///   var element = parent.CreateChild("my-class")
    ///                       .WithName("myElement")
    ///                       .WithSize(100, 50);
    public static class VisualElementExtensions 
    {
        #region Creation & Hierarchy

        /// Creates a new VisualElement as a child of the parent, with optional CSS classes
        /// parent: The parent element to add the child to
        /// classes: CSS class names to apply to the new element
        /// Returns: The newly created child element
        public static VisualElement CreateChild(this VisualElement parent, params string[] classes) 
        {
            var child = new VisualElement();
            child.AddClass(classes).AddTo(parent);
            return child;
        }

        /// Creates a new typed VisualElement as a child of the parent, with optional CSS classes
        /// T: The type of VisualElement to create (e.g., Button, Label, Slot)
        /// parent: The parent element to add the child to
        /// classes: CSS class names to apply to the new element
        /// Returns: The newly created typed child element
        public static T CreateChild<T>(this VisualElement parent, params string[] classes) where T : VisualElement, new() 
        {
            var child = new T();
            child.AddClass(classes).AddTo(parent);
            return child;
        }

        /// Adds this element to a parent element (fluent API)
        /// child: The element to add
        /// parent: The parent to add it to
        /// Returns: The child element for method chaining
        public static T AddTo<T>(this T child, VisualElement parent) where T : VisualElement 
        {
            parent.Add(child);
            return child;
        }

        #endregion

        #region CSS Class Management

        /// Adds one or more CSS classes to the element
        /// Multiple classes can be added in one call: element.AddClass("class1", "class2")
        /// visualElement: The element to add classes to
        /// classes: CSS class names to add
        /// Returns: The element for method chaining
        public static T AddClass<T>(this T visualElement, params string[] classes) where T : VisualElement 
        {
            foreach (string cls in classes) {
                if (!string.IsNullOrEmpty(cls)) {
                    visualElement.AddToClassList(cls);
                }
            }
            return visualElement;
        }

        /// Removes one or more CSS classes from the element
        /// Multiple classes can be removed in one call: element.RemoveClass("class1", "class2")
        /// visualElement: The element to remove classes from
        /// classes: CSS class names to remove
        /// Returns: The element for method chaining
        public static T RemoveClass<T>(this T visualElement, params string[] classes) where T : VisualElement 
        {
            foreach (string cls in classes) {
                if (!string.IsNullOrEmpty(cls)) {
                    visualElement.RemoveFromClassList(cls);
                }
            }
            return visualElement;
        }

        /// Conditionally adds one of two classes based on a boolean condition
        /// Example: element.AddClassIf(isValid, "valid", "invalid")
        /// visualElement: The element to add class to
        /// condition: If true, adds trueClass; otherwise adds falseClass
        /// trueClass: Class to add when condition is true
        /// falseClass: Class to add when condition is false (optional)
        /// Returns: The element for method chaining
        public static T AddClassIf<T>(this T visualElement, bool condition, string trueClass, string falseClass = null) 
            where T : VisualElement 
        {
            string classToAdd = condition ? trueClass : falseClass;
            if (!string.IsNullOrEmpty(classToAdd)) {
                visualElement.AddToClassList(classToAdd);
            }
            return visualElement;
        }

        #endregion

        #region Property Setters (Fluent API)

        /// Sets the name property of the element (fluent API)
        /// Example: element.WithName("myElement")
        /// visualElement: The element to name
        /// name: The name to assign
        /// Returns: The element for method chaining
        public static T WithName<T>(this T visualElement, string name) where T : VisualElement 
        {
            visualElement.name = name;
            return visualElement;
        }

        /// Sets the picking mode (whether element receives pointer events)
        /// Example: element.WithPickingMode(PickingMode.Ignore) for ghost images
        /// visualElement: The element to configure
        /// mode: The picking mode (Position = receives events, Ignore = transparent to events)
        /// Returns: The element for method chaining
        public static T WithPickingMode<T>(this T visualElement, PickingMode mode) where T : VisualElement 
        {
            visualElement.pickingMode = mode;
            return visualElement;
        }

        /// Adds a manipulator to the element (e.g., for drag/drop, click handling)
        /// Example: element.WithManipulator(new Clickable(() => Debug.Log("Clicked!")))
        /// visualElement: The element to add manipulator to
        /// manipulator: The manipulator to add
        /// Returns: The element for method chaining
        public static T WithManipulator<T>(this T visualElement, IManipulator manipulator) where T : VisualElement 
        {
            visualElement.AddManipulator(manipulator);
            return visualElement;
        }

        #endregion

        #region Style Setters

        /// Sets both width and height of the element
        /// Example: element.WithSize(100, 50)
        /// visualElement: The element to size
        /// width: Width in pixels
        /// height: Height in pixels
        /// Returns: The element for method chaining
        public static T WithSize<T>(this T visualElement, float width, float height) where T : VisualElement 
        {
            visualElement.style.width = width;
            visualElement.style.height = height;
            return visualElement;
        }

        /// Sets the position style to Absolute (for manual positioning)
        /// Use this before setting left/top/right/bottom positions
        /// Example: element.WithAbsolutePosition()
        /// visualElement: The element to make absolutely positioned
        /// Returns: The element for method chaining
        public static T WithAbsolutePosition<T>(this T visualElement) where T : VisualElement 
        {
            visualElement.style.position = Position.Absolute;
            return visualElement;
        }

        /// Sets the background image of the element
        /// Example: element.WithBackgroundImage(myTexture)
        /// visualElement: The element to set background on
        /// texture: The texture to use as background (null to clear)
        /// Returns: The element for method chaining
        public static T WithBackgroundImage<T>(this T visualElement, UnityEngine.Texture2D texture) where T : VisualElement 
        {
            visualElement.style.backgroundImage = texture != null 
                ? new StyleBackground(texture) 
                : new StyleBackground();
            return visualElement;
        }

        /// Configures multiple style properties via an action
        /// Example: element.WithStyle(s => { s.width = 100; s.height = 50; })
        /// visualElement: The element to style
        /// configureStyle: Action that receives the style object
        /// Returns: The element for method chaining
        public static T WithStyle<T>(this T visualElement, Action<IStyle> configureStyle) where T : VisualElement 
        {
            configureStyle?.Invoke(visualElement.style);
            return visualElement;
        }

        /// Shows or hides the element by setting display style
        /// Example: element.SetVisible(false) to hide
        /// visualElement: The element to show/hide
        /// visible: True to show (display: flex), false to hide (display: none)
        /// Returns: The element for method chaining
        public static T SetVisible<T>(this T visualElement, bool visible) where T : VisualElement 
        {
            visualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            return visualElement;
        }

        #endregion
    }
}
