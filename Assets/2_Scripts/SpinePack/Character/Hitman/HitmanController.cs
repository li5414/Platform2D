using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class HitmanController : TempGameCharacter
{
    /*
    ActionState[] ableList =
    {
        ActionState.IDLE,
        ActionState.WALK,
        ActionState.RUN,
        ActionState.JUMP,
        ActionState.FALL,
        ActionState.WALLSLIDE,
        ActionState.SLIDE,
        ActionState.ATTACK,
        ActionState.DOWNATTACK,
        ActionState.UPATTACK
    };
    */
    
    public Vector2 currentVelocity;

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
	bool velocityLock;//다운어택중 극적인 효과를 위해 속도를 고정

	//wall slide
    float wallSlideWatchdog;
    float wallSlideStartTime;
    bool wallSlideFlip;
    bool wasWallJump;

	//slide시 적용, slide 동안 x 적용 
	//jump loop, fall loop 점프 후 땅에 착지했을때 실적용
    float savedXVelocity;
	float airControlLockoutTime = 0;//벽점프 시 현재보다 조금 뒤로. x이동과 플립에 관여.

    override protected void Start()
    {
        base.Start();
        //Time.timeScale = 2;
    }

    override protected void HandleComplete(Spine.AnimationState state, int trackIndex, int loopCount)
    {
        var entry = state.GetCurrent(trackIndex);
        if (entry.Animation.Name == attackAnim || entry.Animation.Name == upAttackAnim)
        {
            //attack complete
            skeletonAnimation.AnimationName = idleAnim;
            SetState( ActionState.IDLE );
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
                        if (movingPlatform) velocity.y += movingPlatform.velocity.y;
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
                    if (OnFootstep != null) OnFootstep(transform);
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
        if (HandleInput != null) HandleInput(this);

        ProcessInput();

        UpdateAnim();
        
        currentVelocity = mRb.velocity;
    }

    void ProcessInput()
    {
        //지상이고, IDLE WALK RUN 이거나.
        //낙하중이고 아직 점프 횟수가남아있다면
        if (((OnGround || movingPlatform) && state < ActionState.JUMP) ||
            (state == ActionState.FALL && mJumpCount < maxJumps))
        {
            //아래로 점프하거나 위로 점프한다.
            if ( mJumpPressed == false )
            {
                if ( inputJumpWasPressed && mPassThroughPlatform == null && inputedAxis.y < -0.25f )
                {
                    var platform = PlatformCast( mCastOriginCenterGround );
                    if (platform != null)
                    {
                        DoPassThrough(platform);
                    }
                    else
                    {
                        mDoJump = true;
                        mJumpPressed = true;
                    }
                }
                else
                {
                    mDoJump = inputJumpWasPressed;
                    if (mDoJump)
                    {
                        mJumpPressed = true;
                    }
                }
            }
            
            //IDLE WALK RUN 이고 슬라이드가 아니며 슬라이드 눌렀다면 슬라이드
            if ( mDoJump == false && doSlide == false )
            {
                if (state < ActionState.JUMP)
                {
                    doSlide = inputSlidePressed;
                }
            }
        }
        //지상이고 슬라이딩 중이라면 고스팅을 끄고 점프입력
        else if (OnGround && state == ActionState.SLIDE)
        {
            mDoJump = inputJumpWasPressed;
            if (mDoJump)
            {
                if (skeletonGhost != null) skeletonGhost.ghostingEnabled = false;
                mJumpPressed = true;
            }
        }
        //벽타는 중인 경우 점프 트리거
        else if (state == ActionState.WALLSLIDE)
        {
            mDoJump = inputJumpWasPressed;
            if (mDoJump)
            {
                mJumpPressed = true;
            }
        }

        this.mAxis = inputedAxis;
        attackWasPressed = inputAttackPressed;
        mJumpPressed = inputJumpIsPressed;
    }
    
    void UpdateAnim()
    {
        // skeletonAnimation.state.SetAnimation( 0, runAnim, true );
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
                break;
                
            case ActionState.WALK:
                skeletonAnimation.AnimationName = walkAnim;
                break;
            case ActionState.RUN:
                skeletonAnimation.AnimationName = runAnim;
                break;
            case ActionState.JUMP:
                skeletonAnimation.AnimationName = jumpAnim;
                break;
            case ActionState.FALL:
                skeletonAnimation.AnimationName = fallAnim;
                break;
            case ActionState.WALLSLIDE:
                skeletonAnimation.AnimationName = wallSlideAnim;
                break;
            case ActionState.SLIDE:
                skeletonAnimation.AnimationName = slideAnim;
                break;
        }
    }

    //cache x velocity to ensure speed restores after heavy impact that results in physics penalties
    override protected void HandlePhysics()
    {
        mIsOnSlope = false;

		float axisX = mAxis.x;
		float axisY = mAxis.y;
        float absX = Mathf.Abs(axisX);
        
        float xVelocity = 0;
        float platformXVelocity = 0;
        float platformYVelocity = 0;
        
        Vector2 velocity = mRb.velocity;
        
        //무빙플랫폼 처리
        movingPlatform = GetMovingPlatform();
        if (movingPlatform)
        {
            platformXVelocity = movingPlatform.velocity.x;
            platformYVelocity = movingPlatform.velocity.y;
        }
        
        //------------------------------------------------------------------------------
        // do jump
        //------------------------------------------------------------------------------
        if (mDoJump && state != ActionState.JUMP)
        {
            SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
            
            //벽점프라면 x속도를 설정한다.
            if (state == ActionState.WALLSLIDE)
            {
                if ( IsPressingAgainstWall == false )
                {
                    airControlLockoutTime = Time.time + 0.5f;
                    velocity.x = wallJumpXSpeed * (mFlipped ? -1 : 1) * 2;
                }
                else
                {
                    velocity.x = wallJumpXSpeed * (mFlipped ? -1 : 1);
                }
                wasWallJump = true;
            }
            else
            {
                wasWallJump = false;
                if (state == ActionState.SLIDE)
                {
                    primaryCollider.transform.localScale = Vector3.one;
                }
            }
            
            //jump!
            velocity.y = mJumpCount > 0 ? airJumpSpeed : jumpSpeed;
            velocity.y += platformYVelocity;
            mJumpStartTime = Time.time;
            mDoJump = false;
            
            //jump 이펙트 생성
            if (airJumpPrefab != null && mJumpCount > 0)
            {
                Instantiate(airJumpPrefab, transform.position, Quaternion.identity);
            }
            else if (groundJumpPrefab != null && mJumpCount == 0)
            {
                if (wasWallJump)
                {
                    SpawnAtFoot(groundJumpPrefab, Quaternion.Euler(0, 0, velocity.x >= 0 ? 90 : -90), new Vector3(velocity.x >= 0 ? 1 : -1, -1, 1));
                }
                else
                {
                    SpawnAtFoot(groundJumpPrefab, Quaternion.identity, new Vector3(mFlipped ? -1 : 1, 1, 1));
                }

            }
            mJumpCount++;

            SetState( ActionState.JUMP );
            
            if (OnJump != null) OnJump(transform);

        }
        //------------------------------------------------------------------------------
        // do sliding
        //------------------------------------------------------------------------------
        else if (doSlide)
        {
            SoundPalette.PlaySound(slideSound, 1, 1, transform.position);
            slideStartTime = Time.time;
            SetState( ActionState.SLIDE );
            SetFriction(movingFriction);
            doSlide = false;
            velocity.x = mFlipped ? -slideVelocity : slideVelocity;
            savedXVelocity = velocity.x;
            velocity.x += platformXVelocity;
            primaryCollider.transform.localScale = new Vector3(1, slideSquish, 1);
            IgnoreCharacterCollisions(true);

            if (skeletonGhost != null) skeletonGhost.ghostingEnabled = true;
        }
        
        //------------------------------------------------------------------------------
        // x move
        //------------------------------------------------------------------------------
        //IDLE WALK RUN. 즉 점프나 슬라이딩을 하지 않았다면.
        if (state < ActionState.JUMP)
        {
            if (OnGround || movingPlatform)
            {
                mJumpCount = 0;
                upAttackUsed = false;
                
                // > run
                if (absX > runThreshold)
                {
                    xVelocity = runSpeed * Mathf.Sign(axisX);
                    velocity.x = Mathf.MoveTowards(velocity.x, xVelocity + platformXVelocity, Time.deltaTime * 15);
                    if (movingPlatform) velocity.y = platformYVelocity;
                    SetState( ActionState.RUN );
                    SetFriction(movingFriction);
                }
                // > walk
                else if (absX > deadZone)
                {
                    xVelocity = walkSpeed * Mathf.Sign(axisX);
                    velocity.x = Mathf.MoveTowards(velocity.x, xVelocity + platformXVelocity, Time.deltaTime * 25);
                    if (movingPlatform) velocity.y = platformYVelocity;
                    SetState( ActionState.WALK );
                    SetFriction(movingFriction);
                }
                // > idle
                else
                {
                    velocity.x = movingPlatform ? platformXVelocity : Mathf.MoveTowards(velocity.x, 0, Time.deltaTime * 10);
                    if (movingPlatform) velocity.y = platformYVelocity;
                    SetState( ActionState.IDLE );
                    SetFriction(movingPlatform ? movingFriction : idleFriction);
                }

                if (attackWasPressed)
                {
                    if (mAxis.y < 0.5f)
                    {
                        SetState( ActionState.ATTACK );
                        skeletonAnimation.AnimationName = attackAnim;
                    }
                    else
                    {
                        SetState( ActionState.UPATTACK );
                        skeletonAnimation.AnimationName = upAttackAnim;
                        upAttackUsed = true;
                    }
                }
            }
            //IDLE WALK RUN 이였는데 땅에 닿는게 없는경우.
            else
            {
                SetFallState(true);
            }
        }
        //JUMP loop
        else if (state == ActionState.JUMP)
        {
            float jumpTime = Time.time - mJumpStartTime;
            savedXVelocity = velocity.x;
            
            if ( mJumpPressed == false || jumpTime >= jumpDuration || downAttackRecovery )
            {
                mJumpStartTime -= jumpDuration;

                if (velocity.y > 0) velocity.y = Mathf.MoveTowards(velocity.y, 0, Time.deltaTime * 30);

                if (velocity.y <= 0 || (jumpTime < jumpDuration && OnGround))
                {
                    if ( downAttackRecovery == false )
                    {
                        SetFallState(false);
                    }
                    else
                    {
                        downAttackRecovery = false;
                    }

                }
            }
        }
        //fall loop
        else if (state == ActionState.FALL)
        {
            if (OnGround)
            {
                SoundPalette.PlaySound(landSound, 1, 1, transform.position);
                if (absX > runThreshold)
                {
                    velocity.x = savedXVelocity;
                    SetState( ActionState.RUN );
                }
                else if (absX > deadZone)
                {
                    velocity.x = savedXVelocity;
                    SetState( ActionState.WALK );
                }
                else
                {
                    velocity.x = savedXVelocity;
                    SetState( ActionState.IDLE );
                }
            }
            else
            {
                EnemyBounceCheck(ref velocity);
                savedXVelocity = velocity.x;
            }
        }
        //wall slide loop
        else if (state == ActionState.WALLSLIDE)
        {
            mJumpCount = 0;
            if (OnGround && Mathf.Abs(velocity.x) < 0.1f && Time.time > (wallSlideStartTime + 0.2f))
            {
                SoundPalette.PlaySound(landSound, 1, 1, transform.position);
                SetState( ActionState.IDLE );
            }
			else if ( IsPressingAgainstWall == false )
            {
				if ( EnemyBounceCheck(ref velocity) == false )
                {
                    if (axisY > -0.5f)
                    {
                        wallSlideWatchdog -= Time.deltaTime;
                        if (wallSlideWatchdog <= 0)
                        {
                            SetFallState(true);
                        }
                    }
                    else
                    {
                        SetFallState(true);
                    }
                }
            }
            else
            {
                EnemyBounceCheck(ref velocity);
                wallSlideWatchdog = wallSlideWatchdogDuration;
                skeletonAnimation.Skeleton.FlipX = wallSlideFlip;
            }
        }
        
        //------------------------------------------------------------------------------
        // x move
        //------------------------------------------------------------------------------
        //air control
        if (state == ActionState.JUMP || state == ActionState.FALL)
        {
            if (Time.time > airControlLockoutTime)
            {
                if (absX > runThreshold)
                {
                    velocity.x = Mathf.MoveTowards(velocity.x, runSpeed * Mathf.Sign(axisX), Time.deltaTime * 8);
                }
                else if (absX > deadZone)
                {
                    velocity.x = Mathf.MoveTowards(velocity.x, walkSpeed * Mathf.Sign(axisX), Time.deltaTime * 8);
                }
                else
                {
                    velocity.x = Mathf.MoveTowards(velocity.x, 0, Time.deltaTime * 8);
                }
            }
            else
            {
                if (wasWallJump)
                {
                    //벽점프를 한상태에서 움직이면 에어컨트롤lock을 캔슬 한다.
                    if (absX > deadZone) airControlLockoutTime = Time.time - 1;
                }
            }
            
            //아래를 누르고 공격을 눌렀다면 다운 어택으로 간다.
            if (attackWasPressed && mAxis.y < -0.5f)
            {
                velocity.x = 0;
                velocity.y = 0;
                SetState( ActionState.DOWNATTACK );
                upAttackUsed = true;
                skeletonAnimation.AnimationName = downAttackAnim;
            }
            //위를 누르고 공격을 눌렀다면 공중에서 업어택을 한다.
            else if (attackWasPressed && mAxis.y > 0.5f)
            {
				if ( upAttackUsed== false )
                {
                    SetState( ActionState.UPATTACK );
                    skeletonAnimation.AnimationName = upAttackAnim;
                    upAttackUsed = true;
                    velocity.y = 1;
                }
            }
            
            //공중 어택을 하지 않아 여전히 jump 이거나 fall 인 경우.
            if (state == ActionState.JUMP || state == ActionState.FALL)
            {
                if (Time.time != mJumpStartTime && IsPressingAgainstWall)
                {
                    //Mathf.Abs(mRb.velocity.x) > 0.1f 를 absX 검사 외에 따로 검사하는 이유는? 
                    if (Mathf.Abs(mRb.velocity.x) > 0.1f || (state == ActionState.FALL && absX > deadZone))
                    {
                        if ( wasWallJump ==false && state == ActionState.JUMP)
                        {
                            //dont do anything if still going up
                        }
                        else
                        {
                            //벽타자
                            SetState( ActionState.WALLSLIDE );
                            mJumpCount = 0;
                            wallSlideWatchdog = wallSlideWatchdogDuration;
                            wallSlideStartTime = Time.time;
                            upAttackUsed = false;
                            if (Mathf.Abs(mRb.velocity.x) > 0.1)
                            {
                                wallSlideFlip = mRb.velocity.x > 0;
                            }
                            else
                            {
                                wallSlideFlip = axisX > 0;
                            }
                        }
                    }
                }
            }
        }

        //떨어지자
        if (state == ActionState.FALL)
		{
			velocity.y += fallGravity * Time.deltaTime;
		}
        else if (state == ActionState.WALLSLIDE)
        {
            velocity.y = Mathf.Clamp(velocity.y, wallSlideSpeed, 0);
        }

        //슬라이딩 loop
        if (state == ActionState.SLIDE)
        {
            float slideTime = Time.time - slideStartTime;

			//슬라이딩 멈추고 idle 로 가자.
            if (slideTime > slideDuration)
            {
                primaryCollider.transform.localScale = Vector3.one;
                IgnoreCharacterCollisions(false);
                if (skeletonGhost != null) skeletonGhost.ghostingEnabled = false;
                SetState( ActionState.IDLE );
            }
            else
            {
                axisX = Mathf.Sign(savedXVelocity);
                velocity.x = savedXVelocity + platformXVelocity;
                if (movingPlatform) velocity.y = platformYVelocity;
            }

			if ( OnGround == false )
            {
                //Fell off edge while sliding
                primaryCollider.transform.localScale = Vector3.one;
                IgnoreCharacterCollisions(false);
                if (skeletonGhost != null) skeletonGhost.ghostingEnabled = false;
                SetFallState(true);
            }
        }

        //공격
        if (state == ActionState.ATTACK)
        {
            if (attackWasPressed)
            {
                attackWasPressed = false;

				//다음 공격 입력 대기 상태이면 다음 공격을 플레이 한다.
                if (waitingForAttackInput)
                {
                    waitingForAttackInput = false;
                    skeletonAnimation.state.GetCurrent(0).TimeScale = 1;
                }
            }

            //무빙플랫폼 적용
            velocity.x = Mathf.MoveTowards(velocity.x, platformXVelocity, Time.deltaTime * 8);
            if (movingPlatform) velocity.y = Mathf.MoveTowards(velocity.y, platformYVelocity, Time.deltaTime * 15);

			//콤보 대기 상태이면 지속적으로 입력 대기 시간을 감소시킨다.
            if (waitingForAttackInput)
            {
                SetFriction(idleFriction);
                attackWatchdog -= Time.deltaTime;
                //cancel combo
                if (attackWatchdog < 0) SetState( ActionState.IDLE );
            }
            else
            {
                SetFriction(movingFriction);
            }
        }

		//ActionState.IDLE,WALK,RUN,JUMP,FALL,WALLSLIDE,SLIDE 에서 WALLSLIDE 제외.
        if (state < ActionState.ATTACK && state != ActionState.WALLSLIDE)
        {
            if (Time.time > airControlLockoutTime)
            {
                if (axisX > deadZone) skeletonAnimation.Skeleton.FlipX = false;
                else if (axisX < -deadZone) skeletonAnimation.Skeleton.FlipX = true;
            }
            else
            {
				//airControllLockout 상태라면 입력받은 방향에 상관없이 현재 속도에 따라 플립 결정한다.
                if (velocity.x > deadZone) skeletonAnimation.Skeleton.FlipX = false;
                else if (velocity.x < deadZone) skeletonAnimation.Skeleton.FlipX = true;
            }
        }

        //down attack loop.
        if (state == ActionState.DOWNATTACK)
        {
			//아래로 떨어지고 있는 중이다. 아직 땅에 닿지 않은 상태
			if (downAttackRecovery == false )
            {
				//땅에 닿았다.
				if (OnGround)
				{
					SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
					downAttackRecoveryTime = 2f;//적절한 연출을 위해 하드코딩으로 회복시간을 가진다.
					downAttackRecovery = true;

					//아래 두줄은 뭘 의미할까?
					skeletonAnimation.skeleton.Data.FindAnimation(clearAttackAnim).Apply(skeletonAnimation.skeleton, 0, 1, false, null);
					skeletonAnimation.state.GetCurrent(0).Time = (downAttackFrameSkip / 30f);

					if (downAttackPrefab) Instantiate(downAttackPrefab, transform.position + new Vector3(0, 0.25f, 0), Quaternion.identity);
					if (movingPlatform) velocity = movingPlatform.velocity;
				}
				else
				{
					//다운어택으로 떨어지는 중.
				}
            }
			//땅에 닿은 후 회복중.
            else
            {
				if (downAttackRecoveryTime > 0)
				{
					downAttackRecoveryTime -= Time.deltaTime;
					velocity = Vector2.zero;
					if (movingPlatform) velocity = movingPlatform.velocity;
				}
				//회복이 끝나면 점프 상태로 변경한다.
				else
				{
					SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
					velocity.y = jumpSpeed + (platformYVelocity >= 0 ? platformYVelocity : 0);
					mJumpStartTime = Time.time;
					SetState( ActionState.JUMP );
					mDoJump = false;
					mJumpPressed = false;
				}
            }

            if (velocityLock) velocity = Vector2.zero;
        }

		//플립처리. velociy 적용.
        mFlipped = skeletonAnimation.Skeleton.FlipX;
        mRb.velocity = velocity;
    }
    
    //Bounce off a player in an angry way
    bool EnemyBounceCheck(ref Vector2 velocity)
    {
		Rigidbody2D character = OnTopOfCharacter();

		if( character == null ) return false;

		SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
		character.SendMessage("Hit", 1, SendMessageOptions.DontRequireReceiver);
		velocity.y = headBounceSpeed;
		mJumpStartTime = Time.time;
		SetState( ActionState.JUMP );
		mDoJump = false;
		return true;
    }
}


