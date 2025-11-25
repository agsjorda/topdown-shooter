using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private Player player;

    private PlayerControls controls;

    [Header("UI Document")]
    [SerializeField] private UIDocument mainDocument;

    [Header("UI Panel Names")]
    [SerializeField] private string hudPanelName = "hud-panel";
    [SerializeField] private string inventoryPanelName = "inventory-panel";

    [Header("Debug")]
    [Range(0f, 1f)]
    [SerializeField] private float testHealth = 1f;

    private GameHUD_UI gameHUD;
    private Inventory_UI inventoryUI;
    private bool isInitialized = false;

    [Header("cameras")]
    [SerializeField] private Camera characterPreviewCamera;
    [SerializeField] private Camera minimapCamera;

    private void Start()
    {
        player = Object.FindFirstObjectByType<Player>();
        if (player == null) {
            Debug.LogError("UIManager: Player not found in scene");
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

        // Initialize UI components
        gameHUD = new GameHUD_UI(root.Q<VisualElement>(hudPanelName));
        inventoryUI = new Inventory_UI(root.Q<VisualElement>(inventoryPanelName));

        // Validate initialization
        if (!gameHUD.IsValid) {
            Debug.LogError("UIManager: GameHUD failed to initialize");
            return;
        }

        // Set initial state
        gameHUD.Show();
        gameHUD.SetHealthPercent(testHealth);

        if (inventoryUI.IsValid) {
            inventoryUI.Hide();
            characterPreviewCamera.enabled = false;
        }

        isInitialized = true;
        Debug.Log("UIManager: Initialized successfully");
    }

    private void Update()
    {
        if (!isInitialized) return;

        HandleInputEvents();
    }
    public void ToggleMinimap(bool show)
    {
        minimapCamera.enabled = show;
    }

    public void ToggleCharacterPreview(bool show)
    {
        characterPreviewCamera.enabled = show;
    }

    private void HandleInputEvents()
    {
        controls = player.controls;
        controls.Character.InventoryToggle.performed += context =>
        {
            Debug.Log("Inventory toggle pressed");
            ToggleInventory();
            ToggleHUD();
            ToggleCharacterPreview(inventoryUI.IsVisible);
        };
        controls.Character.HudToggle.performed += context =>
        {
            ToggleHUD();
            if (inventoryUI.IsVisible)
                ToggleInventory();
        };
        if (Input.GetKeyDown(KeyCode.UpArrow)) ChangeHealth(0.1f);
        if (Input.GetKeyDown(KeyCode.DownArrow)) ChangeHealth(-0.1f);
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
    public void ToggleInventory() => inventoryUI?.Toggle();
    public void ShowHUD() => gameHUD?.Show();
    public void HideHUD() => gameHUD?.Hide();
    public void ShowInventory() => inventoryUI?.Show();
    public void HideInventory() => inventoryUI?.Hide();

    private void ChangeHealth(float amount)
    {
        testHealth = Mathf.Clamp01(testHealth + amount);
        gameHUD?.SetHealthPercent(testHealth);
    }

    public bool IsInitialized => isInitialized;
}