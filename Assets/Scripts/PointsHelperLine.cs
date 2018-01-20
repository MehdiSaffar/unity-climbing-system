using System.Linq;

using NUnit.Framework;

using UnityEngine;

using Debug = System.Diagnostics.Debug;

/// <summary>
/// Helper class to quickly set climb points along a line defined from <see cref="startPosition"/> to <see cref="endPosition"/>
/// </summary>
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
	/// Count of points to be placed along the line
	/// </summary>
	[SerializeField] private int pointsCount;

	/// <summary>
	/// Length of the gizmo ray that shows the normal of the points
	/// </summary>
	[SerializeField] private float normalRayLength;

	void Start ()
	{
		Point firstPoint = null;
		for (int i = 0; i <= pointsCount; i++)
		{
			var position = Vector3.Lerp(startPosition.position, endPosition.position, (float) i / pointsCount);
			var newPoint = Instantiate(pointPrefab.gameObject, position, transform.rotation, pointsList)
				.GetComponent<Point>();
			newPoint._pointsList = pointsList;
			if (i == 0)
			{
				firstPoint = newPoint;
			}
		}
		Debug.Assert(firstPoint != null, nameof(firstPoint) + " != null");
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
