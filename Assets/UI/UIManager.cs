using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
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
        }

        isInitialized = true;
        Debug.Log("UIManager: Initialized successfully");
    }

    private void Update()
    {
        if (!isInitialized) return;

        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.I)) ToggleInventory();
        if (Input.GetKeyDown(KeyCode.H)) ToggleHUD();
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

    public void ToggleHUD() => gameHUD?.Toggle();
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