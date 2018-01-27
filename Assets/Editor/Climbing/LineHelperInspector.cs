using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEditor;

using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

using Object = UnityEngine.Object;

[CustomEditor(typeof(PointsHelperLine))]
public class LineHelperInspector : Editor{
	private PointsHelperLine line;
	private Transform handleTransform;
	private Quaternion handleRotation;
	/// <summary>
	/// Normal of the line
	/// </summary>
	private Vector3 normal;
	/// <summary>
	/// Color of the normal vector
	/// </summary>
	private readonly Color normalColor = Color.black;
	/// <summary>
	/// Color of the line
	/// </summary>
	private readonly Color lineColor = Color.black;
	/// <summary>
	/// Midpoint of the line
	/// </summary>
	private Vector3 midpoint;

	[SerializeField] private static Transform _pointsList;
	/// <summary>
	/// Reference to the empty object in the scene that holds all the points
	/// </summary>
	private static Transform PointsList{
		get
		{
			if (_pointsList == null) {
				_pointsList = GameObject.FindGameObjectWithTag("PointsList").transform;
			}

			return _pointsList;
		}
	}

	/// <summary>
	/// Size of the cube handle representing a climb point
	/// </summary>
	private float climbPointCubeSize = 0.05f;

	/// <summary>
	/// Color of the cube handle representing a climb point
	/// </summary>
	private Color climbPointCubeColor = Color.blue;


	/// <summary>
	/// Snap angle of the normal rotation in degrees
	/// </summary>
	private const float normalSnapAngle = 22.5F;
	/// <summary>
	/// Display length of the normal vector (in world space)
	/// </summary>
	private const float normalVectorLength = 0.75f;
	/// <summary>
	/// Display radius of the normal disk (in world space)
	/// </summary>
	private const float normalDiskRadius = normalVectorLength / 2f;

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
		normal = line.Normal;
		midpoint = Vector3.Lerp(p0, p1, 0.5f);
		ShowNormal();

		Handles.color = lineColor;
		Handles.DrawLine(p0, p1);

		for (int i = 0; i < line.ClimbPointCount; i++) {
//			Debug.Log($"Index {i}");
			ShowClimbPoint(i);
		}
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
		EditorGUI.BeginChangeCheck();
		var shouldSpawnClimbPoints = GUILayout.Button("Spawn Points");
		if(EditorGUI.EndChangeCheck()) {
			if (shouldSpawnClimbPoints) {
				for (int i = 0; i < line.ClimbPointCount; i++) {
					var spawnedPoint = GenerateClimbPoint(i);
					Undo.RecordObject(spawnedPoint, "Spawn climb points");
					EditorUtility.SetDirty(spawnedPoint);
				}
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
		float buttonSize = 0.1f * HandleUtility.GetHandleSize(pos);
		Handles.Button(pos, Quaternion.identity, buttonSize, buttonSize, Handles.SphereHandleCap);
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
	/// Shows a climb point on the line
	/// </summary>
	/// <param name="index"></param>
	private void ShowClimbPoint(int index){
		var worldPos = handleTransform.TransformPoint(line.GetClimbPointPosition(index));
		var normalRotation = Quaternion.LookRotation(normal, line.GetDirectionVector());
		Handles.color = climbPointCubeColor;
		if(Event.current.type == EventType.Repaint)
			Handles.CubeHandleCap(0, worldPos, normalRotation, climbPointCubeSize, EventType.Repaint);
	}

	/// <summary>
	/// Shows the normal
	/// </summary>
	/// <returns>New normal vector in world space</returns>
	private Vector3 ShowNormal(){
		// Get handle size
//		var handleSize = HandleUtility.GetHandleSize(midpoint);

		// Draw the normal
		var worldNormal = handleTransform.TransformVector(normal);
		Handles.color = normalColor;
		Handles.DrawLine(midpoint, midpoint + worldNormal * normalVectorLength);

		var normalRotation = Quaternion.LookRotation(worldNormal, line.GetDirectionVector());
		Handles.ArrowHandleCap(0, midpoint + normalVectorLength * worldNormal, normalRotation, 1f, EventType.Repaint);
//		Handles.DrawArrow(0, midpoint + normalVectorLength * worldNormal, normalRotation, 1f);
		EditorGUI.BeginChangeCheck();
		var newNormalRotation = Handles.Disc(normalRotation,
			midpoint,
			line.GetDirectionVector(),
			normalDiskRadius/* * handleSize */,
			false,
			normalSnapAngle);
		if(EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(line, "Change line normal");
			EditorUtility.SetDirty(line);
			worldNormal = newNormalRotation * Quaternion.Inverse(normalRotation) * worldNormal;
			normal = handleTransform.InverseTransformVector(worldNormal);
			line.SetNormal(normal);
		}
		return worldNormal;
	}

	/// <summary>
	/// Generates and keeps track of the climb point's gameobject
	/// </summary>
	private GameObject GenerateClimbPoint(int index) {
		var worldPos = handleTransform.TransformPoint(line.GetClimbPointPosition(index));
		var gameobject = Instantiate(GetClimbPointGameObject(line.GetClimbPointType(index)));
		gameobject.transform.SetParent(_pointsList);
		gameobject.transform.position = worldPos;
		gameobject.transform.rotation = Quaternion.LookRotation(normal);
		return gameobject;
	}

	/// <summary>
	/// Gets the gameobject corresponding to the climb type of the point
	/// </summary>
	private GameObject GetClimbPointGameObject(PointsHelperLine.ClimbPointType climbPointType){
		switch (climbPointType) {
			case PointsHelperLine.ClimbPointType.Hang:
				return Resources.Load("Braced Hang") as GameObject;
			default:
				throw new InvalidOperationException($"Climb point type {climbPointType} not recognized/implemented.");
		}
	}
}
