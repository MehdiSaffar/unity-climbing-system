using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour
{
	[Serializable]
	public class HangInfo
	{
		public enum HangState
		{
			Final,
			Midpoint
		}

		public enum Direction
		{
			None,
			Right,
			Left
		}
		public HangState state;
		public Direction currentDirection;
		public Point currentPoint;
		public Point nextPoint;

	}
	public HangInfo _hang;

	private CharacterAnimationController controller;
	private Rigidbody _rigidbody;
	public Transform pointList;

	private Point _closestPoint;
	[Header("Character")]
	public float rightVectorLength;
	public float forwardVectorLength;
	public float _downRayLength;

	public Transform rightHand;
	public Transform leftHand;
	public Transform rightFoot;
	public Transform leftFoot;

	private Vector3 _footAverage => Vector3.Lerp(leftFoot.position, rightFoot.position, 0.5f);

	[Header("Threshold")]
	public float _pointDistanceThreshold;
	public float _groundDistanceThreshold;

//	public float _distanceToGroundStartPlayingFallingToIdle;
	private float distanceToGround;
	private float _pointSqrDistanceThreshold;

	[Header("States")]
	public bool _isJumping;
	public bool _isGrounded;
	public bool _isCrouching;
	public bool _isMidair;
	public bool _isHanging;
	public bool _isLandingFromJumping;
	public bool _isFalling;

	public bool _wasJumping;
	public bool _wasMidair;
	public bool _wasGrounded;
	public bool _wasCrouching;
	public bool _wasHanging;
	public bool _wasFalling;


	[Header("Speeds")]
	public float currentVelocityY;
	public float walkSpeed;
	public float jogSpeed;
	public float jumpVelocityY;
	private float lastSpeed;

	private bool spaceUp;
	private bool crouchUp;
	private bool hangUp;
	private bool rightUp;
	private bool leftUp;
	private bool shiftPressed;
	private bool forwardPressed;
	private bool backwardPressed;
	private bool leftPressed;
	private bool rightPressed;
	public float fallVelocityThreshold;

	// Use this for initialization
	void Start ()
	{
		controller = GetComponent<CharacterAnimationController>();
		_rigidbody = GetComponent<Rigidbody>();
		_hang = new HangInfo();

		controller.walkSpeed = walkSpeed;
		controller.jogSpeed = jogSpeed;

		_pointSqrDistanceThreshold = _pointDistanceThreshold * _pointDistanceThreshold;

	}

	/// <summary>
	/// Checks if the character is grounded and sets the corresponding field
	/// </summary>
	private void CheckIsGrounded()
	{
		var ray = new Ray(_footAverage + Vector3.up * 0.04f, Vector3.down);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, _downRayLength))
		{
			distanceToGround = hit.distance - 0.04f;
		}
		else
		{
			distanceToGround = _downRayLength - 0.04f;
		}
		if (distanceToGround >= _groundDistanceThreshold)
		{
			_isMidair = true;
			_isGrounded = false;
		} else
		{
			_isMidair = false;
			_isGrounded = true;
		}
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		_closestPoint = GetClosestPoint();

		CheckIsGrounded();
		controller.distanceToGround = distanceToGround;

		// Sets the "keyPressed"...
		GetInputState();

		// Movement input vector
		var inputVector = Vector3.zero;
//		if (!_isJumping)
//		{
			if (forwardPressed)
				inputVector.z += 1;
			if (backwardPressed)
				inputVector.z -= 1;
			if (rightPressed)
				inputVector.x += 1;
			if (leftPressed)
				inputVector.x -= 1;
