using System;

[Serializable]
public class HangInfo
{
    /// <summary>
    /// State of the hang
    /// Final means that the player is grabbing the climb point with two hands
    /// and is able to move to any other point
    /// Midpoint means that the player is currently transitioning from a point to another
    /// he generally can only do one of two things in this state
    /// either keep moving in <see cref="currentDirection"/> or cancel his movement
    /// by moving back to the previous point
    /// </summary>
    public enum HangState
    {
        /// <summary>
        /// Final means the character has both hands on the ledge and can move
        /// </summary>
        Final,
        /// <summary>
        /// Midpoint means the character is holding two points and can only move forward or cancel his movement
        /// </summary>
        Midpoint,
        /// <summary>
        /// The character is undergoing a transition animation (e.g. going from <see cref="Final"/> to Midpoint states
        /// </summary>
        Transition
    }
    /// <summary>
    /// Direction of the hang
    /// </summary>
    public enum Direction
    {
        None,
        Right,
        Left
    }
    /// <summary>
    /// State of the hang
    /// </summary>
    public HangState state;
    /// <summary>
    /// Direction of the current climbing movement
    /// </summary>
    public Direction currentDirection;
    /// <summary>
    /// Current point to which the player is grabbing
    /// </summary>
    public Point currentPoint;
    /// <summary>
    /// Next point to which the player should grab
    /// </summary>
    public Point nextPoint;
    /// <summary>
    /// State of the hang at the next point
    /// </summary>
    public HangState nextState;
}