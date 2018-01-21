using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Rendering;

/// <summary>
/// Component used to control the player's character and keep track of its states
/// </summary>
[RequireComponent(typeof(CharacterAnimationController), typeof(Rigidbody))]
public class PlayerController : MyMonoBehaviour{
	/// <summary>
	/// Character animation controller
	/// </summary>
	private CharacterAnimationController animationController;

	private Rigidbody _rigidbody;

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

	/// <summary>
	/// Length of the gizmo ray shooting to the right of the player
	/// </summary>
	[Header("Gizmo")] public float rightRayLength;

	/// <summary>
	/// Length of the gizmo ray shooting in front of the player
	/// </summary>
	public float forwardRayLength;

	public float downRayLength;

	/// <summary>
	/// Reference to the transform of the right point
	/// </summary>
	public Transform rightFoot;

	/// <summary>
	/// Reference to the transform of the left point
	/// </summary>
	public Transform leftFoot;

	/// <summary>
	/// Position of the midpoint of the two feet
	/// </summary>
	private Vector3 feetMidpoint => Vector3.Lerp(leftFoot.position, rightFoot.position, 0.5f);

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

	/// <summary>
	/// Is the player jumping?
	/// </summary>
	[Header("States")] public bool _isJumping;

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

	/// <summary>
	/// Current vertical velocity of the player
	/// </summary>
	[Header("Speeds")] public float currentVelocityY;

	/// <summary>
	/// Walk speed constant
	/// </summary>
	public float walkSpeed;

	/// <summary>
	/// Jog speed constant
	/// </summary>
	public float jogSpeed;

	/// <summary>
	/// Vertical jump starting velocity constant
	/// </summary>
	public float jumpVelocityY;

	/// <summary>
	/// Speed of the player in the last frame
	/// </summary>
	private float lastSpeed;

	/// <summary>
	/// Is the spacebar pressed up?
	/// </summary>
	private bool spaceUp;

	/// <summary>
	/// Is the crouch button pressed up?
	/// </summary>
	private bool crouchUp;

	/// <summary>
	/// Is the hanging button pressed up?
	/// </summary>
	private bool hangUp;

	/// <summary>
	/// Is the right movement button pressed up?
	/// </summary>
	private bool rightUp;

	/// <summary>
	/// Is the left movement button pressed up?
	/// </summary>
	private bool leftUp;

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

	/// <summary>
	/// Maximum vertical velocity beyond which the player is considered falling
	/// </summary>
	public float fallVelocityThreshold;

	/// <summary>
	/// Duration of the shimmy animation
	/// </summary>
	public float shimmyAnimationDuration;

	private float timelapseShimmy;

	private IKPositions ik;

	private void Awake(){
		animationController = GetComponent<CharacterAnimationController>();
		_rigidbody = GetComponent<Rigidbody>();
		ik = new IKPositions();
		_hang = new HangInfo();

	}

	// Use this for initialization
	void Start(){
		animationController.walkSpeed = walkSpeed;
		animationController.jogSpeed = jogSpeed;

		_pointSqrDistanceThreshold = pointDistanceThreshold * pointDistanceThreshold;
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

		animationController.distanceToGround = distanceToGround;
	}

	// Update is called once per frame
	private void FixedUpdate(){
		closestClimbPoint = GetClosestPoint();
		CheckIsGrounded();

		// Sets the "keyPressed"...
		GetInputState();

		// Movement input vector
		var inputVector = Vector3.zero;
		if (!IsInputLocked) {
			if (forwardPressed)
				inputVector.z += 1;
			if (backwardPressed)
				inputVector.z -= 1;
			if (rightPressed)
				inputVector.x += 1;
			if (leftPressed)
				inputVector.x -= 1;
		}

		// State transition conditions
		bool canJump = !_isJumping && _isGrounded && !_isCrouching;
		bool canCrouch = _isGrounded;
		bool canWalk = /*_isGrounded && */!_isJumping;
		bool canJog = /*_isGrounded && */!_isCrouching /* && !_isJumping*/;
		bool canHang = _isGrounded && closestClimbPoint != null && !_isCrouching && !_isJumping;
		bool canClimb = _isHanging;
		// If the point requires the player to be facing a certain way, then we add the condition
		if (canHang && closestClimbPoint.HasNormal) {
			canHang = Vector3.Dot(transform.forward, closestClimbPoint.normal) <= -0.9f;
		}

		if (!_isFalling && !_isGrounded && _rigidbody.velocity.y < fallVelocityThreshold) {
			Debug.Log("Falling!!");
			_isFalling = true;
		}

		if (_isGrounded) {
			_isFalling = false;
		}

		// Jump
		if (_isJumping) {

			if ((_rigidbody.velocity.y < -0.01 && _isGrounded) || (_isLandingFromJump)) {
				Debug.Log("Stopped Jumping");
				_isJumping = false;
				_isFalling = false;
			}
		}
		else {
			if (canJump && spaceUp) {
				_isJumping = true;
				Debug.Log("Started Jumping");
			}
		}

		if (crouchUp && canCrouch) {
			_isCrouching = !_isCrouching;
			Debug.Log(_isCrouching ? "Started Crouching" : "Stopped Crouching");
		}

		if (hangUp) {
			if (_isHanging && _hang.state == HangInfo.HangState.Final) {
				Unhang();
			}
			else if (canHang) {
				Hang(closestClimbPoint);
			}
		}

		if (_isHanging) {
			if (rightUp) {
				Shimmy(HangInfo.Direction.Right);
			}

			if (leftUp) {
				Shimmy(HangInfo.Direction.Left);
			}

			if (spaceUp && canClimb) {
				Climb();
			}
		}

		UpdateTransform(inputVector, canWalk, canJog);

		SetAnimationControllerStates();
		SetLastFrameStates();
		currentVelocityY = _rigidbody.velocity.y;
	}

