using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used for various gizmo drawings
/// </summary>
public class GizmosUtil : MyMonoBehaviour
{
	/// <summary>
	/// Length of an arrow cap
	/// </summary>
	private const float DEFAULT_ARROW_CAP_LENGTH = 0.2f;
	/// <summary>
	/// Arrow that the arrow cap makes with the arrow itself
	/// </summary>
	private const float DEFAULT_ARROW_ANGLE = 30f;
	/// <summary>
	/// Color of the cube
	/// </summary>
	[SerializeField] private Color cubeColor;
	/// <summary>
	/// Length of the cube's edges
	/// </summary>
	[SerializeField] private float cubeLength;

	private void OnDrawGizmos()
	{
		Gizmos.color = cubeColor;
		Gizmos.DrawWireCube(transform.position, new Vector3(cubeLength, cubeLength, cubeLength));
	}

	/// <summary>
	/// Draws an arrow
	/// </summary>
	/// <param name="from">Start position</param>
	/// <param name="to">End position</param>
	/// <param name="arrowAngle">Angle of the arrow caps from the arrow's axis</param>
	/// <param name="arrowCapLength">Length of each of the arrow cap's "threads" </param>
	public static void DrawArrow(Vector3 from, Vector3 to, float arrowAngle = DEFAULT_ARROW_ANGLE, float arrowCapLength = DEFAULT_ARROW_CAP_LENGTH)
	{
		Gizmos.DrawLine(from, to);
		var direction = to - from;
		if (direction == Vector3.zero) return;
		var right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowAngle,0) * Vector3.forward;
		var left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowAngle,0) * Vector3.forward;
		var up = Quaternion.LookRotation(direction) * Quaternion.Euler(180 + arrowAngle,0,0) * Vector3.forward;
		var down = Quaternion.LookRotation(direction) * Quaternion.Euler(180 - arrowAngle,0,0) * Vector3.forward;
		Gizmos.DrawRay(from + direction, right * arrowCapLength);
		Gizmos.DrawRay(from + direction, left * arrowCapLength);
		Gizmos.DrawRay(from + direction, up * arrowCapLength);
		Gizmos.DrawRay(from + direction, down * arrowCapLength);
	}
}
