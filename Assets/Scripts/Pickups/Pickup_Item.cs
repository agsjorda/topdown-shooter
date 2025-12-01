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

    private void OnValidate()
    {
        // Only update the name in OnValidate, visuals are handled elsewhere
        if (itemData != null) {
            gameObject.name = "Pickup_Item - " + itemData.itemName;

            // Optional: Update visuals if in play mode and spriteRenderer exists
            if (Application.isPlaying && spriteRenderer != null) {
                UpdateVisuals();
            }
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
        playerInventory = Object.FindFirstObjectByType<Inventory_Base>();

        if (playerInventory != null) {
            playerInventory.AddItem(itemToAdd);
            Destroy(gameObject);
        } else {
            Debug.LogWarning("No Inventory_Base found in scene");
        }
    }
}