///////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////

//     override protected void HandleComplete(Spine.AnimationState state, int trackIndex, int loopCount)
//     {
//         var entry = state.GetCurrent(trackIndex);
//         if (entry.Animation.Name == attackAnim || entry.Animation.Name == upAttackAnim)
//         {
//             //attack complete
//             skeletonAnimation.AnimationName = idleAnim;
//             SetState( ActionState.IDLE );
//         }
//     }

//     override protected void HandleEvent(Spine.AnimationState state, int trackIndex, Spine.Event e)
//     {
//         var entry = state.GetCurrent(trackIndex);
//         if (entry != null)
//         {
//             if (entry.Animation.Name == attackAnim || entry.Animation.Name == upAttackAnim)
//             {
//                 switch (e.Data.Name)
//                 {
//                     case "XVelocity":
//                         Vector2 velocity = mRb.velocity;
//                         velocity.x = mFlipped ? -punchVelocity * e.Float : punchVelocity * e.Float;
//                         mRb.velocity = velocity;
//                         break;
//                     case "YVelocity":
//                         velocity = mRb.velocity;
//                         velocity.y = uppercutVelocity * e.Float;
//                         if (movingPlatform) velocity.y += movingPlatform.Velocity.y;
//                         mRb.velocity = velocity;
//                         break;
//                     case "Pause":
//                         attackWatchdog = attackWatchdogDuration;
//                         waitingForAttackInput = true;
//                         entry.TimeScale = 0;
//                         break;
//                 }
//             }
//             else if (entry.Animation.Name == downAttackAnim)
//             {
//                 switch (e.Data.Name)
//                 {
//                     case "YVelocity":
//                         Vector2 velocity = mRb.velocity;
//                         velocity.y = downAttackVelocity * e.Float;
//                         mRb.velocity = velocity;
//                         break;
//                     case "Pause":
//                         velocityLock = e.Int == 1 ? true : false;
//                         break;
//                 }
//             }

