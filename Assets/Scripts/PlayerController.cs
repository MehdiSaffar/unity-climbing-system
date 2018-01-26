using System;
using System.Linq;

using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Component used to control the player's character and keep track of its states
/// </summary>
[RequireComponent(typeof(CharacterAnimationController), typeof(Rigidbody))]
public class PlayerController : MyMonoBehaviour{
	#region Components
	/// <summary>
	/// Character animation controller
	/// </summary>
	private CharacterAnimationController controller;

	/// <summary>
	/// Rigidbody component
	/// </summary>
	private Rigidbody rb;
	#endregion

	/// <summary>
	/// Reference to the level's list of climb points
	/// </summary>
	[SerializeField] private Transform pointList;

	/// <summary>
	/// Closest possible climb point within <see cref="pointDistanceThreshold"/>
	/// </summary>
	private Point closestClimbPoint;

	[Header("Character")]
	/// <summary>
	/// Contains the information of the hang if the player is in climb mode
	/// </summary>
	[SerializeField]
	private HangInfo _hang;

	#region Gizmos Settings
	/// <summary>
	/// Should the velocity be displayed?
	/// </summary>
	[SerializeField] private bool showVelocityVector;

	/// <summary>
	/// Color of the velocity vector
	/// </summary>
	[SerializeField] private Color velocityVectorColor;

	/// <summary>
	/// Should the camera desire vectors be displayed?
	/// </summary>
	[SerializeField] private bool showCameraDesireVectors;

	/// <summary>
	/// Color of the camera desire vectos
	/// </summary>
	[SerializeField] private Color cameraDesireVectorColor;

	/// <summary>
	/// Should the character's axis be shown?
	/// </summary>
	[SerializeField] private bool showCharacterAxis;

	/// <summary>
	/// Color of the player's forward and right vectors
	/// </summary>
	[SerializeField] private Color characterAxisVectorColor;

	/// <summary>
	/// Should the ground ray be shown?
	/// </summary>
	[SerializeField] private bool showGroundAxis;
	/// <summary>
	/// Color of the ground ray
	/// </summary>
	[SerializeField] private Color groundAxisVectorColor;

	/// <summary>
	/// Should the vector to the closest climbable point be shown?
	/// </summary>
	[SerializeField] private bool showClosestPointVector;
	/// <summary>
	/// Color of the vector to the closest point
	/// </summary>
	[SerializeField] private Color closestPointVectorColor;

	/// <summary>
	/// Should the collision normal be shown?
	/// </summary>
	[SerializeField] private bool showCollisionNormal;

	/// <summary>
	/// Color of the collision normal
	/// </summary>
	[SerializeField] private Color collisionNormalColor;

	/// <summary>
	/// Length of the gizmo ray shooting to the right of the player
	/// </summary>
	[Header("Gizmo")] public float rightRayLength;

	/// <summary>
	/// Length of the gizmo ray shooting in front of the player
	/// </summary>
	public float forwardRayLength;

	/// <summary>
	/// Length of the gizmo ray shooting under the feet of the player
	/// </summary>
	public float downRayLength;
	#endregion

	/// <summary>
	/// Reference to the transform of the right point
	/// </summary>
	public Transform rightFoot;

	/// <summary>
	/// Reference to the transform of the left point
	/// </summary>
	public Transform leftFoot;

	/// <summary>
	/// Position of the midpoint of the two feet (taking the y coordinate of the lowest of either feet)
	/// </summary>
	private Vector3 feetMidpoint{
		get
		{
			var midpointPosition = Vector3.Lerp(rightFoot.position, leftFoot.position, 0.5f);
			if (leftFoot.position.y < rightFoot.position.y) {
				midpointPosition.y = leftFoot.position.y;
			}
			else {
				midpointPosition.y = rightFoot.position.y;
			}
			return midpointPosition;
		}
	}

	/// <summary>
	/// Maximum distance between a climb point and the player
	/// </summary>
	[Header("Threshold")] [SerializeField] private float pointDistanceThreshold;

	/// <summary>
	/// Maximum distance between the feet of the player and the ground to be considered grounded
	/// </summary>
	[SerializeField] private float groundDistanceThreshold;

	/// <summary>
	/// Current distance of the feet from the ground
	/// </summary>
	private float distanceToGround;

