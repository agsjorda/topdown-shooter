using InventorySystem;
using UnityEngine;

public class Pickup_Item : Interactable
{
    [Header("Item Data")]
    [SerializeField] private Item_DataSO itemData;

    private SpriteRenderer spriteRenderer;
    private InventoryItem inventoryItem;
    [SerializeField] private InventoryViewModel inventoryViewModel; // Use ViewModel for MVVM
    private InventoryModel playerInventory; // Only for ViewModel creation if needed

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
            inventoryItem = new InventoryItem(itemData);
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
        EnsureViewModel();
        if (inventoryViewModel == null) {
            Debug.LogError($"{name}: No InventoryViewModel found or created!");
            return;
        }

        Debug.Log($"Attempting to add {inventoryItem.itemData.itemName} to inventory");

        if (inventoryViewModel.CanAddItem()) {
            inventoryViewModel.AddItem(inventoryItem);
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
        EnsureViewModel();
        return inventoryViewModel != null && inventoryViewModel.CanAddItem();
    }

    private void EnsureViewModel()
    {
        if (inventoryViewModel == null) {
            playerInventory = Object.FindFirstObjectByType<InventoryModel>();
            if (playerInventory != null) {
                inventoryViewModel = new InventoryViewModel(playerInventory);
            }
        }
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