using System;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [Header("References")]
    private Player player;
    private PlayerControls controls;

    [Header("Aim Visuals")]
    [SerializeField] private LineRenderer aimLaser;
    [SerializeField] private Transform aimTransform;
    [SerializeField] private Transform cameraTarget;

    [Header("Aim Settings")]
    [SerializeField] private LayerMask aimLayerMask;
    [SerializeField] private bool isAimingPrecisely = false;
    [SerializeField] private bool isLockingToTarget = false;

    [Header("Camera Settings")]
    [Range(0.2f, 1f)]
    [SerializeField] private float minCameraDistance = 0.9f;
    [Range(0.5f, 3f)]
    [SerializeField] private float maxCameraDistance = 1.6f;
    [SerializeField] private float cameraFollowSmoothTime = 0.12f;

    // Internal state
    private Vector2 mouseInput;
    private RaycastHit lastKnownMouseHit;
    private Vector3 cameraVelocity;

    #region Properties
    public Transform Aim => aimTransform;
    public bool CanAimPrecisely => isAimingPrecisely;
    public bool IsLockingToTarget => isLockingToTarget;
    #endregion

    private void Start()
    {
        player = GetComponent<Player>();
        controls = player.controls;
        InitializeInput();
    }

    private void Update()
    {
        HandleDebugInput();
        UpdateAimVisuals();
        UpdateAimPosition();
        UpdateCameraPosition();
    }

    #region Input Handling
    private void InitializeInput()
    {
        controls.Character.Aim.performed += ctx => mouseInput = ctx.ReadValue<Vector2>();
        controls.Character.Aim.canceled += ctx => mouseInput = Vector2.zero;
    }

    private void HandleDebugInput()
    {
        // For debugging - remove in production
        if (Input.GetKeyDown(KeyCode.P))
            isAimingPrecisely = !isAimingPrecisely;

        if (Input.GetKeyDown(KeyCode.L))
            isLockingToTarget = !isLockingToTarget;
    }
    #endregion

    #region Aim Visuals
    private void UpdateAimVisuals()
    {
        bool shouldShowLaser = player.weapon?.IsWeaponReady ?? false;
        aimLaser.enabled = shouldShowLaser;

        if (!shouldShowLaser) return;

        UpdateWeaponAimDirection();
        UpdateLaserRenderer();
    }

    private void UpdateWeaponAimDirection()
    {
        WeaponModel weaponModel = player.weaponVisuals?.GetCurrentWeaponModel();
        if (weaponModel == null) return;

        // Make weapon and gun point look at aim
        weaponModel.transform.LookAt(aimTransform);
        weaponModel.gunPoint.LookAt(aimTransform);
    }

    private void UpdateLaserRenderer()
    {
        Transform gunPoint = player.weapon != null ? player.weapon.GunPoint() : null;
        if (gunPoint == null) return;

        // Fix CS1061: 'PlayerWeaponController' does not contain a definition for 'BulletDirection'
        // Use gunPoint.forward directly, since BulletDirection() does not exist.
        Vector3 laserDirection = gunPoint.forward;
        float gunDistance = player.weapon != null && player.weapon.CurrentWeapon != null
            ? player.weapon.CurrentWeapon.gunDistance
            : 4f;

        CalculateLaserEndPoints(gunPoint.position, laserDirection, gunDistance,
            out Vector3 mainEndPoint, out float laserTipLength);

        aimLaser.SetPositions(new Vector3[] {
            gunPoint.position,
            mainEndPoint,
            mainEndPoint + laserDirection * laserTipLength
        });
    }

    private void CalculateLaserEndPoints(Vector3 startPoint, Vector3 direction, float maxDistance,
        out Vector3 endPoint, out float tipLength)
    {
        tipLength = 0.5f;

        if (Physics.Raycast(startPoint, direction, out RaycastHit hit, maxDistance, aimLayerMask)) {
            endPoint = hit.point;
            tipLength = 0f;
        } else {
            endPoint = startPoint + direction * maxDistance;
        }
    }
    #endregion

    #region Aim Position
    private void UpdateAimPosition()
    {
        Transform target = GetCurrentTarget();

        if (target != null && isLockingToTarget) {
            aimTransform.position = target.position;
            return;
        }

        aimTransform.position = CalculateAimPosition();
    }

    private Vector3 CalculateAimPosition()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();

        if (!isAimingPrecisely) {
            // Keep aim at player's height for non-precise aiming
            mouseWorldPosition.y = transform.position.y + 1f;
        }

        return mouseWorldPosition;
    }

    public Transform GetCurrentTarget()
    {
        RaycastHit mouseHit = GetMouseRaycastHit();
        Transform hitTransform = mouseHit.transform;

        if (hitTransform != null && hitTransform.GetComponent<Target>() != null)
            return hitTransform;

        return null;
    }

    public RaycastHit GetMouseRaycastHit()
    {
        if (Camera.main == null) return lastKnownMouseHit;

        Ray ray = Camera.main.ScreenPointToRay(mouseInput);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, aimLayerMask)) {
            lastKnownMouseHit = hit;
            return hit;
        }

        return lastKnownMouseHit;
    }

    private Vector3 GetMouseWorldPosition()
    {
        return GetMouseRaycastHit().point;
    }
    #endregion

    #region Camera Control
    private void UpdateCameraPosition()
    {
        Vector3 targetPosition = CalculateCameraTargetPosition();
        cameraTarget.position = Vector3.SmoothDamp(
            cameraTarget.position,
            targetPosition,
            ref cameraVelocity,
            cameraFollowSmoothTime
        );
    }

    private Vector3 CalculateCameraTargetPosition()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3 aimDirection = (mouseWorldPos - transform.position).normalized;

        // Adjust max distance when moving backwards
        float actualMaxDistance = player.movement?.moveInput.y < -0.5f ?
            minCameraDistance : maxCameraDistance;

        float distanceToMouse = Vector3.Distance(transform.position, mouseWorldPos);
        float clampedDistance = Mathf.Clamp(distanceToMouse, minCameraDistance, actualMaxDistance);

        Vector3 cameraPosition = transform.position + aimDirection * clampedDistance;
        cameraPosition.y = transform.position.y + 1f; // Keep at player's height

        return cameraPosition;
    }
    #endregion

    #region Public API
    public void SetPreciseAiming(bool precise) => isAimingPrecisely = precise;
    public void SetTargetLocking(bool locking) => isLockingToTarget = locking;
    public Vector3 GetAimDirection() => (aimTransform.position - transform.position).normalized;
    #endregion
}