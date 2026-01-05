using InventorySystem;

namespace InventorySystem
{
    /// <summary>
    /// Interface for equipment systems that manage equipped items.
    /// Implement this interface to create custom equipment behaviors.
    /// </summary>
    public interface IEquipmentSystem
    {
        /// <summary>
        /// Equips an item to the specified equipment slot.
        /// </summary>
        /// <param name="item">The inventory item to equip</param>
        /// <param name="slotType">The equipment slot type (Weapon, Headgear, Vest, Boots)</param>
        void EquipItem(InventoryItem item, EquipmentSlotType slotType);

        /// <summary>
        /// Unequips the item from the specified equipment slot.
        /// </summary>
        /// <param name="slotType">The equipment slot type to clear</param>
        void UnequipSlot(EquipmentSlotType slotType);

        /// <summary>
        /// Gets the currently equipped item in the specified slot.
        /// </summary>
        /// <param name="slotType">The equipment slot type to check</param>
        /// <returns>The equipped inventory item, or null if the slot is empty</returns>
        InventoryItem GetEquippedItem(EquipmentSlotType slotType);
    }
}
