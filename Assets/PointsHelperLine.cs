using System.Linq;
using UnityEngine;

public class PointsHelperLine : MonoBehaviour
{
	public Transform pointsList;
	public Point pointPrefab;
	public Transform startPosition;
	public Transform endPosition;
	public int pointsCount;

	public float normalRayLength;

	// Use this for initialization
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

		firstPoint._isRoot = true;
	}

	// Update is called once per frame
	void Update () {

	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		Gizmos.DrawLine(startPosition.position, endPosition.position);

		Vector3 midline = Vector3.Lerp(startPosition.position, endPosition.position, 0.5f);
		GizmosUtil.DrawArrow(midline, midline + transform.forward * normalRayLength);
//		Gizmos.DrawLine(midline, midline + transform.forward * normalRayLength);
	}
}
