using UnityEngine;

public class Player : MonoBehaviour {

    public PlayerControls controls;

    public void Awake() {
        controls = new PlayerControls();

    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();
}
