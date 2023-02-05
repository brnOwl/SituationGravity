using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Store the controls from the input system
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InputAction jumpAction, moveAction, sprintAction;
    [SerializeField] private float playerSpeed = 6.0f;
    [SerializeField] private float playerSprintSpeed = 12.0f;
    [SerializeField] private float jumpForce = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;
    //[SerializeField] private Rigidbody rigidBody;
    [SerializeField] private bool groundedPlayer = true;

    public CharacterController controller;
    public Transform camTransform;
    private Vector3 playerVelocity;
    private float controllerSpeed;
    

    // Rotation Variables
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    private void Awake()
    {
        // Input system 
        playerInput = GetComponent<PlayerInput>();
        jumpAction = playerInput.actions["Jump"];
        moveAction = playerInput.actions["Move"];
        sprintAction = playerInput.actions["Sprint"];
        
        
    }

    private void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        // Player Movement Values
        Vector2 direction2 = moveAction.ReadValue<Vector2>();
        Vector3 direction = new Vector3(direction2.x, transform.position.y, direction2.y);

        if (direction2.x != 0 || direction2.y != 0)
        {
            // Face the direction of movement
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            if (sprintAction.IsPressed()) controllerSpeed = playerSprintSpeed;
            else controllerSpeed = playerSpeed;

            // Move the Player
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDirection.normalized * controllerSpeed * Time.deltaTime);

            
        }

        

        // Changes the height position of the player
        if (jumpAction.triggered && groundedPlayer)
        {
            //rigidBody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            playerVelocity.y += Mathf.Sqrt(jumpForce * -3.0f * gravityValue);
            
        }
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        //playerVelocity.y += gravityValue * Time.deltaTime;

        // Output to Console
        Debug.Log(direction);
        if (jumpAction.triggered) Debug.Log("Hell yeah");
    }
    
}
