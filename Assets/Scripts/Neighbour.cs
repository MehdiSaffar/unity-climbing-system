using JetBrains.Annotations;

using UnityEngine;

/// <summary>
/// Data for the neighbour point
/// </summary>
public class Neighbour {
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="origin">Origin point</param>
    /// <param name="point">Point considered neighbour to <paramref name="origin"/> point</param>
    /// <param name="movementType"><inheritdoc cref="MovementType"/>. default is <see cref="MovementType.Regular"/></param>
    public Neighbour([NotNull] Point origin, [NotNull] Point point, MovementType movementType = MovementType.Regular){
        this.point = point;
        displacement = point.transform.position - origin.transform.position;
        direction = displacement.normalized;
        this.movementType = movementType;
    }

    /// <summary>
    /// Reference to the neighbouring point
    /// </summary>
    public Point point;
    /// <summary>
    /// Type of movement required to move to it
    /// </summary>
    public MovementType movementType;
    /// <summary>
    /// Vector from this point to the target point
    /// </summary>
    public Vector3 displacement;

    /// <summary>
    /// A cached normalized vector pointing towards the target point
    /// </summary>
    public Vector3 direction;
}