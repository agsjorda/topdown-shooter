using System;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    private Player player;
    private PlayerControls controls;

    [Header("Aim Visual - Laser")]
    [SerializeField] private LineRenderer aimLaser; //this is on the weapon holder object(child of player)


    [Header("Aim Control")]
    [SerializeField] private Transform aim;

    [SerializeField] private bool isAimingPrecisely;
    [SerializeField] private bool isLockingToTarget;

    [Header("Camera Control")]
    [SerializeField] private Transform cameraTarget;
    [Range(0.2f, 1f)]
    [SerializeField] private float minCameraDistance = .9f;
    [Range(0.5f, 3f)]
    [SerializeField] private float maxCameraDistance = 1.6f;
    [Range(1f, 10f)]
    [SerializeField] private float cameraSensitivity = 3f;

    [Tooltip("Smooth time used by SmoothDamp. Lower = snappier following.")] //remove if change update camera position to Lerp
    [SerializeField] private float cameraFollowSmoothTime = 0.12f; //remove if change update camera position to Lerp

    [Space]

    [SerializeField] private LayerMask aimLayerMask;

    private Vector2 mouseInput;
    private RaycastHit lastKnownMouseHit;
    private Vector3 cameraVelocity;  //remove if change update camera position to Lerp

    private void Start()
    {
        player = GetComponent<Player>();
        controls = player.controls;
        InitializeAimInput();
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.P))
            isAimingPrecisely = !isAimingPrecisely;

        if (Input.GetKeyDown(KeyCode.L))
            isLockingToTarget = !isLockingToTarget;

        UpdateAimVisuals();
        UpdateAimPosition();
        UpdateCameraPosition();
    }

    private void UpdateAimVisuals()
    {

        aimLaser.enabled = player.weapon.IsWeaponReady();

        if (aimLaser.enabled == false)
            return;

        WeaponModel weaponModel = player.weaponVisuals.CurrentWeaponModel();

        weaponModel.transform.LookAt(aim);
        weaponModel.gunPoint.LookAt(aim);

        Transform gunPoint = player.weapon.GunPoint();
        Vector3 laserDirection = player.weapon.BulletDirection();

        float laserTipLength = 0.5f;
        float gunDistance = player.weapon.CurrentWeapon().gunDistance;

        Vector3 endPoint = gunPoint.position + laserDirection * gunDistance;

        if (Physics.Raycast(gunPoint.position, laserDirection, out RaycastHit hitInfo, gunDistance, aimLayerMask)) {
            endPoint = hitInfo.point;
            laserTipLength = 0;

        }

        aimLaser.SetPosition(0, gunPoint.position);
        aimLaser.SetPosition(1, endPoint);
        aimLaser.SetPosition(2, endPoint + laserDirection * laserTipLength);
    }
    private void UpdateAimPosition()
    {
        Transform target = Target();

        if (target != null && isLockingToTarget) {
            aim.position = target.position;
            return;
        }

        aim.position = GetMouseHitInfo().point;

        if (!isAimingPrecisely)
            aim.position = new Vector3(aim.position.x, transform.position.y + 1, aim.position.z); // Keep the aim at the player's height
    }


    public Transform Target()
    {
        Transform target = null;
        var hitTransform = GetMouseHitInfo().transform;
        if (hitTransform != null && hitTransform.GetComponent<Target>() != null)
            target = hitTransform;
        return target;
    }
    public Transform Aim() => aim;
    public bool CanAimPrecisely() => isAimingPrecisely;
    public RaycastHit GetMouseHitInfo()
    {
        Ray ray = Camera.main.ScreenPointToRay(mouseInput);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, aimLayerMask)) {
            lastKnownMouseHit = hitInfo;
            return hitInfo;
        }

        return lastKnownMouseHit; // Placeholder for actual mouse position logic
    }

    #region Camera Region
    private void UpdateCameraPosition()
    {
        //cameraTarget.position = Vector3.Lerp(cameraTarget.position, DesiredCameraPosition(), cameraSensitivity * Time.deltaTime);
        cameraTarget.position = Vector3.SmoothDamp(cameraTarget.position, DesiredCameraPosition(), ref cameraVelocity, cameraFollowSmoothTime);
    }

    private Vector3 DesiredCameraPosition()
    {

        float actualMaxCameraDistance = player.movement.moveInput.y < -.5f ? minCameraDistance : maxCameraDistance;

        Vector3 desiredCameraPosition = GetMouseHitInfo().point;
        Vector3 aimDirection = (desiredCameraPosition - transform.position).normalized;

        float distanceToMouse = Vector3.Distance(transform.position, desiredCameraPosition);
        float clampedDistance = Mathf.Clamp(distanceToMouse, minCameraDistance, actualMaxCameraDistance);

        desiredCameraPosition = transform.position + aimDirection * clampedDistance;
        desiredCameraPosition.y = transform.position.y + 1; // Keep the aim at the player's height

        return desiredCameraPosition;
    }

    #endregion
    private void InitializeAimInput()
    {
        controls.Character.Aim.performed += context => mouseInput = context.ReadValue<Vector2>();
        controls.Character.Aim.canceled += context => mouseInput = Vector2.zero;
    }


}
