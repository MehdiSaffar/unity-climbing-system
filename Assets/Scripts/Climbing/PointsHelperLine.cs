using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEditor;

using UnityEngine;

/// <summary>
/// Helper class to quickly set climb points along a line defined from <see cref="p0"/> to <see cref="p1"/>
/// </summary>
[ExecuteInEditMode]
public class PointsHelperLine : MyMonoBehaviour
{
	/// <summary>
	/// Reference the gameobject that parents all the level's climb points
	/// </summary>
	[SerializeField] private Transform pointsList;
	/// <summary>
	/// Point prefab to be used for placement
	/// </summary>
	[SerializeField] private Point pointPrefab;

	public Vector3[] points;

	/// <summary>
	/// Normal of the line
	/// </summary>
	public Vector3 normal;

	/// <summary>
	/// Distance between points
	/// </summary>
	[SerializeField] [Range(0.1f,3f)] private float intervalDistance;

	[SerializeField] private List<Point> placedPoints;

	private void Awake(){
		placedPoints = new List<Point>();
		Reset();
	}

	/// <summary>
	///  Resets the line
	/// </summary>
	public void Reset(){
		points = new[]{
			Vector3.left,
			Vector3.right,
		};
		normal = Vector3.forward;
	}

//	private void Update(){
//		if (update) {
//			update = false;
//			RemovePointsFromScene();
//			SetPointsInScene();
//		}
//	}

//	private void RemovePointsFromScene(){
//		foreach (var placedPoint in placedPoints) {
//			if(placedPoint)
//				DestroyImmediate(placedPoint.gameObject);
//
//		}
//		placedPoints.Clear();
//	}

//	private void SetPointsInScene(){
//		Point firstPoint = null;
//		var totalDistance = (endPosition.transform.position - startPosition.transform.position).magnitude;
//		var pointsCount = totalDistance / intervalDistance;
//		for (int i = 0; i < pointsCount; i++) {
//			var position = Vector3.Lerp(startPosition.position, endPosition.position, i / pointsCount);
//			var newPoint = Instantiate(pointPrefab.gameObject, position, transform.rotation, pointsList)
//				.GetComponent<Point>();
//			newPoint._pointsList = pointsList;
//			if (i == 0) {
//				firstPoint = newPoint;
//			}
//			placedPoints.Add(newPoint);
//		}
//
//		if(firstPoint)
//			firstPoint._isRoot = true;
//	}

//	private void OnDrawGizmos()
//	{
//		Gizmos.color = Color.black;
//		Gizmos.DrawLine(startPosition.position, endPosition.position);
//
//		var midline = Vector3.Lerp(startPosition.position, endPosition.position, 0.5f);
//		GizmosUtil.DrawArrow(midline, midline + transform.forward * normalRayLength);
//	}

	/// <summary>
	/// Gets the point with the specified index
	/// </summary>
	/// <param name="index">Index of the point in the line</param>
	/// <returns>The coordinates of the point in line local space</returns>
	public Vector3 GetPoint(int index) => points[index];

	/// <summary>
	/// Returns the line  vector from point at <paramref name="startIndex"/>, to point at <paramref name="endIndex"/>
	/// Returns the vector of the first line segment by default
	/// </summary>
	/// <param name="startIndex">Index of the start point</param>
	/// <param name="endIndex">Index of the last point</param>
	/// <returns>A line vector in the line's local space</returns>
	public Vector3 GetLineVector(int startIndex = 0, int endIndex = 1) => points[endIndex] - points[startIndex];

	/// <summary>
	/// Returns the direction of the line from point at <paramref name="startIndex"/>, to point at <paramref name="endIndex"/>
	/// Returns the direction of the first line segment by default
	/// </summary>
	/// <param name="startIndex">Index of the start point</param>
	/// <param name="endIndex">Index of the last point</param>
	/// <returns>A line direction vector in the line's local space</returns>
	public Vector3 GetDirectionVector(int startIndex = 0, int endIndex = 1) => GetLineVector(startIndex, endIndex).normalized;

	public void SetPoint(int index, Vector3 position){
		if(index > 1) throw new ArgumentException("Index > 1");
		var oldLineVector = GetDirectionVector();
		if(index == 0)
			points[0] = position;
		else
			points[1] = position;
		EnforceNormal(oldLineVector);

	}

	public void EnforceNormal(Vector3 oldLine){
		var newLineVector = GetDirectionVector();
		var cross = Vector3.Cross(oldLine, newLineVector);
		var crossMagnitude = cross.magnitude;
		var angle = Mathf.Rad2Deg * Mathf.Asin(crossMagnitude);
		var rotation = Quaternion.AngleAxis(angle, cross);
		normal = rotation * normal;
	}

	/// <summary>
	/// Returns the normal of the line
	/// </summary>
	public Vector3 GetNormal(){
		return normal;
	}

	/// <summary>
	/// Sets the normal of the line
	/// </summary>
	/// <param name="norm">Normal vector of the line</param>
	public void SetNormal(Vector3 norm){
		normal = norm;
	}
}
