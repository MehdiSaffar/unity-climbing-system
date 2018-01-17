using System;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering;

public class CharacterAnimationController : MonoBehaviour
{
    // Triggers
    private static readonly string[] TRIGGERS =
    {
        "JumpTrigger",
        "CrouchTrigger",
        "UncrouchTrigger",
        "BracedHangTrigger",
        "BracedHangUntrigger",
        "BracedShimmyRightTrigger",
        "BracedShimmyLeftTrigger",
        "FreeHangTrigger",
        "FreeHangUntrigger",
        "FreeHangShimmyRight",
        "FreeHangShimmyLeft"
    };

    private const int TRIGGER_JUMP = 0;
    private const int TRIGGER_CROUCH = 1;

    private const int TRIGGER_UNCROUCH = 2;

    // Braced Hangs
    private const int TRIGGER_BRACED_HANG = 3;
    private const int TRIGGER_BRACED_UNHANG = 4;
    private const int TRIGGER_BRACED_SHIMMY_RIGHT = 5;

    private const int TRIGGER_BRACED_SHIMMY_LEFT = 6;

    // Free Hangs
    private const int TRIGGER_FREE_HANG_HANG = 7;
    private const int TRIGGER_FREE_HANG_UNHANG = 8;
    private const int TRIGGER_FREE_HANG_SHIMMY_RIGHT = 9;
    private const int TRIGGER_FREE_HANG_SHIMMY_LEFT = 10;

    // States
    private static readonly string[] STATES =
    {
        "Standing Blend Tree",
        "Crouching Blend Tree",
        "Idle Braced Hang",
        "Idle Free Hang"
    };

    private const int STATE_STANDING = 0;
    private const int STATE_CROUCHING = 1;
    private const int STATE_BRACED_HANG = 2;
    private const int STATE_FREE_HANG = 3;

    public event Action OnBracedShimmyAnimationEnd;

    private Vector3 hipsFrontRayEnd;
    private Vector3 hipsFrontRayOrigin;

    public Transform hips;
    public Transform rightFoot;
    public Transform leftFoot;
    public Transform rightHand;
    public Transform leftHand;

    public Transform rightHandIK;
    public Transform leftHandIK;

    public bool isCrouching;
    public bool isHanging;
    public bool isJumping;
    private bool wasCrouching;
    private bool wasHanging;
    private bool wasJumping;

    [HideInInspector] public float currentSpeed;
    [HideInInspector] public float jogSpeed;
    [HideInInspector] public float walkSpeed;

    private Animator mAnimator;
    public bool isShimmyRight;
    private bool wasShimmyRight;
    private bool wasShimmyLeft;
    public bool isShimmyLeft;
    public Point.HangType hangType;

