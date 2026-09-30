using System;
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
        [Tooltip("Panel Renderer containing the inventory panel. Auto-filled from the same GameObject.")]
        [SerializeField] private PanelRenderer panelRenderer;

        [Tooltip("Element name of the inventory panel root inside the Panel Renderer's UXML")]
        [SerializeField] private string inventoryPanelName = "inventory-panel";

        [Tooltip("Hide the panel when the scene starts")]
        [SerializeField] private bool startHidden = true;

        private Inventory_UI inventoryUI;
        private VisualElement loadedRoot;
        private int loadedVersion;

        public bool IsInventoryOpen => inventoryUI?.IsVisible ?? false;
        public bool IsInitialized { get; private set; }

        /// <summary>Fired after the panel opens (true) or closes (false).</summary>
        public event Action<bool> OnInventoryToggled;

        private void Awake()
        {
            if (panelRenderer == null) panelRenderer = GetComponent<PanelRenderer>();
        }

        private void OnEnable()
        {
            if (panelRenderer == null) {
                Debug.LogError("[InventoryUIController] No PanelRenderer assigned or found on this GameObject");
                return;
            }
            // Replays immediately when the UI is already loaded, and fires again on every reload
            panelRenderer.RegisterUIReloadCallback(OnUIReloaded);
        }

        private void OnDisable()
        {
            if (panelRenderer != null) panelRenderer.UnregisterUIReloadCallback(OnUIReloaded);
        }

        private void OnUIReloaded(PanelRenderer renderer, VisualElement root, int version)
        {
            if (IsInitialized && root == loadedRoot && version == loadedVersion) return;
            loadedRoot = root;
            loadedVersion = version;
            // A reload replaces the whole tree; carry the open state over to the new panel
            bool keepOpen = IsInitialized ? IsInventoryOpen : !startHidden;
            IsInitialized = false;
            InitializeUI(root, keepOpen);
        }

        private void InitializeUI(VisualElement root, bool open)
        {
            if (root == null) {
                Debug.LogError("[InventoryUIController] Panel Renderer delivered a null root element");
                return;
            }

            // The panel toggles via .ui-hidden. That rule lives in InventoryCore and must be
            // on the root before the first Hide(), which can run before InventoryUIConfig.
            var coreStyles = Resources.Load<StyleSheet>("InventorySystem/InventoryCore");
            if (coreStyles != null && !root.styleSheets.Contains(coreStyles))
                root.styleSheets.Add(coreStyles);

            inventoryUI = new Inventory_UI(root.Q<VisualElement>(inventoryPanelName));
            if (!inventoryUI.IsValid) {
                Debug.LogError($"[InventoryUIController] Inventory panel '{inventoryPanelName}' not found in the Panel Renderer's UXML");
                inventoryUI = null;
                return;
            }

            if (open) inventoryUI.Show();
            else inventoryUI.Hide();
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
