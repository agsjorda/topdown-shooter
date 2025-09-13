using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    private Player player;

    private PlayerControls controls;
    private CharacterController characterController;
    private Animator animator;
    private Transform meshTransform;

    [Header("Movement Info")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float runSpeed = 3f;
    [SerializeField] private float turnSpeed;
    private float verticalVelocity;

    public Vector2 moveInput { get; private set; }
    private Vector3 moveDirection;

    [SerializeField] private float gravity = 9.81f;
    private bool isRunning;


    private void Start() {
        player = GetComponent<Player>();

        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        meshTransform = animator.transform; // grab the mesh/animator child
        moveSpeed = walkSpeed;

        AssignInputEvents();
    }

    private void Update() {
        ApplyMovement();
        ApplyRotation();
        AnimatorControllers();
    }
    private void LateUpdate() {
        // Make sure the mesh follows the parent exactly
        if (meshTransform != null) {
            meshTransform.localPosition = Vector3.zero;
            meshTransform.localRotation = Quaternion.identity;
        }
    }

    private void AnimatorControllers() {
        float xVelocity = Vector3.Dot(moveDirection, transform.right);
        float zVelocity = Vector3.Dot(moveDirection, transform.forward);

        animator.SetFloat("xVelocity", xVelocity, .1f, Time.deltaTime);
        animator.SetFloat("zVelocity", zVelocity, .1f, Time.deltaTime);

        bool playRunAnimation = isRunning && moveInput.magnitude > 0;
        animator.SetBool("isRunning", playRunAnimation);
    }

    private void ApplyRotation() {

        //Debug.DrawLine(ray.origin, hit.point, Color.red);
        Vector3 lookingDirection = player.aim.GetMouseHitInfo().point - transform.position;
        lookingDirection.y = 0; // Ignore vertical aiming
        lookingDirection.Normalize();

        Quaternion desiredRotation = Quaternion.LookRotation(lookingDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, turnSpeed * Time.deltaTime);
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
        controls = player.controls;

        controls.Character.Movement.performed += context => moveInput = context.ReadValue<Vector2>();
        controls.Character.Movement.canceled += context => moveInput = Vector2.zero;



        controls.Character.Run.performed += context => {
            isRunning = true;
            moveSpeed = runSpeed;
        };
        controls.Character.Run.canceled += context => {
            isRunning = false;
            moveSpeed = walkSpeed;
        };

    }

}
