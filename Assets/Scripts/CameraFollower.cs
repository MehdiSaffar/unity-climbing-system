using UnityEngine;

/// <summary>
/// Follows the target's translational movements
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFollower : MyMonoBehaviour
{
	/// <summary>
	/// Target to follow
	/// </summary>
	public Transform mTarget;
	/// <summary>
	/// Displacement vector from the camera to the target
	/// </summary>
	private Vector3 mDisplacement;
	void Start ()
	{
		mDisplacement = mTarget.position - transform.position;
	}

	void Update ()
	{
		transform.position = mTarget.position - mDisplacement;
	}
}
