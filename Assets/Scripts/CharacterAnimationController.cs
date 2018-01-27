using UnityEngine;

/// <summary>
///     Animation controller of the player character
/// </summary>
[RequireComponent(typeof(Animator))]
public class CharacterAnimationController : MyMonoBehaviour{



    /// <summary>
    ///     Animator responsible for the animations
    /// </summary>
    [HideInInspector] public Animator animator;

    /// <summary>
    ///     Current player velocity
    /// </summary>
    public Vector3 localVelocity;

    /// <summary>
    ///     How is the player hanging on the ledge?
    /// </summary>
    public Point.HangType hangType;


    /// <summary>
    ///     Player's local velocity
    /// </summary>
    private Vector3 _localVelocityBlend;
    /// <summary>
    ///     Player's local velocity blend
    /// </summary>
    private Vector3 localVelocityBlend{
        get { return _localVelocityBlend; }
        set
        {
            _localVelocityBlend = value;
            SetFloat(FLOAT_X_VELOCITY, _localVelocityBlend.x);
            SetFloat(FLOAT_Y_VELOCITY, _localVelocityBlend.y);
            SetFloat(FLOAT_Z_VELOCITY, _localVelocityBlend.z);
        }
    }

    /// <summary>
    /// Blend for forward speed (not velocity), used for actions that can only move forward
    /// Like crouching (Idle -> Crouch walking -> Crouch running)
    /// </summary>
    private float forwardSpeedBlend {
        get { return GetFloat(FLOAT_FORWARD_SPEED); }
        set
        {
            SetFloat(FLOAT_FORWARD_SPEED, value);
        }
    }

    /// <summary>
    ///     Distance of the feet of the player from the ground
    /// </summary>
    public float distanceToGround{
        get { return GetFloat(FLOAT_DISTANCE_TO_GROUND); }
        set { SetFloat(FLOAT_DISTANCE_TO_GROUND, value); }
    }

