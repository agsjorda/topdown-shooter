using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerWeaponVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponModel[] weaponModels;
    [SerializeField] private Rig rig;
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private Transform leftHandIK_Target;

    [Header("Animation Settings")]
    [SerializeField] private float rigIncreaseStep = 2f;
    [SerializeField] private float leftHandIKWeightIncreaseRate = 2f;

    private Player player;
    private Animator anim;
    private bool shouldIncreaseRigWeight;
    private bool shouldIncreaseLeftHandIKWeight;

    private void Start()
    {
        player = GetComponent<Player>();
        anim = GetComponentInChildren<Animator>();

        if (rig == null) rig = GetComponentInChildren<Rig>();
        if (weaponModels == null || weaponModels.Length == 0)
            weaponModels = GetComponentsInChildren<WeaponModel>(true);
    }

    private void Update()
    {
        UpdateRigWeight();
        UpdateLeftHandIKWeight();
    }

    #region Weapon Visual Updates
    public void UpdateWeaponVisuals(WeaponType currentWeaponType, bool showBackupModels)
    {
        // Hide all models first
        HideAllWeaponModels();

        // Show current primary weapon
        ShowWeaponModel(currentWeaponType, WeaponModelType.Primary);

        // Show backup models if needed
        if (showBackupModels)
            UpdateBackupVisuals();

        // Update animation and IK
        UpdateAnimationLayer(currentWeaponType);
        AttachLeftHandToCurrentWeapon();
    }

    public void UpdateBackupVisuals()
    {
        if (player.weapon.CurrentWeapon == null) return;

        WeaponType currentType = player.weapon.CurrentWeapon.weaponType;
        var allWeapons = player.weapon.GetAllWeapons();

        foreach (var weapon in allWeapons) {
            if (weapon.weaponType != currentType) {
                ShowWeaponModel(weapon.weaponType, WeaponModelType.Backup);
            }
        }
    }

    private void HideAllWeaponModels()
    {
        foreach (var model in weaponModels)
            model.SetActive(false);
    }

    private void ShowWeaponModel(WeaponType weaponType, WeaponModelType modelType)
    {
        WeaponModel model = GetWeaponModel(weaponType, modelType);
        if (model != null)
            model.SetActive(true);
    }

    public WeaponModel GetWeaponModel(WeaponType weaponType, WeaponModelType modelType)
    {
        foreach (var model in weaponModels) {
            if (model.weaponType == weaponType && model.modelType == modelType)
                return model;
        }
        return null;
    }

    public WeaponModel GetCurrentWeaponModel()
    {
        if (player.weapon.CurrentWeapon == null) return null;

        return GetWeaponModel(player.weapon.CurrentWeapon.weaponType, WeaponModelType.Primary);
    }
    #endregion

    #region Animation Methods
    private void UpdateAnimationLayer(WeaponType weaponType)
    {
        WeaponModel model = GetWeaponModel(weaponType, WeaponModelType.Primary);
        if (model == null) return;

        for (int i = 1; i < anim.layerCount; i++) {
            anim.SetLayerWeight(i, i == (int)model.holdType ? 1 : 0);
        }
    }

    public void PlayFireAnimation() => anim.SetTrigger("Fire");

    public void PlayReloadAnimation()
    {
        if (player.weapon.CurrentWeapon != null) {
            float reloadSpeed = player.weapon.CurrentWeapon.reloadSpeed;
            PlayReloadAnimation(reloadSpeed);
        }
    }

    public void PlayReloadAnimation(float reloadSpeed)
    {
        anim.SetFloat("ReloadSpeed", reloadSpeed);
        anim.SetTrigger("Reload");
        ReduceRigWeight();
    }

    public void PlayWeaponEquipAnimation()
    {
        WeaponModel currentModel = GetCurrentWeaponModel();
        if (currentModel == null || player.weapon.CurrentWeapon == null) return;

        leftHandIK.weight = 0;
        ReduceRigWeight();

        anim.SetFloat("EquipType", (float)currentModel.equipAnimationType);
        anim.SetFloat("EquipSpeed", player.weapon.CurrentWeapon.equipSpeed);
        anim.SetTrigger("EquipWeapon");
    }

    #region Animation Event Methods
    // Called by animation event when equip animation finishes
    public void OnEquipAnimationComplete()
    {
        MaximizeRigWeight();
        MaximizeLeftHandWeight();
        player.weapon.SetWeaponReady(true);
    }

    // Called by animation event when reload animation finishes
    public void OnReloadAnimationComplete()
    {
        player.weapon.CompleteReload();
        MaximizeRigWeight();
        player.weapon.SetWeaponReady(true);
    }
    #endregion

    #region Backward Compatibility Methods
    // For backward compatibility with existing calls
    public void SwitchOnCurrentWeaponModel()
    {
        if (player.weapon.CurrentWeapon != null)
            UpdateWeaponVisuals(player.weapon.CurrentWeapon.weaponType, !player.weapon.HasOnlyOneWeapon());
    }

    public void SwitchOnBackupWeaponModel()
    {
        UpdateBackupVisuals();
    }

    public void SwitchOffWeaponModels()
    {
        HideAllWeaponModels();
    }

    public void SwitchOffBackupWeaponModels()
    {
        foreach (var model in weaponModels) {
            if (model.modelType == WeaponModelType.Backup)
                model.SetActive(false);
        }
    }
    #endregion
    #endregion

    #region Rig and IK Methods
    private void UpdateRigWeight()
    {
        if (shouldIncreaseRigWeight && rig != null) {
            rig.weight = Mathf.MoveTowards(rig.weight, 1f, rigIncreaseStep * Time.deltaTime);
            if (rig.weight >= 0.99f)
                shouldIncreaseRigWeight = false;
        }
    }

    private void UpdateLeftHandIKWeight()
    {
        if (shouldIncreaseLeftHandIKWeight && leftHandIK != null) {
            leftHandIK.weight = Mathf.MoveTowards(leftHandIK.weight, 1f,
                leftHandIKWeightIncreaseRate * Time.deltaTime);
            if (leftHandIK.weight >= 0.99f)
                shouldIncreaseLeftHandIKWeight = false;
        }
    }

    private void ReduceRigWeight()
    {
        if (rig != null) rig.weight = 0.15f;
    }

    private void AttachLeftHandToCurrentWeapon()
    {
        WeaponModel currentModel = GetCurrentWeaponModel();
        if (currentModel == null || leftHandIK_Target == null) return;

        Transform targetTransform = currentModel.holdPoint;
        leftHandIK_Target.localPosition = targetTransform.localPosition;
        leftHandIK_Target.localRotation = targetTransform.localRotation;

        shouldIncreaseLeftHandIKWeight = true;
    }

    public void MaximizeRigWeight() => shouldIncreaseRigWeight = true;
    public void MaximizeLeftHandWeight() => shouldIncreaseLeftHandIKWeight = true;
    #endregion
}