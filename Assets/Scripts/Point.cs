using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Defines a climb point
/// </summary>
public class Point : MyMonoBehaviour{

	/// <summary>
	/// Enumeration of the possible ways a player can hang onto the climb point
	/// </summary>
	public enum HangType{
		BracedHang,
		FreeHang,
	}

	/// <summary>
	/// Reference the level list that contains all climb points
	/// </summary>
	[SerializeField] public Transform _pointsList;

	[Header("Point Characteristics")]

	/// <summary>
	/// Is the point considered root?
	/// </summary>
	public bool _isRoot;

	/// <summary>
	/// The way with which the player hangs to this points
	/// </summary>
	public HangType hangType;

	/// <summary>
	/// Position where the character's root transform should be at
	/// </summary>
	public Transform characterRoot;

	/// <summary>
	/// Position where the character's right hand should be at
	/// </summary>
	public Transform rightHand;

	/// <summary>
	/// Position where the character's left hand should be at
	/// </summary>
	public Transform leftHand;

	/// <summary>
	/// The distance within which this point can see neighbouring points
	/// </summary>
	public float _discoverDistance;

	private float _oldDiscoverDistance = -1;

	/// <summary>
	/// Cache of the square of <see cref="_discoverDistance"/> for quicker calculation
	/// </summary>
	private float _discoverSqrDistance;

	/// <summary>
	/// How divergent can the dot product be from 1 (in case of
	/// <see cref="GetNextPoint"/> )
	/// </summary>
	[SerializeField] private float DirectionDotProductThresholdValue = 0.8f;

	[Header("Gizmo")]
	/// <summary>
	/// Size of the cube gizmo
	/// </summary>
	[SerializeField] private float _gizmoCubeSize;

	/// <summary>
	/// Should this point display its range gizmo?
	/// </summary>
	[SerializeField] private bool shouldDisplayRange;

	/// <summary>
	/// Length of the normal ray
	/// </summary>
	[SerializeField] private float normalRayLength;

	[Header("Other")]
	/// <summary>
	/// List of the neighbouring points to which this point is connected
	/// </summary>
	public List<Point> _neighbourPoints;


	/// <summary>
	/// Normal of the point that the character should face
	/// </summary>
	public Vector3 normal => transform.forward;

	/// <summary>
	/// Is this node visited? (used during graph search)
	/// </summary>
	private bool isVisited;

	// TODO: Needs to be changed to something more rigorous
	/// <summary>
	/// Does this point require the player to be facing a certain way?
	/// </summary>
	public bool HasNormal => normal != Vector3.zero;

	private float _epsilonDistance = 0.001f;

	private void Awake(){
		_discoverSqrDistance = _discoverDistance * _discoverDistance;
		isVisited = false;
	}

	// Use this for initialization
	void Start(){
		if (_isRoot) {
			DiscoverNeighbours();
		}
	}

	private void Update(){
		if (Math.Abs(_discoverDistance - _oldDiscoverDistance) < _epsilonDistance) {
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

	/// <summary>
	/// Removes all neighbours
	/// </summary>
	private void ResetNeighbours(){
		// TODO: Probably should also make the points disconnect from this point
		_neighbourPoints.Clear();
	}

	/// <summary>
	/// Recursively traverses the neighbouring points and connects them
	/// based on the distance between them (shortest distances first)
	/// </summary>
	public void DiscoverNeighbours(){
		isVisited = true;
		var points = _pointsList.GetComponentsInChildren<Point>()
			// Get all points
			// 1) that different than this one,
			// 2) to whom this point is not connected
			// 3) that are not connected to me
			.Where(point => point != this)
			//  && !IsConnected(point) && !point.IsConnected(this)
			// Calculate the distance of each point and "attach" this information to a dictionary
			// in which
			// Key: the points themselves
			// Value: The distance to this point
			.ToDictionary(
				point => point,
				point => (point.transform.position - transform.position).sqrMagnitude)
			// Keep only the points close enough
			.Where(pair => pair.Value <= _discoverSqrDistance)
			// Order the points by increasing distance
			.OrderBy(pair => pair.Value)
			// Get the points
			.Select(pair => pair.Key)
			// And form a List<Point>
			.ToList();
		foreach (var point in points) {
			if (!point.isVisited) {
				// For every point with which we have no link
				AddNeighbour(point);
				point.AddNeighbour(this);
				point.DiscoverNeighbours();
				// Let the point also discover its neighbours
				// I believe this makes a breadth first search?
			}
		}
	}

	/// <summary>
	/// Adds the points in bulk to the list of neighbouring points
	/// </summary>
	/// <param name="points">Points to add</param>
	private void AddNeighbours(IEnumerable<Point> points){
		_neighbourPoints.AddRange(points);
	}

	/// <summary>
	/// Adds a point to the list of neighbouring points
	/// </summary>
	/// <param name="point">Point to add</param>
	private void AddNeighbour(Point point){
		_neighbourPoints.Add(point);
	}

	/// <summary>
	/// Is <paramref name="otherPoint"/> a neighbour of this point?
	/// </summary>
	public bool IsConnected(Point otherPoint){
		return _neighbourPoints.Exists(pt => otherPoint == pt);
	}

	/// <summary>
	/// Gets the neighbouring point in a certain direction
	/// </summary>
	/// <param name="directionVector">Direction to consider</param>
	/// <returns></returns>
	public Point GetNextPoint(Vector3 directionVector){
		if (directionVector == Vector3.zero) {
			throw new ArgumentException($"{nameof(directionVector)} cannot be Vector3.zero.");
		}
		var normalizedRightVector = directionVector.normalized;
		foreach (var point in _neighbourPoints) {
			var pointVector = (point.transform.position - transform.position).normalized;
			var dotProduct = Vector3.Dot(pointVector, normalizedRightVector);
			if (dotProduct >= DirectionDotProductThresholdValue) {
				return point;
			}
		}

		return null;
	}

	private void OnDrawGizmos(){
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(transform.position, new Vector3(_gizmoCubeSize, _gizmoCubeSize, _gizmoCubeSize));

		foreach (var neighbourPoint in _neighbourPoints) {
			Gizmos.DrawLine(transform.position, neighbourPoint.transform.position);
		}

		if (shouldDisplayRange) {
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, _discoverDistance);
		}

		if (normal != Vector3.zero) {
			Gizmos.color = Color.red;
			GizmosUtil.DrawArrow(transform.position, transform.position + normal * normalRayLength);
		}
	}
}
