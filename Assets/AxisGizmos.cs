using UnityEngine;

public class AxisGizmos : MyMonoBehaviour {
	private void OnDrawGizmos(){
		Gizmos.color = Color.red;
		GizmosUtil.DrawArrow(transform.position, transform.position + transform.right);

		Gizmos.color = Color.green;
		GizmosUtil.DrawArrow(transform.position, transform.position + transform.up);

		Gizmos.color = Color.blue;
		GizmosUtil.DrawArrow(transform.position, transform.position + transform.forward);
	}
}
