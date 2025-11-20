using UnityEngine;
using UnityEngine.UIElements;

[ExecuteAlways]
public class UIManager : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument mainDocument;

    [Header("UI References")]
    [SerializeField] private string hudPanelName = "hud-panel";
    [SerializeField] private string inventoryPanelName = "inventory-panel";

    // All UI components
    private GameHUD_UI gameHUD;
    private Inventory_UI inventoryUI;

    private bool isInitialized = false;

    private void OnEnable()
    {
        InitializeUI();
    }

    [Header("TEST ONLY — Adjust Health Fill")]
    [Range(0f, 1f)]
    [SerializeField] private float healthFill = 1f;
    private float previousHealthFill;

    private void OnValidate()
    {
        InitializeUI();

        if (isInitialized && !Mathf.Approximately(healthFill, previousHealthFill)) {
            UpdateHealthUI();
            previousHealthFill = healthFill;
        }
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (!isInitialized) return;

        // Example input handling
        if (Input.GetKeyDown(KeyCode.I)) {
            ToggleInventory();
        }

        if (Input.GetKeyDown(KeyCode.H)) {
            ToggleHUD();
        }
    }

    private void InitializeUI()
    {
        if (mainDocument == null || mainDocument.rootVisualElement == null) {
            isInitialized = false;
            return;
        }

        var root = mainDocument.rootVisualElement;

        // Initialize all UI components
        gameHUD = new GameHUD_UI(root.Q<VisualElement>(hudPanelName));
        inventoryUI = new Inventory_UI(root.Q<VisualElement>(inventoryPanelName));

        isInitialized = (gameHUD != null && gameHUD.IsValid);

        if (isInitialized) {
            // Start with HUD visible, inventory hidden
            gameHUD.Show();
            inventoryUI?.Hide();
            Debug.Log("UIManager: All UI components initialized");
        }
    }

    // Public API for other systems
    public void SetHealth(float current, float max)
    {
        if (!isInitialized) return;
        healthFill = Mathf.Clamp01(current / max);
        gameHUD?.SetHealth(current, max);
        previousHealthFill = healthFill;
    }

    public void ShowHUD() => gameHUD?.Show();
    public void HideHUD() => gameHUD?.Hide();
    public void ToggleHUD() => gameHUD?.Toggle(); // Using extension method!

    public void ShowInventory() => inventoryUI?.Show();
    public void HideInventory() => inventoryUI?.Hide();
    public void ToggleInventory() => inventoryUI?.Toggle(); // Using extension method!

    public void UpdateHealthUI()
    {
        if (gameHUD == null || !gameHUD.IsValid) return;
        gameHUD.SetHealthPercent(healthFill);
    }

    public bool IsInitialized => isInitialized;
}