	private float _pointSqrDistanceThreshold;

	#region Current Abilities
	/// <summary>
	/// Can the player jump?
	/// </summary>
	[Header("Abilities")]
	[SerializeField] private bool canJump;
	/// <summary>
	/// Can the player climb?
	/// </summary>
	[SerializeField] private bool canClimb;
	/// <summary>
	/// CAn the player crouch?
	/// </summary>
	[SerializeField] private bool canCrouch;
	/// <summary>
	/// Can the player walk?
	/// </summary>
	[SerializeField] private bool canWalk;
	/// <summary>
	/// Can the player jog?
	/// </summary>
	[SerializeField] private bool canJog;
	/// <summary>
	/// Can the player hang?
	/// </summary>
	[SerializeField] private bool canHang;
	#endregion

	#region Current Player States
	/// <summary>
	/// Is the player colliding with a wall?
	/// </summary>
	[SerializeField] private bool isCollidingWithWall;

	/// <summary>
	/// Is the player grounded?
	/// </summary>
	public bool _isGrounded;

	/// <summary>
	/// Is the player crouching?
	/// </summary>
	public bool _isCrouching;

	/// <summary>
	/// Is the player midair?
	/// </summary>
	public bool _isMidair;

	/// <summary>
	/// Is the player hanging?
	/// </summary>
	public bool _isHanging;

	/// <summary>
	/// Is the player landing from a jump?
	/// </summary>
	public bool _isLandingFromJump;

	/// <summary>
	/// Is the player falling?
	/// </summary>
	public bool _isFalling;

	/// <summary>
	/// Is the player climbing?
	/// </summary>
	public bool _isClimbing;

	/// <summary>
	/// Is the player jumping?
	/// </summary>
	[Header("States")] public bool _isJumping;

	/// <summary>
	/// Is the character strafing right?
	/// </summary>
	[SerializeField] private bool isStrafingRight;

	/// <summary>
	/// Is the character strafing left?
	/// </summary>
	[SerializeField] private bool isStrafingLeft;
	#endregion

	#region Previous Player States
	/// <summary>
	/// Was the player jumping in the last frame?
	/// </summary>
	[UsedImplicitly] public bool _wasJumping;

	/// <summary>
	/// Was the player midair in the last frame?
	/// </summary>
	[UsedImplicitly] public bool _wasMidair;

	/// <summary>
	/// Was the player grounded in the last frame?
	/// </summary>
	[UsedImplicitly] public bool _wasGrounded;

	/// <summary>
	/// Was the player crouching in the last frame?
	/// </summary>
	[UsedImplicitly] public bool _wasCrouching;

	/// <summary>
	/// Was the player hanging in the last frame?
	/// </summary>
	[UsedImplicitly] public bool _wasHanging;

	/// <summary>
	/// Was the player falling in the last frame?
	/// </summary>
	[UsedImplicitly] public bool _wasFalling;

	/// <summary>
	/// Was the player climbing in the last frame?
	/// </summary>
	[UsedImplicitly] public bool _wasClimbing;
	#endregion

	#region Speeds
	/// <summary>
	/// Strafe speed
	/// </summary>
	[SerializeField] private float strafeSpeed;

	/// <summary>
	/// Walk speed constant
	/// </summary>
	public float walkSpeed;

	/// <summary>
	/// Jog speed constant
	/// </summary>
	public float jogSpeed;

	/// <summary>
	/// Speed of the player in the last frame
	/// </summary>
	private float lastSpeed;

	/// <summary>
	/// Vertical jump starting velocity constant
	/// </summary>
	public float jumpVelocityY;

	/// <summary>
	/// Current vertical velocity of the player
	/// </summary>
	[Header("Speeds")]
	public float currentVelocityY;

	/// <summary>
	/// Maximum vertical velocity beyond which the player is considered falling
	/// </summary>
	public float fallVelocityThreshold;

	public float CurrentVelocityY{
		get { return currentVelocityY; }
		set { currentVelocityY = value; }
	}
	#endregion

	#region Input States
	/// <summary>
	/// Current input vector (y axis: forward, x axis: sideways)
	/// </summary>
	private Vector3 inputVector;

	/// <summary>
	/// Is the player able to input?
	/// </summary>
	private bool IsInputLocked => _isJumping || _isFalling;