    private void Awake(){
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate(){
        SetBlends();
        ProcessStateCases();
        SetLastFrameStates();
    }

    #region Mecanim Animator Controller Methods
    /// <summary>
    ///     Triggers the trigger referred to by <paramref name="triggerIndex" /> by default, or resets it
    /// </summary>
    /// <param name="triggerIndex">Index of the trigger</param>
    /// <param name="setOrReset">True by default sets the trigger, false resets it</param>
    private void Trigger(int triggerIndex, bool setOrReset = true){
        if (setOrReset)
            animator.SetTrigger(TRIGGERS[triggerIndex]);
        else
            animator.ResetTrigger(TRIGGERS[triggerIndex]);
    }

    /// <summary>
    ///     Get the boolean parameter referred to by <paramref name="boolIndex" />
    /// </summary>
    /// <param name="boolIndex">Index of the boolean</param>
    /// <returns>The value of the boolean in the animator parameters</returns>
    private bool GetBool(int boolIndex){
        return animator.GetBool(BOOLS[boolIndex]);
    }

    /// <summary>
    ///     Get the float parameter referred to by <paramref name="floatIndex" />
    /// </summary>
    /// <param name="floatIndex">Index of the float</param>
    /// <returns>The value of the float in the animator parameters</returns>
    private float GetFloat(int floatIndex){
        return animator.GetFloat(FLOATS[floatIndex]);
    }

    /// <summary>
    ///     Sets the boolean referred to by <paramref name="boolIndex" />
    /// </summary>
    /// <param name="boolIndex">Index of the boolean</param>
    /// <param name="value">New value of the boolean</param>
    private void SetBool(int boolIndex, bool value){
        animator.SetBool(BOOLS[boolIndex], value);
    }

    /// <summary>
    ///     Sets the float referred to by <paramref name="floatIndex" />
    /// </summary>
    /// <param name="floatIndex">Index of the float</param>
    /// <param name="value">New value of the float</param>
    private void SetFloat(int floatIndex, float value){
        animator.SetFloat(FLOATS[floatIndex], value);
    }
    #endregion

    /// <summary>
    /// Sets the blends based on multiple factors (e.g. sets locomotion blend tree based on player's current speed)
    /// </summary>
    private void SetBlends(){
        // Set the velocity blends based on the current velocity by normalizing it with the highest speed we have
        localVelocityBlend = new Vector3(
            localVelocity.x / jogSpeed, // JogSpeed is currently the highest speed, TODO: Find a better way to signal that
            localVelocity.y,
            localVelocity.z / jogSpeed);

        // Set the forward speed (for crouching)
        forwardSpeedBlend = Mathf.Abs(localVelocity.z / walkSpeed);
    }

    /// <summary>
    ///     Processes state transitions case by case
    /// </summary>
    private void ProcessStateCases(){
        switch (currentState) {
            case STATE_STANDING:
            {
                if (!wasHanging && isHanging)
                    switch (hangType) {
                        case Point.HangType.BracedHang:
                            Trigger(TRIGGER_BRACED_HANG);
                            break;
                        case Point.HangType.FreeHang:
                            Trigger(TRIGGER_FREE_HANG_HANG);
                            break;
                    }

                break;
            }
            case STATE_BRACED_HANG:
            {
                if (!wasShimmyRight && isShimmyRight)
                    switch (hangType) {
                        case Point.HangType.BracedHang:
                            Debug.Log("Trigger RIGHT!");
                            Trigger(TRIGGER_BRACED_SHIMMY_RIGHT);
                            break;
                        case Point.HangType.FreeHang:
//                            Trigger(TRIGGER_FREE_HANG_SHIMMY_RIGHT);
                            Trigger(TRIGGER_FREE_HANG_HANG);
                            break;
                    }

                if (!wasShimmyLeft && isShimmyLeft)
                    switch (hangType) {
                        case Point.HangType.BracedHang:
                            Trigger(TRIGGER_BRACED_SHIMMY_LEFT);
                            break;
                        case Point.HangType.FreeHang:
//                            Trigger(TRIGGER_FREE_HANG_SHIMMY_LEFT);
                            Trigger(TRIGGER_FREE_HANG_HANG);
                            break;
                    }

                break;
            }
            case STATE_FREE_HANG:
            {
                if (wasHanging && !isHanging) Trigger(TRIGGER_FALLING_IDLE);

                if (!wasShimmyRight && isShimmyRight)
                    switch (hangType) {
                        case Point.HangType.BracedHang:
//                            Trigger(TRIGGER_BRACED_SHIMMY_RIGHT);
                            Trigger(TRIGGER_BRACED_HANG);
                            break;
                        case Point.HangType.FreeHang:
                            Debug.Log("right");
                            Trigger(TRIGGER_FREE_HANG_SHIMMY_RIGHT);
                            break;
                    }

                if (!wasShimmyLeft && isShimmyLeft)
                    switch (hangType) {
                        case Point.HangType.BracedHang:
                            Trigger(TRIGGER_BRACED_HANG);
//                            Trigger(TRIGGER_BRACED_SHIMMY_LEFT);
                            break;
                        case Point.HangType.FreeHang:
                            Debug.Log("left");
                            Trigger(TRIGGER_FREE_HANG_SHIMMY_LEFT);
                            break;
                    }
            }
                break;
        }
    }

    /// <summary>
    ///     Keeps in memory this frame's state variables for the next frame
    /// </summary>
    private void SetLastFrameStates(){
//        wasCrouching = isCrouching;
        wasJumping = isJumping;
        wasHanging = isHanging;
        wasShimmyRight = isShimmyRight;
        wasShimmyLeft = isShimmyLeft;
    }

    /// <summary>
    ///     Gets called during IK pass
    /// </summary>
    /// <param name="layerIndex"></param>
    private void OnAnimatorIK(int layerIndex){
        if (isHanging) { }
    }

    #region Triggers
    /// <summary>
    ///     List of triggers used to transition
    /// </summary>
    private static readonly string[] TRIGGERS = {
        "BracedHangTrigger",
        "BracedHangUntrigger",
        "BracedShimmyRightTrigger",
        "BracedShimmyLeftTrigger",
        "FreeHangTrigger",
        "FreeHangUntrigger",
        "FreeHangShimmyRight",
        "FreeHangShimmyLeft",
        "FallTrigger"
    };

    // Braced Hangs
    private const int TRIGGER_BRACED_HANG = 0;
    private const int TRIGGER_BRACED_UNHANG = 1;
    private const int TRIGGER_BRACED_SHIMMY_RIGHT = 2;

    private const int TRIGGER_BRACED_SHIMMY_LEFT = 3;

    // Free Hangs
    private const int TRIGGER_FREE_HANG_HANG = 4;
    private const int TRIGGER_FREE_HANG_UNHANG = 5;
    private const int TRIGGER_FREE_HANG_SHIMMY_RIGHT = 6;

    private const int TRIGGER_FREE_HANG_SHIMMY_LEFT = 7;

    // Fall
    private const int TRIGGER_FALLING_IDLE = 8;
    #endregion

    #region Booleans
    /// <summary>
    ///     List of booleans used to tronsition
    /// </summary>
    private static readonly string[] BOOLS = {
        "isGrounded",
        "isCrouching",
        "isJumping",
        "isClimbing",
        "isHanging",
        "isBracedHanging",
        "isFreeHanging"
    };

    private const int BOOL_IS_GROUNDED = 0;
    private const int BOOL_IS_CROUCHING = 1;
    private const int BOOL_IS_JUMPING = 2;
    private const int BOOL_IS_CLIMBING = 3;
    private const int BOOL_IS_HANGING = 4;
    private const int BOOL_IS_BRACED_HANGING = 5;
    private const int BOOL_IS_FREE_HANGING = 6;
    #endregion

    #region Floats
    /// <summary>
    ///     List of floats used to transitiom
    /// </summary>
    private static readonly string[] FLOATS = {
        "distanceToGround",
        "xVelocity",
        "yVelocity",
        "zVelocity",
        "forwardSpeed",
    };

    private const int FLOAT_DISTANCE_TO_GROUND = 0;
    private const int FLOAT_X_VELOCITY = 1;
    private const int FLOAT_Y_VELOCITY = 2;
    private const int FLOAT_Z_VELOCITY = 3;
    private const int FLOAT_FORWARD_SPEED = 4;
    #endregion

    #region States
    /// <summary>
    ///     List of some states used from the code
    /// </summary>
    private static readonly string[] STATES = {
        "Standing Blend Tree",
        "Crouching Blend Tree",
        "Idle Braced Hang",
        "Idle Free Hang"
    };

    /// <summary>
    ///     Returns the index of the current animator state
    /// </summary>
    public int currentState{
        get
        {
            for (var stateIndex = 0; stateIndex < STATES.Length; stateIndex++) {
                var stateName = STATES[stateIndex];
                if (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName)) return stateIndex;
            }

            return -1;
        }
    }

