using UnityEngine;

/// Detects double-click events with configurable timing
///
/// For Junior Developers:
/// This class tracks when the last click happened and checks if a second click
/// occurs within a time window (default 0.3 seconds = 300ms).
/// 
/// Think of it like a stopwatch:
/// - First click: Start the stopwatch
/// - Second click: Check if stopwatch is under the time limit
/// - If yes = Double-click detected!
/// - If no = Reset and treat as first click
public class DoubleClickHandler
{
    private readonly float doubleClickTimeWindow;
    private float lastClickTime;
    private int lastClickedSlotIndex = -1;
    private EquipmentSlotType lastClickedEquipmentType;
    private bool wasEquipmentSlot;

    /// Create a new double-click handler
    /// timeWindow: Maximum time between clicks to count as double-click (in seconds)
    public DoubleClickHandler(float timeWindow = 0.3f)
    {
        doubleClickTimeWindow = timeWindow;
        Reset();
    }

    /// Call this when a slot is clicked. Returns true if it's a double-click.
    /// slotIndex: Index of the inventory slot clicked (-1 for equipment slots)
    /// isEquipmentSlot: True if clicking an equipment slot
    /// equipmentType: Type of equipment slot (only relevant if isEquipmentSlot is true)
    public bool RegisterClick(int slotIndex, bool isEquipmentSlot = false, EquipmentSlotType equipmentType = EquipmentSlotType.Weapon)
    {
        float currentTime = Time.unscaledTime; // Use unscaledTime so it works even if game is paused
        float timeSinceLastClick = currentTime - lastClickTime;

        // Check if this is a double-click
        bool isDoubleClick = timeSinceLastClick <= doubleClickTimeWindow &&
                            IsSameSlot(slotIndex, isEquipmentSlot, equipmentType);

        // Update tracking
        lastClickTime = currentTime;
        lastClickedSlotIndex = slotIndex;
        wasEquipmentSlot = isEquipmentSlot;
        lastClickedEquipmentType = equipmentType;

        return isDoubleClick;
    }

    /// Check if the clicked slot is the same as the last clicked slot
    private bool IsSameSlot(int slotIndex, bool isEquipmentSlot, EquipmentSlotType equipmentType)
    {
        // Equipment slots and inventory slots are different
        if (isEquipmentSlot != wasEquipmentSlot) return false;

        if (isEquipmentSlot) {
            // For equipment slots, compare the equipment type
            return equipmentType == lastClickedEquipmentType;
        } else {
            // For inventory slots, compare the slot index
            return slotIndex == lastClickedSlotIndex;
        }
    }

    /// Reset the double-click tracker (useful when UI closes/opens)
    public void Reset()
    {
        lastClickTime = -doubleClickTimeWindow * 2; // Set far in the past
        lastClickedSlotIndex = -1;
        wasEquipmentSlot = false;
        lastClickedEquipmentType = EquipmentSlotType.Weapon;
    }
}
