using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    PlayerManager playerManager;
    AnimatorManager animatorManager;
    InputManager inputManager;

    Vector3 moveDirection;
    Transform cameraObject;
    Rigidbody playerRigidBody;

    [Header("Falling")]
    public float inAirTimer;
    public float leapingVelocity;
    public float fallingVelocity;
    public float rayCastHeightOffSet = -5f;
    public LayerMask groundLayer;
    
    [Header("Movement Flags")]
    public bool isSprinting;
    public bool isGrounded;
    public bool isJumping;

    [Header("Movement Speeds")]
    public float walkingSpeed = 1.5f;
    public float runningSpeed = 5f;
    public float sprintingSpeed = 7;
    public float rotationSpeed = 15f;

    [Header("Jump Speeds")]
    public float jumpingHeight = 2;
    public float gravityIntensity = -15;

    private void Awake()
    {
        isGrounded = true;
        playerManager = GetComponent<PlayerManager>();
        inputManager = GetComponent<InputManager>();
        animatorManager = GetComponent<AnimatorManager>(); 
        playerRigidBody = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;
    }

    public void HandleAllMovement()
    {
        HandleFallingAndLanding();

        if(playerManager.isInteracting)
        {
            return;
        }
        HandleMovement();
        HandleRotation();
    }
    private void HandleMovement()
    {
        if (isJumping)
        {
            return;
        }
        moveDirection = cameraObject.forward * inputManager.verticalInput;
        moveDirection = moveDirection + cameraObject.right * inputManager.horizontalInput;
        moveDirection.Normalize();

        moveDirection.y = 0;

        if (isSprinting)
        {
            moveDirection = moveDirection * sprintingSpeed;
        }
        else
        {
            if (inputManager.moveAmount >= 0.5f)
            {
                moveDirection = moveDirection * runningSpeed;
            }
            else
            {
                moveDirection = moveDirection * walkingSpeed;
            }
        }


        

        Vector3 movementVelocity = moveDirection;
        playerRigidBody.linearVelocity = movementVelocity;  

    }

    private void HandleRotation()
    {
        if (isJumping)
        {
            return;
        }
        Vector3 targetDirection = Vector3.zero;


        targetDirection = cameraObject.forward * inputManager.verticalInput;

        targetDirection = targetDirection + cameraObject.right * inputManager.horizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;

        if (targetDirection == Vector3.zero)
        {
            targetDirection = transform.forward;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.rotation = playerRotation;
    }

    private void HandleFallingAndLanding()
    {
        RaycastHit hit;
        Vector3 rayCastOrigin = transform.position;
        Vector3 targetPosition;
        rayCastOrigin.y += rayCastHeightOffSet;
        targetPosition = transform.position;

        if (!isGrounded && !isJumping)
        {
            if (!playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Falling", true);
            }

            inAirTimer += Time.deltaTime;
            playerRigidBody.AddForce(transform.forward * leapingVelocity * Time.deltaTime);
            playerRigidBody.AddForce(-Vector3.up * fallingVelocity * inAirTimer * Time.deltaTime);
        }

        if (Physics.SphereCast(rayCastOrigin, 0.2f, -Vector3.up, out hit, groundLayer))
        {
            if (!isGrounded && !playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Landing", true);
                Debug.Log("Landed and played Landing animation");
            }


            Vector3 rayCastHitPoints = hit.point;
            targetPosition.y = rayCastHitPoints.y;
            inAirTimer = 0;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        if(isGrounded && !isJumping)
        {
            if (playerManager.isInteracting || inputManager.moveAmount >0) 
            { 
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime/0.1f);
            }
            else 
            { 
                transform.position = targetPosition;
            }
        }
    }

    public void HandleJumping()
    {
        if (isGrounded)
        {
            animatorManager.animator.SetBool("isJumping", true);
            animatorManager.PlayTargetAnimation("Jump", false);

            float jumpingVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpingHeight);
            Vector3 playerVelocity = moveDirection;
            playerVelocity.y = jumpingVelocity;
            playerRigidBody.linearVelocity = playerVelocity;
        }
    }
}