//             switch (e.Data.Name)
//             {
//                 case "Footstep":
//                     if (OnFootstep != null)
//                         OnFootstep(transform);
//                     break;
//                 case "Sound":
//                     SoundPalette.PlaySound(e.String, 1f, 1, transform.position);
//                     break;
//                 case "Effect":
//                     switch (e.String)
//                     {
//                         case "GroundJump":
//                             if (groundJumpPrefab && OnGround)
//                                 SpawnAtFoot(groundJumpPrefab, Quaternion.identity, new Vector3(mFlipped ? -1 : 1, 1, 1));
//                             break;
//                     }
//                     break;
//             }
//         }
//     }
    


//     //cache x velocity to ensure speed restores after heavy impact that results in physics penalties
//     override protected void HandlePhysics()
//     {
        
//         -- todo
//air control


//         //down attack loop.
//         if (state == ActionState.DOWNATTACK)
//         {
// 			//아래로 떨어지고 있는 중이다. 아직 땅에 닿지 않은 상태
// 			if (downAttackRecovery == false )
//             {
// 				//땅에 닿았다.
// 				if (OnGround)
// 				{
// 					SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
// 					downAttackRecoveryTime = 2f;//적절한 연출을 위해 하드코딩으로 회복시간을 가진다.
// 					downAttackRecovery = true;

