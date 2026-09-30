using InventorySystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Game-side UI orchestrator: HUD, minimap/preview cameras, and input wiring.
/// Inventory panel state lives in the InventorySystem module's InventoryUIController;
/// this class only binds the game's input to it and reacts to its events.
/// </summary>
public class UIManager : MonoBehaviour
{
    private Player player;

    private PlayerControls controls;

    [Header("UI Panel")]
    [Tooltip("Panel Renderer holding the HUD. Auto-filled from the same GameObject.")]
    [SerializeField] private PanelRenderer mainPanel;

    [Header("UI Panel Names")]
    [SerializeField] private string hudPanelName = "hud-panel";

    [Header("Inventory")]
    [SerializeField] private InventoryUIController inventoryController;

    [Header("Debug")]
    [Range(0f, 1f)]
    [SerializeField] private float testHealth = 1f;

    private GameHUD_UI gameHUD;
    private bool isInitialized = false;
    private VisualElement loadedRoot;
    private int loadedVersion;

    [Header("cameras")]
    [SerializeField] private Camera characterPreviewCamera;
    [SerializeField] private Camera minimapCamera;

    private void Start()
    {
        // Player_Preview also carries a disabled Player whose controls are never enabled.
        // Bind to the enabled one so the toggle keys reach a live input map.
        foreach (var candidate in Object.FindObjectsByType<Player>()) {
            if (candidate.isActiveAndEnabled) {
                player = candidate;
                break;
            }
        }
        if (player == null) {
            Debug.LogError("UIManager: No enabled Player found in scene");
            return;
        }

        // Subscribe exactly once (the old code re-subscribed every frame in Update)
        controls = player.controls;
        controls.Character.InventoryToggle.performed += OnInventoryTogglePerformed;
        controls.Character.HudToggle.performed += OnHudTogglePerformed;
    }

    private void OnDestroy()
    {
        if (controls != null) {
            controls.Character.InventoryToggle.performed -= OnInventoryTogglePerformed;
            controls.Character.HudToggle.performed -= OnHudTogglePerformed;
        }
    }

    private void OnEnable()
    {
        if (mainPanel == null) mainPanel = GetComponent<PanelRenderer>();
        if (mainPanel == null) {
            Debug.LogError("UIManager: No PanelRenderer assigned or found on this GameObject");
            return;
        }
        // Replays immediately when the UI is already loaded, and fires again on every reload
        mainPanel.RegisterUIReloadCallback(OnUIReloaded);
    }

    private void OnDisable()
    {
        if (mainPanel != null) mainPanel.UnregisterUIReloadCallback(OnUIReloaded);
    }

    private void OnUIReloaded(PanelRenderer renderer, VisualElement root, int version)
    {
        if (isInitialized && root == loadedRoot && version == loadedVersion) return;
        loadedRoot = root;
        loadedVersion = version;
        InitializeUI(root);
    }

    private void InitializeUI(VisualElement root)
    {
        if (root == null) {
            Debug.LogError("UIManager: Panel Renderer delivered a null root element");
            return;
        }

        // A reload replaces the tree; keep the HUD's visibility across it
        bool hudVisible = gameHUD == null || gameHUD.IsVisible;
        isInitialized = false;

        gameHUD = new GameHUD_UI(root.Q<VisualElement>(hudPanelName));

        if (!gameHUD.IsValid) {
            Debug.LogError("UIManager: GameHUD failed to initialize");
            return;
        }

        inventoryController ??= GetComponent<InventoryUIController>();
        if (inventoryController == null) {
            Debug.LogError("UIManager: InventoryUIController not assigned");
        }

        // Set initial state
        if (hudVisible) gameHUD.Show();
        else gameHUD.Hide();
        gameHUD.SetHealthPercent(testHealth);
        characterPreviewCamera.enabled = IsInventoryOpen();

        isInitialized = true;
        Debug.Log("UIManager: Initialized successfully");
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Debug health controls
        if (Input.GetKeyDown(KeyCode.UpArrow)) ChangeHealth(0.1f);
        if (Input.GetKeyDown(KeyCode.DownArrow)) ChangeHealth(-0.1f);
    }

    private void OnInventoryTogglePerformed(InputAction.CallbackContext context)
    {
        if (!isInitialized) return;
        ToggleInventory();
        ToggleHUD();
        ToggleCharacterPreview(IsInventoryOpen());
    }

    private void OnHudTogglePerformed(InputAction.CallbackContext context)
    {
        if (!isInitialized) return;
        ToggleHUD();
        if (IsInventoryOpen())
            ToggleInventory();
    }

    public void ToggleMinimap(bool show)
    {
        minimapCamera.enabled = show;
    }

    public void ToggleCharacterPreview(bool show)
    {
        characterPreviewCamera.enabled = show;
    }

    private void OnValidate()
    {
        if (isInitialized && Application.isPlaying) {
            gameHUD?.SetHealthPercent(testHealth);
        }
    }

    // Public API
    public void SetHealth(float current, float max)
    {
        testHealth = Mathf.Clamp01(current / max);
        gameHUD?.SetHealthPercent(testHealth);
    }

    public void ToggleHUD()
    {
        gameHUD?.Toggle();

        ToggleMinimap(gameHUD.IsVisible);
    }
    public void ToggleInventory() => inventoryController?.Toggle();
    public void ShowHUD() => gameHUD?.Show();
    public void HideHUD() => gameHUD?.Hide();
    public void ShowInventory() => inventoryController?.Open();
    public void HideInventory() => inventoryController?.Close();

    /// <summary>
    /// Check if the inventory UI is currently open/visible
    /// Used to disable weapon firing and other gameplay actions
    /// </summary>
    public bool IsInventoryOpen() => inventoryController != null && inventoryController.IsInventoryOpen;

    private void ChangeHealth(float amount)
    {
        testHealth = Mathf.Clamp01(testHealth + amount);
        gameHUD?.SetHealthPercent(testHealth);
    }

    public bool IsInitialized => isInitialized;
}
