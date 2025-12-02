using UnityEngine;

public class Pickup_Item : Interactable
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Item_DataSO itemData;

    private Inventory_Item itemToAdd;
    private Inventory_Base playerInventory;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (itemData != null) {
            itemToAdd = new Inventory_Item(itemData);
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer != null && itemData != null) {
            spriteRenderer.sprite = itemData.icon;
        }
    }

    public override void Interaction()
    {
        if (playerInventory == null) {
            playerInventory = Object.FindFirstObjectByType<Inventory_Base>();
            if (playerInventory == null) {
                Debug.LogError("Pickup_Item: No Inventory_Base found in scene!");
                return;
            }
        }

        if (playerInventory != null && itemToAdd != null) {
            Debug.Log($"Attempting to add {itemToAdd.itemData.itemName} to inventory");
            playerInventory.AddItem(itemToAdd);

            if (playerInventory.CanAddItem()) {
                Destroy(gameObject);
            } else {
                Debug.LogWarning("Failed to add item - inventory might be full");
            }
        }
    }
}