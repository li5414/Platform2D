using UnityEngine;
using System.Collections;

public class Fighter : GamePlayer
{
    public float wallJumpXSpeed = 3;

    //------------------------------------------------------------------------------
    // slide
    //------------------------------------------------------------------------------
    [Header("Slide")]
    public float slideDuration = 0.5f;
    public float slideSpeed = 6;
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

    //slide
    float slideStartTime;

    //콤보 검사
    bool waitingForAttackInput;
    float attackWatchdog;
    //다운어택
    bool downAttackRecovery = false;
    float downAttackRecoveryTime = 0;
    bool velocityLock;
    //다운어택중 극적인 효과를 위해 속도를 고정

    //wall slide
    float wallSlideWatchdog;
    float wallSlideStartTime;
    bool wasWallJump;

    //jump loop, fall loop 점프 후 땅에 착지했을때 실적용
    float airControlLockoutTime = 0;
    //벽점프 시 현재보다 조금 뒤로. x이동과 플립에 관여.

    override protected void Start()
    {
        base.Start();

        SetState(ActionState.IDLE);
    }

    override protected void HandleComplete(Spine.AnimationState state, int trackIndex, int loopCount)
    {
        var entry = state.GetCurrent(trackIndex);
        if (entry.Animation.Name == attackAnim || entry.Animation.Name == upAttackAnim)
        {
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
                        //mVelocity.x = mFlipped ? -punchVelocity * e.Float : punchVelocity * e.Float;
                        //공격하면서 앞으로 전진하자 .
                        break;
                    case "YVelocity":
                        //업어택하면서 위로 올라갖.
                        //mVelocity.y = uppercutVelocity * e.Float;
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
                        //down attack 하면서 아래로ㅅ슈욱 가자
                        //mVelocity.y = downAttackVelocity * e.Float;
                        break;
                    case "Pause":
                        velocityLock = e.Int == 1 ? true : false;
                        break;
                }
            }

