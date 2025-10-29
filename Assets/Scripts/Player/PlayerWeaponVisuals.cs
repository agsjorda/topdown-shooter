using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerWeaponVisuals : MonoBehaviour
{

    private Player player;
    private Animator anim;

    [SerializeField] private WeaponModel[] weaponModels;
    [SerializeField] private BackupWeaponModel[] backupWeaponModels;

    [Header("Rig")]
    [SerializeField] private float rigIncreaseStep;
    private bool shouldIncrease_RigWeight;
    private Rig rig;


    [Header("Left and IK")]
    [SerializeField] private float leftHandIKWeightIncreaseRate;
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private Transform leftHandIK_Target;
    private bool shouldIncrease_LeftHandIKWeight;



    private void Start()
    {
        player = GetComponent<Player>();
        anim = GetComponentInChildren<Animator>();
        rig = GetComponentInChildren<Rig>();
        weaponModels = GetComponentsInChildren<WeaponModel>(true);
        backupWeaponModels = GetComponentsInChildren<BackupWeaponModel>(true);
    }

    private void Update()
    {

        UpdateRigWeight();
        UpdateLeftHandIKWeight();
    }

    public void PlayFireAnimation() => anim.SetTrigger("Fire");

    public void PlayReloadAnimation()
    {

        float reloadSpeed = player.weapon.CurrentWeapon().reloadSpeed;
        anim.SetFloat("ReloadSpeed", reloadSpeed);
        anim.SetTrigger("Reload");
        ReduceRigWeight();
    }


    public void PlayWeaponEquipAnimation()
    {

        EquipType equipType = CurrentWeaponModel().equipAnimationType;

        float equipSpeed = player.weapon.CurrentWeapon().equipSpeed;

        leftHandIK.weight = 0;
        ReduceRigWeight();
        anim.SetFloat("EquipType", ((float)equipType));
        anim.SetFloat("EquipSpeed", equipSpeed);
        anim.SetTrigger("EquipWeapon");
    }


    public void SwitchOnCurrentWeaponModel()
    {

        int animationIndex = (int)CurrentWeaponModel().holdType;

        SwitchOffWeaponModels();

        SwitchOffBackupWeaponModels();

        if (player.weapon.hasHasOnlyOneWeapon() == false)
            SwitchOnBackupWeaponModel();

        SwitchAnimationLayer(animationIndex);
        CurrentWeaponModel().gameObject.SetActive(true);

        AttachLeftHand();
    }

    public void SwitchOffWeaponModels()
    {

        for (int i = 0; i < weaponModels.Length; i++) {
            weaponModels[i].gameObject.SetActive(false);
        }
    }

    private void SwitchOffBackupWeaponModels()
    {
        foreach (BackupWeaponModel backupWeapon in backupWeaponModels) {
            backupWeapon.Activate(false);
        }
    }

    public void SwitchOnBackupWeaponModel()
    {
        SwitchOffBackupWeaponModels();

        BackupWeaponModel lowHangWeapon = null;
        BackupWeaponModel backHangWeapon = null;
        BackupWeaponModel sideHangWeapon = null;

        foreach (BackupWeaponModel backupModel in backupWeaponModels) {

            if (backupModel.weaponType == player.weapon.CurrentWeapon().weaponType)
                continue;

            if (player.weapon.WeaponInSlots(backupModel.weaponType) != null) {

                if (backupModel.HangTypeIs(HangType.LowBackHang))
                    lowHangWeapon = backupModel;

                if (backupModel.HangTypeIs(HangType.BackHang))
                    backHangWeapon = backupModel;

                if (backupModel.HangTypeIs(HangType.SideHang))
                    sideHangWeapon = backupModel;
            }
        }

        lowHangWeapon?.Activate(true);
        backHangWeapon?.Activate(true);
        sideHangWeapon?.Activate(true);
    }

    private void SwitchAnimationLayer(int layerIndex)
    {
        for (int i = 1; i < anim.layerCount; i++) {
            anim.SetLayerWeight(i, i == layerIndex ? 1 : 0);
        }
    }
    public WeaponModel CurrentWeaponModel()
    {

        WeaponModel weaponModel = null;

        WeaponType weaponType = player.weapon.CurrentWeapon().weaponType;

        for (int i = 0; i < weaponModels.Length; i++) {
            if (weaponModels[i].weaponType == weaponType) {
                weaponModel = weaponModels[i];
            }
        }

        return weaponModel;

    }

    #region Animation Rigging Methods
    private void UpdateLeftHandIKWeight()
    {
        if (shouldIncrease_LeftHandIKWeight) {
            leftHandIK.weight += leftHandIKWeightIncreaseRate * Time.deltaTime;
            if (leftHandIK.weight >= 1) {
                shouldIncrease_LeftHandIKWeight = false;
            }
        }
    }

    private void UpdateRigWeight()
    {
        if (shouldIncrease_RigWeight) {
            rig.weight += rigIncreaseStep * Time.deltaTime;
            if (rig.weight >= 1) {
                shouldIncrease_RigWeight = false;
            }
        }
    }

    private void ReduceRigWeight()
    {
        rig.weight = .15f;
    }
    public void MaximizeRigWeight() => shouldIncrease_RigWeight = true;
    public void MaximizeLeftHandWeight() => shouldIncrease_LeftHandIKWeight = true;
    private void AttachLeftHand()
    {
        Transform targetTransform = CurrentWeaponModel().holdPoint;

        leftHandIK_Target.localPosition = targetTransform.localPosition;
        leftHandIK_Target.localRotation = targetTransform.localRotation;
    }
    #endregion

}


