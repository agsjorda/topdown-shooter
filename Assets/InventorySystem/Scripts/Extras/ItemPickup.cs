using System;
using UnityEngine;
using UnityEngine.Events;

namespace InventorySystem.Extras
{
    /// <summary>
    /// Optional drop-in world pickup: holds an item and adds it to the player's inventory
    /// via InventoryService. Works trigger-based (walk over it) or call-based (your own
    /// interaction system calls TryPickup()). The world object is only consumed when the
    /// add actually succeeded. Delete the Extras folder if your game has its own pickups.
    /// </summary>
    public class ItemPickup : MonoBehaviour
    {
        [Header("Item")]
        [SerializeField] private Item_DataSO item;
        [Min(1)]
        [SerializeField] private int quantity = 1;

        [Header("Trigger Pickup")]
        [Tooltip("Pick up automatically when something enters this object's trigger collider")]
        [SerializeField] private bool pickupOnTriggerEnter = true;
        [Tooltip("Layers allowed to trigger the pickup")]
        [SerializeField] private LayerMask pickupLayers = ~0;

        [Header("On Success")]
        [Tooltip("Destroy the GameObject after pickup; otherwise it is deactivated")]
        [SerializeField] private bool destroyOnPickup = true;
        [SerializeField] private UnityEvent onPickedUp;

        /// <summary>Fired after a successful pickup with the created inventory item.</summary>
        public event Action<ItemPickup, InventoryItem> OnPickedUp;

        public Item_DataSO Item { get => item; set => item = value; }
        public int Quantity { get => quantity; set => quantity = Mathf.Max(1, value); }

        private void OnTriggerEnter(Collider other)
        {
            if (!pickupOnTriggerEnter) return;
            if ((pickupLayers.value & (1 << other.gameObject.layer)) == 0) return;
            TryPickup();
        }

        /// <summary>
        /// Attempts to add the item to the player's inventory.
        /// Returns false (and leaves the pickup in the world) when the inventory
        /// is missing or full.
        /// </summary>
        public bool TryPickup()
        {
            if (item == null) return false;

            var inventory = InventoryService.GetPlayerInventory();
            if (inventory == null) {
                Debug.LogWarning("[ItemPickup] No player inventory found via InventoryService");
                return false;
            }

            var inventoryItem = new InventoryItem(item, quantity);
            if (!inventory.AddItem(inventoryItem)) return false;

            OnPickedUp?.Invoke(this, inventoryItem);
            onPickedUp?.Invoke();

            if (destroyOnPickup) {
                Destroy(gameObject);
            } else {
                gameObject.SetActive(false);
            }
            return true;
        }
    }
}
