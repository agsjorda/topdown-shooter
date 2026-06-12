using UnityEngine;

namespace InventorySystem
{
    /// Extension methods for UnityEngine.Rect
    /// Provides utilities for working with rectangles, especially for UI bounds calculations
    /// 
    /// Example usage:
    ///   var paddedBounds = slot.worldBound.Padded(20);
    ///   if (paddedBounds.Contains(mousePosition)) { /* ... */ }
    public static class RectExtensions 
    {
        /// Returns a new Rect with equal padding on all sides
        /// Useful for creating hit areas larger than the visual bounds
        /// 
        /// Example: If rect is (10, 10, 50, 50) and padding is 5:
        ///   Result is (5, 5, 60, 60) - expanded by 5 pixels on all sides
        /// 
        /// rect: The original rectangle
        /// padding: Amount to expand on all sides (in pixels)
        /// Returns: A new Rect expanded by the padding amount
        public static Rect Padded(this Rect rect, float padding) 
        {
            return new Rect(
                rect.x - padding,           // Move left edge left
                rect.y - padding,           // Move top edge up
                rect.width + padding * 2,   // Increase width by padding on both sides
                rect.height + padding * 2   // Increase height by padding on both sides
            );
        }

        /// Returns a new Rect with different padding per side
        /// Allows precise control over expansion on each edge
        /// 
        /// Example: Expand more on right/bottom for asymmetric hit areas
        /// 
        /// rect: The original rectangle
        /// left: Amount to expand left edge
        /// top: Amount to expand top edge
        /// right: Amount to expand right edge
        /// bottom: Amount to expand bottom edge
        /// Returns: A new Rect with specified padding on each side
        public static Rect Padded(this Rect rect, float left, float top, float right, float bottom) 
        {
            return new Rect(
                rect.x - left,                  // Move left edge
                rect.y - top,                   // Move top edge
                rect.width + left + right,      // Increase width by both sides
                rect.height + top + bottom      // Increase height by both sides
            );
        }

        /// Shrinks the rect by the specified amount on all sides (negative padding)
        /// Useful for creating inner bounds or margins
        /// 
        /// Example: Create a 10-pixel margin inside a rect
        ///   var innerRect = outerRect.Shrunk(10);
        /// 
        /// rect: The original rectangle
        /// amount: Amount to shrink on all sides (positive number)
        /// Returns: A new Rect shrunk by the amount
        public static Rect Shrunk(this Rect rect, float amount) 
        {
            return rect.Padded(-amount);
        }

        /// Returns the center point of the rectangle
        /// Convenient shorthand for rect.center (but returns Vector2)
        /// 
        /// rect: The rectangle
        /// Returns: Center point as Vector2
        public static Vector2 GetCenter(this Rect rect) 
        {
            return rect.center;
        }
    }
}
