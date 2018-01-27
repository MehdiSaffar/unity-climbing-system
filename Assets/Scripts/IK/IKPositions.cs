using System;

/// <summary>
/// Description of the IK positions of the various body parts of a human
/// </summary>
[Serializable]
public class IKPositions {
    /// <summary>
    /// Target IK for the head
    /// </summary>
    public IKTarget head = new IKTarget();

    /// <summary>
    /// Target IK for the right hand
    /// </summary>
    public IKTarget rightHand= new IKTarget();

    /// <summary>
    /// Target IK for the left hand
    /// </summary>
    public IKTarget leftHand= new IKTarget();

    /// <summary>
    /// Target IK for the hips
    /// </summary>
    public IKTarget hips= new IKTarget();

    /// <summary>
    /// Target IK for the right foot
    /// </summary>
    public IKTarget rightFoot= new IKTarget();

    /// <summary>
    /// Target IK for the left foot
    /// </summary>
    public IKTarget leftFoot= new IKTarget();
}