    private const int STATE_STANDING = 0;
    private const int STATE_CROUCHING = 1;
    private const int STATE_BRACED_HANG = 2;
    private const int STATE_FREE_HANG = 3;
    #endregion

    #region Transforms
    /// <summary>
    ///     Reference to the transform that hints where the right hand should be
    /// </summary>
    [HideInInspector] public Transform rightHandIK;

    /// <summary>
    ///     Reference to the transform that hints where the left hand should be
    /// </summary>
    [HideInInspector] public Transform leftHandIK;
    #endregion

    #region Speeds
    /// <summary>
    ///     Speed of the player when jogging
    /// </summary>
    [HideInInspector] public float jogSpeed;

    /// <summary>
    ///     Speed of the player when walking
    /// </summary>
    [HideInInspector] public float walkSpeed;
    #endregion

    #region Player Current States
    /// <summary>
    /// Is the player hanging on a ledge?
    /// </summary>
    private bool _isHanging;

    /// </summary>
    ///     Is the player hanging on a ledge?
    /// <summary>
    public bool isHanging{
        get { return _isHanging; }
        set
        {
            _isHanging = value;
            SetBool(BOOL_IS_HANGING, _isHanging);
        }
    }

    /// <summary>
    ///     Is the player crouching?
    /// </summary>
    public bool isCrouching{
        get { return GetBool(BOOL_IS_CROUCHING); }
        set { SetBool(BOOL_IS_CROUCHING, value); }
    }

