using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

/// Extension methods for UnityEngine.UIElements.UQueryBuilder
/// Provides LINQ-style query operations for UI element searches
/// UQueryBuilder is Unity's query system for finding UI elements
/// 
/// Example usage:
///   var slots = root.Query<Slot>().OrderBy(s => s.SlotIndex, Comparer<int>.Default);
///   var firstEmpty = root.Query<Slot>().FirstOrDefault();
///   var itemCount = root.Query<Slot>().CountWhere(s => s.HasItem);
public static class UQueryBuilderExtensions 
{
    /// Sorts UI elements in ascending order according to a key
    /// Similar to LINQ OrderBy but works with UQueryBuilder
    /// 
    /// Example:
    ///   var sortedSlots = root.Query<Slot>()
    ///                         .OrderBy(s => s.SlotIndex, Comparer<int>.Default);
    /// 
    /// query: The query builder containing elements to sort
    /// keySelector: Function to extract the sort key from each element
    /// comparer: The comparer to use for sorting
    /// Returns: Sorted collection of elements
    public static IEnumerable<T> OrderBy<T, TKey>(
        this UQueryBuilder<T> query, 
        Func<T, TKey> keySelector, 
        Comparer<TKey> comparer)
        where T : VisualElement
    {
        // Convert query to list and use LINQ OrderBy
        return query.ToList().OrderBy(keySelector, comparer);
    }

    /// Sorts UI elements by a numeric value in ascending order
    /// Convenient shorthand for sorting by numbers (int, float, etc.)
    /// 
    /// Example:
    ///   var sortedBySize = root.Query<VisualElement>()
    ///                          .SortByNumericValue(e => e.layout.width);
    /// 
    /// query: The query builder containing elements to sort
    /// keySelector: Function to extract a numeric value from each element
    /// Returns: Elements sorted by the numeric value
    public static IEnumerable<T> SortByNumericValue<T>(
        this UQueryBuilder<T> query, 
        Func<T, float> keySelector)
        where T : VisualElement
    {
        return query.OrderBy(keySelector, Comparer<float>.Default);
    }

    /// Returns the first element from the query, or null if no element is found
    /// Similar to LINQ FirstOrDefault but works with UQueryBuilder
    /// 
    /// Example:
    ///   var firstSlot = root.Query<Slot>().FirstOrDefault();
    ///   if (firstSlot != null) { /* use it */ }
    /// 
    /// query: The query builder to get the first element from
    /// Returns: First element or null if query is empty
    public static T FirstOrDefault<T>(this UQueryBuilder<T> query)
        where T : VisualElement
    {
        return query.ToList().FirstOrDefault();
    }

    /// Counts how many elements in the query satisfy a condition
    /// Similar to LINQ Count(predicate) but works with UQueryBuilder
    /// 
    /// Example:
    ///   var filledSlots = root.Query<Slot>().CountWhere(s => s.HasItem);
    ///   Debug.Log($"Found {filledSlots} slots with items");
    /// 
    /// query: The query builder containing elements to count
    /// predicate: Function to test each element (returns true to count it)
    /// Returns: Number of elements that satisfy the condition
    public static int CountWhere<T>(this UQueryBuilder<T> query, Func<T, bool> predicate) 
        where T : VisualElement 
    {
        return query.ToList().Count(predicate);
    }
}
