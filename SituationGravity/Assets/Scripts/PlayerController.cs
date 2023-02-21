using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Store the controls from the input system
    [Header("Input Objects")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] public InputAction jumpAction, moveAction, sprintAction;

    [Header("Player Movement Settings - Changeable")]
    [SerializeField] public float playerMoveSpeed = 6.0f;
    [SerializeField] public float playerClimbingSpeed = 3.0f;
    [SerializeField] private float playerSprintSpeed = 12.0f;
    [SerializeField] private float playerAcceleration = 0.1f;
    [SerializeField] private float jumpForce = 1.0f;
    [SerializeField] private float doubleJumpForce = 2.0f;
    [SerializeField] private float gravityValue = -9.81f;


    [Header("Observables")]
    [SerializeField] public bool groundedPlayer = true;
    [SerializeField] public bool canDoubleJump = false;
    [SerializeField] public float controllerSpeed;
    [SerializeField] public Vector3 playerDirection;
    [SerializeField] public bool isHorizontalMoving = false;

    public CharacterController controller;
    public Transform camTransform;
    public Vector3 jumpVelocity;
    
    // Rotation Variables
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        climbing,
        crouching,
        sliding,
        air,
        idle
    }

    public bool climbing;

    private void StateHandler()
    {
        // Mode - Climbing
        if (climbing && !groundedPlayer)
        {
            state = MovementState.climbing;
            controllerSpeed = Mathf.Lerp(controllerSpeed, playerClimbingSpeed, playerAcceleration);
        }

        // Mode - Sprinting
        else if (sprintAction.IsPressed() && groundedPlayer && isHorizontalMoving)
        {
            state = MovementState.sprinting;
            controllerSpeed = Mathf.Lerp(controllerSpeed, playerSprintSpeed, playerAcceleration);
        }

        // Mode - Walking
        else if (groundedPlayer && isHorizontalMoving)
        {
            state = MovementState.walking;
            controllerSpeed = Mathf.Lerp(controllerSpeed, playerMoveSpeed, playerAcceleration);
        }
        // Mode - Air
        else if (!groundedPlayer)
        {
            state = MovementState.air;
            controllerSpeed = Mathf.Lerp(controllerSpeed, playerMoveSpeed, playerAcceleration);
        }
        else
        {
            state = MovementState.idle;
            controllerSpeed = Mathf.Lerp(controllerSpeed, 0, playerAcceleration);
        }
    }

    private void Awake()
    {
        // Input system 
        playerInput = GetComponent<PlayerInput>();
        jumpAction = playerInput.actions["Jump"];
        moveAction = playerInput.actions["Move"];
        sprintAction = playerInput.actions["Sprint"];
        controllerSpeed = 0;
    }

    private void Update()
    {
        // Player Movement Functions
        GroundCheck();
        PlayerMove();
        PlayerJump();
    }

    private void PlayerMove()
    {
        Vector2 direction2 = moveAction.ReadValue<Vector2>();
        Vector3 direction = new Vector3(direction2.x, transform.position.y, direction2.y);

        if (direction2.x != 0 || direction2.y != 0) isHorizontalMoving = true;
        else isHorizontalMoving = false;

        StateHandler();

        if (isHorizontalMoving)
        {
            // Face the direction of movement
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Move the Player
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            playerDirection = moveDirection.normalized * controllerSpeed * Time.deltaTime;
            controller.Move(playerDirection);
        }
        else
        {
            controllerSpeed = Mathf.Lerp(controllerSpeed, 0, playerAcceleration);
        }
        // Output to Console
        // Debug.Log(direction);
        // if (jumpAction.triggered) Debug.Log("Hell yeah");
        
    }

    private void GroundCheck()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && jumpVelocity.y < -1)
        {
            jumpVelocity.y = -0.5f;
            canDoubleJump = true;
        }
        if (groundedPlayer)
        {
            jumpVelocity.x = 0;
            jumpVelocity.z = 0;
        }
    }

    private void PlayerJump()
    {
        // Jump
        if (jumpAction.IsPressed() && groundedPlayer)
        {
            jumpVelocity.y = Mathf.Sqrt(jumpForce * -3.0f * gravityValue);
        }
        // Double Jump
        else if (jumpAction.triggered && !groundedPlayer && canDoubleJump)
        {
            jumpVelocity.y = Mathf.Sqrt(doubleJumpForce * -3.0f * gravityValue);
            canDoubleJump = false;
        }

        jumpVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(jumpVelocity * Time.deltaTime);
    }
}
