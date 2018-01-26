using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(PointsHelperLine))]
public class LineHelperInspector : Editor{
	private PointsHelperLine line;
	private Transform handleTransform;
	private Quaternion handleRotation;
	private void OnSceneGUI(){
		// Get the target script
		line = (PointsHelperLine) target;

		// Get the transform
		handleTransform = line.transform;

		// Set the rotation to local or global based on the user choice
		handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

		var start = line.startPosition;
		var end = line.endPosition;
		ShowPoint(start);
		ShowPoint(end);

	}

	private void ShowPoint(Transform point){
		var p = point.position;

	}
}
