using System;
using System.Collections.Generic;
using System.Linq;

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

	private TextMesh _textMesh;
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
	/// Contains the position rotation targets of the character's body parts
	/// </summary>
	public IKPositions ik;

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

	/// <summary>
	/// Size of the cube gizmo
	/// </summary>
	[Header("Gizmo")]
	[SerializeField] private float _gizmoCubeSize;

	/// <summary>
	/// Should this point display its range gizmo?
	/// </summary>
	[SerializeField] private bool shouldDisplayRange;

	/// <summary>
	/// Should this point display its normal vector?
	/// </summary>
	[SerializeField] private bool shouldDisplayNormal;

	/// <summary>
	/// Length of the normal ray
	/// </summary>
	[SerializeField] private float normalRayLength;

	/// <summary>
	/// List of the neighbouring points to which this point is connected
	/// </summary>
	[Header("Other")]
	public List<Neighbour> neighbours;

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
//		_textMesh = GetComponent<TextMesh>();
		neighbours = new List<Neighbour>();
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
		neighbours.Clear();
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
	private void AddNeighbours(IEnumerable<Point> points, MovementType movementType = MovementType.Regular){
		foreach (var point in points) {
			var neighbour = new Neighbour(this, point, movementType);
			neighbours.Add(neighbour);
		}
	}

	/// <summary>
	/// Adds a point to the list of neighbouring points
	/// </summary>
	/// <param name="point">Point to add</param>
	private void AddNeighbour(Point point, MovementType movementType = MovementType.Regular){
		var neighbour = new Neighbour(this, point, movementType);
		neighbours.Add(neighbour);
	}

	/// <summary>
	/// Is <paramref name="otherPoint"/> a neighbour of this point?
	/// </summary>
	public bool IsConnected(Point otherPoint){
		return neighbours.Exists(pt => otherPoint == pt.point);
	}

	/// <summary>
	/// Gets the neighbouring point in a certain direction
	/// </summary>
	/// <param name="directionVector">Direction to consider</param>
	/// <returns>Returns the point if found otherwise null</returns>
	/// TODO Should be changed to <see cref="Neighbour"/>
	public Point GetNextPoint(Vector3 directionVector){
		if (directionVector == Vector3.zero) {
			throw new ArgumentException($"{nameof(directionVector)} cannot be a zero vector.");
		}
		var normalizedRightVector = directionVector.normalized;
		foreach (var neighbour in neighbours) {
			var dotProduct = Vector3.Dot(neighbour.direction, normalizedRightVector);
			if (dotProduct >= DirectionDotProductThresholdValue) {
				return neighbour.point;
			}
		}

		return null;
	}

	private void OnDrawGizmos(){
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(transform.position, new Vector3(_gizmoCubeSize, _gizmoCubeSize, _gizmoCubeSize));

		if (neighbours != null) {
			foreach (var neighbourPoint in neighbours) {
				Gizmos.DrawLine(transform.position, neighbourPoint.point.transform.position);
			}
		}

		if (shouldDisplayRange) {
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, _discoverDistance);
		}

		if (shouldDisplayNormal) {
			if (normal != Vector3.zero) {
				Gizmos.color = Color.red;
				GizmosUtil.DrawArrow(transform.position, transform.position + normal * normalRayLength);
			}
		}
	}
}