	/// <summary>
	/// Is the spacebar pressed up?
	/// </summary>
	private bool spaceDown;

	/// <summary>
	/// Is the crouch button pressed up?
	/// </summary>
	private bool crouchDown;

	/// <summary>
	/// Is the hanging button pressed up?
	/// </summary>
	private bool hangDown;

	/// <summary>
	/// Is the right movement button pressed up?
	/// </summary>
	private bool rightDown;

	/// <summary>
	/// Is the left movement button pressed up?
	/// </summary>
	private bool leftDown;

	/// <summary>
	/// Is the shift button pressed up?
	/// </summary>
	private bool shiftPressed;

	/// <summary>
	/// Is the forward movement button pressed?
	/// </summary>
	private bool forwardPressed;

	/// <summary>
	/// Is the backward movement button pressed?
	/// </summary>
	private bool backwardPressed;

	/// <summary>
	/// Is the left movement button pressed?
	/// </summary>
	private bool leftPressed;

	/// <summary>
	/// Is the right movement button pressed?
	/// </summary>
	private bool rightPressed;
	#endregion

	#region Camera
	private Vector3 cameraForwardDesireVector;
	private Vector3 cameraRightDesireVector;
	#endregion

	/// <summary>
	/// Duration of the shimmy animation
	/// </summary>
	[UsedImplicitly] [HideInInspector] [SerializeField] private float shimmyAnimationDuration;

	private float timelapseShimmy;

	/// <summary>
	/// Contains the IK positions of every body part
	/// </summary>
	private IKPositions ik;

	/// <summary>
	/// Normal of the wall the player is colliding against (if he is)
	/// </summary>
	private Vector3 collisionNormal;

	/// <summary>
	/// Point of collision (if any)
	/// </summary>
	private Vector3 collisionPoint;

	[SerializeField] private Vector3 targetInputVector;
	[SerializeField] private float lerpSpeed;

	private void Awake(){
		controller = GetComponent<CharacterAnimationController>();
		rb = GetComponent<Rigidbody>();
		ik = new IKPositions();
		_hang = new HangInfo();

	}

	private void Start(){
		controller.walkSpeed = walkSpeed;
		controller.jogSpeed = jogSpeed;

		_pointSqrDistanceThreshold = pointDistanceThreshold * pointDistanceThreshold;
	}

	private void FixedUpdate(){
//		closestClimbPoint = GetClosestPoint();
		GetClosestPoint();
		// Check if the player is grounded or midair
		CheckIsGrounded();

		// Sets the "keyPressed"...
		GetInputState();

		// Set the camera desire vectors
		SetCameraDesireVectors();

		// Movement input vector
		GetInputVector();

		// State transition conditions
		CheckAbilities();

		CheckStateChanges();

		UpdateTransform();
		SetAnimationControllerStates();
		SetLastFrameStates();

	}

	/// <summary>
	/// Checks if the player needs to change state (from moving forward to jumping or climbing for example)
	/// based on the input and current abilities
	/// </summary>
	private void CheckStateChanges(){
		if (!_isFalling && !_isGrounded && rb.velocity.y < fallVelocityThreshold) {
			Debug.Log("Falling!!");
			_isFalling = true;
		}

		if (_isGrounded) {
			_isFalling = false;
		}

		// Jump
		if (_isJumping) {
			if (rb.velocity.y < -0.01 && _isGrounded || _isLandingFromJump) {
				Debug.Log("Stopped Jumping");
				_isJumping = false;
				_isFalling = false;
			}
		}
		else {
			if (canJump && spaceDown) {
				_isJumping = true;
				Debug.Log("Started Jumping");
			}
		}

		if (crouchDown && canCrouch) {
			_isCrouching = !_isCrouching;
			Debug.Log(_isCrouching ? "Started Crouching" : "Stopped Crouching");
		}

		if (hangDown) {
			if (_isHanging && _hang.state == HangInfo.HangState.Final) {
				Unhang();
			}
			else if (canHang) {
				Hang(closestClimbPoint);
			}
		}

		if (_isHanging) {
			if (rightDown) {
				Shimmy(HangInfo.Direction.Right);
			}

			if (leftDown) {
				Shimmy(HangInfo.Direction.Left);
			}

			if (spaceDown && canClimb) {
				Climb();
			}
		}
	}