// 					//아래 두줄은 뭘 의미할까?
// 					skeletonAnimation.skeleton.Data.FindAnimation(clearAttackAnim).Apply(skeletonAnimation.skeleton, 0, 1, false, null);
// 					skeletonAnimation.state.GetCurrent(0).Time = (downAttackFrameSkip / 30f);

// 					if (downAttackPrefab) Instantiate(downAttackPrefab, transform.position + new Vector3(0, 0.25f, 0), Quaternion.identity);
// 					if (movingPlatform) velocity = movingPlatform.Velocity;
// 				}
// 				else
// 				{
// 					//다운어택으로 떨어지는 중.
// 				}
//             }
// 			//땅에 닿은 후 회복중.
//             else
//             {
// 				if (downAttackRecoveryTime > 0)
// 				{
// 					downAttackRecoveryTime -= Time.deltaTime;
// 					velocity = Vector2.zero;
// 					if (movingPlatform) velocity = movingPlatform.Velocity;
// 				}
// 				//회복이 끝나면 점프 상태로 변경한다.
// 				else
// 				{
// 					SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
// 					velocity.y = jumpSpeed + (platformYVelocity >= 0 ? platformYVelocity : 0);
// 					mJumpStartTime = Time.time;
// 					SetState( ActionState.JUMP );
// 					mDoJump = false;
// 					mJumpPressed = false;
// 				}
//             }

//             if (velocityLock) velocity = Vector2.zero;
//         }

// 		//플립처리. velociy 적용.
//         mFlipped = skeletonAnimation.Skeleton.FlipX;
//         mRb.velocity = velocity;
//     }
    
//     //Bounce off a player in an angry way
//     bool EnemyBounceCheck(ref Vector2 velocity)
//     {
// 		Rigidbody2D character = OnTopOfCharacter();

// 		if( character == null ) return false;

// 		SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
// 		character.SendMessage("Hit", 1, SendMessageOptions.DontRequireReceiver);
// 		velocity.y = headBounceSpeed;
// 		mJumpStartTime = Time.time;
// 		SetState( ActionState.JUMP );
// 		mDoJump = false;
// 		return true;
//     }
// }