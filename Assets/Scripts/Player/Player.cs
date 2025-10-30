using UnityEngine;

public class Player : MonoBehaviour
{

    public PlayerControls controls { get; private set; }
    public PlayerAim aim { get; private set; }
    public PlayerMovement movement { get; private set; }
    public PlayerWeaponController weapon { get; private set; }
    public PlayerWeaponVisuals weaponVisuals { get; private set; }

    private void Awake()
    {
        controls = new PlayerControls();
        aim = GetComponent<PlayerAim>();
        movement = GetComponent<PlayerMovement>();
        weapon = GetComponent<PlayerWeaponController>();
        weaponVisuals = GetComponent<PlayerWeaponVisuals>();
    }

    private void OnEnable()
    {
        // defensive: ensure controls exists before enabling to avoid NRE (domain reload / exec-order cases)
        controls ??= new PlayerControls();
        controls.Enable();
    }

    private void OnDisable()
    {
        // defensive: only disable if it was created
        controls?.Disable();
    }
}