	/// <summary>
	/// Checks the abilities of the player for the current frame
	/// </summary>
	private void CheckAbilities(){
		canJump = !_isJumping && _isGrounded && !_isCrouching;
		canCrouch = _isGrounded;
		canWalk = /*_isGrounded && */!_isJumping;
		canJog = /*_isGrounded && */!_isCrouching /* && !_isJumping*/;
		canHang = _isGrounded && closestClimbPoint != null && !_isCrouching && !_isJumping;
		canClimb = _isHanging;
		// If the point requires the player to be facing a certain way, then we add the condition
		if (canHang && closestClimbPoint.HasNormal) {
			canHang = Vector3.Dot(transform.forward, closestClimbPoint.normal) <= -0.9f;
		}
	}

	/// <summary>
	/// Checks if the character is grounded and sets the corresponding field
	/// </summary>
	private void CheckIsGrounded(){
		var ray = new Ray(feetMidpoint + Vector3.up * 0.04f, Vector3.down);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, downRayLength)) {
			distanceToGround = hit.distance - 0.04f;
		}
		else {
			distanceToGround = downRayLength - 0.04f;
		}

		if (distanceToGround >= groundDistanceThreshold) {
			_isMidair = true;
			_isGrounded = false;
		}
		else {
			_isMidair = false;
			_isGrounded = true;
		}

		controller.distanceToGround = distanceToGround;
	}

	/// <summary>
	/// Returns the a vector representing how the "user input" is poiting
	/// Y contains the Y axis information (can be interpreted as move forward / backward)
	/// X contains the X axis information (can be interpreted as move rightward / leftward)
	/// </summary>
	/// <returns>The input vector</returns>
	private void GetInputVector(){
		inputVector = Vector3.zero;
		// if the input is locked (such as when falling) we return a zero vector
		if (IsInputLocked)
			return;
		if (forwardPressed)
			inputVector.y++;
		if (backwardPressed)
			inputVector.y--;
		if (rightPressed)
			inputVector.x++;
		if (leftPressed)
			inputVector.x--;
	}

	/// <summary>
	/// Sets the camera desire vectors in memory
	/// </summary>
	private void SetCameraDesireVectors(){
		var mainCamera = Camera.main;
		var cameraForward = mainCamera.transform.forward;
		var cameraRight = mainCamera.transform.right;
			cameraForwardDesireVector = Vector3.Slerp(
				cameraForwardDesireVector,
				Vector3.ProjectOnPlane(cameraForward, transform.up).normalized,
				lerpSpeed);
		cameraRightDesireVector = Vector3.Slerp(
			cameraRightDesireVector,
			Vector3.ProjectOnPlane(cameraRight, transform.up).normalized,
			lerpSpeed);
	}

	/// <summary>
	/// Gets the input state (of the keyboard for now)
	/// </summary>
	private void GetInputState(){
		// Action keys
		spaceDown = Input.GetKeyDown(KeyCode.Space);
		crouchDown = Input.GetKeyDown(KeyCode.C);
		hangDown = Input.GetKeyDown(KeyCode.LeftControl);
		//upDown = Input.GetKeyDown(KeyCode.W);
		rightDown = Input.GetKeyDown(KeyCode.D);
		leftDown = Input.GetKeyDown(KeyCode.A);
		// State keys
		shiftPressed = Input.GetKey(KeyCode.LeftShift);
		// Movement keys
		forwardPressed = Input.GetKey(KeyCode.W);
		backwardPressed = Input.GetKey(KeyCode.S);
		leftPressed = Input.GetKey(KeyCode.A);
		rightPressed = Input.GetKey(KeyCode.D);
	}

	/// <summary>
	/// Updates velocity, rotation etc. of the character
	/// </summary>
	/// <param name="inputVector">Input of the player</param>
	/// <param name="canWalk">Can the player walk?</param>
	/// <param name="canJog">Can the player jog?</param>
	/// TODO: Requires refactoring of the parameters
	private void UpdateTransform(){
		targetInputVector = inputVector == Vector3.zero
			? inputVector
			: Vector3.Slerp(targetInputVector, inputVector, lerpSpeed);
//		targetInputVector = inputVector;
		var forward = (cameraForwardDesireVector * targetInputVector.y);
		var right = (cameraRightDesireVector * targetInputVector.x);
		float speed;
		if (_isJumping || _isHanging || _isFalling || _isClimbing) {
			// We keep the same speed as last time and the same forward
			speed = lastSpeed;
			forward = transform.forward;
		}
		else {
			if (_isCrouching && canWalk) {
				speed = walkSpeed;
			}
			else if (shiftPressed && canWalk) {
				speed = walkSpeed;
			}
			else if (!shiftPressed && canJog) {
				speed = jogSpeed;
			}
			else {
				speed = 0f;
			}
		}

		// We can update the rigidbody's velocity when he is neither falling nor hanging/climbing
		if (!_isHanging || !_isFalling) {
			var worldVelocity = (forward + right).normalized * speed;

			worldVelocity.y = rb.velocity.y;

			if (isCollidingWithWall && Vector3.Dot(worldVelocity, collisionNormal) < 0f) {
				worldVelocity = Vector3.ProjectOnPlane(worldVelocity, collisionNormal);
			}


			// We let the animation controller know about the character's current velocity
			// relative to it's forward...
			if (_isCrouching) {
				controller.localVelocity.z = targetInputVector == Vector3.zero ? 0 : speed;
			}
			else {
				controller.localVelocity.z = targetInputVector.y * speed;
			}
			controller.localVelocity.x = targetInputVector.x * speed;
			controller.localVelocity.y = worldVelocity.y;

			// Set the new rigidbody velocity
			rb.velocity = worldVelocity;

			if(_isHanging) {}
			else if (!_isCrouching) {
				// Make the character face the camera view
				transform.rotation = Quaternion.LookRotation(cameraForwardDesireVector, Vector3.up);
			}
			else {
				// Because we do not have strafing animations, we will have to make the player face it's velocity
				// and not the camera
				if(worldVelocity != Vector3.zero)
					transform.rotation = Quaternion.LookRotation(worldVelocity, Vector3.up);
			}

		}
		currentVelocityY = rb.velocity.y;

		// We keep the current speed in memory for next frame
		lastSpeed = speed;
	}

	/// <summary>
	/// Gets the closest climbing point below the distance threshold
	/// </summary>
	private void GetClosestPoint(){
		var points = pointList.GetComponentsInChildren<Point>();
		float minimalSqrDistance = _pointSqrDistanceThreshold;
		closestClimbPoint = null;
		foreach (var point in points) {
			var displacement = point.transform.position - transform.position;
			var direction = displacement.normalized;
			float sqrDistance = displacement.sqrMagnitude;
			var closeEnough = sqrDistance < minimalSqrDistance;
			var inFront = Vector3.Dot(transform.forward, direction) >= 0;
			var upEnough = Vector3.Dot(transform.up, direction) >= 0.5f;
			var facingNormal = Vector3.Dot(transform.forward, -point.normal) >= 0.9f;
			if (closeEnough && inFront && upEnough && facingNormal) {
				minimalSqrDistance = sqrDistance;
				closestClimbPoint = point;
			}
		}

	}

	/// <summary>
	/// Sets the IK position of the right hand to the <paramref name="point"/>
	/// </summary>
	/// <param name="point">Point to set the right hand position to</param>
	/// <param name="weight">Weight of IK</param>
	private void SetRightHand(Transform point, float weight = 1){
//		animationController.rightHandIK = point.transform;
		ik.rightHand.transform = point;
		ik.rightHand.weight = weight;
	}

	/// <summary>
	/// Sets the IK position of the left hand to the <paramref name="point"/>
	/// </summary>
	/// <param name="point">Point to set the left hand position to</param>
	/// <param name="weight">Weight of IK</param>
	private void SetLeftHand(Transform point, float weight = 1){
//		animationController.leftHandIK = point.transform;
		ik.leftHand.transform = point;
		ik.leftHand.weight = weight;
	}

	private Vector3 GetVectorFromDirection(HangInfo.Direction direction){
		switch (direction) {
			case HangInfo.Direction.Left:
				return -transform.right;
			case HangInfo.Direction.Right:
				return transform.right;
			case HangInfo.Direction.None:
				Debug.LogWarning("Given direction is None... returning zero vector");
				return Vector3.zero;
			default:
				throw new Exception("Dude something wrong");
		}
	}

	/// <summary>
	/// Sets all the animation controller states
	/// </summary>
	private void SetAnimationControllerStates(){
//		controller.localVelocity = rb.velocity;
		controller.isFalling = _isFalling;
		controller.isJumping = _isJumping;
		controller.isCrouching = _isCrouching;
		controller.isGrounded = _isGrounded;
		controller.isHanging = _isHanging;
		controller.isClimbing = _isClimbing;
	}

	/// <summary>
	/// Sets all the current player states in memory for the next frame
	/// </summary>
	private void SetLastFrameStates(){
		_wasMidair = _isMidair;
		_wasGrounded = _isGrounded;
		_wasJumping = _isJumping;
		_wasHanging = _isHanging;
		_wasCrouching = _isCrouching;
		_wasFalling = _isFalling;
		_wasClimbing = _isClimbing;
	}

	#region Jumping-related methods
	[UsedImplicitly]
	public void OnActualJumpStart(AnimationEvent animationEvent){
		var newVelocity = rb.velocity;
		Debug.Log("Thrusting up!");
		newVelocity.y = jumpVelocityY;
		rb.velocity = newVelocity;
	}

	[UsedImplicitly]
	public void OnActualJumpLand(AnimationEvent animationEvent){
		// TODO: Might require checking if we are actually grounded and not in midair
		// as the jump might take very long
		rb.velocity = Vector3.zero;
		_isLandingFromJump = true;
	}

	[UsedImplicitly]
	public void OnActualJumpEnd(AnimationEvent animationEvent){
		_isLandingFromJump = false;
	}
	#endregion

	#region Climbing-related methods
	private void Climb(){
		_isHanging = false;
		_isClimbing = true;
		rb.isKinematic = true;
		_hang.currentPoint = null;
		_hang.currentDirection = HangInfo.Direction.None;
	}

	[UsedImplicitly]
	public void OnFinishClimb(AnimationEvent animationEvent){
		_isClimbing = false;
		rb.isKinematic = false;
	}

	private void Hang(Point point){
		Debug.Log("Started Hanging");
		_isHanging = true;
		rb.isKinematic = true;
		_hang.state = HangInfo.HangState.Final;
		_hang.currentDirection = HangInfo.Direction.None;
		_hang.currentPoint = point;
		controller.hangType = _hang.currentPoint.hangType;

		SetLeftHand(_hang.currentPoint.ik.leftHand.transform);
		SetRightHand(_hang.currentPoint.ik.rightHand.transform);
		transform.position = _hang.currentPoint.characterRoot.transform.position;
		transform.rotation = Quaternion.LookRotation(-_hang.currentPoint.normal, Vector3.up);
//		transform.rotation.SetLookRotation(-_hang.currentPoint.normal, Vector3.up);
	}

	private void Unhang(){
		Debug.Log("Stopped Hanging");
		_isHanging = false;
		_isFalling = true;
		rb.isKinematic = false;
		_hang.currentPoint = null;
		_hang.currentDirection = HangInfo.Direction.None;
	}

	private void Shimmy(HangInfo.Direction direction){
		if (direction == HangInfo.Direction.None) return;
		Debug.Log("Shimmy " + direction + " entered");
		Debug.Log("Current state is " + _hang.state);
		if (_hang.state != HangInfo.HangState.Final) return;

		// We obtain the next point depending on the direction intended to shimmy to
		_hang.nextPoint = _hang.currentPoint.GetNextPoint(GetVectorFromDirection(direction));
		// If there is no point in that direction, simply return, nothing to do here
		if (_hang.nextPoint == null) {
			Debug.Log("No point on the " + direction);
			return;
		}

		// We keep the current shimmy direction in memory
		_hang.currentDirection = direction;
		// We set the new state
		_hang.state = HangInfo.HangState.Transition;
		_hang.nextState = HangInfo.HangState.Final;
		// TODO: Handle IKs for other directions
		switch (_hang.currentDirection) {
			case HangInfo.Direction.Right:
				controller.isShimmyRight = true;
				break;
			case HangInfo.Direction.Left:
				controller.isShimmyLeft = true;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		SetRightHand(_hang.nextPoint.ik.rightHand.transform, 0);
		SetLeftHand(_hang.nextPoint.ik.leftHand.transform, 0);

	}
	#endregion

	private void OnCollisionEnter(Collision other){
		var contact = other.contacts.FirstOrDefault();
		CheckWallCollision(contact);
	}

	/// <summary>
	/// Checks if the character is colliding with a wall (and not the ground for example)
	/// </summary>
	/// <param name="contact">Contact point</param>
	private void CheckWallCollision(ContactPoint contact){
		if (Mathf.Abs(Vector3.Dot(contact.normal, Vector3.up)) < 0.5f && !_isHanging) {
			isCollidingWithWall = true;
			collisionNormal = contact.normal;
			collisionPoint = contact.point;
		}
	}

	private void OnCollisionStay(Collision other){
		var contact = other.contacts.FirstOrDefault();
		CheckWallCollision(contact);
	}

	private void OnCollisionExit(Collision other){
		isCollidingWithWall = false;
		collisionNormal = Vector3.zero;
		collisionPoint = Vector3.zero;
	}

	private void OnAnimatorIK(int layerIndex){
		if (_isHanging) {
			// Set right hand's IK
			controller.animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ik.rightHand.weight);
			controller.animator.SetIKPosition(AvatarIKGoal.RightHand, ik.rightHand.transform.position);
			// Set left hand's IK
			controller.animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ik.leftHand.weight);
			controller.animator.SetIKPosition(AvatarIKGoal.LeftHand, ik.leftHand.transform.position);
		}
		if (_isHanging && _hang.state == HangInfo.HangState.Transition) {
			var percentage = timelapseShimmy / shimmyAnimationDuration;
			timelapseShimmy += Time.fixedDeltaTime;
			if (percentage > 0.999) {

				switch (_hang.currentDirection) {
					case HangInfo.Direction.Left:
						controller.isShimmyLeft = false;
						break;
					case HangInfo.Direction.Right:
						controller.isShimmyRight = false;
						break;
				}
				_hang.currentPoint = _hang.nextPoint;
				SetRightHand(_hang.currentPoint.ik.rightHand.transform);
				SetLeftHand(_hang.currentPoint.ik.leftHand.transform);
				_hang.currentDirection = HangInfo.Direction.None;
				_hang.nextPoint = null;
				_hang.state = HangInfo.HangState.Final;
				_hang.nextState = HangInfo.HangState.Final;
				timelapseShimmy = 0;
			}
			else if(percentage > 0.9) {
			transform.position = Vector3.Lerp(_hang.currentPoint.characterRoot.transform.position,
				_hang.nextPoint.characterRoot.transform.position,
				percentage);

			}
			else {
				transform.position = controller.animator.rootPosition;
				transform.rotation = controller.animator.rootRotation;

				ik.rightHand.weight = percentage;
				ik.leftHand.weight = percentage;
			}

		}
		controller.animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
		controller.animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);


	}

	private void OnDrawGizmos(){
		if (showGroundAxis) {
			Gizmos.color = groundAxisVectorColor;
			Gizmos.DrawLine(feetMidpoint + Vector3.up * 0.04f, feetMidpoint + Vector3.up * 0.02f + Vector3.down * downRayLength);

		}
		if (showCharacterAxis) {
			Gizmos.color = characterAxisVectorColor;
			GizmosUtil.DrawArrow(transform.position, transform.position + transform.forward * forwardRayLength);
			GizmosUtil.DrawArrow(transform.position, transform.position + transform.right * rightRayLength);
		}
		if (showClosestPointVector && closestClimbPoint != null) {
			Gizmos.color = closestPointVectorColor;
			Gizmos.DrawLine(transform.position, closestClimbPoint.transform.position);
		}
		if (showCameraDesireVectors) {
			Gizmos.color = cameraDesireVectorColor;
			GizmosUtil.DrawArrow(transform.position, transform.position + cameraForwardDesireVector);
			GizmosUtil.DrawArrow(transform.position, transform.position + cameraRightDesireVector);

		}
		if (showVelocityVector) {
			if (rb != null) {
				Gizmos.color = velocityVectorColor;
				GizmosUtil.DrawArrow(transform.position, transform.position + rb.velocity);
			}
		}
		if (showCollisionNormal && isCollidingWithWall) {
			Gizmos.color = collisionNormalColor;
			GizmosUtil.DrawArrow(collisionPoint, collisionPoint + collisionNormal);
		}
	}
}