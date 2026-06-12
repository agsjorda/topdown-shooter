using UnityEngine;
using UnityEngine.InputSystem;

namespace InventorySystem.Demos
{
    /// Pressing 1..9 adds the corresponding sample item to the inventory —
    /// demonstrates the integration API games use to grant items from code.
    public class DemoItemGiver : MonoBehaviour
    {
        [Tooltip("Item granted by key 1, 2, 3, ... in order")]
        [SerializeField] private Item_DataSO[] items;

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            for (int i = 0; i < items.Length && i < 9; i++) {
                if (items[i] == null) continue;
                if (keyboard[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame) {
                    bool added = InventoryService.GetPlayerInventory()?.AddItem(new InventoryItem(items[i])) ?? false;
                    Debug.Log(added
                        ? $"[Demo] Added {items[i].itemName}"
                        : $"[Demo] Could not add {items[i].itemName} (inventory full?)");
                }
            }
        }
    }
}
