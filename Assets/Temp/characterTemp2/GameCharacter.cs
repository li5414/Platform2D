﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

using druggedcode.engine;

public enum ActionState
{
    NULL,
    IDLE,
    WALK,
    RUN,
    JUMP,
    FALL,
    WALLSLIDE,
    JETPACK,
    SLIDE,
    ATTACK,
    DOWNATTACK,
    UPATTACK,
    DANCE,
    DEAD
}
//추가해야 할 스테이트.
// CROUCH, CROUCH_MOVE
// LADDER_CLIMB,LADDER_CLIMB_MOVE,
// LOOKUP,

[RequireComponent(typeof(FailCharacterController))]
public class GameCharacter : MonoBehaviour
{
    static List<GameCharacter> All = new List<GameCharacter>();

    public ActionState state { get; protected set; }

    public AnimationType bodyType;


    [Header("Speeds & Timings")]
    public float walkSpeed = 1;
    public float runSpeed = 5;
    public float crouchSpeed = 2f;
    public float jumpSpeed = 12;
    public float airJumpSpeed = 10;
    public float headBounceSpeed = 16;//캐릭터를 밟았을 때 vy
    public float jumpDuration = 0.5f;

    [Range(1, 3)]
    public int maxJumps = 1;

    [Header("Friction")]
    public float idleFriction = 20;//gunman 4
    public float movingFriction = 0;

    [Header("References")]
    public Transform body;
    public GameObject groundJumpPrefab;
    public GameObject airJumpPrefab;

    [SpineBone(dataField: "skeletonAnimation")]
    public string footEffectBone;

    //--------------------------------------------------------------------------
    // Inspector에 표시되지 않는 public 
    //--------------------------------------------------------------------------
    //input injection
    public System.Action<GameCharacter> HandleInput;
    //events
    public System.Action<Transform> OnFootstep;
    public System.Action<Transform> OnJump;


    //--------------------------------------------------------------------------
    // protected, private
    //--------------------------------------------------------------------------

    SkeletonAnimation mSkeletonAnimation;
    protected SkeletonGhost mGhost;
    protected Facing mFacaing;

    //character state
    public int JumpCount { get; protected set; }


    protected float mJumpStartTime;

    public FailCharacterController controller { get; private set; }

    virtual protected void Awake()
    {
        controller = GetComponent<FailCharacterController>();
    }

    virtual protected void Start()
    {
        switch (bodyType)
        {
            case AnimationType.SPINE:
                mSkeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
                mSkeletonAnimation.state.Event += HandleEvent;
                mSkeletonAnimation.state.Complete += HandleComplete;
                break;
        }
    }


    virtual protected void OnEnable()
    {
        Register();
    }

    virtual protected void OnDisable()
    {
        Unregister();
    }

    void Register()
    {
        if (All.Contains(this) == false) All.Add(this);
    }

    void Unregister()
    {
        if (All.Contains(this)) All.Remove(this);
    }


    virtual protected void HandleEvent(Spine.AnimationState state, int trackIndex, Spine.Event e)
    {
        //handle spine anim event
    }

    virtual protected void HandleComplete(Spine.AnimationState state, int trackIndex, int loopCount)
    {
        //Handle Spine Animation Complete callbacks
    }

    protected void PlayAnimation(string animName)
    {
        switch (bodyType)
        {
            case AnimationType.SPINE:
                mSkeletonAnimation.AnimationName = animName;
                break;
        }
    }

    protected void NextAttack()
    {
        mSkeletonAnimation.state.GetCurrent(0).TimeScale = 1;
    }


    //todo 플립 적용
    public void SetFacing(Facing facing)
    {
        if (mFacaing == facing) return;

        mFacaing = facing;
        switch (mFacaing)
        {
            case Facing.RIGHT:
                body.localScale = new Vector3(1f, body.localScale.y, body.localScale.z);
                break;

            case Facing.LEFT:
                body.localScale = new Vector3(-1f, body.localScale.y, body.localScale.z);
                break;
        }
    }

    //실제 고개 방향과 별도로 캐릭터만 반대로 돌리고 싶을 때 사용한다.
    protected bool AnimFlip
    {
        set { mSkeletonAnimation.Skeleton.flipX = value; }
    }

    public bool IsFlipped { get { return mSkeletonAnimation.Skeleton.flipX; } }

    //todo 아래가 무슨 코드 인지 파악 후 추상화 시킬 것.
    protected void DownAttackStart(string animName, float time)
    {
        mSkeletonAnimation.skeleton.Data.FindAnimation(animName).Apply(mSkeletonAnimation.skeleton, 0, 1, false, null);
        mSkeletonAnimation.state.GetCurrent(0).Time = time;
    }


    public void SetState(ActionState next)
    {
        if (state == next)
        {
            Debug.Log("---------same state!!!!! " + state + " > " + next);
            //return;
        }
        else
        {
            StateExit();
            Debug.Log(state + " > " + next);
            state = next;
        }

        StateEnter();
    }

    virtual protected void StateExit()
    {

    }

    virtual protected void StateEnter()
    {

    }

    //--------------------------------------------------------------------------
    // behaviour
    //--------------------------------------------------------------------------

    //--------------------------------------------------------------------------
    // Graphics
    //--------------------------------------------------------------------------

    virtual protected void Update()
    {
        if (HandleInput != null) HandleInput(this);

        StateUpdate();
    }

    virtual protected void StateUpdate()
    {

    }

    //--------------------------------------------------------------------------
    // ?
    //--------------------------------------------------------------------------

    //ActionState.FALL은 직접적으로 호출 하지 말도록 하자. 점프 후 떨어지는 것과 지면에서 갑자기 떨어지는 것은 차이가 있따.
    //이 차이는 점프 수를 소비하냐 아니냐의 차이이다.
    //가령 이미 점프 중이였다면 fall 상태로 가면서 점프 수는 변동이 되지 않지만 갑자기 지면에서 떨어진 경우 점프를 소비 시켜야 한다.
    protected void SetFallState(bool useJump)
    {
        if (useJump) JumpCount++;
        SetState(ActionState.FALL);
    }
    //Special effects helper function
    protected void SpawnAtFoot(GameObject prefab, Quaternion rotation )
    {
        var bone = mSkeletonAnimation.Skeleton.FindBone(footEffectBone);
        Vector3 pos = mSkeletonAnimation.transform.TransformPoint(bone.WorldX, bone.WorldY, 0);
        Vector3 scale = new Vector3( mFacaing == Facing.RIGHT ? 1:-1,1,1);
        ((GameObject)Instantiate(prefab, pos, rotation)).transform.localScale = scale;
    }

    protected void IgnoreCharacterCollisions(bool ignore)
    {
        foreach (GameCharacter gc in All)
        {
            if (gc == this) continue;
            gc.IgnoreCollision(controller.primaryCollider, ignore);
        }
    }

    void IgnoreCollision(Collider2D tgCollider, bool ignore)
    {
        Physics2D.IgnoreCollision(controller.primaryCollider, tgCollider, ignore);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Handles.Label(transform.position + new Vector3(0, 1.2f, 0), state.ToString());
    }
#endif
}