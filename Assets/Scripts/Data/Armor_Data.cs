using UnityEngine;
using InventorySystem;

[CreateAssetMenu(fileName = "New Armor Data", menuName = "Armor System/Armor Data")]
public class Armor_Data : Item_DataSO
{
    // Which slot this armor fits is declared via the inherited compatibleSlots list
    // (e.g. a hat lists SlotType_Headgear), not a dedicated armor-type field.
    
    [Header("Protection Stats")]
    [Tooltip("Damage reduction percentage (0-100)")]
    [Range(0, 100)]
    public float damageReduction = 0f;
    
    [Tooltip("Additional health points provided by this armor")]
    public int bonusHealth = 0;
    
    [Header("Armor Durability")]
    [Tooltip("Maximum durability of this armor piece")]
    public int maxDurability = 100;
    
    [Header("Visual Settings")]
    [Tooltip("3D model prefab for equipped appearance")]
    public GameObject armorModelPrefab;
    
    public Color armorTintColor = Color.white;
}
