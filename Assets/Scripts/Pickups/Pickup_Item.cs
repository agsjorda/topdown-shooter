using UnityEngine;
using InventorySystem;

/// <summary>
/// World pickup for any inventory item (consumables, armor, misc). The item data decides
/// everything item-specific: its category places it in the inventory, and data that
/// implements IPickupModelSource (e.g. Armor_Data) supplies the world model.
/// Uses InventoryService for centralized inventory access.
/// </summary>
public class Pickup_Item : Interactable
{
    [Header("Item Data")]
    [SerializeField] private Item_DataSO itemData;
    [Min(1)]
    [SerializeField] private int quantity = 1;

    [Header("Visual")]
    [Tooltip("Child object shown in the world. Replaced when the item data supplies a model prefab; a SpriteRenderer here shows the item icon.")]
    [SerializeField] private GameObject visual;

    private InventoryItem inventoryItem;

    protected override void Awake()
    {
        base.Awake();
        if (itemData != null && inventoryItem == null)
            inventoryItem = new InventoryItem(itemData, quantity);
        SetupVisuals();
    }

    protected override void InitializeComponents()
    {
        if (visual == null && transform.childCount > 0)
            visual = transform.GetChild(0).gameObject;
        base.InitializeComponents();
    }

    #region Setup Methods
    /// <summary>Sets up the pickup with item data at a position (e.g. spawning loot).</summary>
    public void SetupPickup(Item_DataSO data, Vector3 position, int amount = 1)
    {
        SetupPickup(data != null ? new InventoryItem(data, amount) : null, position);
    }

    /// <summary>Sets up the pickup from an existing inventory item (e.g. dropping from the inventory).</summary>
    public void SetupPickup(InventoryItem item, Vector3 position)
    {
        if (item == null || item.itemData == null) return;
        inventoryItem = item;
        itemData = item.itemData;
        quantity = item.quantity;
        transform.position = position;
        SetupVisuals();
    }

    private void SetupVisuals()
    {
        if (itemData == null) return;

        gameObject.name = $"Pickup_Item - {itemData.itemName}";

        if (itemData is IPickupModelSource modelSource && modelSource.PickupModelPrefab != null) {
            if (visual != null) {
                if (Application.isPlaying) Destroy(visual);
                else DestroyImmediate(visual);
            }
            visual = Instantiate(modelSource.PickupModelPrefab, transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            if (modelSource.PickupModelTint != Color.white) {
                var tintRenderer = visual.GetComponentInChildren<Renderer>();
                if (tintRenderer != null)
                    tintRenderer.material.color = modelSource.PickupModelTint;
            }
        } else if (visual != null) {
            var spriteRenderer = visual.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null && itemData.icon != null)
                spriteRenderer.sprite = itemData.icon;
        }

        // Highlight whatever is now visible
        var visibleRenderer = visual != null ? visual.GetComponentInChildren<Renderer>() : null;
        if (visibleRenderer != null) {
            defaultSharedMaterial = null;
            UpdateRenderer(visibleRenderer);
        }
    }
    #endregion

    #region Interaction
    public override void Interaction()
    {
        var inventory = InventoryService.GetPlayerInventory();
        if (inventory == null) {
            Debug.LogError($"{name}: No player inventory found via InventoryService!");
            return;
        }

        // Only consume the pickup when the add actually succeeded
        if (inventory.AddItem(inventoryItem)) {
            Debug.Log($"Picked up {inventoryItem.quantity}x {itemData.itemName}");
            ReturnToPool();
        } else {
            Debug.LogWarning($"{name}: Failed to add item - inventory is full");
        }
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
        if (!base.CanInteract(playerTransform) || inventoryItem == null)
            return false;

        var inventory = InventoryService.GetPlayerInventory();
        return inventory != null && inventory.CanAddItem();
    }
    #endregion

    #region Editor
    [ContextMenu("Update Visual")]
    private void UpdateVisualInEditor()
    {
        SetupVisuals();
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (itemData != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "d_Toolbar Plus", true);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.75f, itemData.itemName);
#endif
        }
    }
    #endregion
}
