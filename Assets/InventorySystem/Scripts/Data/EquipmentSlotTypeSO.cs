using UnityEngine;

namespace InventorySystem
{
    /// <summary>
    /// Defines an equipment slot type as an authorable asset (weapon, headgear, ring, ...).
    /// Items declare which slot types they fit via Item_DataSO.compatibleSlots — slots
    /// themselves carry no accepted-item lists, keeping a single source of truth.
    /// </summary>
    [CreateAssetMenu(fileName = "SlotType_", menuName = "Inventory System/Equipment Slot Type")]
    public class EquipmentSlotTypeSO : ScriptableObject
    {
        [Tooltip("Stable identifier, e.g. \"weapon\", \"headgear\"")]
        public string id;

        [Tooltip("Human-readable name for UI/tooling")]
        public string displayName;

        [Tooltip("EquipmentSlotView adds the USS class \"equipment-slot--{suffix}\"; falls back to the id when empty")]
        public string ussClassSuffix;

        public override string ToString() => string.IsNullOrEmpty(id) ? name : id;
    }
}