            switch (e.Data.Name)
            {
                case "Footstep":
                    if (OnFootstep != null) OnFootstep(transform);
                    break;
                case "Sound":
                    SoundPalette.PlaySound(e.String, 1f, 1, transform.position);
                    break;
                case "Effect":
                    switch (e.String)
                    {
                        case "GroundJump":
                            if (groundJumpPrefab && controller.OnGround)
                            {
                                SpawnAtFoot(groundJumpPrefab, Quaternion.identity, new Vector3(IsFlipped ? -1 : 1, 1, 1));
                            }
                            break;
                    }
                    break;
            }
        }
    }

    override protected void Update()
    {
        if (HandleInput != null) HandleInput(this);

        StateUpdate();
    }

    void UpdateFacing(bool useVX = false)
    {
        if (useVX)
        {
            if (controller.vx > 0.1f) controller.SetFacing(Facing.RIGHT);
            else if (controller.vx < -0.1f) controller.SetFacing(Facing.LEFT);
        }
        else
        {
            if (input.axisX > 0.1f) controller.SetFacing(Facing.RIGHT);
            else if (input.axisX < -0.1f) controller.SetFacing(Facing.LEFT);
        }
    }


    override protected void StateEnter()
    {
        switch (state)
        {
            //모든 땅에서의 움직임은 Idle에서 시작한다.
            case ActionState.IDLE:
                if (controller.CenterOnGround) PlayAnimation(idleAnim);
                else if (controller.IsOnSlope) PlayAnimation(idleAnim);
                else if (controller.BackOnGround) PlayAnimation(balanceForward);
                else if (controller.ForwardOnGround) PlayAnimation(balanceBackward);

                if (controller.standingPlatform == null) controller.SetFriction(movingFriction);
                else controller.SetFriction(idleFriction);

                controller.CurrentSpeed = 0f;

                mJumpCount = 0;
                break;

            case ActionState.WALK:
                PlayAnimation(walkAnim);
                controller.SetFriction(movingFriction);
                controller.CurrentSpeed = walkSpeed;
                break;

            case ActionState.RUN:
                PlayAnimation(runAnim);
                controller.SetFriction(movingFriction);
                controller.CurrentSpeed = runSpeed;
                break;

            case ActionState.JUMP:
                PlayAnimation(jumpAnim);

                controller.ClearPlatform();

                SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
                
                controller.vy = 0f;
                controller.vy = mJumpCount > 0 ? airJumpSpeed : jumpSpeed;
                // controller.AddForceVertical(mJumpCount > 0 ? airJumpSpeed : jumpSpeed);
                mJumpStartTime = Time.time;

                //jump 이펙트 생성
                if (airJumpPrefab != null && mJumpCount > 0)
                {
                    Instantiate(airJumpPrefab, transform.position, Quaternion.identity);
                }
                else if (groundJumpPrefab != null && mJumpCount == 0)
                {
                    if (wasWallJump) SpawnAtFoot(groundJumpPrefab, Quaternion.Euler(0, 0, controller.vx >= 0 ? 90 : -90), new Vector3(controller.vx >= 0 ? 1 : -1, -1, 1));
                    else SpawnAtFoot(groundJumpPrefab, Quaternion.identity, new Vector3(IsFlipped ? -1 : 1, 1, 1));
                }

                mJumpCount++;
                if (OnJump != null) OnJump(transform);
                break;

            case ActionState.FALL:
                PlayAnimation(fallAnim);
                break;

            case ActionState.WALLSLIDE:
                PlayAnimation(wallSlideAnim);
                mJumpCount = 0;
                wallSlideStartTime = Time.time;
                wallSlideWatchdog = wallSlideWatchdogDuration;

                if (Mathf.Abs(controller.vx) > 0.1)
                {
                    AnimFlip = controller.vx > 0;
                }
                else
                {
                    AnimFlip = input.axisX > 0;
                }
                break;

            case ActionState.SLIDE:
                PlayAnimation(slideAnim);

                controller.SetFriction(movingFriction);
                controller.UpdateColliderSize(1, slideSquish);

                SoundPalette.PlaySound(slideSound, 1, 1, transform.position);
                slideStartTime = Time.time;

                controller.CurrentSpeed = slideSpeed;

                IgnoreCharacterCollisions(true);

                if (mGhost != null) mGhost.ghostingEnabled = true;
                break;

            case ActionState.ATTACK:
                break;

            case ActionState.DOWNATTACK:
                PlayAnimation(downAttackAnim);
                break;

            case ActionState.UPATTACK:
                PlayAnimation(upAttackAnim);
                break;
        }
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

                break;

            case ActionState.WALK:
                if (CheckJump()) return;
                if (CheckSlide()) return;
                if (CheckGroundFall()) return;

                if (CheckIdle()) return;
                if (CheckRun()) return;

                UpdateFacing();
                break;

            case ActionState.RUN:
                if (CheckJump()) return;
                if (CheckSlide()) return;
                if (CheckGroundFall()) return;

                if (CheckRunStop()) return;

                UpdateFacing();
                break;

            case ActionState.JUMP:
                if (CheckJumpFall()) return;
                if (CheckAirAttack()) return;
                if (CheckWallSlide()) return;

                UpdateFacing();

                float jumpElapsedTime = Time.time - mJumpStartTime;
                if (input.jumpPressed == false || jumpElapsedTime >= jumpDuration || downAttackRecovery)
                {
                    mJumpStartTime -= jumpDuration;
                }
                break;

            case ActionState.FALL:
                if (CheckJump()) return;
                if (CheckBounceCheck()) return;
                if (CheckAirAttack()) return;
                if (CheckWallSlide()) return;
                if (CheckFallToGround()) return;

                UpdateFacing();
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
                if (controller.IsPressAgainstWall) wallSlideWatchdog = wallSlideWatchdogDuration;
                else wallSlideWatchdog -= Time.deltaTime;

                break;

            case ActionState.SLIDE:
                if (CheckSlideToIdle()) return;
                if (CheckGroundFall()) return;

                UpdateFacing(true);
                break;

            case ActionState.ATTACK:
                if (waitingForAttackInput)
                {
                    waitingForAttackInput = false;
                    NextAttack();
                    return;
                }

                if (waitingForAttackInput)
                {
                    controller.SetFriction(idleFriction);
                    attackWatchdog -= Time.deltaTime;
                    if (attackWatchdog < 0) SetState(ActionState.IDLE);
                    return;
                }

                controller.SetFriction(movingFriction);
                break;

            case ActionState.DOWNATTACK:
                //아래로 떨어지고 있는 중이다. 아직 땅에 닿지 않은 상태
                if (downAttackRecovery == false)
                {
                    //땅에 닿았다.
                    if (controller.OnGround)
                    {
                        SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
                        downAttackRecoveryTime = 2f;//적절한 연출을 위해 하드코딩으로 회복시간을 가진다.
                        downAttackRecovery = true;

                        //아래 두줄은 뭘 의미할까?
                        DownAttackStart(clearAttackAnim, downAttackFrameSkip / 30f);
                        if (downAttackPrefab) Instantiate(downAttackPrefab, transform.position + new Vector3(0, 0.25f, 0), Quaternion.identity);

                        controller.CurrentSpeed = 0f;
                    }
                    else
                    {
                        //다운어택으로 떨어지는 중.
                    }
                }
                //땅에 닿은 후 회복중.
                else
                {
                    //회복타임이 남았따.
                    if (downAttackRecoveryTime > 0)
                    {
                        downAttackRecoveryTime -= Time.deltaTime;
                    }
                    //회복이 끝나면 점프 상태로 변경한다.
                    else
                    {
                        //기존 소스는 점프 하면서 점프스피드를 적용하는데 현재 이상태로 되면 airjumpspped로 적용됨.
                        SetState(ActionState.JUMP);
                    }
                }

                if (velocityLock) controller.CurrentSpeed = 0f;
                break;

            case ActionState.UPATTACK:
                break;
        }
    }

    override protected void StateExit()
    {
        switch (state)
        {
            case ActionState.IDLE:
                break;

            case ActionState.WALK:
                break;

            case ActionState.RUN:
                break;

            case ActionState.JUMP:
                break;

            case ActionState.FALL:
                break;

            case ActionState.WALLSLIDE:
                break;

            case ActionState.SLIDE:
                controller.ResetColliderSize();
                IgnoreCharacterCollisions(false);
                if (mGhost != null) mGhost.ghostingEnabled = false;
                break;

            case ActionState.ATTACK:
                break;

            case ActionState.DOWNATTACK:
                break;

            case ActionState.UPATTACK:
                break;
        }
    }

    bool CheckSlideToIdle()
    {
        float slideTime = Time.time - slideStartTime;
        if (slideTime < slideDuration) return false;

        SetState(ActionState.IDLE);
        return true;
    }

    bool CheckWallSlideToGround()
    {
        //지상이고, x속도가 없고 월슬라이드 한지 최소 0.2f 는 넘었을때  idle 로 가자.
        if (controller.OnGround == false) return false;
        if (controller.vx != 0f) return false; // ? 이건 왜? 
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
        if (controller.IsPressAgainstWall == false)
        {
            airControlLockoutTime = Time.time + 0.5f;
            vx = wallJumpXSpeed * (IsFlipped ? -1 : 1) * 2;
        }
        else
        {
            vx = wallJumpXSpeed * (IsFlipped ? -1 : 1);
        }

        wasWallJump = true;

        return true;
    }

    bool CheckWallSlide()
    {
        if (Time.time == mJumpStartTime) return false;
        if (controller.IsPressAgainstWall == false) return false;
        //입력 외에 추가로 실제 rdbd의 x 를 검사해야 할 수 도 있다.

        SetState(ActionState.WALLSLIDE);
        return true;
    }

    bool CheckJump()
    {
        if (input.jumpTrigger == false) return false;

        print("Cehckjump: " + mJumpCount + " : " + maxJumps);

        if (mJumpCount >= maxJumps) return false;

        if (IsAblePassOneWay())
        {
            controller.PassOneway();
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
        if (controller.OnOneway == false) return false;

        return true;
    }

    bool CheckAirAttack()
    {
        if (input.attackTrigger == false) return false;
        if (input.axisY < -0.5f)
        {
            SetState(ActionState.DOWNATTACK);
        }
        else if (input.axisY > 0.5f)
        {
            SetState(ActionState.UPATTACK);

        }

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
        if (controller.OnGround) return false;//movingplatform 검사해야 하나?

        SetFallState(true);
        return true;
    }

    bool CheckJumpFall()
    {
        if (controller.vx > 0) return false;

        SetFallState(false);
        return true;
    }

    bool CheckFallToGround()
    {
        if (controller.OnGround == false) return false;

        SoundPalette.PlaySound(landSound, 1, 1, transform.position);

        if (input.axisX == 0) SetState(ActionState.IDLE);
        else SetState(ActionState.WALK);

        return true;
    }

    bool CheckBounceCheck()
    {
        if (controller.StompableObject == null) return false;

        controller.StompableObject.SendMessage("Hit", 1, SendMessageOptions.DontRequireReceiver);

        controller.AddForceVertical(headBounceSpeed);

        //todo 이후 JumpEnter 에서 위 헤더 점프 바운스가 무시되다. 수정하자.
        SetState(ActionState.JUMP);
        return true;
    }
}
