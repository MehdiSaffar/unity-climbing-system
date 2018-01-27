using System;

using UnityEngine;

/// <summary>
/// Description of a target IK transform
/// </summary>
[Serializable]
public class IKTarget {
    /// <summary>
    /// Target transform
    /// </summary>
    public Transform transform;
    /// <summary>
    /// Weight assigned (0..1)
    /// </summary>
    public float weight;
}