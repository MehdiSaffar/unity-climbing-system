using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Follows the target's translational movements
/// </summary>
//[RequireComponent(typeof(Camera))]
public class CameraFollower : MyMonoBehaviour
{
	#region Settings
	/// <summary>
	/// Target to follow
	/// </summary>
 	[Header("Settings")]

	public Transform target;
	/// <summary>
	/// Should the camera be controller with mouse movement?
	/// </summary>
	[SerializeField] private bool useMouse;

	/// <summary>
	/// Speed in the vertical direction (pitch)
	/// </summary>
	[SerializeField] private float verticalSpeed;
	/// <summary>
	/// Speed in the horizontal direction (roll)
	/// </summary>
	[SerializeField] private float horizontalSpeed;

	/// <summary>
	/// A count of how many offset vectors should be kept in memory
	/// </summary>
	[SerializeField] private int maxOldOffsetCount;

	/// <summary>
	/// Should the cursor be locked to center of the screen?
	/// </summary>
	[SerializeField] private bool lockCursor;
	#endregion
	#region Gizmos
	/// <summary>
	/// Should the camera show a trail of offsets?
	/// </summary>
	[Header("Gizmos")]
	[SerializeField] private bool showOffsetTrails;

	/// <summary>
	/// Should the camera show the pitch limits?
	/// </summary>
	[SerializeField] private bool showPitchLimits;

	/// <summary>
	/// Should the camera show the current offset vector?
	/// </summary>
	[SerializeField] private bool showOffset;
	#endregion
	#region Constraints
	/// <summary>
	///  Maximum allowed pitch angle (in degrees)
	/// </summary>
	[Header("Constraints")]
	[SerializeField][Range(-90, 90)] private float maximumPitchAngle;

	/// <summary>
	/// Minimum allowed pitch angle (in degrees)
	/// </summary>
	[SerializeField][Range(-90, 90)]private float minimumPitchAngle;
	#endregion
	#region Private fields
	/// <summary>
	/// Displacement vector from the target to the camera
	/// </summary>
	private Vector3 offset;

	/// <summary>
	/// Normalized vector from target position to the camera
	/// </summary>
	private Vector3 offsetDirection;

	/// <summary>
	/// A queue of the previous offset vectors
	/// Has a maximum size of
	/// </summary>
	private Queue<Vector3> oldOffsets;
	#endregion

	private void Awake(){
		oldOffsets = new Queue<Vector3>();
	}

	private void Start ()
	{
		offset = transform.position - target.transform.position;
		offsetDirection = offset.normalized;
	}

	private void LateUpdate(){
		if (lockCursor) {
			Cursor.lockState = CursorLockMode.Locked;
		}
		float verticalDelta;
		float horizontalDelta;
		if (useMouse) {
			// Calculate mouse delta position
			var deltaMousePosition = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
			// Calculate vertical and horizontal delta
			verticalDelta = -deltaMousePosition.y;
			horizontalDelta = deltaMousePosition.x;
		}
		else {
			verticalDelta = Input.GetAxis("Vertical");
			horizontalDelta = Input.GetAxis("Horizontal");
		}

		// Current direction from camera to target
		offsetDirection = offset.normalized;
		// Current camera pitch
		var currentPitch = 90f - (float) (Mathf.Rad2Deg * Math.Acos(Vector3.Dot(Vector3.up, offsetDirection)));
		// Maximum positive delta pitch
		var maxPosDeltaPitch = maximumPitchAngle - currentPitch;
		// Maximum negative delta pitch
		var maxNegDeltaPitch = minimumPitchAngle - currentPitch;

		// Apply speeds
		verticalDelta *= Time.unscaledDeltaTime * verticalSpeed;
		horizontalDelta *= Time.unscaledDeltaTime * horizontalSpeed;

		// Clamped delta pitch
		var deltaPitch = Mathf.Clamp(verticalDelta, maxNegDeltaPitch, maxPosDeltaPitch);
		var deltaRoll = horizontalDelta;

		// Rotate deltaPitch degrees around X, deltaRoll degrees around Y
		var desiredRotation = Quaternion.AngleAxis(deltaRoll, Vector3.up) * Quaternion.AngleAxis(deltaPitch, transform.right);
		offset = desiredRotation * offset;
		transform.position = target.position + offset;
		transform.LookAt(target);
		HandleOldOffsets();
	}

	/// <summary>
	/// Handles the storing of old offset vectors in memory
	/// </summary>
	private void HandleOldOffsets(){
		if (showOffsetTrails) {
			oldOffsets.Enqueue(offset);
			if (oldOffsets.Count > maxOldOffsetCount) {
				// Never allow the queue to grow more than maxOldOffsetCount
				oldOffsets.Dequeue();
			}
		}
	}

	private void OnDrawGizmos(){
		Gizmos.color = Color.red;
		if (showOffset) {
			GizmosUtil.DrawArrow(target.position, transform.position);
		}

		if (showPitchLimits) {
			if (maximumPitchAngle >= 0f) {
				DebugExtension.DrawCone(target.transform.position, Vector3.up * offset.magnitude * Mathf.Sin(Mathf.Deg2Rad * maximumPitchAngle), Color.red,
					90f - maximumPitchAngle);
			}
			else {
				DebugExtension.DrawCone(target.transform.position, Vector3.down * offset.magnitude * -Mathf.Sin(Mathf.Deg2Rad * maximumPitchAngle), Color.red,
					90f + maximumPitchAngle);
			}

			if (minimumPitchAngle >= 0f) {
				DebugExtension.DrawCone(target.transform.position, Vector3.up * offset.magnitude * Mathf.Sin(Mathf.Deg2Rad * minimumPitchAngle), Color.red,
					90f - minimumPitchAngle);
			}
			else {
				DebugExtension.DrawCone(target.transform.position, Vector3.down * offset.magnitude * -Mathf.Sin(Mathf.Deg2Rad * minimumPitchAngle), Color.red,
					90f + minimumPitchAngle);
			}
		}

		if (showOffsetTrails && oldOffsets != null) {
			float biggestX = 0;
			float biggestY = 0;
			float biggestZ = 0;
			foreach (var oldOffset in oldOffsets) {
				if (oldOffset.x > biggestX) biggestX = oldOffset.x;
				if (oldOffset.y > biggestY) biggestY= oldOffset.y;
				if (oldOffset.z > biggestZ) biggestZ = oldOffset.z;
			}

			foreach (var oldOffset in oldOffsets) {
				Gizmos.color = new Color(oldOffset.x / biggestX, oldOffset.y / biggestY, oldOffset.z / biggestZ);
				GizmosUtil.DrawArrow(target.position, target.position + oldOffset);
			}
		}
	}
}
