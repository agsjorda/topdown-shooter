using UnityEngine;

[CreateAssetMenu(fileName = "New Armor Data", menuName = "Armor System/Armor Data")]
public class Armor_Data : Item_DataSO
{
    [Header("Armor Details")]
    [Tooltip("Determines which equipment slot this armor can be equipped in")]
    public ArmorType armorType;
    
    [Header("Protection Stats")]
    [Tooltip("Damage reduction percentage (0-100)")]
    [Range(0, 100)]
    public float damageReduction = 0f;
    
    [Tooltip("Additional health points provided by this armor")]
    public int bonusHealth = 0;
    
    [Header("Armor Durability")]
    [Tooltip("Maximum durability of this armor piece")]
    public int maxDurability = 100;
    
    [Tooltip("Current durability (set at runtime)")]
    public int currentDurability = 100;
    
    [Header("Visual Settings")]
    [Tooltip("3D model prefab for equipped appearance")]
    public GameObject armorModelPrefab;
    
    public Color armorTintColor = Color.white;

    /// <summary>
    /// Calculates actual damage after armor reduction
    /// </summary>
    public float CalculateReducedDamage(float incomingDamage)
    {
        if (currentDurability <= 0)
            return incomingDamage; // No protection if broken
            
        float reduction = damageReduction / 100f;
        float reducedDamage = incomingDamage * (1f - reduction);
        
        return Mathf.Max(0, reducedDamage);
    }
    
    /// <summary>
    /// Reduces armor durability when taking damage
    /// </summary>
    public void TakeDurabilityDamage(float damageAmount)
    {
        if (currentDurability <= 0) return;
        
        int durabilityLoss = Mathf.CeilToInt(damageAmount * 0.1f); // 10% of damage affects durability
        currentDurability = Mathf.Max(0, currentDurability - durabilityLoss);
        
        if (currentDurability <= 0)
        {
            Debug.Log($"{itemName} has been destroyed!");
        }
    }
    
    /// <summary>
    /// Repairs the armor to full durability
    /// </summary>
    public void RepairArmor()
    {
        currentDurability = maxDurability;
    }
    
    /// <summary>
    /// Gets the durability percentage (0-100)
    /// </summary>
    public float GetDurabilityPercentage()
    {
        return (float)currentDurability / maxDurability * 100f;
    }
}
