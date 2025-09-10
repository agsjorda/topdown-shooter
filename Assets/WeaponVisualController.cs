using UnityEngine;

public class WeaponVisualController : MonoBehaviour {

    private Animator anim;

    [SerializeField] private Transform[] gunTransforms;

    [SerializeField] private Transform pistol;
    [SerializeField] private Transform revolver;
    [SerializeField] private Transform autoRifle;
    [SerializeField] private Transform shotgun;
    [SerializeField] private Transform rifle;

    private Transform currentGun;

    [Header("Left and IK")]
    [SerializeField] private Transform leftHand;

    private void Start() {
        SwitchOn(pistol);

        anim = GetComponentInParent<Animator>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SwitchOn(pistol);
            SwitchAnimationLayer(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SwitchOn(revolver);
            SwitchAnimationLayer(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            SwitchOn(autoRifle);
            SwitchAnimationLayer(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            SwitchOn(shotgun);
            SwitchAnimationLayer(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            SwitchOn(rifle);
            SwitchAnimationLayer(3);
        }

    }

    private void SwitchOn(Transform gunTransform) {
        SwitchOffGuns();
        gunTransform.gameObject.SetActive(true);
        currentGun = gunTransform;

        AttachLeftHand();
    }

    private void SwitchOffGuns() {

        for (int i = 0; i < gunTransforms.Length; i++) {
            gunTransforms[i].gameObject.SetActive(false);
        }
    }

    private void AttachLeftHand() {
        Transform targetTransform = currentGun.GetComponentInChildren<LeftHandTargetTransform>().transform;

        leftHand.localPosition = targetTransform.localPosition;
        leftHand.localRotation = targetTransform.localRotation;
    }

    private void SwitchAnimationLayer(int layerIndex) {
        for (int i = 1; i < anim.layerCount; i++) {
            anim.SetLayerWeight(i, i == layerIndex ? 1 : 0);
        }
    }
}
