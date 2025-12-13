using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public static class Helpers 
{
    /// Creates a deterministic GUID from a string
    /// Same input string always produces the same GUID (uses MD5 hashing)
    /// Useful for creating consistent unique identifiers from names/paths
    /// 
    /// Example:
    ///   var guid = Helpers.CreateGuidFromString("PlayerInventory");
    ///   // Always produces the same GUID for "PlayerInventory"
    /// 
    /// input: The string to convert to a GUID
    /// Returns: A GUID generated from the string
    public static Guid CreateGuidFromString(string input) 
    {
        // Use MD5 to create a 128-bit hash from the string
        // Convert the hash bytes to a GUID
        return new Guid(MD5.Create().ComputeHash(Encoding.Default.GetBytes(input)));
    }
    
    /// Clamps a UI element's position to stay within screen bounds
    /// Prevents elements from being positioned off-screen
    /// Takes element size into account
    /// 
    /// Example:
    ///   var safePos = Helpers.ClampToScreen(tooltip, mousePosition);
    ///   tooltip.style.left = safePos.x;
    ///   tooltip.style.top = safePos.y;
    /// 
    /// element: The UI element to position
    /// targetPosition: The desired position
    /// Returns: A clamped position that keeps the element on-screen
    public static Vector2 ClampToScreen(VisualElement element, Vector2 targetPosition) 
    {
        // Clamp X to keep element within horizontal screen bounds
        float x = Mathf.Clamp(targetPosition.x, 0, Screen.width - element.layout.width);
        
        // Clamp Y to keep element within vertical screen bounds
        float y = Mathf.Clamp(targetPosition.y, 0, Screen.height - element.layout.height);

        return new Vector2(x, y);
    }
}