	/// <summary>
	/// Updates velocity, rotation etc. of the character
	/// </summary>
	/// <param name="inputVector">Input of the player</param>
	/// <param name="canWalk">Can the player walk?</param>
	/// <param name="canJog">Can the player jog?</param>
	/// TODO: Requires refactoring of the parameters
	private void UpdateTransform(Vector3 inputVector, bool canWalk, bool canJog){
		var forward = inputVector;
		if (forward.sqrMagnitude > 1) {
			forward.Normalize();
		}

		float speed = 0;
		if (_isJumping || _isHanging || _isFalling || _isClimbing) {
//			Debug.Log($"Setting to last speed {lastSpeed} because jumping");
			speed = lastSpeed;
			forward = transform.forward;
		}
		else {
			if (inputVector != Vector3.zero) {
				if (_isCrouching && canWalk) {
					speed = walkSpeed;
				}
				else if (shiftPressed && canWalk) {
					speed = walkSpeed;
				}
				else if (!shiftPressed && canJog) {
					speed = jogSpeed;
				}
			}
		}

		lastSpeed = speed;
		animationController.currentSpeed = speed;
		if (!_isHanging) {
			var velocity = forward * speed;
			velocity.y = _rigidbody.velocity.y;
			if (forward != Vector3.zero)
				transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
			_rigidbody.velocity = velocity;
//			if (landingFromJumping)
//			{
//				_rigidbody.velocity /= 2;
//			}
//			_rigidbody.AddForce(forward * speed * Time.deltaTime);
		}
	}

	/// <summary>
	/// Is the player able to input?
	/// </summary>
	private bool IsInputLocked => _isJumping || _isFalling;

	private void GetInputState(){
		// Action keys
		spaceUp = Input.GetKeyUp(KeyCode.Space);
		crouchUp = Input.GetKeyUp(KeyCode.C);
		hangUp = Input.GetKeyUp(KeyCode.LeftControl);
		//upUp = Input.GetKeyUp(KeyCode.W);
		rightUp = Input.GetKeyUp(KeyCode.D);
		leftUp = Input.GetKeyUp(KeyCode.A);
		// State keys
		shiftPressed = Input.GetKey(KeyCode.LeftShift);
		// Movement keys
		forwardPressed = Input.GetKey(KeyCode.W);
		backwardPressed = Input.GetKey(KeyCode.S);
		leftPressed = Input.GetKey(KeyCode.A);
		rightPressed = Input.GetKey(KeyCode.D);
	}

	private void Climb(){
		_isHanging = false;
		_isClimbing = true;
		_rigidbody.isKinematic = true;
		_hang.currentPoint = null;
		_hang.currentDirection = HangInfo.Direction.None;
	}

	[UsedImplicitly]
	public void OnFinishClimb(AnimationEvent animationEvent){
		FinishClimb();
	}

	private void FinishClimb(){
		_isClimbing = false;
		_rigidbody.isKinematic = false;
	}

	private void Hang(Point point){
		Debug.Log("Started Hanging");
		_isHanging = true;
		_rigidbody.isKinematic = true;
		_hang.state = HangInfo.HangState.Final;
		_hang.currentDirection = HangInfo.Direction.None;
		_hang.currentPoint = point;
		animationController.hangType = _hang.currentPoint.hangType;

		SetLeftHand(_hang.currentPoint.ik.leftHand.transform);
		SetRightHand(_hang.currentPoint.ik.rightHand.transform);
		transform.position = _hang.currentPoint.characterRoot.transform.position;

	}

	private void Unhang(){
		Debug.Log("Stopped Hanging");
		_isHanging = false;
		_isFalling = true;
		_rigidbody.isKinematic = false;
		_hang.currentPoint = null;
		_hang.currentDirection = HangInfo.Direction.None;
	}

