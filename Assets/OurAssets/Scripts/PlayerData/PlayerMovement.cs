using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    PlayerAnimator playerAnimator; // David added. David - Using this instead of animator so other classes can change values if needed maybe like the direction the player falls when they die
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpHeight = 1f; // David - Changed to set jump height instead of velocity
    public float gravity = 10f;
    public float defaultHeight = 1.6f;
    public float crouchHeight = 0.8f;
    public float crouchSpeed = 3f;
    [SerializeField, Min(0f)]
    float groundDistance = 0.1f; // David added
    [SerializeField]
    LayerMask groundMask; // David added
    [SerializeField]
    Transform cameraTarget; // David added
    [SerializeField, Range(0f, 0.1f)]
    float deadZone = 0.05f; // David added
    [SerializeField, Min(0f)]
    float turnSpeed = 6f; // David added
    [SerializeField, Min(0f)]
    float runTurnSpeed = 12f; // David added
    [SerializeField, Range(0f, 1f)]
    float coyoteTime = 0.2f; // David added
    [SerializeField, Range(0f, 1f)]
    float jumpBuffer = 0.1f; // David added

    bool isGrounded; // David added

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController characterController;

    private bool canMove = true; // David - What is this for Abhi?
    bool isRunning = false; // David - Made this a member instead of local variable to work with new input system
    bool isCrouching = false; // David added

    Vector2 MoveInput => InputSystem.actions.FindAction("Move").ReadValue<Vector2>(); // David added
    bool crouchToggle = true; // David added (can maybe add a setting for players)

    Transform startingParent; // David added

    Transform lastStableGround; // David added
    Vector3 stableGroundOffset; // David added

    bool jumpInputWasPressed; // David added

    bool isJumping; // David added
    bool isFalling; // David added

    float currentCoyoteTime; // David added
    float currentJumpBuffer; // David added

    void Awake()
    {
        // David - Moved get component to awake because it's better to do here
        // compared to start
        characterController = GetComponent<CharacterController>();
        #region Add Action Listeners
        // David - Work with new input system
        InputSystem.actions.FindAction("Jump").started += HandleJumpInput;
        InputSystem.actions.FindAction("Jump").performed += HandleJumpInput;
        InputSystem.actions.FindAction("Jump").canceled += HandleJumpInput;
        InputSystem.actions.FindAction("Run").started += HandleRunInput;
        InputSystem.actions.FindAction("Run").canceled += HandleRunInput;
        InputSystem.actions.FindAction("Crouch").started += HandleCrouchInput;
        InputSystem.actions.FindAction("Crouch").canceled += HandleCrouchInput;
        #endregion
        startingParent = transform.parent; // David added
    }

    // David added
    void OnDestroy()
    {
        #region Remove Action Listeners
        InputSystem.actions.FindAction("Jump").started -= HandleJumpInput;
        InputSystem.actions.FindAction("Jump").performed -= HandleJumpInput;
        InputSystem.actions.FindAction("Jump").canceled -= HandleJumpInput;
        InputSystem.actions.FindAction("Run").started -= HandleRunInput;
        InputSystem.actions.FindAction("Run").canceled -= HandleRunInput;
        InputSystem.actions.FindAction("Crouch").started -= HandleCrouchInput;
        InputSystem.actions.FindAction("Crouch").canceled -= HandleCrouchInput;
        #endregion
    }

	void Update()
    {
        // David - character controller is grounded is very buggy I've found it
        // only updates every other frame, so I am using sphere cast as it is more
        // reliable. Also it's been a while since I first wrote this, but I think
        // I do sphere cast instead of check sphere because ig check sphere would
        // have a chance to leave a gap between it and the player especially if the
        // ground distance is large and sphere cast fixes that, idk if this actually
        // matters too much or not, but it's such an edge case that testing is hard
        isGrounded = transform.parent != startingParent
            || Physics.SphereCast(
            origin: transform.position + Vector3.up * (characterController.radius + 0.01f), // David - Move slightly up because otherwise returns false sometimes (specfically on start)
            radius: characterController.radius,
            direction: Vector3.down,
            hitInfo: out RaycastHit _,
            maxDistance: groundDistance + 0.01f, // David - Move slightly further to compensate for being slightly higher
            layerMask: groundMask,
            queryTriggerInteraction: QueryTriggerInteraction.Ignore); // David - On floating/moving platform or on ground

        // David - Set last stable ground if we're grounded and can move so can teleport back
        // if needed
        if (canMove)
        {
            if (isGrounded)
            {
				if (IsFullyOnStableGround) // David - Not partially over the edge
				{
					lastStableGround = transform.parent;
					stableGroundOffset = transform.position - lastStableGround.position;
				}
                currentCoyoteTime = coyoteTime;
            }
            else
            {
                currentCoyoteTime -= Time.deltaTime;
                currentJumpBuffer -= Time.deltaTime;
            }
            JumpChecks();
        }

        // David - Use the camera target forward and right instead of player for movement
        Vector3 forward = new Vector3(cameraTarget.forward.x, 0f, cameraTarget.forward.z).normalized;
        Vector3 right = new Vector3(cameraTarget.right.x, 0f, cameraTarget.right.z).normalized;

        // David - Only move horizontally if move input magnitude is above dead zone
        float xIn = MoveInput.x;
        float zIn = MoveInput.y;
        Vector3 movementDirectionXZ = Vector3.ClampMagnitude(forward * zIn + right * xIn, 1f);
        // David - merged curSpeedX and curSpeedZ into curSpeedXZ
        Vector3 curSpeedXZ = Vector3.zero;
        if (canMove && movementDirectionXZ.sqrMagnitude >= deadZone * deadZone)
        {
            // David - Rotate character to movement direction
            Quaternion targetRotation = Quaternion.LookRotation(movementDirectionXZ);
            float currentSpeedMultiplier = isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed); // David - This is so we don't hard code the values when we stop crouching
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Mathf.Clamp01((isRunning ? runTurnSpeed : turnSpeed) * Time.deltaTime));
            curSpeedXZ = movementDirectionXZ * currentSpeedMultiplier;
        }

        // David - Got rid of movementDirectionY because we can just use
        // (Vector3.up * moveDirection.y) to do the exact same thing
        moveDirection = curSpeedXZ + (Vector3.up * moveDirection.y);

        if (!isGrounded)
        {
            // David - Changed to moveDirection.y because movementDirectionY was local so
            // changing it here did nothing since it was only applied to moveDirection.y
            // before here
            moveDirection.y -= gravity * Time.deltaTime;
            if (moveDirection.y < 0f) isFalling = true;
        }

        // David - Shorter version of what Abhi wrote that also adjusts the centre since
        // player position is now at y = 0 and the speed is already calculated
        characterController.height = isCrouching && canMove ? crouchHeight : defaultHeight;
        characterController.center = new Vector3(characterController.center.x, characterController.height / 2f, characterController.center.z);

        SetAnimatorValues();

        characterController.Move(moveDirection * Time.deltaTime);
    }

	void SetAnimatorValues()
	{
        Vector3 hVel = moveDirection;
        hVel.y = 0f;
        playerAnimator.Speed = hVel.magnitude;
        playerAnimator.IsCrouching = isCrouching;
        playerAnimator.IsGrounded = isGrounded;
	}

	#region Input Handling
	// David - Added jump function to work with new input system
	void HandleJumpInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started || ctx.performed) jumpInputWasPressed = true;
        else if (ctx.canceled) jumpInputWasPressed = false;
    }

    // David - Added run function to work with new input system
    void HandleRunInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started) isRunning = true;
        else if (ctx.canceled) isRunning = false;
    }

    // David - Added crouch start function to work with new input system. When started, if crouchToggle
    // is off then player will always start crouching, if crouchToggle is on then isCrouching
    // toggles between true and false. When canceled, if crouchToggle is off then the player will always stop
    // crouching, if crouchToggle is on then isCrouching remains the same
    void HandleCrouchInput(InputAction.CallbackContext ctx)
    {
        if (ctx.started) isCrouching = !crouchToggle || !isCrouching;
        else if (ctx.canceled) isCrouching = crouchToggle && isCrouching;
    }
    #endregion

    #region Jumping
    void JumpChecks()
    {
        if (!canMove) return;
        if (jumpInputWasPressed) currentJumpBuffer = jumpBuffer;
        if (currentJumpBuffer > 0f && !isJumping && (isGrounded || currentCoyoteTime > 0f)) InitiateJump();
        if ((isJumping || isFalling) && isGrounded && moveDirection.y < 0f)
        {
            isJumping = false;
            isFalling = false;
            moveDirection.y = -1f;
        }
    }

    void InitiateJump()
    {
        isJumping = true;
        currentJumpBuffer = 0f;
        moveDirection.y = Mathf.Sqrt(2f * gravity * jumpHeight);
    }
    #endregion

    // David - Attach the player to the platform so that it follows the movement of the
    // platform
    public void AttachToPlatform(Transform platform) => transform.parent = platform;

    // David - Detach the player from the platform so that it stops following the movement
    // of the platform, but only do this if the parent is the platform we want to detach
    // from
    public void DetachFromPlatform(Transform platform)
    {
        if (transform.parent == platform) transform.parent = startingParent;
    }

    // David - Added ability to be able to enable input (when dialogue ends)
    public void EnableMovement() => canMove = true;

    // David - Added ability to be able to disable input (when dialogue starts)
    public void DisableMovement() => canMove = false;

	// David - Check if all extremes of the characterController collider are above the same
	// stable ground (none of them are over an edge or above a different collider)
	bool IsFullyOnStableGround
	{
		get
		{
			if (transform.parent == null) return false;
			// David - Check if all points are on ground
			if (!Physics.Raycast(
				origin: transform.position + transform.up * characterController.radius,
				direction: -transform.up,
				hitInfo: out RaycastHit hit0,
				maxDistance: characterController.radius + groundDistance,
				layerMask: groundMask,
                queryTriggerInteraction: QueryTriggerInteraction.Ignore)) return false;
			if (!Physics.Raycast(
				origin: transform.position + transform.up * characterController.radius + transform.forward * characterController.radius,
				direction: -transform.up,
				hitInfo: out RaycastHit hit1,
				maxDistance: characterController.radius + groundDistance,
				layerMask: groundMask,
				queryTriggerInteraction: QueryTriggerInteraction.Ignore)) return false;
			if (!Physics.Raycast(
				origin: transform.position + transform.up * characterController.radius - transform.forward * characterController.radius,
				direction: -transform.up,
				hitInfo: out RaycastHit hit2,
				maxDistance: characterController.radius + groundDistance,
				layerMask: groundMask,
				queryTriggerInteraction: QueryTriggerInteraction.Ignore)) return false;
			if (!Physics.Raycast(
				origin: transform.position + transform.up * characterController.radius + transform.right * characterController.radius,
				direction: -transform.up,
				hitInfo: out RaycastHit hit3,
				maxDistance: characterController.radius + groundDistance,
				layerMask: groundMask,
				queryTriggerInteraction: QueryTriggerInteraction.Ignore)) return false;
			if (!Physics.Raycast(
				origin: transform.position + transform.up * characterController.radius - transform.right * characterController.radius,
				direction: -transform.up,
				hitInfo: out RaycastHit hit4,
				maxDistance: characterController.radius + groundDistance,
				layerMask: groundMask,
				queryTriggerInteraction: QueryTriggerInteraction.Ignore)) return false;
			// David - Check if all points are on the same ground
			return hit0.collider == hit1.collider && hit1.collider == hit2.collider && hit2.collider == hit3.collider && hit3.collider == hit4.collider;
		}
	}

    // David - Reset the player to the last stable ground
    public void ResetToLastStableGround()
    {
        moveDirection = Vector3.up * -1f;
        transform.parent = lastStableGround;
        transform.position = lastStableGround.position + stableGroundOffset;
    }

    // got rid of floating issue.
}
