using UnityEngine;
using UnityEngine.UIElements;

[ExecuteAlways] // Enables editing in Inspector + updates UI in Edit Mode
public class UIManager : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument mainDocument;

    [Header("TEST ONLY — Adjust Health Fill")]
    [Range(0f, 1f)]
    [SerializeField] private float healthFill = 1f;

    private GameHUD gameHUD;

    private void OnEnable()
    {
        InitializeUI();
        UpdateUI();
    }

    private void OnValidate()
    {
        InitializeUI();
        UpdateUI();
    }

    private void InitializeUI()
    {
        if (mainDocument == null) return;

        VisualElement root = mainDocument.rootVisualElement;
        if (root == null) return;

        // Build HUD module
        if (gameHUD == null)
            gameHUD = new GameHUD(root.Q<VisualElement>("hud-panel"));
    }

    private void UpdateUI()
    {
        if (gameHUD == null) return;

        // Apply inspector slider value to HUD
        gameHUD.SetHealthPercent(healthFill);
    }
}
