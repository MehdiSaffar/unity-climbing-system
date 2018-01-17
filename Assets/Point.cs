using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using NUnit.Framework;
using UnityEngine;

public class Point : MonoBehaviour
{
	public enum HangType
	{
		BracedHang,
		FreeHang,
	}

	public Transform _pointsList;
	public Transform characterRoot;
	public Transform rightHand;
	public Transform leftHand;
	private float _oldDiscoverDistance = -1;
	public float _discoverDistance;
	private float _discoverSqrDistance;
	public float _gizmoSize;
	public bool _isRoot;
	public List<Point> _neighbourPoints;
	public bool displayRange;

	public Vector3 wallNormal
	{
		get { return transform.forward; }
	}

	public HangType hangType;
	public bool visited;

	public bool HasNormal
	{
		// TODO: Needs to be changed to something more rigorous
		get { return wallNormal != Vector3.zero; }
	}

	public float wallNormalRayLength;

	private float _epsilonDistance = 0.001f;

	private void Awake()
	{
		_discoverSqrDistance = _discoverDistance * _discoverDistance;
		visited = false;
	}

	// Use this for initialization
	void Start()
	{

		if (_isRoot)
		{
			DiscoverNeighbours();
		}

	}

	private void Update()
	{
		if (Math.Abs(_discoverDistance - _oldDiscoverDistance) < _epsilonDistance)
		{
			_oldDiscoverDistance = _discoverDistance;
			_discoverSqrDistance = _discoverDistance * _discoverDistance;
		}

//		if (_isRoot)
//		{
//			if (transform.hasChanged)
//			{
//				DiscoverNeighbours();
//			}
//		}
	}

	private void ResetNeighbours()
	{
		_neighbourPoints.Clear();
	}

	private void DiscoverNeighbours()
	{
		visited = true;
		var allPoints = _pointsList.GetComponentsInChildren<Point>()
			// Get all points
			// 1) that different than this one,
			// 2) to whom this point is not connected
			// 3) that are not connected to me
			.Where(point => point != this);
		//  && !IsConnected(point) && !point.IsConnected(this)
		// Calculate the distance of each point and "attach" this information to a dictionary
		// in which
		// Key: the points themselves
		// Value: The distance to this point
		var dictionary = allPoints.ToDictionary(
			point => point,
			point => (point.transform.position - transform.position).sqrMagnitude);
		var points = dictionary
			// Keep only the points close enough
			.Where(pair => pair.Value <= _discoverSqrDistance)
			// Order the points by increasing distance
			.OrderBy(pair => pair.Value)
			// Get the points
			.Select(pair => pair.Key)
			// And form a List<Point>
			.ToList();
		foreach (var point in points)
		{
			if (!point.visited)
			{
				// For every point with which we have no link
				AddNeighbour(point);
				point.AddNeighbour(this);
				point.DiscoverNeighbours();
				// Let the point also discover its neighbours
				// I believe this makes a breadth first search?
			}
		}
	}

	private void AddNeighbours(IEnumerable<Point> points)
	{
		_neighbourPoints.AddRange(points);
	}

	private void AddNeighbour(Point point)
	{
		_neighbourPoints.Add(point);
	}

	public bool IsConnected(Point otherPoint)
	{
		return _neighbourPoints.Exists(pt => otherPoint == pt);
	}

	public Point GetNextPoint(Vector3 directionVector)
	{
		var normalizedRightVector = directionVector.normalized;
		foreach (var point in _neighbourPoints)
		{
			var pointVector = point.transform.position - transform.position;
			pointVector.Normalize();
			var dotProduct = Vector3.Dot(pointVector, normalizedRightVector);
			float thresholdValue = 0.8f;
			if (dotProduct >= thresholdValue)
			{
				return point;
			}
		}

		return null;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(transform.position, new Vector3(_gizmoSize, _gizmoSize, _gizmoSize));

		foreach (var neighbourPoint in _neighbourPoints)
		{
			Gizmos.DrawLine(transform.position, neighbourPoint.transform.position);
		}

		if (displayRange)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, _discoverDistance);
		}

		if (wallNormal != Vector3.zero)
		{
			Gizmos.color = Color.red;
			GizmosUtil.DrawArrow(transform.position, transform.position + wallNormal * wallNormalRayLength);
		}
	}
}
