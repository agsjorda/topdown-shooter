using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour {
    private PlayerWeaponVisuals visualController;
    private PlayerWeaponController weaponController;

    private void Start() {
        visualController = GetComponentInParent<PlayerWeaponVisuals>();
        weaponController = GetComponentInParent<PlayerWeaponController>();
    }

    public void ReloadIsOver() {
        visualController.MaximizeRigWeight();
        weaponController.CurrentWeapon().ReloadBullets();

        weaponController.SetWeaponReady(true);
    }

    public void ReturnRig() {
        visualController.MaximizeRigWeight();
        visualController.MaximizeLeftHandWeight();
    }
    public void WeaponEquippingIsOver() {
        weaponController.SetWeaponReady(true);
    }

    public void SwitchOnWeaponModel() => visualController.SwitchOnCurrentWeaponModel();
}
