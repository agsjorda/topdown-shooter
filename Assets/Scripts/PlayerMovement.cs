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
    public float jumpHeight = 2.0f; // How high the character can jump

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
        if (Input.GetKeyDown(KeyCode.Space) && characterController.isGrounded) {
            Jump();
        }

        CalculateGravity(); // Now ONLY updates verticalVelocity, doesn't apply it.
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
    private void CalculateGravity() {
        if (characterController.isGrounded) {
            // If grounded, reset vertical velocity to a small negative value to stay planted.
            // Only do this if we aren't moving upwards (i.e., not jumping this frame)
            if (verticalVelocity < 0) {
                verticalVelocity = -2f; // A stronger force to ensure grounding
            }
        } else {
            // If NOT grounded, apply gravity to accelerate downward
            verticalVelocity -= gravity * Time.deltaTime;
        }
    }
    private void Jump() {
        // Physics formula: v = sqrt(2 * gravity * jumpHeight)
        Debug.Log("Jump!");
        verticalVelocity = Mathf.Sqrt(2f * jumpHeight * gravity);
        // Optional: Set a flag for animations
        //isJumping = true;
        // Optional: Trigger a jump animation if you have one
        // animator.SetTrigger("Jump");
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
        Vector3 lookingDirection = player.aim.GetMouseHitInfo().point - transform.position;
        lookingDirection.y = 0; // Ignore vertical aiming

        if (lookingDirection.sqrMagnitude > 0.0001f) {
            lookingDirection.Normalize();
            Quaternion desiredRotation = Quaternion.LookRotation(lookingDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, turnSpeed * Time.deltaTime);
        }
        // else: Do nothing, keep current rotation
    }

    private void ApplyMovement() {
        // 1. Create HORIZONTAL movement direction from input AND STORE IT IN moveDirection
        moveDirection = new Vector3(moveInput.x, 0, moveInput.y); // <-- This line is crucial for animations
                                                                  // Apply speed to horizontal movement
        Vector3 horizontalVelocity = moveDirection * moveSpeed; // Use a temporary variable for speed

        // 2. Create the FINAL movement vector by COMBINING horizontal and vertical
        Vector3 finalMovement = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);

        // 3. Move the character using the combined vector
        characterController.Move(finalMovement * Time.deltaTime);
    }

    //private void ApplyGravity() {
    //    if (!characterController.isGrounded) {
    //        verticalVelocity -= gravity * Time.deltaTime;
    //        moveDirection.y = verticalVelocity;
    //    } else {
    //        moveDirection.y = -.5f; // small downward force to keep grounded
    //    }
    //}

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
