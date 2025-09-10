using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour {
    private WeaponVisualController visualController;

    private void Start() {
        visualController = GetComponentInParent<WeaponVisualController>();
    }

    public void ReloadIsOver() {
        visualController.ReturnRigWeightToOne();

        //Refill bullets in the weapon
    }
}
