using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace InventorySystem
{
    /// Extension methods for IEnumerable<T>
    /// Provides LINQ-style operations and batch operations for collections
    /// Especially useful for working with collections of UI elements
    /// 
    /// Example usage:
    ///   slots.Where(s => s.HasItem).ForEach(s => s.AddClass("highlight"));
    ///   emptySlots.AddClassToAll("empty-highlight");
    public static class EnumerableExtensions 
    {
        /// Performs an action on each element in the collection
        /// Similar to List<T>.ForEach but works on any IEnumerable
        /// 
        /// Example:
        ///   slots.ForEach(slot => slot.ClearItem());
        ///   items.ForEach(item => Debug.Log(item.name));
        /// 
        /// source: The collection to iterate
        /// action: Action to perform on each element
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) 
        {
            if (source == null) return;
            if (action == null) return;

            foreach (var item in source) {
                action(item);
            }
        }

        /// Adds a CSS class to all VisualElements in the collection
        /// Skips null elements automatically
        /// 
        /// Example:
        ///   emptySlots.AddClassToAll("empty-highlight");
        ///   inventorySlots.Where(s => !s.HasItem).AddClassToAll("available");
        /// 
        /// elements: Collection of VisualElements
        /// className: CSS class name to add to all elements
        public static void AddClassToAll<T>(this IEnumerable<T> elements, string className) 
            where T : VisualElement 
        {
            if (elements == null || string.IsNullOrEmpty(className)) return;

            foreach (var element in elements) {
                element?.AddToClassList(className);
            }
        }

        /// Removes a CSS class from all VisualElements in the collection
        /// Skips null elements automatically
        /// 
        /// Example:
        ///   allSlots.RemoveClassFromAll("highlight");
        ///   previouslyHighlighted.RemoveClassFromAll("selected");
        /// 
        /// elements: Collection of VisualElements
        /// className: CSS class name to remove from all elements
        public static void RemoveClassFromAll<T>(this IEnumerable<T> elements, string className) 
            where T : VisualElement 
        {
            if (elements == null || string.IsNullOrEmpty(className)) return;

            foreach (var element in elements) {
                element?.RemoveFromClassList(className);
            }
        }

        /// Adds multiple CSS classes to all VisualElements in the collection
        /// Convenient for applying multiple styles at once
        /// 
        /// Example:
        ///   selectedSlots.AddClassesToAll("selected", "highlight", "active");
        /// 
        /// elements: Collection of VisualElements
        /// classNames: CSS class names to add
        public static void AddClassesToAll<T>(this IEnumerable<T> elements, params string[] classNames) 
            where T : VisualElement 
        {
            if (elements == null || classNames == null) return;

            foreach (var element in elements) {
                if (element == null) continue;
                foreach (var className in classNames) {
                    if (!string.IsNullOrEmpty(className)) {
                        element.AddToClassList(className);
                    }
                }
            }
        }

        /// Removes multiple CSS classes from all VisualElements in the collection
        /// 
        /// Example:
        ///   allSlots.RemoveClassesFromAll("selected", "highlight", "active");
        /// 
        /// elements: Collection of VisualElements
        /// classNames: CSS class names to remove
        public static void RemoveClassesFromAll<T>(this IEnumerable<T> elements, params string[] classNames) 
            where T : VisualElement 
        {
            if (elements == null || classNames == null) return;

            foreach (var element in elements) {
                if (element == null) continue;
                foreach (var className in classNames) {
                    if (!string.IsNullOrEmpty(className)) {
                        element.RemoveFromClassList(className);
                    }
                }
            }
        }

        /// Returns true if the collection contains any elements (shorthand for Any())
        /// More readable than checking Count > 0
        /// 
        /// Example:
        ///   if (slots.HasAny()) { /* do something */ }
        ///   if (selectedItems.HasAny(i => i.IsEquipped)) { /* ... */ }
        /// 
        /// source: The collection to check
        /// Returns: True if collection has at least one element
        public static bool HasAny<T>(this IEnumerable<T> source) 
        {
            return source != null && source.Any();
        }

        /// Returns true if collection has any elements matching the predicate
        /// 
        /// source: The collection to check
        /// predicate: Condition to test
        /// Returns: True if any element matches the condition
        public static bool HasAny<T>(this IEnumerable<T> source, Func<T, bool> predicate) 
        {
            return source != null && source.Any(predicate);
        }

        /// Returns true if the collection is null or empty
        /// More readable than checking !collection.Any()
        /// 
        /// Example:
        ///   if (items.IsNullOrEmpty()) { Debug.Log("No items!"); }
        /// 
        /// source: The collection to check
        /// Returns: True if collection is null or has no elements
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source) 
        {
            return source == null || !source.Any();
        }
    }
}
