using UnityEngine;
using System.Collections;

public class FighterController : GamePlayer
{
    public float wallJumpXSpeed = 3;

    //------------------------------------------------------------------------------
    // slide
    //------------------------------------------------------------------------------
    public float slideDuration = 0.5f;
    public float slideVelocity = 6;
    public float slideSquish = 0.6f;


    //------------------------------------------------------------------------------
    // wallSlide
    //------------------------------------------------------------------------------
    public float wallSlideSpeed = -2;
    //아무런 입력이 없이 슬라이딩이 지속될 시간
    public float wallSlideWatchdogDuration = 10f;

    //------------------------------------------------------------------------------
    // anim events
    //------------------------------------------------------------------------------
    // 펀치 공격 혹은 어퍼컷에서 XVelocity 이벤트가 발생할 경우 x축에 영향을 끼칠 속도.
    public float punchVelocity = 7;
    //어퍼컷공격 시 YVelocity 이벤트가 발생한 경우 y 축에 영향을 끼칠 속도.
    public float uppercutVelocity = 5;
    //downattack 에서 YVelocity 이벤트가 발생한 경우 y 축에 영향을 끼칠 속도.
    public float downAttackVelocity = 20;
    //공격 시 Pause 이벤트가 발생한 경우. 공격 콤보를 위한 다음 입력을 기다릴 시간.
    public float attackWatchdogDuration = 0.5f;


    //------------------------------------------------------------------------------
    // anim animation names
    //------------------------------------------------------------------------------
    [Header("Animations")]
    [SpineAnimation]
    public string walkAnim;
    [SpineAnimation]
    public string runAnim;
    [SpineAnimation]
    public string idleAnim;
    [SpineAnimation]
    public string balanceBackward;
    [SpineAnimation]
    public string balanceForward;
    [SpineAnimation]
    public string jumpAnim;
    [SpineAnimation]
    public string fallAnim;
    [SpineAnimation]
    public string wallSlideAnim;
    [SpineAnimation]
    public string slideAnim;
    [SpineAnimation]
    public string attackAnim;
    [SpineAnimation]
    public string downAttackAnim;
    //the frame to skip to when a Down Attack impacts a solid ground
    public float downAttackFrameSkip;
    [SpineAnimation]
    public string upAttackAnim;
    //this animation is used strictly to turn off all attacking bounding box attachment based hitboxes
    [SpineAnimation]
    public string clearAttackAnim;

    //------------------------------------------------------------------------------
    // sound
    //------------------------------------------------------------------------------
    [Header("Sounds")]
    public string footstepSound;
    public string landSound;
    public string jumpSound;
    public string slideSound;

    //------------------------------------------------------------------------------
    // effectPrefab
    //------------------------------------------------------------------------------
    public GameObject downAttackPrefab;

    //------------------------------------------------------------------------------
    // private
    //------------------------------------------------------------------------------

    //behavior trigger
    bool doSlide;

    //slide
    float slideStartTime;

    //콤보 검사
    bool waitingForAttackInput;
    float attackWatchdog;
    //업어택을 했냐 안했냐
    bool upAttackUsed;
    //다운어택
    bool downAttackRecovery = false;
    float downAttackRecoveryTime = 0;
    bool velocityLock;
    //다운어택중 극적인 효과를 위해 속도를 고정

    //wall slide
    float wallSlideWatchdog;
    float wallSlideStartTime;
    bool wallSlideFlip;
    bool wasWallJump;

    //slide시 적용, slide 동안 x 적용
    //jump loop, fall loop 점프 후 땅에 착지했을때 실적용
    float savedXVelocity;
    float airControlLockoutTime = 0;
    //벽점프 시 현재보다 조금 뒤로. x이동과 플립에 관여.

    override protected void Start()
    {
        base.Start();
    }

    override protected void HandleComplete(Spine.AnimationState state, int trackIndex, int loopCount)
    {
        var entry = state.GetCurrent(trackIndex);
        if (entry.Animation.Name == attackAnim || entry.Animation.Name == upAttackAnim)
        {
            //attack complete
            skeletonAnimation.AnimationName = idleAnim;
            SetState(ActionState.IDLE);
        }
    }