	/// <summary>
	/// Returns the closest climbing point below the distance threshold
	/// </summary>
	/// <returns>Closest climbing point</returns>
	private Point GetClosestPoint(){
		var points = pointList.GetComponentsInChildren<Point>();
		float minimalSqrDistance = _pointSqrDistanceThreshold;
		Point closestPoint = null;
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
				closestPoint = point;
			}
		}

		return closestPoint;
	}

	private void SetAnimationControllerStates(){
		animationController.yVelocity = _rigidbody.velocity.y;
		animationController.isFalling = _isFalling;
		animationController.isJumping = _isJumping;
		animationController.isCrouching = _isCrouching;
		animationController.isGrounded = _isGrounded;
		animationController.isHanging = _isHanging;
		animationController.isClimbing = _isClimbing;
	}

	private void SetLastFrameStates(){
		_wasMidair = _isMidair;
		_wasGrounded = _isGrounded;
		_wasJumping = _isJumping;
		_wasHanging = _isHanging;
		_wasCrouching = _isCrouching;
		_wasFalling = _isFalling;
		_wasClimbing = _isClimbing;
	}

	private void OnDrawGizmos(){
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(feetMidpoint + Vector3.up * 0.04f, feetMidpoint + Vector3.up * 0.02f + Vector3.down * downRayLength);
		GizmosUtil.DrawArrow(transform.position, transform.position + transform.forward * forwardRayLength);
		GizmosUtil.DrawArrow(transform.position, transform.position + transform.right * rightRayLength);
		if (closestClimbPoint != null) {
			Gizmos.color = Color.black;
			Gizmos.DrawLine(transform.position, closestClimbPoint.transform.position);
		}
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

				animationController.isShimmyRight = true;
				break;
			case HangInfo.Direction.Left:
				animationController.isShimmyLeft = true;
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
		SetRightHand(_hang.nextPoint.ik.rightHand.transform, 0);
		SetLeftHand(_hang.nextPoint.ik.leftHand.transform, 0);

	}


	private void SetRightHand(Transform point, float weight = 1){
//		animationController.rightHandIK = point.transform;
		ik.rightHand.transform = point;
		ik.rightHand.weight = weight;
	}

	private void SetLeftHand(Transform point, float weight = 1){
//		animationController.leftHandIK = point.transform;
		ik.leftHand.transform = point;
		ik.leftHand.weight = weight;
	}

	private static bool DirectionIsSame(HangInfo.Direction first, HangInfo.Direction second){
		return first == second;
	}

	private static bool DirectionIsOpposite(HangInfo.Direction first, HangInfo.Direction second){
		// TODO: Check the other directions (up, down)
		return first == HangInfo.Direction.Left && second == HangInfo.Direction.Right
		       || first == HangInfo.Direction.Right && second == HangInfo.Direction.Left;
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

	[UsedImplicitly]
	public void OnActualJumpStart(AnimationEvent animationEvent){
		var newVelocity = _rigidbody.velocity;
		Debug.Log("Thrusting up!");
		newVelocity.y = jumpVelocityY;
		_rigidbody.velocity = newVelocity;
	}

	[UsedImplicitly]
	public void OnActualJumpLand(AnimationEvent animationEvent){
		// TODO: Might require checking if we are actually grounded and not in midair
		// as the jump might take very long
		_rigidbody.velocity = Vector3.zero;
		_isLandingFromJump = true;
	}

	[UsedImplicitly]
	public void OnActualJumpEnd(AnimationEvent animationEvent){
		_isLandingFromJump = false;
	}

	private void OnAnimatorIK(int layerIndex){
		if (_isHanging) {
			// Set right hand's IK
			animationController.animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ik.rightHand.weight);
			animationController.animator.SetIKPosition(AvatarIKGoal.RightHand, ik.rightHand.transform.position);
			// Set left hand's IK
			animationController.animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ik.leftHand.weight);
			animationController.animator.SetIKPosition(AvatarIKGoal.LeftHand, ik.leftHand.transform.position);
		}
		if (_isHanging && _hang.state == HangInfo.HangState.Transition) {
			var percentage = timelapseShimmy / shimmyAnimationDuration;
			timelapseShimmy += Time.fixedDeltaTime;
			if (percentage > 0.999) {

				switch (_hang.currentDirection) {
					case HangInfo.Direction.Left:
						animationController.isShimmyLeft = false;
						break;
					case HangInfo.Direction.Right:
						animationController.isShimmyRight = false;
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
				transform.position = animationController.animator.rootPosition;
				transform.rotation = animationController.animator.rootRotation;

				ik.rightHand.weight = percentage;
				ik.leftHand.weight = percentage;
			}

		}


	}

//	private void OnAnimatorMove()
//	{
//		if (_isHanging)
//		{
//			var neededPosition = Vector3.zero;
//
//			if (_hang.state == HangInfo.HangState.Midpoint)
//			{
//				neededPosition = Vector3.Lerp(
//					_hang.currentPoint.characterRoot.transform.position,
//					_hang.nextPoint.characterRoot.transform.position,
//					0.5f);
//			}
//			else
//			{
//				neededPosition = _hang.currentPoint.characterRoot.transform.position;
//			}
//
//			transform.position = Vector3.Slerp(transform.position, neededPosition, timeSinceShimmy / _animationDuration);
//		}
//	}
}