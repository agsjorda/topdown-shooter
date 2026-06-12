using UnityEngine;
using UnityEngine.InputSystem;

namespace InventorySystem.Demos
{
    /// Minimal input binding for the demo: one key toggles the inventory panel.
    /// Real games bind their own input and call InventoryUIController.Toggle().
    public class DemoInventoryInput : MonoBehaviour
    {
        [SerializeField] private InventoryUIController inventoryController;
        [SerializeField] private Key toggleKey = Key.I;

        private void Awake()
        {
            if (inventoryController == null)
                inventoryController = GetComponent<InventoryUIController>();
            if (inventoryController == null)
                inventoryController = Object.FindFirstObjectByType<InventoryUIController>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard[toggleKey].wasPressedThisFrame) {
                inventoryController?.Toggle();
            }
        }
    }
}
