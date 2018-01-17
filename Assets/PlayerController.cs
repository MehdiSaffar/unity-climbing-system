using System;
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

	public CharacterAnimationController _controller;
	public Transform pointList;

	private Point _closestPoint;
	[Header("Character")]
	public float rightVectorLength;
	public float forwardVectorLength;

	public Transform rightHand;
	public Transform leftHand;
	public Transform rightFoot;
	public Transform leftFoot;

	private Vector3 _footAverage => Vector3.Lerp(leftFoot.position, rightFoot.position, 0.5f);

	[Header("Collision")]
	public float _downRayLength;
	public float _pointDistanceThreshold;
	private float _pointSqrDistanceThreshold;

	public bool _isJumping;
	public bool _isGrounded;
	public bool _isCrouching;
	public bool _isHanging;
	public bool _wasJumping;
	public bool _wasGrounded;
	public bool _wasCrouching;
	public bool _wasHanging;
	[Header("Speeds")]
	public float walkSpeed;
	public float jogSpeed;

	// Use this for initialization
	void Start ()
	{
		_controller = GetComponent<CharacterAnimationController>();
		_hang = new HangInfo();
	}

	/// <summary>
	/// Checks if the character is grounded and sets the corresponding field
	/// </summary>
	private void CheckIsGrounded()
	{
		var ray = new Ray(_footAverage + Vector3.up * 0.04f, Vector3.down);
		RaycastHit hit;
		_isGrounded = Physics.Raycast(ray, out hit, _downRayLength);
	}

	// Update is called once per frame
	void Update()
	{
		_pointSqrDistanceThreshold = _pointDistanceThreshold * _pointDistanceThreshold;
		_closestPoint = GetClosestPoint();

		_controller.walkSpeed = walkSpeed;
		_controller.jogSpeed = jogSpeed;

		CheckIsGrounded();
		// Action keys
		bool spaceUp = Input.GetKeyUp(KeyCode.Space);
		bool crouchUp = Input.GetKeyUp(KeyCode.C);
		bool hangUp = Input.GetKeyUp(KeyCode.LeftControl);
		bool upUp = Input.GetKeyUp(KeyCode.W);
		bool rightUp = Input.GetKeyUp(KeyCode.D);
		bool leftUp = Input.GetKeyUp(KeyCode.A);
		// State keys
		bool shiftPressed = Input.GetKey(KeyCode.LeftShift);
		// Movement keys
		bool forwardPressed = Input.GetKey(KeyCode.W);
		bool backwardPressed = Input.GetKey(KeyCode.S);
		bool leftPressed = Input.GetKey(KeyCode.A);
		bool rightPressed = Input.GetKey(KeyCode.D);

		// Movement input vector
		var inputVector = Vector3.zero;
		if (forwardPressed)
			inputVector.z += 1;
		if (backwardPressed)
			inputVector.z -= 1;
		if (rightPressed)
			inputVector.x += 1;
		if (leftPressed)
			inputVector.x -= 1;

		// State transition conditions
		bool canJump = _isGrounded && !_isCrouching;
		bool canCrouch = _isGrounded;
		bool canWalk = _isGrounded && !_isJumping;
		bool canJog = _isGrounded && !_isCrouching && !_isJumping;
		bool canHang = _isGrounded && _closestPoint != null && !_isCrouching && !_isJumping;
		// If the point requires the player to be facing a certain way, then we add the condition
		if (canHang && _closestPoint.HasNormal)
		{
			canHang = Vector3.Dot(transform.forward, _closestPoint.wallNormal) <= -0.9f;
		}

		// Jump
		if (canJump && spaceUp)
		{
			_isJumping = true;
			Debug.Log("Started Jumping");
		}
		if (!_wasGrounded && _isGrounded && _isJumping)
		{
			Debug.Log("Stopped Jumping");
			_isJumping = false;
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

//		if (upUp)
//		{
//			if (_isHanging)
//			{
//				if (_hang.state == HangInfo.HangState.Final)
//				{
//					_hang.nextPoint = _hang.currentPoint.GetNextPoint(transform.up);
//					if (_hang.nextPoint)
//					{
//						_hang.currentDirection = HangInfo.Direction.Right;
//						_controller.rightHandIK = _hang.nextPoint.transform;
//						transform.position = Vector3.Lerp(
//								_hang.currentPoint.characterShouldBe.position,
//								_hang.nextPoint.characterShouldBe.position, 0.5f);
//						_hang.state = HangInfo.HangState.Midpoint;
//						_controller.isShimmyRight = true;
//					}
//				}
//				else if (_hang.state == HangInfo.HangState.Midpoint)
//				{
//					if (_hang.currentDirection == HangInfo.Direction.Right)
//					{
//						_hang.currentPoint = _hang.nextPoint;
//					}
//					else if(_hang.currentDirection == HangInfo.Direction.Left)
//					{
//						_hang.nextPoint = _hang.currentPoint;
//
//					}
//
//					_controller.isShimmyRight = true;
//					_hang.nextPoint = null;
//					_controller.leftHandIK = _hang.currentPoint.transform;
//					transform.position = _hang.currentPoint.characterShouldBe.transform.position;
//					_hang.state = HangInfo.HangState.Final;
//				}
//			}
//		}
//		if (rightUp)
//		{
//			if (_isHanging)
//			{
//				if (_hang.state == HangInfo.HangState.Final)
//				{
//					_hang.nextPoint = _hang.currentPoint.GetNextPoint(transform.right);
//					if (_hang.nextPoint)
//					{
//						_hang.currentDirection = HangInfo.Direction.Right;
//						_controller.rightHandIK = _hang.nextPoint.transform;
//						transform.position = Vector3.Lerp(
//							_hang.currentPoint.characterShouldBe.position,
//							_hang.nextPoint.characterShouldBe.position, 0.5f);
//						_hang.state = HangInfo.HangState.Midpoint;
//						_controller.isShimmyRight = true;
//					}
//				}
//				else if (_hang.state == HangInfo.HangState.Midpoint)
//				{
//					if (_hang.currentDirection == HangInfo.Direction.Right)
//					{
//						_hang.currentPoint = _hang.nextPoint;
//					}
//					else if (_hang.currentDirection == HangInfo.Direction.Left)
//					{
//						_hang.nextPoint = _hang.currentPoint;
//
//					}
//
//					_controller.isShimmyRight = true;
//					_hang.nextPoint = null;
//					_controller.leftHandIK = _hang.currentPoint.transform;
//					transform.position = _hang.currentPoint.characterShouldBe.transform.position;
//					_hang.state = HangInfo.HangState.Final;
//				}
//			}
//		}
//		if (leftUp)
//		{
//			if (_isHanging)
//			{
//				if (_hang.state == HangInfo.HangState.Final)
//				{
//					_hang.nextPoint = _hang.currentPoint.GetNextPoint(-transform.right);
//					if (_hang.nextPoint)
//					{
//						_hang.currentDirection = HangInfo.Direction.Left;
//						_controller.leftHandIK = _hang.nextPoint.transform;
//						transform.position = Vector3.Lerp(
//							_hang.currentPoint.characterShouldBe.position,
//							_hang.nextPoint.characterShouldBe.position, 0.5f);
//						_hang.state = HangInfo.HangState.Midpoint;
//						_controller.isShimmyLeft = true;
//					}
//
//				}
//				else if (_hang.state == HangInfo.HangState.Midpoint)
//				{
//					if (_hang.currentDirection == HangInfo.Direction.Left)
//					{
//						_hang.currentPoint = _hang.nextPoint;
//					}
//					else if(_hang.currentDirection == HangInfo.Direction.Right)
//					{
//						_hang.nextPoint = _hang.currentPoint;
//					}
//					_controller.isShimmyLeft = true;
//					_hang.currentPoint = _hang.nextPoint;
//					_hang.nextPoint = null;
//					_controller.rightHandIK = _hang.currentPoint.transform;
//					transform.position = _hang.currentPoint.characterShouldBe.transform.position;
//					_hang.state = HangInfo.HangState.Final;
//
//				}
//			}
//		}

		var forward = inputVector;
		if (forward.sqrMagnitude > 1)
		{
			forward.Normalize();
		}

		float speed = 0;
		if (inputVector != Vector3.zero)
		{
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
		_controller.currentSpeed = speed;
		if (!_isHanging)
		{
			transform.LookAt(transform.position + forward, Vector3.up);
			var velocity = forward * speed * Time.deltaTime;
			transform.position += velocity;
		}

		SetAnimationControllerStates();
		SetLastFrameStates();
	}

	private void Hang(Point point)
	{
		Debug.Log("Started Hanging");
		_isHanging = true;
		_hang.state = HangInfo.HangState.Final;
		_hang.currentPoint = point;
		_controller.hangType = _hang.currentPoint.hangType;

		SetLeftHand(_hang.currentPoint);
		SetRightHand(_hang.currentPoint);
		transform.position = _hang.currentPoint.characterRoot.transform.position;
	}

	private void Unhang()
	{
		Debug.Log("Stopped Hanging");
		_isHanging = false;
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
		_controller.isJumping = _isJumping;
		_controller.isCrouching = _isCrouching;
		_controller.isHanging = _isHanging;
	}

	private void SetLastFrameStates()
	{
		_wasGrounded = _isGrounded;
		_wasJumping = _isJumping;
		_wasHanging = _isHanging;
		_wasCrouching = _isCrouching;
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
						SetRightHand(_hang.nextPoint);
						SetLeftHand(_hang.currentPoint);
						break;
					case HangInfo.Direction.Left:
						SetRightHand(_hang.currentPoint);
						SetLeftHand(_hang.nextPoint);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				// Set the transform position of the character
				transform.position = Vector3.Lerp(
					_hang.currentPoint.characterRoot.transform.position,
					_hang.nextPoint.characterRoot.transform.position,
					0.5f);
				_controller.isShimmyLeft = false;
				_controller.isShimmyRight = false;
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
				_controller.hangType = _hang.currentPoint.hangType;
// We set the hand IKs
				SetRightHand(_hang.currentPoint);
				SetLeftHand(_hang.currentPoint);
				// Set the transform position of the character
				transform.position = _hang.currentPoint.characterRoot.transform.position;
				_hang.state = HangInfo.HangState.Final;
				switch (direction)
				{
					case HangInfo.Direction.Left:
						_controller.isShimmyLeft = true;
						break;
					case HangInfo.Direction.Right:
						_controller.isShimmyRight = true;
						break;
				}

				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void SetRightHand(Point point)
	{
		_controller.rightHandIK = point.rightHand.transform;
	}

	private void SetLeftHand(Point point)
	{
		_controller.leftHandIK = point.leftHand.transform;
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
}
