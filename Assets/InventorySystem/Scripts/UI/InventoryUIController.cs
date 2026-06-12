using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace InventorySystem
{
    /// <summary>
    /// Drop-in component that owns the inventory panel: open/close state and change events.
    /// Deliberately has no input, player, or camera references — the game binds its own
    /// input and calls Toggle()/Open()/Close(), reacting via OnInventoryToggled.
    /// </summary>
    [DisallowMultipleComponent]
    public class InventoryUIController : MonoBehaviour
    {
        [Tooltip("UIDocument containing the inventory panel")]
        [SerializeField] private UIDocument document;

        [Tooltip("Element name of the inventory panel root inside the UIDocument")]
        [SerializeField] private string inventoryPanelName = "inventory-panel";

        [Tooltip("Hide the panel when the scene starts")]
        [SerializeField] private bool startHidden = true;

        private Inventory_UI inventoryUI;

        public bool IsInventoryOpen => inventoryUI?.IsVisible ?? false;
        public bool IsInitialized { get; private set; }

        /// <summary>Fired after the panel opens (true) or closes (false).</summary>
        public event Action<bool> OnInventoryToggled;

        private void Awake()
        {
            document ??= GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (!IsInitialized) {
                StartCoroutine(InitializeWithDelay());
            }
        }

        // UIDocument builds its visual tree during the first frame; query after it exists
        private IEnumerator InitializeWithDelay()
        {
            yield return new WaitForEndOfFrame();
            InitializeUI();
        }

        private void InitializeUI()
        {
            var root = document != null ? document.rootVisualElement : null;
            if (root == null) {
                Debug.LogError("[InventoryUIController] UIDocument or root element is null");
                return;
            }

            inventoryUI = new Inventory_UI(root.Q<VisualElement>(inventoryPanelName));
            if (!inventoryUI.IsValid) {
                Debug.LogError($"[InventoryUIController] Inventory panel '{inventoryPanelName}' not found in UIDocument");
                inventoryUI = null;
                return;
            }

            if (startHidden) {
                inventoryUI.Hide();
            }
            IsInitialized = true;
        }

        public void Toggle()
        {
            if (inventoryUI == null) return;
            inventoryUI.Toggle();
            OnInventoryToggled?.Invoke(IsInventoryOpen);
        }

        public void Open()
        {
            if (inventoryUI == null || IsInventoryOpen) return;
            inventoryUI.Show();
            OnInventoryToggled?.Invoke(true);
        }

        public void Close()
        {
            if (inventoryUI == null || !IsInventoryOpen) return;
            inventoryUI.Hide();
            OnInventoryToggled?.Invoke(false);
        }
    }
}
