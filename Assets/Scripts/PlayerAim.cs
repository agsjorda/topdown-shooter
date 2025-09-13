using UnityEngine;

public class PlayerAim : MonoBehaviour {
    private Player player;
    private PlayerControls controls;


    [Header("Aim Control")]
    [SerializeField] private Transform aim;

    [Header("Camera Control")]
    [SerializeField] private Transform cameraTarget;
    [Range(0.5f, 1f)]
    [SerializeField] private float minCameraDistance = 1.5f;
    [Range(1f, 3f)]
    [SerializeField] private float maxCameraDistance = 2.5f;
    [Range(3f, 5f)]
    [SerializeField] private float cameraSensitivity = 3f;

    [Space]

    [SerializeField] private LayerMask aimLayerMask;

    private Vector2 aimInput;
    private RaycastHit lastKnownMouseHit;

    private void Start() {
        player = GetComponent<Player>();
        controls = player.controls;
        InitializeAimInput();
    }

    private void Update() {
        aim.position = GetMouseHitInfo().point;
        aim.position = new Vector3(aim.position.x, transform.position.y + 1, aim.position.z); // Keep the aim at the player's height

        cameraTarget.position = Vector3.Lerp(cameraTarget.position, DesiredCameraPosition(), cameraSensitivity * Time.deltaTime);
    }

    private Vector3 DesiredCameraPosition() {

        float actualMaxCameraDistance = player.movement.moveInput.y < -.5f ? minCameraDistance : maxCameraDistance;

        Vector3 desiredCameraPosition = GetMouseHitInfo().point;
        Vector3 aimDirection = (desiredCameraPosition - transform.position).normalized;

        float distanceToMouse = Vector3.Distance(transform.position, desiredCameraPosition);
        float clampedDistance = Mathf.Clamp(distanceToMouse, minCameraDistance, actualMaxCameraDistance);

        desiredCameraPosition = transform.position + aimDirection * clampedDistance;
        desiredCameraPosition.y = transform.position.y + 1; // Keep the aim at the player's height

        return desiredCameraPosition;
    }
    public RaycastHit GetMouseHitInfo() {
        Ray ray = Camera.main.ScreenPointToRay(aimInput);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, aimLayerMask)) {
            lastKnownMouseHit = hitInfo;
            return hitInfo;
        }

        return lastKnownMouseHit; // Placeholder for actual mouse position logic
    }

    private void InitializeAimInput() {
        controls.Character.Aim.performed += context => aimInput = context.ReadValue<Vector2>();
        controls.Character.Aim.canceled += context => aimInput = Vector2.zero;
    }


}