    public int CurrentAnimatorState
    {
        get
        {
            for (var stateIndex = 0; stateIndex < STATES.Length; stateIndex++)
            {
                var stateName = STATES[stateIndex];
                if (mAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
                {
                    return stateIndex;
                }
            }

            return -1;
        }
    }

    private float IdleWalkBlend
    {
        get { return mAnimator.GetFloat(nameof(IdleWalkBlend)); }
        set { mAnimator.SetFloat(nameof(IdleWalkBlend), value); }
    }

    private float IdleWalkCrouchBlend
    {
        get { return mAnimator.GetFloat(nameof(IdleWalkCrouchBlend)); }
        set { mAnimator.SetFloat(nameof(IdleWalkCrouchBlend), value); }
    }

    private float WalkJogBlend
    {
        get { return mAnimator.GetFloat(nameof(WalkJogBlend)); }

        set { mAnimator.SetFloat(nameof(WalkJogBlend), value); }
    }

    private float RightLeftWalkBlend
    {
        get { return mAnimator.GetFloat(nameof(RightLeftWalkBlend)); }
        set { mAnimator.SetFloat(nameof(RightLeftWalkBlend), value); }
    }

    private float ForwardBackwardWalkBlend
    {
        get { return mAnimator.GetFloat(nameof(ForwardBackwardWalkBlend)); }
        set { mAnimator.SetFloat(nameof(ForwardBackwardWalkBlend), value); }
    }


    private bool IsInState(int stateIndex)
    {
        return mAnimator.GetCurrentAnimatorStateInfo(0).IsName(STATES[stateIndex]);
    }

    private void Trigger(int triggerIndex, bool setOrReset = true)
    {
        if (setOrReset)
            mAnimator.SetTrigger(TRIGGERS[triggerIndex]);
        else
            mAnimator.ResetTrigger(TRIGGERS[triggerIndex]);
    }

    // Use this for initialization
    private void Start()
    {
        mAnimator = GetComponent<Animator>();
        var bracedHangShimmyRightBehaviour = mAnimator.GetBehaviour<BracedHangShimmyRightBehaviour>();
        var bracedHangShimmyLeftBehaviour = mAnimator.GetBehaviour<BracedHangShimmyLeftBehaviours>();
        bracedHangShimmyRightBehaviour.OnAnimationEnd += () =>
        {
            isShimmyRight = false;
            OnBracedShimmyAnimationEnd?.Invoke();
        };
        bracedHangShimmyLeftBehaviour.OnAnimationEnd += () =>
        {
            isShimmyLeft = false;
            OnBracedShimmyAnimationEnd?.Invoke();
        };
    }

    // Update is called once per frame
    private void Update()
    {
        if (currentSpeed >= jogSpeed)
            IdleWalkBlend = 2;
        else if (currentSpeed >= walkSpeed)
            IdleWalkBlend = 1;
        else
            IdleWalkBlend = 0;

        switch (CurrentAnimatorState)
        {
            case STATE_STANDING:
            {
                if (!wasJumping && isJumping)
                    Trigger(TRIGGER_JUMP);
                else if (!wasCrouching && isCrouching)
                    Trigger(TRIGGER_CROUCH);
                else if (!wasHanging && isHanging)
                {
                    switch (hangType)
                    {
                        case Point.HangType.BracedHang:
                            Trigger(TRIGGER_BRACED_HANG);
                            break;
                        case Point.HangType.FreeHang:
                            Trigger(TRIGGER_FREE_HANG_HANG);
                            break;
                    }
                }

                break;
            }
            case STATE_CROUCHING:
            {
                if (wasCrouching && !isCrouching)
                {
                    Trigger(TRIGGER_UNCROUCH);
                }

                break;
            }
            case STATE_BRACED_HANG:
            {
                if (wasHanging && !isHanging)
                {
                    Trigger(TRIGGER_BRACED_UNHANG);
                }

                if (!wasShimmyRight && isShimmyRight)
                {
                    switch (hangType)
                    {
                        case Point.HangType.BracedHang:
//                            Trigger(TRIGGER_BRACED_SHIMMY_RIGHT);
                            break;
                        case Point.HangType.FreeHang:
//                            Trigger(TRIGGER_FREE_HANG_SHIMMY_RIGHT);
                            Trigger(TRIGGER_FREE_HANG_HANG);
                            break;
                    }
                }

                if (!wasShimmyLeft && isShimmyLeft)
                {
                    switch (hangType)
                    {
                        case Point.HangType.BracedHang:
//                            Trigger(TRIGGER_BRACED_SHIMMY_LEFT);
                            break;
                        case Point.HangType.FreeHang:
//                            Trigger(TRIGGER_FREE_HANG_SHIMMY_LEFT);
                            Trigger(TRIGGER_FREE_HANG_HANG);
                            break;
                    }
                }

                break;
            }
            case STATE_FREE_HANG:
            {
                if (wasHanging && !isHanging)
                {
                    Trigger(TRIGGER_FREE_HANG_UNHANG);
                }

                if (!wasShimmyRight && isShimmyRight)
                {
                    switch (hangType)
                    {
                        case Point.HangType.BracedHang:
//                            Trigger(TRIGGER_BRACED_SHIMMY_RIGHT);
                            Trigger(TRIGGER_BRACED_HANG);
                            break;
                        case Point.HangType.FreeHang:
//                            Trigger(TRIGGER_FREE_HANG_SHIMMY_RIGHT);
                            break;
                    }
                }

                if (!wasShimmyLeft && isShimmyLeft)
                {
                    switch (hangType)
                    {
                        case Point.HangType.BracedHang:
                            Trigger(TRIGGER_BRACED_HANG);
//                            Trigger(TRIGGER_BRACED_SHIMMY_LEFT);
                            break;
                        case Point.HangType.FreeHang:
//                            Trigger(TRIGGER_FREE_HANG_SHIMMY_LEFT);
                            break;
                    }
                }
            }
                break;
        }

        SetLastFrameStates();
    }

    private void SetLastFrameStates()
    {
        wasCrouching = isCrouching;
        wasJumping = isJumping;
        wasHanging = isHanging;
        wasShimmyRight = isShimmyRight;
        wasShimmyLeft = isShimmyLeft;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (isHanging)
        {
            mAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            mAnimator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIK.position);
            mAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            mAnimator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIK.position);
        }
    }
}