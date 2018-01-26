using UnityEngine;

public class SplineWalker : MyMonoBehaviour{
	public SplineWalkerMode splineWalkerMode = SplineWalkerMode.PingPong;
	private float movementSign = 1f;
	public BezierSpline spline;
	public float duration;
	private float progress;
	public bool lookForward;

	private void Update(){
		progress += movementSign * Time.deltaTime / duration;
		if (progress > 1f) {
			switch (splineWalkerMode) {
				case SplineWalkerMode.Once:
					movementSign = 0f;
					progress = 1f;
					break;
				case SplineWalkerMode.Loop:
					progress = 0f;
					break;
				case SplineWalkerMode.PingPong:
					movementSign = -movementSign;
					progress = 1f;
					break;

			}
		} else if (progress < 0f) {
			progress = -0f;
			movementSign = -movementSign;
		}

		var position = spline.GetPoint(progress);
		transform.localPosition = position;
		if (lookForward) {
			transform.rotation = Quaternion.LookRotation(spline.GetVelocity(progress));
		}
	}
}

public enum SplineWalkerMode{
	Once, Loop, PingPong
}