using InventorySystem;
using System.Collections;
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

    [Header("UI Document")]
    [SerializeField] private UIDocument mainDocument;

    [Header("UI Panel Names")]
    [SerializeField] private string hudPanelName = "hud-panel";

    [Header("Inventory")]
    [SerializeField] private InventoryUIController inventoryController;

    [Header("Debug")]
    [Range(0f, 1f)]
    [SerializeField] private float testHealth = 1f;

    private GameHUD_UI gameHUD;
    private bool isInitialized = false;

    [Header("cameras")]
    [SerializeField] private Camera characterPreviewCamera;
    [SerializeField] private Camera minimapCamera;

    private void Start()
    {
        player = Object.FindFirstObjectByType<Player>();
        if (player == null) {
            Debug.LogError("UIManager: Player not found in scene");
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
        StartCoroutine(InitializeWithDelay());
    }

    private IEnumerator InitializeWithDelay()
    {
        yield return new WaitForEndOfFrame();
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (mainDocument?.rootVisualElement == null) {
            Debug.LogError("UIManager: UIDocument or root element is null");
            return;
        }

        var root = mainDocument.rootVisualElement;

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
        gameHUD.Show();
        gameHUD.SetHealthPercent(testHealth);
        characterPreviewCamera.enabled = false;

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