//		}

		// State transition conditions
		bool canJump = !_isJumping && _isGrounded && !_isCrouching;
		bool canCrouch = _isGrounded;
		bool canWalk = /*_isGrounded && */!_isJumping;
		bool canJog = /*_isGrounded && */!_isCrouching/* && !_isJumping*/;
		bool canHang = _isGrounded && _closestPoint != null && !_isCrouching && !_isJumping;
		// If the point requires the player to be facing a certain way, then we add the condition
		if (canHang && _closestPoint.HasNormal)
		{
			canHang = Vector3.Dot(transform.forward, _closestPoint.wallNormal) <= -0.9f;
		}

		if (!_isFalling && !_isGrounded && _rigidbody.velocity.y < fallVelocityThreshold)
		{
			Debug.Log("Falling!!");
			_isFalling = true;
		}

		if (_isGrounded)
		{
			_isFalling = false;
		}
		// Jump
		if (_isJumping)
		{

			if ((_rigidbody.velocity.y < -0.01 && _isGrounded) || (_isLandingFromJumping))
			{
				Debug.Log("Stopped Jumping");
				_isJumping = false;
				_isFalling = false;
			}
		}
		else
		{
			if (canJump && spaceUp)
			{
				_isJumping = true;
				Debug.Log("Started Jumping");
			}
		}

		if (crouchUp && canCrouch)
		{
			_isCrouching = !_isCrouching;
			if (_isCrouching)
			{
				Debug.Log("Started Crouching");
			}
			else
			{
				Debug.Log("Stopped Crouching");
			}
		}
		if (hangUp)
		{
			if (_isHanging && _hang.state == HangInfo.HangState.Final)
			{
				Unhang();
			}
			else if (canHang)
			{
				Hang(_closestPoint);
			}
		}

		if (_isHanging)
		{
			if (rightUp)
			{
				Shimmy(HangInfo.Direction.Right);
			}

			if (leftUp)
			{
				Shimmy(HangInfo.Direction.Left);
			}
		}

		var forward = inputVector;
		if (forward.sqrMagnitude > 1)
		{
			forward.Normalize();
		}

		float speed = 0;
		if (_isJumping || _isHanging || _isFalling)
		{
//			Debug.Log($"Setting to last speed {lastSpeed} because jumping");
			speed = lastSpeed;
			forward = transform.forward;
		}
		else
		{
			if (inputVector != Vector3.zero) {
				if (_isCrouching && canWalk)
				{
					speed = walkSpeed;
				}
				else if (shiftPressed && canWalk)
				{
					speed = walkSpeed;
				} else if (!shiftPressed && canJog)
				{
					speed = jogSpeed;
				}
			}
		}

		lastSpeed = speed;
		controller.currentSpeed = speed;
		if (!_isHanging)
		{
			var velocity = forward * speed;
			velocity.y = _rigidbody.velocity.y;
			if(forward != Vector3.zero)
				transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
			_rigidbody.velocity = velocity;
//			if (landingFromJumping)
//			{
//				_rigidbody.velocity /= 2;
//			}
//			_rigidbody.AddForce(forward * speed * Time.deltaTime);
		}

		SetAnimationControllerStates();
		SetLastFrameStates();
		currentVelocityY = _rigidbody.velocity.y;
	}

	private void GetInputState()
	{
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

	private void Hang(Point point)
	{
		Debug.Log("Started Hanging");
		_isHanging = true;
		_rigidbody.isKinematic = true;
		_hang.state = HangInfo.HangState.Final;
		_hang.currentPoint = point;
		controller.hangType = _hang.currentPoint.hangType;

		SetLeftHand(_hang.currentPoint.leftHand);
		SetRightHand(_hang.currentPoint.rightHand);
		transform.position = _hang.currentPoint.characterRoot.transform.position;
	}

	private void Unhang()
	{
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
	private Point GetClosestPoint()
	{
		var points = pointList.GetComponentsInChildren<Point>();
		float minimalSqrDistance = _pointSqrDistanceThreshold;
		Point closestPoint = null;
		foreach (var point in points)
		{
			float sqrDistance = (point.transform.position - transform.position).sqrMagnitude;
			if (sqrDistance < minimalSqrDistance)
			{
				minimalSqrDistance = sqrDistance;
				closestPoint = point;
			}
		}

		return closestPoint;
	}

	private void SetAnimationControllerStates()
	{
		controller.yVelocity = _rigidbody.velocity.y;
		controller.isFalling = _isFalling;
		controller.isJumping = _isJumping;
		controller.isCrouching = _isCrouching;
		controller.isGrounded = _isGrounded;
		controller.isHanging = _isHanging;
	}

	private void SetLastFrameStates()
	{
		_wasMidair = _isMidair;
		_wasGrounded = _isGrounded;
		_wasJumping = _isJumping;
		_wasHanging = _isHanging;
		_wasCrouching = _isCrouching;
		_wasFalling = _isFalling;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(_footAverage + Vector3.up * 0.04f, _footAverage + Vector3.up * 0.02f + Vector3.down * _downRayLength);
		GizmosUtil.DrawArrow(transform.position, transform.position + transform.forward * forwardVectorLength);
		GizmosUtil.DrawArrow(transform.position, transform.position + transform.right * rightVectorLength);
		if (_closestPoint != null)
		{
			Gizmos.color = Color.black;
			Gizmos.DrawLine(transform.position, _closestPoint.transform.position);
		}
	}

	private void Shimmy(HangInfo.Direction direction)
	{
		if (direction == HangInfo.Direction.None) return;
		Debug.Log("Shimmy " + direction + " entered");
		Debug.Log("Current state is " + _hang.state);
		switch (_hang.state)
		{
			case HangInfo.HangState.Final:
			{
				// We obtain the next point depending on the direction intended to shimmy to
				_hang.nextPoint = _hang.currentPoint.GetNextPoint(GetVectorFromDirection(direction));
				// If there is no point in that direction, simply return, nothing to do here
				if (_hang.nextPoint == null)
				{
					Debug.Log("No point on the " + direction);
					return;
				}
				// We keep the current shimmy direction in memory
				_hang.currentDirection = direction;
				// We set the new state
				_hang.state = HangInfo.HangState.Midpoint;
				// TODO: Handle IKs for other directions
				switch (_hang.currentDirection)
				{
					case HangInfo.Direction.Right:
						SetRightHand(_hang.nextPoint.transform);
						SetLeftHand(_hang.currentPoint.transform);
						controller.isShimmyRight = true;

						break;
					case HangInfo.Direction.Left:
						SetRightHand(_hang.currentPoint.transform);
						SetLeftHand(_hang.nextPoint.transform);
						controller.isShimmyLeft = true;

						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				// Set the transform position of the character
				transform.position = Vector3.Lerp(
					_hang.currentPoint.characterRoot.transform.position,
					_hang.nextPoint.characterRoot.transform.position,
					0.5f);
				break;
			}
			case HangInfo.HangState.Midpoint:
			{
				// If we are trying to continue the movement
				if (DirectionIsSame(_hang.currentDirection, direction))
				{
					Debug.Log("Direction is same: " + direction);
					// Then the current point will become the next point;
					_hang.currentPoint = _hang.nextPoint;
				}
				// If we are trying to cancel the movement
				else if (DirectionIsOpposite(_hang.currentDirection, direction))
				{
					Debug.Log("Direction is opposite: " + direction);
				}

				// And we will no longer have a "next" point
				_hang.nextPoint = null;
				// We tell the controller how we are actually hanging to that point
				controller.hangType = _hang.currentPoint.hangType;
// We set the hand IKs
				SetRightHand(_hang.currentPoint.rightHand);
				SetLeftHand(_hang.currentPoint.leftHand);
				// Set the transform position of the character
				transform.position = _hang.currentPoint.characterRoot.transform.position;
				_hang.state = HangInfo.HangState.Final;
				switch (direction)
				{
					case HangInfo.Direction.Left:
						controller.isShimmyLeft = false;
						break;
					case HangInfo.Direction.Right:
						controller.isShimmyRight = false;
						break;
				}

				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void SetRightHand(Transform point)
	{
		controller.rightHandIK = point.transform;
	}

	private void SetLeftHand(Transform point)
	{
		controller.leftHandIK = point.transform;
	}

	private static bool DirectionIsSame(HangInfo.Direction first, HangInfo.Direction second)
	{
		return first == second;
	}

	private static bool DirectionIsOpposite(HangInfo.Direction first, HangInfo.Direction second)
	{
		// TODO: Check the other directions (up, down)
		return first == HangInfo.Direction.Left && second == HangInfo.Direction.Right
		       || first == HangInfo.Direction.Right && second == HangInfo.Direction.Left;
	}

	private Vector3 GetVectorFromDirection(HangInfo.Direction direction)
	{
		switch (direction)
		{
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

	public void OnActualJumpStart(AnimationEvent animationEvent)
	{
		var newVelocity = _rigidbody.velocity;
		Debug.Log("Thrusting up!");
		newVelocity.y = jumpVelocityY;
		_rigidbody.velocity = newVelocity;
	}

	public void OnActualJumpLand(AnimationEvent animationEvent)
	{
		// TODO: Might require checking if we are actually grounded and not in midair
		// as the jump might take very long
		_rigidbody.velocity = Vector3.zero;
		_isLandingFromJumping = true;
	}

	public void OnActualJumpEnd(AnimationEvent animationEvent)
	{
		_isLandingFromJumping = false;
	}
}
