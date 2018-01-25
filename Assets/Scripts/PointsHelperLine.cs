using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using UnityEngine;

using Debug = System.Diagnostics.Debug;

/// <summary>
/// Helper class to quickly set climb points along a line defined from <see cref="startPosition"/> to <see cref="endPosition"/>
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
	/// <summary>
	/// Start position of climb points
	/// </summary>
	[SerializeField] private Transform startPosition;
	/// <summary>
	/// End position of climb points
	/// </summary>
	[SerializeField] private Transform endPosition;

	/// <summary>
	/// Distance between points
	/// </summary>
	[SerializeField] [UnityEngine.Range(0.1f,3f)] private float intervalDistance;

	/// <summary>
	/// Length of the gizmo ray that shows the normal of the points
	/// </summary>
	[SerializeField] private float normalRayLength;

	[SerializeField] private List<Point> placedPoints;

	[SerializeField] private bool update;

	private void Awake(){
		placedPoints = new List<Point>();
		update = false;
	}

	private void Update(){
		if (update) {
			update = false;
			RemovePointsFromScene();
			SetPointsInScene();
		}
	}

	private void RemovePointsFromScene(){
		foreach (var placedPoint in placedPoints) {
			if(placedPoint)
				DestroyImmediate(placedPoint.gameObject);

		}
		placedPoints.Clear();
	}

	private void SetPointsInScene(){
		Point firstPoint = null;
		var totalDistance = (endPosition.transform.position - startPosition.transform.position).magnitude;
		var pointsCount = totalDistance / intervalDistance;
		for (int i = 0; i < pointsCount; i++) {
			var position = Vector3.Lerp(startPosition.position, endPosition.position, i / pointsCount);
			var newPoint = Instantiate(pointPrefab.gameObject, position, transform.rotation, pointsList)
				.GetComponent<Point>();
			newPoint._pointsList = pointsList;
			if (i == 0) {
				firstPoint = newPoint;
			}
			placedPoints.Add(newPoint);
		}

		if(firstPoint)
			firstPoint._isRoot = true;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		Gizmos.DrawLine(startPosition.position, endPosition.position);

		var midline = Vector3.Lerp(startPosition.position, endPosition.position, 0.5f);
		GizmosUtil.DrawArrow(midline, midline + transform.forward * normalRayLength);
	}
}
