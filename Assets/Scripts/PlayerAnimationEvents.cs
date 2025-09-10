using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour {
    private PlayerWeaponVisuals visualController;

    private void Start() {
        visualController = GetComponentInParent<PlayerWeaponVisuals>();
    }

    public void ReloadIsOver() {
        visualController.MaximizeRigWeight();

        //Refill bullets in the weapon
    }

    public void ReturnRig() {
        visualController.MaximizeRigWeight();
        visualController.MaximizeLeftHandWeight();
    }
    public void WeaponGrabIsOver() {
        visualController.SetBusyGrabbingWeaponTo(false);
    }
}