    override protected void HandleEvent(Spine.AnimationState state, int trackIndex, Spine.Event e)
    {
        var entry = state.GetCurrent(trackIndex);
        if (entry != null)
        {
            if (entry.Animation.Name == attackAnim || entry.Animation.Name == upAttackAnim)
            {
                switch (e.Data.Name)
                {
                    case "XVelocity":
                        Vector2 velocity = mRb.velocity;
                        velocity.x = mFlipped ? -punchVelocity * e.Float : punchVelocity * e.Float;
                        mRb.velocity = velocity;
                        break;
                    case "YVelocity":
                        velocity = mRb.velocity;
                        velocity.y = uppercutVelocity * e.Float;
                        if (movingPlatform)
                            velocity.y += movingPlatform.Velocity.y;
                        mRb.velocity = velocity;
                        break;
                    case "Pause":
                        attackWatchdog = attackWatchdogDuration;
                        waitingForAttackInput = true;
                        entry.TimeScale = 0;
                        break;
                }
            }
            else if (entry.Animation.Name == downAttackAnim)
            {
                switch (e.Data.Name)
                {
                    case "YVelocity":
                        Vector2 velocity = mRb.velocity;
                        velocity.y = downAttackVelocity * e.Float;
                        mRb.velocity = velocity;
                        break;
                    case "Pause":
                        velocityLock = e.Int == 1 ? true : false;
                        break;
                }
            }

            switch (e.Data.Name)
            {
                case "Footstep":
                    if (OnFootstep != null)
                        OnFootstep(transform);
                    break;
                case "Sound":
                    SoundPalette.PlaySound(e.String, 1f, 1, transform.position);
                    break;
                case "Effect":
                    switch (e.String)
                    {
                        case "GroundJump":
                            if (groundJumpPrefab && OnGround)
                                SpawnAtFoot(groundJumpPrefab, Quaternion.identity, new Vector3(mFlipped ? -1 : 1, 1, 1));
                            break;
                    }
                    break;
            }
        }
    }

    override protected void Update()
    {
        //		if( mIsActive == false ) return;
        //		if (_state.IsDead) return;
        //
        //		mFsm.Update();
        //		_state.Reset();
        //----------------------------------------------

        if (HandleInput != null)
            HandleInput(this);
        //
        //		ProcessInput();
        //
        //		UpdateAnim();
    }

    override protected void StateExit()
    {
        switch (state)
        {
            case ActionState.IDLE:
                if (CheckJump()) return;
                break;

            case ActionState.WALK:
                if (CheckJump()) return;
                break;

            case ActionState.RUN:
                if (CheckJump()) return;
                break;

            case ActionState.JUMP:
                break;

            case ActionState.FALL:
                if (CheckJump()) return;
                break;

            case ActionState.WALLSLIDE:
                break;

            case ActionState.SLIDE:
                //고스팅을 끄자
                //primaryCollider.transform.localScale = Vector3.one;
                break;

            case ActionState.ATTACK:
                break;

            case ActionState.DOWNATTACK:
                break;

            case ActionState.UPATTACK:
                break;
        }
    }

