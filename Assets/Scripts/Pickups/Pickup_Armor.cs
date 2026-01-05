using UnityEngine;
using InventorySystem;

/// <summary>
/// Handles armor pickups that can be added to the player's inventory.
/// Uses InventoryService for centralized inventory access.
/// </summary>
public class Pickup_Armor : Interactable
{
    [Header("Armor Data")]
    [SerializeField] private Armor_Data armorData;
    [SerializeField] private GameObject armorVisual;

    private InventoryItem armorItem;
    private Material defaultMaterial;

    protected override void Awake()
    {
        base.Awake();

        if (armorData != null && armorItem == null)
            armorItem = new InventoryItem(armorData);

        SetupVisuals();
    }

    protected override void InitializeComponents()
    {
        base.InitializeComponents();

        // Auto-populate armor visual if not set
        if (armorVisual == null && transform.childCount > 0)
            armorVisual = transform.GetChild(0).gameObject;
    }

    #region Setup Methods
    /// <summary>
    /// Sets up the armor pickup with specific armor data and position.
    /// </summary>
    public void SetupPickupArmor(Armor_Data armor, Vector3 position)
    {
        armorData = armor;
        armorItem = new InventoryItem(armorData);
        transform.position = position;

        SetupVisuals();
    }

    /// <summary>
    /// Sets up the armor pickup from an existing inventory item.
    /// </summary>
    public void SetupPickupArmorFromItem(InventoryItem item, Vector3 position)
    {
        if (item == null || item.itemData == null) return;

        armorData = item.itemData as Armor_Data;
        if (armorData == null) {
            Debug.LogWarning($"Pickup_Armor: Item {item.itemData.itemName} is not armor data!");
            return;
        }

        armorItem = item;
        transform.position = position;

        SetupVisuals();
    }

    private void SetupVisuals()
    {
        if (armorData == null) return;

        // Update name for clarity
        gameObject.name = $"Pickup_Armor - {armorData.armorType} - {armorData.itemName}";

        // Update armor visual if prefab is set
        if (armorData.armorModelPrefab != null && armorVisual != null) {
            // Clear existing visual
            if (armorVisual != null)
                Destroy(armorVisual);

            // Instantiate new visual
            armorVisual = Instantiate(armorData.armorModelPrefab, transform);
            armorVisual.transform.localPosition = Vector3.zero;
            armorVisual.transform.localRotation = Quaternion.identity;

            // Apply tint color if set
            if (armorData.armorTintColor != Color.white) {
                Renderer renderer = armorVisual.GetComponent<Renderer>();
                if (renderer != null) {
                    renderer.material.color = armorData.armorTintColor;
                }
            }
        }

        // Update renderer for highlighting
        UpdateActiveRenderer();
    }

    private void UpdateActiveRenderer()
    {
        if (armorVisual != null) {
            Renderer modelRenderer = armorVisual.GetComponent<Renderer>();
            if (modelRenderer != null) {
                objectRenderer = modelRenderer;
                if (defaultMaterial == null)
                    defaultMaterial = modelRenderer.sharedMaterial;
            }
        } else {
            // Fallback to self renderer
            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer != null && defaultMaterial == null)
                defaultMaterial = objectRenderer.sharedMaterial;
        }
    }
    #endregion

    #region Interaction
    public override void Interaction()
    {
        var inventory = InventoryService.GetPlayerInventory();
        if (inventory == null) {
            Debug.LogWarning($"{name}: No inventory found via InventoryService");
            return;
        }

        // Check if inventory can accept the item
        if (!inventory.CanAddItem()) {
            Debug.Log($"{name}: Inventory is full!");
            return;
        }

        // Add armor to inventory
        inventory.AddItem(armorItem);
        Debug.Log($"Picked up armor: {armorData.itemName} ({armorData.armorType})");
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (ObjectPool.instance != null)
            ObjectPool.instance.ReturnObject(gameObject);
        else
            Destroy(gameObject);
    }

    public override bool CanInteract(Transform playerTransform)
    {
        if (!base.CanInteract(playerTransform))
            return false;

        // Additional check: ensure armor pickup is valid
        return armorItem != null && armorData != null;
    }
    #endregion

    #region Editor
    [ContextMenu("Update Armor Visual")]
    private void UpdateArmorVisualInEditor()
    {
        SetupVisuals();
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Visual indicator for armor pickups with color coding by type
        if (armorData != null) {
            switch (armorData.armorType) {
                case ArmorType.Headgear:
                    Gizmos.color = Color.green;
                    break;
                case ArmorType.Vest:
                    Gizmos.color = Color.blue;
                    break;
                case ArmorType.Boots:
                    Gizmos.color = new Color(1f, 0.5f, 0f); // Orange
                    break;
                default:
                    Gizmos.color = Color.cyan;
                    break;
            }
            Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "d_PreMatCube", true);

            // Draw armor type label
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.75f,
                $"{armorData.armorType}\n{armorData.itemName}");
#endif
        }
    }
    #endregion
}
