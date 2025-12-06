using UnityEngine;

public class Pickup_Weapon : Interactable
{
    [Header("Weapon Data")]
    [SerializeField] private Weapon_Data weaponData;
    [SerializeField] private WeaponModel[] weaponModels;

    private Weapon weapon;

    // Add this field to fix CS0103
    private Material defaultMaterial;

    protected override void Awake()
    {
        base.Awake();

        if (weaponData != null && weapon == null)
            weapon = new Weapon(weaponData);

        SetupVisuals();
    }

    protected override void InitializeComponents()
    {
        base.InitializeComponents();

        // Auto-populate weapon models if not set
        if (weaponModels == null || weaponModels.Length == 0)
            weaponModels = GetComponentsInChildren<WeaponModel>(true);
    }

    #region Setup Methods
    public void SetupPickupWeapon(Weapon weaponToDrop, Vector3 position)
    {
        weapon = weaponToDrop;
        weaponData = weapon.weaponData;
        transform.position = position;

        SetupVisuals();
    }

    private void SetupVisuals()
    {
        // Update name for clarity
        gameObject.name = $"Pickup_Weapon - {weaponData?.weaponType}";

        // Hide all models first
        HideAllModels();

        // Show appropriate backup model
        ShowWeaponModel();

        // Update renderer for highlighting
        UpdateActiveRenderer();
    }

    private void HideAllModels()
    {
        if (weaponModels == null) return;

        foreach (var model in weaponModels) {
            if (model != null)
                model.SetActive(false);
        }
    }

    private void ShowWeaponModel()
    {
        if (weaponData == null || weaponModels == null) return;

        foreach (var model in weaponModels) {
            if (model != null &&
                model.weaponType == weaponData.weaponType &&
                model.modelType == WeaponModelType.Backup) {
                model.SetActive(true);
                break; // Only show one matching model
            }
        }
    }

    private void UpdateActiveRenderer()
    {
        if (weaponModels == null) return;

        // Find the first active model and use its renderer for highlighting
        foreach (var model in weaponModels) {
            if (model != null && model.gameObject.activeSelf) {
                Renderer modelRenderer = model.GetComponent<Renderer>();
                if (modelRenderer != null) {
                    objectRenderer = modelRenderer;
                    if (defaultMaterial == null)
                        defaultMaterial = modelRenderer.sharedMaterial;
                    break;
                }
            }
        }
    }
    #endregion

    #region Interaction
    public override void Interaction()
    {
        PlayerWeaponController weaponController = Object.FindFirstObjectByType<PlayerWeaponController>();
        if (weaponController == null) {
            Debug.LogWarning($"{name}: No PlayerWeaponController found");
            return;
        }

        // PickupWeapon returns void, so just call it and then return to pool
        weaponController.PickupWeapon(weapon);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (ObjectPool.instance != null)
            ObjectPool.instance.ReturnObject(gameObject);
        else
            Destroy(gameObject);
    }

    public override bool CanInteract(Transform playerTransform)
    {
        if (!base.CanInteract(playerTransform))
            return false;

        // Additional check: ensure weapon pickup is valid
        return weapon != null && weaponData != null;
    }
    #endregion

    #region Editor
    [ContextMenu("Update Weapon Model")]
    private void UpdateWeaponModelInEditor()
    {
        SetupVisuals();
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Visual indicator for weapon pickups
        if (weaponData != null) {
            Gizmos.color = Color.magenta;
            Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, "d_PreMatCube", true);
        }
    }
    #endregion
}