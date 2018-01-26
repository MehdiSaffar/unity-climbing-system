using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEditor;

using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[CustomEditor(typeof(PointsHelperLine))]
public class LineHelperInspector : Editor{
	private PointsHelperLine line;
	private Transform handleTransform;
	private Quaternion handleRotation;
	/// <summary>
	/// Normal of the line
	/// </summary>
	private Vector3 norm;
	/// <summary>
	/// Color of the normal vector
	/// </summary>
	private readonly Color normalColor = Color.black;
	/// <summary>
	/// Color of the line
	/// </summary>
	private readonly Color lineColor = Color.gray;
	/// <summary>
	/// Midpoint of the line
	/// </summary>
	private Vector3 midpoint;

	/// <summary>
	/// Snap angle of the normal rotation in degrees
	/// </summary>
	private const float normalSnapAngle = 22.5F;

	/// <summary>
	/// Display length of the normal vector (in world space)
	/// </summary>
	private const float normalVectorLength = 0.5f;
	/// <summary>
	/// Display radius of the normal disk (in world space)
	/// </summary>
	private const float normalDiskRadius = normalVectorLength / 4f;

	private void OnSceneGUI(){
		// Get the target script
		line = (PointsHelperLine) target;

		// Get the transform
		handleTransform = line.transform;

		// Set the rotation to local or global based on the user choice
		handleRotation = Tools.pivotRotation == PivotRotation.Local ? handleTransform.rotation : Quaternion.identity;

		// Show the point and get the new positions if changed
		var p0 = ShowPoint(0);
		var p1 = ShowPoint(1);

		// Get the nomal of the line
		norm = line.GetNormal();
		midpoint = Vector3.Lerp(p0, p1, 0.5f);
		ShowNormal();

		Handles.color = lineColor;
		Handles.DrawLine(p0, p1);
	}

	public override void OnInspectorGUI(){
		DrawDefaultInspector();
		line = (PointsHelperLine) target;
		EditorGUI.BeginChangeCheck();
		var shouldReset = GUILayout.Button("Reset");
		if(EditorGUI.EndChangeCheck()) {
			if (shouldReset) {
				Undo.RecordObject(line, "Reset line");
				EditorUtility.SetDirty(line);
				line.Reset();
			}
		}
	}

	/// <summary>
	/// Shows the point, changes its position if done by the user and returns
	/// its new position in both cases
	/// </summary>
	/// <param name="index">Index of the point in the line</param>
	/// <returns>The current point position (world space)</returns>
	private Vector3 ShowPoint(int index){
		var pos = handleTransform.TransformPoint(line.GetPoint(index));
		EditorGUI.BeginChangeCheck();
		pos = Handles.DoPositionHandle(pos, handleRotation);
		if(EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(line, "Move point");
			EditorUtility.SetDirty(line);
			line.SetPoint(index, handleTransform.InverseTransformPoint(pos));
		}
		return pos;
	}

	/// <summary>
	/// Shows the normal
	/// </summary>
	/// <returns>New normal vector</returns>
	private Vector3 ShowNormal(){
		// Draw the normal
		var worldNormal = handleTransform.TransformVector(norm);
		Handles.DrawLine(midpoint, midpoint + worldNormal * normalVectorLength);

		var normalRotation = Quaternion.LookRotation(worldNormal, line.GetDirectionVector());
		EditorGUI.BeginChangeCheck();
		var newNormalRotation = Handles.Disc(normalRotation,
			midpoint,
			line.GetDirectionVector(),
			normalDiskRadius,
			false,
			normalSnapAngle);
		if(EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(line, "Change line normal");
			EditorUtility.SetDirty(line);
			var newWorldNormal = newNormalRotation * Quaternion.Inverse(normalRotation) * worldNormal;
			var newLocalNormal = handleTransform.InverseTransformVector(newWorldNormal);
			line.SetNormal(newLocalNormal);
		}
		return Vector3.zero;
	}

}
