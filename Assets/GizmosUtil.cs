using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosUtil : MonoBehaviour
{
	public const float DEFAULT_ARROW_LENGTH = 0.2f;
	public const int DEFAULT_ARROW_COUNT = 4;
	public const float DEFAULT_ARROW_ANGLE = 30f;
	public Color color;
	public float length;

	private void OnDrawGizmos()
	{
		Gizmos.color = color;
		Gizmos.DrawWireCube(transform.position, new Vector3(length, length, length));
	}

	public static void DrawArrow(Vector3 from, Vector3 to, float arrowAngle = DEFAULT_ARROW_ANGLE, float arrowCapLength = DEFAULT_ARROW_LENGTH)
	{
//		Gizmos.DrawLine(from, to);
//		var direction = to - from;
//		var normalizedDirection = direction.normalized;
//		float arrowHeight = (float) (Math.Sin(arrowAngle * Math.PI / 180f) * arrowLength);
//		float arrowBefore = (float) (Math.Cos(arrowAngle * Math.PI / 180f) * arrowLength);
//		var angleStep = 360f / DEFAULT_ARROW_COUNT;
//		for (int i = 0; i <= DEFAULT_ARROW_COUNT; i++)
//		{
//			var angle = i * angleStep;
//			var angleQuat = Quaternion.LookRotation(direction) * Quaternion.Euler(0, angle, 0);
//			Gizmos.DrawLine(to, to - (normalizedDirection * arrowBefore) + );
//		}

		Gizmos.DrawLine(from, to);
		var direction = to - from;
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