    Vector2 vel = new Vector2();
    float speedX;
    override protected void StateEnter()
    {
        //막 땅에 닿으면 점ㅍ 카운트를 0 해야한ㄷ.
        switch (state)
        {
            case ActionState.IDLE:
                if (CenterOnGround)
                {
                    skeletonAnimation.AnimationName = idleAnim;
                }
                else
                {
                    if (mIsOnSlope) skeletonAnimation.AnimationName = idleAnim;
                    else if (BackOnGround) skeletonAnimation.AnimationName = balanceForward;
                    else if (ForwardOnGround) skeletonAnimation.AnimationName = balanceBackward;
                }

                vel.x = movingPlatform ? mMovingPlatformVelocity.x : Mathf.MoveTowards(vel.x, 0, Time.deltaTime * 10);
                vel.y = mMovingPlatformVelocity.y;
                SetFriction(movingPlatform ? movingFriction : idleFriction);
                break;

            case ActionState.WALK:
                skeletonAnimation.AnimationName = walkAnim;

                speedX = walkSpeed * Mathf.Sign(input.axisX);
                vel.x = Mathf.MoveTowards(vel.x, speedX + mMovingPlatformVelocity.x, Time.deltaTime * 25);
                vel.y = mMovingPlatformVelocity.y;
                SetFriction(movingFriction);
                break;

            case ActionState.RUN:
                skeletonAnimation.AnimationName = runAnim;

                speedX = runSpeed * Mathf.Sign(input.axisX);
                vel.x = Mathf.MoveTowards(vel.x, speedX + mMovingPlatformVelocity.x, Time.deltaTime * 15);
                vel.y = mMovingPlatformVelocity.y;
                SetFriction(movingFriction);
                break;

            case ActionState.JUMP:
                skeletonAnimation.AnimationName = jumpAnim;
                mPlatformOneWay = null;
                mPlatformMoving = null;

                SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);

                vel.x = 0f;//RigidbodyVX;
                vel.y = mJumpCount > 0 ? airJumpSpeed : jumpSpeed;
                vel.y += mMovingPlatformVelocity.y;

                mJumpStartTime = Time.time;

                //jump 이펙트 생성
                if (airJumpPrefab != null && mJumpCount > 0)
                {
                    Instantiate(airJumpPrefab, transform.position, Quaternion.identity);
                }
                else if (groundJumpPrefab != null && mJumpCount == 0)
                {
                    if (wasWallJump) SpawnAtFoot(groundJumpPrefab, Quaternion.Euler(0, 0, vel.x >= 0 ? 90 : -90), new Vector3(vel.x >= 0 ? 1 : -1, -1, 1));
                    else SpawnAtFoot(groundJumpPrefab, Quaternion.identity, new Vector3(mFlipped ? -1 : 1, 1, 1));
                }

                mJumpCount++;
                if (OnJump != null) OnJump(transform);
                break;

            case ActionState.FALL:
                skeletonAnimation.AnimationName = fallAnim;
                break;

            case ActionState.WALLSLIDE:
                skeletonAnimation.AnimationName = wallSlideAnim;

                mJumpCount = 0;
                break;

            case ActionState.SLIDE:
                skeletonAnimation.AnimationName = slideAnim;

                SoundPalette.PlaySound(slideSound, 1, 1, transform.position);
                slideStartTime = Time.time;
                SetFriction(movingFriction);

                vel.x = mFlipped ? -slideVelocity : slideVelocity;
                savedXVelocity = vel.x;
                vel.x += mMovingPlatformVelocity.x;

                primaryCollider.transform.localScale = new Vector3(1, slideSquish, 1);
                IgnoreCharacterCollisions(true);

                if (skeletonGhost != null) skeletonGhost.ghostingEnabled = true;
                break;

            case ActionState.ATTACK:
                break;

            case ActionState.DOWNATTACK:
                break;

            case ActionState.UPATTACK:
                break;
        }
    }

    void Move()
    {

    }

    override protected void StateUpdate()
    {
        switch (state)
        {
            case ActionState.IDLE:
                if (CheckJump()) return;
                if (CheckSlide()) return;
                if (CheckGroundFall()) return;

                if (CheckWalk()) return;
                Move();
                break;

            case ActionState.WALK:
                if (CheckJump()) return;
                if (CheckSlide()) return;
                if (CheckGroundFall()) return;

                if (CheckIdle()) return;
                if (CheckRun()) return;
                Move();
                break;

            case ActionState.RUN:
                if (CheckJump()) return;
                if (CheckSlide()) return;
                if (CheckGroundFall()) return;

                if (CheckRunStop()) return;
                Move();
                break;

            case ActionState.JUMP:
                if (CheckJumpFall()) return;
                break;

            case ActionState.FALL:
                if (CheckJump()) return;
                if (CheckBounceCheck()) return;
                if (CheckFallToGround()) return;

                savedXVelocity = vel.x;
                break;

            case ActionState.WALLSLIDE:
                if (CheckWallJump()) return;
                if (CheckBounceCheck()) return;
                if (CheckWallSlideToGround()) return;

                if (input.axisY < -0.5f)
                {
                    SetFallState(true);
                    return;
                }

                if (wallSlideWatchdog <= 0)
                {
                    SetFallState(true);
                    return;
                }
                
                //벽을 지속적으로 누르면 슬라이딩 유지를 할 수 있도록 시간을 갱신한다.
                if (mIsPressAgainstWall )
                {
                    wallSlideWatchdog = wallSlideWatchdogDuration;
                    skeletonAnimation.Skeleton.FlipX = wallSlideFlip;
                }
                else wallSlideWatchdog -= Time.deltaTime;

                break;

            case ActionState.SLIDE:
                break;

            case ActionState.ATTACK:
                break;

            case ActionState.DOWNATTACK:
                break;

            case ActionState.UPATTACK:
                break;
        }

        //추가해야 할 스테이트.
        // CROUCH, CROUCH_MOVE
        // LADDER_CLIMB,LADDER_CLIMB_MOVE,
        // LOOKUP,
    }

    bool CheckWallSlideToGround()
    {
        //지상이고, x속도가 없고 월슬라이드 한지 최소 0.2f 는 넘었을때  idle 로 가자.
        if (OnGround == false) return false;
        if (vel.x != 0f) return false; // ? 이건 왜? 
        if (Time.time - wallSlideStartTime < 0.2f) return false;

        SoundPalette.PlaySound(landSound, 1, 1, transform.position);
        SetState(ActionState.IDLE);

        //
        if (input.axisX == 0) SetState(ActionState.SLIDE);
        else SetState(ActionState.WALK);

        return true;
    }
    //--------------------------------------------------------------------------------------------
    // state 변경 조건 검사
    //--------------------------------------------------------------------------------------------

    bool CheckSlide()
    {
        if (input.specailATrigger == false) return false;

        SetState(ActionState.SLIDE);
        return true;
    }

    bool CheckWallJump()
    {
        if (input.jumpTrigger == false) return false;

        float vx = 0f;
        if (mIsPressAgainstWall == false)
        {
            airControlLockoutTime = Time.time + 0.5f;
            vx = wallJumpXSpeed * (mFlipped ? -1 : 1) * 2;
        }
        else
        {
            vx = wallJumpXSpeed * (mFlipped ? -1 : 1);
        }

        wasWallJump = true;

        return true;
    }

    bool CheckJump()
    {
        if (input.jumpTrigger == false) return false;
        if (mJumpCount < maxJumps) return false;

        if (IsAblePassOneWay())
        {
            mPlatformOneWay = null;
            mPlatformMoving = null;
            DoPassThrough(mPlatformOneWay);
            SetState(ActionState.FALL);
        }
        else
        {
            SetState(ActionState.JUMP);
        }

        return true;
    }

    bool IsAblePassOneWay()
    {
        if (input.axisY > -0.1f) return false;
        //사다리를 타는 중이라면 가능.
        // 현재 밟은 바닥이 원웨이인지 확인해야한다. 모든 바닥이 원웨이.
        if (mPlatformOneWay == null) return false;
        //oneway를 이미 통과중인 상태.
        if (mPassThroughPlatform != null) return false;

        return true;
    }

    bool CheckWalk()
    {
        if (input.axisX == 0f) return false;
        SetState(ActionState.WALK);
        return true;
    }

    bool CheckIdle()
    {
        if (input.axisX != 0f) return false;
        SetState(ActionState.IDLE);
        return true;
    }

    bool CheckRun()
    {
        if (input.inputRun == false) return false;
        SetState(ActionState.RUN);
        return true;
    }

    bool CheckRunStop()
    {
        if (input.inputRun) return false;

        if (input.axisX != 0f) SetState(ActionState.WALK);
        else SetState(ActionState.IDLE);
        return true;
    }

    bool CheckGroundFall()
    {
        if (OnGround) return false;//movingplatform 검사해야 하나?

        SetFallState(true);
        return true;
    }

    bool CheckJumpFall()
    {
        //jump의 loop 가 섞여 있다.
        float jumpTime = Time.time - mJumpStartTime;
        savedXVelocity = vel.x;

        if (input.jumpPressed == false || jumpTime >= jumpDuration || downAttackRecovery)
        {
            mJumpStartTime -= jumpDuration;

            if (vel.y > 0) vel.y = Mathf.MoveTowards(vel.y, 0, Time.deltaTime * 30);

            if (vel.y <= 0 || (jumpTime < jumpDuration && OnGround))
            {
                if (downAttackRecovery == false)
                {
                    SetFallState(false);
                }
                else
                {
                    downAttackRecovery = false;
                }

            }
        }
        //
        SetFallState(false);
        return true;
    }

    bool CheckFallToGround()
    {
        if (OnGround == false) return false;

        SoundPalette.PlaySound(landSound, 1, 1, transform.position);

        if (input.axisX == 0) SetState(ActionState.SLIDE);
        else SetState(ActionState.WALK);

        return true;
    }

    bool CheckBounceCheck()
    {
        if (mStompableObject == null) return false;

        mStompableObject.SendMessage("Hit", 1, SendMessageOptions.DontRequireReceiver);

        vel.y = headBounceSpeed;
        //이후 JumpEnter 에서 위 헤더 점프 바운스가 무시되다. 수정하자.
        SetState(ActionState.JUMP);
        return true;
    }

    //----------------------------------------------------------------

    override protected void FixedUpdate()
    {
        DetectOnGround();
        DetectPlatform();
        DetectStompObject();
        DetectPressAgainstWall();

        HandlePhysics();
    }
    void HandlePhysics()
    {
        Vector2 velocity = mRb.velocity;
    }


    void DetectOnGround()
    {
        BackOnGround = GroundCast(mFlipped ? mCastOriginForwardGround : mCastOrginBackGround);
        ForwardOnGround = GroundCast(mFlipped ? mCastOrginBackGround : mCastOriginForwardGround);
        CenterOnGround = GroundCast(mCastOriginCenterGround);
    }

    void DetectPlatform()
    {
        mPlatformMoving = MovingPlatformCast(mCastOriginCenterGround);
        if (mPlatformMoving == null) mPlatformMoving = MovingPlatformCast(mCastOrginBackGround);
        if (mPlatformMoving == null) mPlatformMoving = MovingPlatformCast(mCastOriginForwardGround);

        mPlatformOneWay = PlatformCast(mCastOriginCenterGround);
        if (mPlatformOneWay == null) mPlatformOneWay = PlatformCast(mCastOrginBackGround);
        if (mPlatformOneWay == null) mPlatformOneWay = PlatformCast(mCastOriginForwardGround);

        if (mPlatformMoving == null) mMovingPlatformVelocity = Vector2.zero;
        else mMovingPlatformVelocity = mPlatformMoving.Velocity;

        //mIsOnSlope 처리해야한다.
    }

    void DetectStompObject()
    {
        mStompableObject = GetRelevantCharacterCast(mCastOriginCenterGround, 0.15f);
        if (mStompableObject == null) mStompableObject = GetRelevantCharacterCast(mCastOrginBackGround, 0.15f);
        if (mStompableObject == null) mStompableObject = GetRelevantCharacterCast(mCastOriginForwardGround, 0.15f);
    }

    void DetectPressAgainstWall()
    {
        float x = mRb.velocity.x;
        bool usingVelocity = true;

        if (Mathf.Abs(x) < 0.1f)
        {
            x = input.axisX;
            if (Mathf.Abs(x) <= deadZone)
            {
                mIsPressAgainstWall = false;
                return;
            }
            else
            {
                usingVelocity = false;
            }
        }

        RaycastHit2D hit = Physics2D.Raycast
        (
            transform.TransformPoint(mCastOriginWall),
            new Vector2(x, 0).normalized,
            mCastOriginWallDistance + (usingVelocity ? x * Time.deltaTime : 0),
            currentMask
        );

        if (hit.collider != null && !hit.collider.isTrigger)
        {
            if (hit.collider.GetComponent<OneWayPlatform>())
            {
                mIsPressAgainstWall = false;
                return;
            }

            mIsPressAgainstWall = true;
            return;
        }

        mIsPressAgainstWall = false;
    }
}
