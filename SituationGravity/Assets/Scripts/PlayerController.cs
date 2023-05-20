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
    [SerializeField] private float climbJumpBackDeacceleration;
    [SerializeField] private float jumpForce = 1.0f;
    [SerializeField] private float doubleJumpForce = 2.0f;
    [SerializeField] private float gravityValue = -9.81f;

    [Header("Climb Settings")]
    [SerializeField] private float wallAngleMax;
    [SerializeField] private float groundAngleMax;
    [SerializeField] private LayerMask layerMaskClimbing;

    [Header("Heights")]
    [SerializeField] private float overpassHeight;
    [SerializeField] private float stepHeight;

    [Header("Offsets")]
    [SerializeField] private Vector3 climbOriginDown;
    [SerializeField] private Vector3 endOffset;

    [Header("Observables")]
    [SerializeField] public bool groundedPlayer = true;
    [SerializeField] public bool canDoubleJump = false;
    [SerializeField] public float controllerSpeed;
    [SerializeField] public Vector3 playerDirection;
    [SerializeField] public bool isHorizontalMoving = false;
    
    private bool isLedgeClimbing;

    public CharacterController controller;
    public Transform camTransform;
    public Vector3 jumpVelocity;
    public bool climbing;

    // Rotation Variables
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    [Header("Particle Systems")]
    [SerializeField] ParticleSystem doubleJumpJet;

    

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
        else if (groundedPlayer && isHorizontalMoving && !sprintAction.IsPressed())
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

        // FIX THIS CRAP
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
        // Reset vertical velocity when player touches the ground
        if (groundedPlayer && jumpVelocity.y < -1)
        {
            jumpVelocity.y = -0.5f;
            canDoubleJump = true;
        }
        // Cancel horizontal velocity when player touches the ground
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
        else if (jumpAction.triggered && !groundedPlayer && canDoubleJump && !climbing)
        {
            jumpVelocity = new Vector3(0, 0, 0);
            jumpVelocity.y = Mathf.Sqrt(doubleJumpForce * -3.0f * gravityValue);
            canDoubleJump = false;
            doubleJumpJet.Play();
        }

        // Slow down x velocity if not zero
        if (jumpVelocity.x != 0.0f) jumpVelocity.x = Mathf.Lerp(jumpVelocity.x, 0, climbJumpBackDeacceleration * Time.deltaTime);
        if (jumpVelocity.z != 0.0f) jumpVelocity.z = Mathf.Lerp(jumpVelocity.z, 0, climbJumpBackDeacceleration * Time.deltaTime);
        if (jumpVelocity.x < 0.1 && jumpVelocity.x > -0.1) jumpVelocity.x = 0;
        if (jumpVelocity.z < 0.1 && jumpVelocity.z > -0.1) jumpVelocity.z = 0;
        
        jumpVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(jumpVelocity * Time.deltaTime);
    }

    private bool CanLedgeClimb()
    {
        // Whether or not our raycast hits something
        bool downHit,
            forwardHit,
            overpassHit;

        float climbHeight,
            groundAngle,
            wallAngle;

        // What our raycast hits
        RaycastHit downRaycastHit,
            forwardRaycastHit,
            overpassRaycastHit;

        Vector3 endPosition,
            forwardDirectionXZ,
            forwardNormalXZ;

        Vector3 downDirection = Vector3.down;
        Vector3 downOrigin = transform.TransformPoint(climbOriginDown);

        downHit = Physics.Raycast(downOrigin, downDirection, out downRaycastHit, climbOriginDown.y - stepHeight, layerMaskClimbing);
        Debug.DrawRay(downOrigin, downDirection, Color.green);
        if (downHit)
        {
            // Debug.DrawRay(transform.position, forward, Color.green);
            // Walk Forward and overpass cast
            float forwardDistance = climbOriginDown.z;
            // How far forward the downward origin is of the character (where the character is now, but just below (0.1) the downhit point)
            Vector3 forwardOrigin = new Vector3(transform.position.x, downRaycastHit.point.y - 0.1f, transform.position.z);
            // Where the character is, but at the overpass height
            Vector3 overpassOrigin = new Vector3(transform.position.x, overpassHeight, transform.position.z);

            // The direction of our cast projected into the XZ plane
            forwardDirectionXZ = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

            // Say if the forward and/or overpass raycasts hit and say what objects they hit (out forwardRaycastHit, out overpassRaycastHit)
            forwardHit = Physics.Raycast(forwardOrigin, forwardDirectionXZ, out forwardRaycastHit, forwardDistance, layerMaskClimbing);
            overpassHit = Physics.Raycast(overpassOrigin, forwardDirectionXZ, out overpassRaycastHit, forwardDistance, layerMaskClimbing);
            // How high is the ledge - subtract player's global position from where the down raycast hits
            climbHeight = downRaycastHit.point.y - transform.position.y;

            if (forwardHit)
                if (overpassHit || climbHeight < overpassHeight)
                {
                    // Angles - facing the wall - YES
                    forwardNormalXZ = Vector3.ProjectOnPlane(forwardRaycastHit.normal, Vector3.up);
                    groundAngle = Vector3.Angle(downRaycastHit.normal, Vector3.up);
                    wallAngle = Vector3.Angle(-forwardNormalXZ, forwardDirectionXZ);

                    if (wallAngle <= wallAngleMax)
                        if(groundAngle <= groundAngleMax)
                        {
                            // Get coefficients of top surface (takes into account an uneven top surface)
                            Vector3 vectSurface = Vector3.ProjectOnPlane(forwardDirectionXZ, downRaycastHit.normal);
                            // Calculate the end position of the ledge (how much space is needed to climb up) by using ledge (even if ledge is uneven) by multiplying it by the vectSurface factor
                            endPosition = downRaycastHit.point + Quaternion.LookRotation(vectSurface, Vector3.up) * endOffset;

                            // De-penetration
                            // The ledge's collider
                            Collider colliderB = downRaycastHit.collider;
                            // To see if there is an overlap
                            bool penetrationOverlap = Physics.ComputePenetration(
                                colliderA: controller,
                                positionA: endPosition,
                                rotationA: transform.rotation,
                                colliderB: colliderB,
                                positionB: colliderB.transform.position,
                                rotationB: colliderB.transform.rotation,
                                direction: out Vector3 penetrationDirection,
                                distance: out float penetrationDistance);
                            if (penetrationOverlap)
                                endPosition += penetrationDirection * penetrationDistance;

                            // Up Sweep

                            // Forward Sweep
                        }
                }
        }

        return false;
    }
    private bool CharacterSweep(Vector3 position, Quaternion rotation, Vector3 direction, float distance, LayerMask layerMask, float inflate)
    {
        return false;
    }
}