    /// <summary>
    ///     Is the player jumping?
    /// </summary>
    public bool isJumping{
        get { return GetBool(BOOL_IS_JUMPING); }
        set { SetBool(BOOL_IS_JUMPING, value); }
    }

    /// <summary>
    ///     Is the player touching/close enough to the ground?
    /// </summary>
    public bool isGrounded{
        get { return GetBool(BOOL_IS_GROUNDED); }
        set { SetBool(BOOL_IS_GROUNDED, value); }
    }

    /// <summary>
    ///     Is the player falling?
    /// </summary>
    public bool isFalling{
        get { return _isFalling; }
        set
        {
            if (!_isFalling && value)
                Trigger(TRIGGER_FALLING_IDLE);
            else if (!value)
                Trigger(TRIGGER_FALLING_IDLE, false);

            _isFalling = value;
        }
    }

    /// <summary>
    ///     Is the player climbing a ledge?
    /// </summary>
    public bool isClimbing{
        get { return GetBool(BOOL_IS_CLIMBING); }
        set
        {
            var _isClimbing = isClimbing;
            if (!_isClimbing && value) {
                animator.applyRootMotion = true;
                Debug.Log("Root motion TRUE");
            }
            else if (_isClimbing && !value) {
                animator.applyRootMotion = false;
                Debug.Log("Root motion FALSE");
            }

            SetBool(BOOL_IS_CLIMBING, value);
        }
    }

    /// <summary>
    ///     Is the player falling?
    /// </summary>
    private bool _isFalling;

    /// <summary>
    ///     Is the player shimmying right?
    /// </summary>
    private bool _isShimmyRight;

    /// <summary>
    ///     Is the player shimmying right?
    /// </summary>
    public bool isShimmyRight{
        get { return _isShimmyRight; }
        set
        {
            animator.applyRootMotion = value;
            _isShimmyRight = value;
        }
    }

    /// <summary>
    ///     Is the player shimmying left?
    /// </summary>
    private bool _isShimmyLeft;

    /// <summary>
    ///     Is the player shimmying left?
    /// </summary>
    public bool isShimmyLeft{
        get { return _isShimmyLeft; }
        set
        {
            animator.applyRootMotion = value;
            _isShimmyLeft = value;
        }
    }


    #endregion

    #region Player Previous States
    /// <summary>
    ///     Was the player hanging in the last frame?
    /// </summary>
    private bool wasHanging;

    /// <summary>
    ///     Was the player jumping in the last frame?
    /// </summary>
    private bool wasJumping;

    /// <summary>
    ///     Was the player shimmying right in the last frame?
    /// </summary>
    private bool wasShimmyRight;

    /// <summary>
    ///     Was the player shimmying left in the last frame?
    /// </summary>
    private bool wasShimmyLeft;

    /// <summary>
    /// Is the character free hanging?
    /// </summary>
    private bool _isFreeHanging;
    /// <summary>
    /// Is the character free hanging?
    /// </summary>
    public bool isFreeHanging{
        get { return _isFreeHanging; }
        set
        {
            _isFreeHanging = value;
            SetBool(BOOL_IS_FREE_HANGING, _isFreeHanging);
        }
    }
    /// <summary>
    /// Is the character braced hanging?
    /// </summary>
    public bool _isBracedHanging;
    /// <summary>
    /// Is the character braced hanging?
    /// </summary>
    public bool isBracedHanging{
        get { return _isFreeHanging; }
        set
        {
            _isFreeHanging = value;
            SetBool(BOOL_IS_BRACED_HANGING, _isFreeHanging);
        }
    }
    #endregion
}