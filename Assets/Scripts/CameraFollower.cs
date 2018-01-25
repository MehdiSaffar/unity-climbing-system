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
	/// <summary>
	/// Target to follow
	/// </summary>
	public Transform mTarget;
	/// <summary>
	/// Displacement vector from the target to the camera
	/// </summary>
	private Vector3 mOffset;

	/// <summary>
	/// Mouse position in previous frame
	/// </summary>
	private Vector3 lastMousePosition;

	/// <summary>
	/// Normalized vector from target position to the camera
	/// </summary>
	private Vector3 mOffsetDirection;

	/// <summary>
	/// Should the camera be controller with mouse movement?
	/// </summary>
	[SerializeField] private bool useMouse;

	/// <summary>
	/// Should the camera show a trail of offsets?
	/// </summary>
	[SerializeField] private bool showOffsetTrails;

	/// <summary>
	///  Maximum allowed pitch
	/// </summary>
	[SerializeField][Range(0, 85)] private float maxPitch;

	[SerializeField][Range(-30, 0)]private float minPitch;

	private Queue<Vector3> mOldOffsets;

		void Start ()
	{
		mOffset = transform.position - mTarget.transform.position;
		mOffsetDirection = mOffset.normalized;
		mOldOffsets = new Queue<Vector3>();
	}

	private void LateUpdate(){
		float verticalDelta;
		float horizontalDelta;
		if (useMouse) {
			var currMousePosition = Input.mousePosition;
			// Calculate mouse delta position
			var deltaMousePosition = currMousePosition - lastMousePosition;
			verticalDelta = -deltaMousePosition.y;
			horizontalDelta = deltaMousePosition.x;
			lastMousePosition = currMousePosition;
		}
		else {
			verticalDelta = Input.GetAxis("Vertical");
			horizontalDelta = Input.GetAxis("Horizontal");
		}

		// Current direction from camera to target
		mOffsetDirection = mOffset.normalized;
		// Current camera pitch
		var currentPitch = 90f - (float) (Mathf.Rad2Deg * Math.Acos(Vector3.Dot(Vector3.up, mOffsetDirection)));
		Debug.Log($"Current pitch: {currentPitch}");


//		// Maximum positive delta pitch
		var maxPosDeltaPitch = maxPitch - currentPitch;
//		// Maximum negative delta pitch
		var maxNegDeltaPitch = minPitch - currentPitch;
		// Clamped delta pitch
		var deltaPitch = Mathf.Clamp(verticalDelta, maxNegDeltaPitch, maxPosDeltaPitch);
//		var deltaPitch = -deltaMousePosition.y;
		var deltaRoll = horizontalDelta;

		// Rotate deltaPitch degrees around X, deltaRoll degrees around Y
		var desiredRotation = Quaternion.AngleAxis(deltaRoll, Vector3.up) * Quaternion.AngleAxis(deltaPitch, transform.right);
		mOffset = desiredRotation * mOffset;
		transform.position = mTarget.position + mOffset;
		transform.LookAt(mTarget);
		if (showOffsetTrails) {
		mOldOffsets.Enqueue(mOffset);
		if(mOldOffsets.Count > 100)
			mOldOffsets.Dequeue();

		}

	}

	private void OnDrawGizmos(){
		Gizmos.color = Color.red;
		GizmosUtil.DrawArrow(mTarget.position, transform.position);

		DebugExtension.DrawCone(mTarget.transform.position, Vector3.up * mOffset.magnitude, Color.red, 90f - maxPitch);
		DebugExtension.DrawCone(mTarget.transform.position, Vector3.down * mOffset.magnitude, Color.red, 90f - minPitch);

		if (showOffsetTrails && mOldOffsets != null) {
			float biggestX = 0;
			float biggestY = 0;
			float biggestZ = 0;
			foreach (var oldOffset in mOldOffsets) {
				if (oldOffset.x > biggestX) biggestX = oldOffset.x;
				if (oldOffset.y > biggestY) biggestY= oldOffset.y;
				if (oldOffset.z > biggestZ) biggestZ = oldOffset.z;
			}

			foreach (var oldOffset in mOldOffsets) {
				Gizmos.color = new Color(oldOffset.x / biggestX, oldOffset.y / biggestY, oldOffset.z / biggestZ);
				GizmosUtil.DrawArrow(mTarget.position, mTarget.position + oldOffset);

			}
		}
	}
}
