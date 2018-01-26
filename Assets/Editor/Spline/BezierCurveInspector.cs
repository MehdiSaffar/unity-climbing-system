using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(BezierCurve))]
public class BezierCurveInspector : Editor{

	private BezierCurve curve;
	private Transform handleTransform;
	private Quaternion handleRotation;
	private Color lineColor = Color.white;
	private Color curveColor = Color.gray;
	[SerializeField] private Color curveTangentColor = Color.red;
	private const float directionScale = 0.5f;

	private const int lineSteps = 25;

	private void OnSceneGUI(){
		curve = target as BezierCurve;
		handleTransform = curve.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

		var p0 = ShowPoint(0);
		var p1 = ShowPoint(1);
		var p2 = ShowPoint(2);
		var p3 = ShowPoint(3);

		Handles.color = lineColor;
		Handles.DrawLine(p0,p1);
//		Handles.DrawLine(p1,p2);
		Handles.DrawLine(p2,p3);

		ShowDirections();
		Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);



	}

	private void ShowDirections(){
		for (int i = 0; i < lineSteps; i++) {
			var t = i / (float) lineSteps;
			var pos = curve.GetPoint(t);
			var dir = curve.GetDirection(t);
			Handles.color = curveTangentColor;
			Handles.DrawLine(pos, pos + dir * directionScale);
		}
	}

	/// <summary>
	/// Shows and returns coordinates of the point
	/// </summary>
	private Vector3 ShowPoint(int index){
		var point = handleTransform.TransformPoint(curve.points[index]);
		EditorGUI.BeginChangeCheck();
		point = Handles.DoPositionHandle(point, handleRotation);
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(curve, "Move point");
			EditorUtility.SetDirty(curve);
			curve.points[index] = handleTransform.InverseTransformPoint(point);
		}

		return point;
	}
}
