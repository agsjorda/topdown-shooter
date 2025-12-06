using UnityEngine;

public class Pickup_Item : Interactable
{
    [Header("Item Data")]
    [SerializeField] private Item_DataSO itemData;

    private SpriteRenderer spriteRenderer;
    private Inventory_Item inventoryItem;
    private Inventory_Base playerInventory;

    protected override void Awake()
    {
        base.Awake();
        InitializePickup();
    }

    protected override void InitializeComponents()
    {
        base.InitializeComponents();

        // Get sprite renderer specifically for 2D items
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Use sprite renderer as the main renderer for highlighting
        if (spriteRenderer != null)
            objectRenderer = spriteRenderer;
    }

    private void InitializePickup()
    {
        if (itemData != null) {
            inventoryItem = new Inventory_Item(itemData);
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer != null && itemData != null) {
            spriteRenderer.sprite = itemData.icon;

            // Update name for clarity in hierarchy
            gameObject.name = $"Pickup_Item - {itemData.itemName}";
        }
    }

    #region Interaction
    public override void Interaction()
    {
        if (playerInventory == null) {
            playerInventory = Object.FindFirstObjectByType<Inventory_Base>();
            if (playerInventory == null) {
                Debug.LogError($"{name}: No Inventory_Base found in scene!");
                return;
            }
        }

        Debug.Log($"Attempting to add {inventoryItem.itemData.itemName} to inventory");

        // Fix: Inventory_Base.AddItem returns void, not bool. Use CanAddItem() to check before adding.
        if (playerInventory.CanAddItem()) {
            playerInventory.AddItem(inventoryItem);
            DestroyPickup();
        } else {
            Debug.LogWarning("Failed to add item - inventory might be full");
        }
    }

    private void DestroyPickup()
    {
        // Try object pooling first, then destroy
        if (ObjectPool.instance != null)
            ObjectPool.instance.ReturnObject(gameObject);
        else
            Destroy(gameObject);
    }

    public override bool CanInteract(Transform playerTransform)
    {
        if (!base.CanInteract(playerTransform))
            return false;

        // Additional check: ensure inventory exists and has space
        if (playerInventory == null)
            playerInventory = Object.FindFirstObjectByType<Inventory_Base>();

        return playerInventory != null && playerInventory.CanAddItem();
    }
    #endregion

    #region Editor
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Visual indicator for item pickups
        if (itemData != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawIcon(transform.position + Vector3.up * 0.3f, "d_Toolbar Plus", true);
        }
    }
    #endregion
}