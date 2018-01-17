using UnityEngine;

public class CameraFollower : MonoBehaviour
{
	public Transform mTarget;
	private Vector3 mDisplacement;
	// Use this for initialization
	void Start ()
	{
		mDisplacement = mTarget.position - transform.position;
	}

	// Update is called once per frame
	void Update ()
	{
		transform.position = mTarget.position - mDisplacement;
	}
}
