using UnityEngine;

public class PlayerInputSet : MonoBehaviour {

    private PlayerControls controls;
    private CharacterController characterController;
    private Animator animator;
    private Transform meshTransform;

    [Header("Movement Info")]
    private Vector3 moveDirection;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float runSpeed = 3f;
    private float verticalVelocity;
    [SerializeField] private float gravity = 9.81f;
    private bool isRunning;

    [Header("Aim Info")]
    [SerializeField] private LayerMask aimLayerMask;
    private Vector3 lookingDirection;
    [SerializeField] private Transform aim;

    private Vector2 moveInput;
    private Vector2 aimInput;

    private void Awake() {
        AssignInputEvents();
    }

    private void Start() {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        meshTransform = animator.transform; // grab the mesh/animator child
        moveSpeed = walkSpeed;
    }

    private void Update() {
        ApplyMovement();
        AimTowardsMouse();
        AnimatorControllers();
    }
    private void LateUpdate() {
        // Make sure the mesh follows the parent exactly
        if (meshTransform != null) {
            meshTransform.localPosition = Vector3.zero;
            meshTransform.localRotation = Quaternion.identity;
        }
    }

    private void Shoot() {
        Debug.Log("Pew Pew");
        animator.SetTrigger("Fire");
    }
    private void AnimatorControllers() {
        float xVelocity = Vector3.Dot(moveDirection, transform.right);
        float zVelocity = Vector3.Dot(moveDirection, transform.forward);

        animator.SetFloat("xVelocity", xVelocity, .1f, Time.deltaTime);
        animator.SetFloat("zVelocity", zVelocity, .1f, Time.deltaTime);

        bool playRunAnimation = isRunning && moveInput.magnitude > 0;
        animator.SetBool("isRunning", playRunAnimation);
    }

    private void AimTowardsMouse() {
        Ray ray = Camera.main.ScreenPointToRay(aimInput);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, aimLayerMask)) {
            //Debug.DrawLine(ray.origin, hit.point, Color.red);
            lookingDirection = hitInfo.point - transform.position;
            lookingDirection.y = 0; // Ignore vertical aiming
            lookingDirection.Normalize();

            transform.forward = lookingDirection; // Rotate player to face the aim direction

            aim.position = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z); // Set aim position
        }
    }

    private void ApplyMovement() {
        moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        ApplyGravity();

        if (moveDirection.magnitude > 0) {
            characterController.Move(moveDirection * Time.deltaTime * moveSpeed);
        }
    }

    private void ApplyGravity() {
        if (!characterController.isGrounded) {
            verticalVelocity -= gravity * Time.deltaTime;
            moveDirection.y = verticalVelocity;
        } else {
            moveDirection.y = -.5f; // small downward force to keep grounded
        }
    }

    private void AssignInputEvents() {
        controls = new PlayerControls();

        controls.Character.Movement.performed += context => moveInput = context.ReadValue<Vector2>();
        controls.Character.Movement.canceled += context => moveInput = Vector2.zero;

        controls.Character.Aim.performed += context => aimInput = context.ReadValue<Vector2>();
        controls.Character.Aim.canceled += context => aimInput = Vector2.zero;

        controls.Character.Run.performed += context => {
            isRunning = true;
            moveSpeed = runSpeed;
        };
        controls.Character.Run.canceled += context => {
            isRunning = false;
            moveSpeed = walkSpeed;
        };

        controls.Character.Fire.performed += context => Shoot();

    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();
}
