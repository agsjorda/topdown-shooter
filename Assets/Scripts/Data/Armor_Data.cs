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
    
    [Header("Visual Settings")]
    [Tooltip("3D model prefab for equipped appearance")]
    public GameObject armorModelPrefab;
    
    public Color armorTintColor = Color.white;

    /// <summary>
    /// Creates a runtime instance with current durability tracking
    /// </summary>
    public ArmorInstance CreateInstance()
    {
        return new ArmorInstance(this);
    }

    /// <summary>
    /// Calculates actual damage after armor reduction
    /// </summary>
    public float CalculateReducedDamage(float incomingDamage, int currentDurability)
    {
        if (currentDurability <= 0)
            return incomingDamage; // No protection if broken
            
        float reduction = damageReduction / 100f;
        float reducedDamage = incomingDamage * (1f - reduction);
        
        return Mathf.Max(0, reducedDamage);
    }
}

/// <summary>
/// Runtime instance of armor that tracks durability and state.
/// IMPORTANT: This is NOT a ScriptableObject - it's a regular class that holds runtime data.
/// This prevents Unity from serializing changes and triggering Burst recompilation.
/// </summary>
[System.Serializable]
public class ArmorInstance
{
    public Armor_Data armorData;
    public int currentDurability;

    public ArmorInstance(Armor_Data data)
    {
        armorData = data;
        currentDurability = data.maxDurability;
    }

    /// <summary>
    /// Calculates actual damage after armor reduction
    /// </summary>
    public float CalculateReducedDamage(float incomingDamage)
    {
        return armorData.CalculateReducedDamage(incomingDamage, currentDurability);
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
            Debug.Log($"{armorData.itemName} has been destroyed!");
        }
    }
    
    /// <summary>
    /// Repairs the armor to full durability
    /// </summary>
    public void RepairArmor()
    {
        currentDurability = armorData.maxDurability;
    }
    
    /// <summary>
    /// Gets the durability percentage (0-100)
    /// </summary>
    public float GetDurabilityPercentage()
    {
        return (float)currentDurability / armorData.maxDurability * 100f;
    }

    /// <summary>
    /// Check if armor is still functional
    /// </summary>
    public bool IsBroken => currentDurability <= 0;
}
