using UnityEngine;
using UnityEngine.UIElements;

[ExecuteAlways]
public class UIManager : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument mainDocument;

    [Header("UI References")]
    private string hudPanelName = "hud-panel";
    private string healthBarFillName = "healthBar_fill";

    [Header("TEST ONLY — Adjust Health Fill")]
    [Range(0f, 1f)]
    [SerializeField] private float healthFill = 1f;
    private float previousHealthFill; // Track changes

    private GameHUD_UI gameHUD;
    private bool isInitialized = false;

    private void OnEnable()
    {
        InitializeUI();
        UpdateUI();
    }

    private void OnValidate()
    {
        // Always (re)attempt init in edit mode so inspector changes take effect
        InitializeUI();

        // Only update if value actually changed and we're initialized
        if (isInitialized && !Mathf.Approximately(healthFill, previousHealthFill)) {
            UpdateUI();
            previousHealthFill = healthFill;
        }
    }

    private void InitializeUI()
    {
        if (mainDocument == null) {
            isInitialized = false;
            return;
        }

        VisualElement root = mainDocument.rootVisualElement;
        if (root == null) {
            isInitialized = false;
            return;
        }

        // GameHUD now manages multiple components internally
        gameHUD = new GameHUD_UI(root.Q<VisualElement>(hudPanelName));

        if (gameHUD != null && gameHUD.IsValid) {
            // Test the health bar and mark initialized
            gameHUD.SetHealthPercent(healthFill);
            isInitialized = true;
            previousHealthFill = healthFill;
        } else {
            isInitialized = false;
        }
    }

    private void UpdateUI()
    {
        if (gameHUD == null || !gameHUD.IsValid) return;

        gameHUD.SetHealthPercent(healthFill);
    }

    // Public API for other systems to use
    public void SetHealth(float current, float max)
    {
        healthFill = Mathf.Clamp01(current / max);
        UpdateUI();
        previousHealthFill = healthFill;
    }
}