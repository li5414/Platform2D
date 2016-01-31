using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class HitmanController : GamePlayer
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

    public float wallJumpXSpeed = 3;
    public float slideDuration = 0.5f;
    public float slideVelocity = 6;
    public float slideSquish = 0.6f;
    /// <summary>
    /// How much velocity to apply during a punch animation when XVelocity Event is fired.
    /// </summary>
    public float punchVelocity = 7;
    /// <summary>
    /// How much velocity to apply during an uppercut when YVelocity Event is fired
    /// </summary>
    public float uppercutVelocity = 5;
    /// <summary>
    /// Downward velocity for duration of Down Attack
    /// </summary>
    public float downAttackVelocity = 20;
    /// <summary>
    /// How long it takes for a punch combo to timeout and go back to idle
    /// </summary>
    public float attackWatchdogDuration = 0.5f;
    /// <summary>
    /// How long to wait before falling off of a wall if no input
    /// </summary>
    public float wallSlideWatchdogDuration = 10f;
    /// <summary>
    /// Fall speed limit when wall sliding
    /// </summary>
    public float wallSlideSpeed = -2;


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

    [Header("Sounds")]
    public string footstepSound;
    public string landSound;
    public string jumpSound;
    public string slideSound;

    public GameObject downAttackPrefab;

    float slideStartTime;
    bool doSlide;

    bool attackWasPressed;
    bool waitingForAttackInput;
    float attackWatchdog;
    float airControlLockoutTime = 0;
    float wallSlideWatchdog;
    float wallSlideStartTime;
    bool wallSlideFlip;
    bool wasWallJump;

    bool upAttackUsed;
    bool downAttackRecovery = false;
    float downAttackRecoveryTime = 0;

    bool velocityLock;

    //box 2d workarounds
    float savedXVelocity;

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

    override protected void ProcessInput()
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
    
    override protected void UpdateAnim()
    {
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

        float x = mAxis.x;
        float y = mAxis.y;
        float absX = Mathf.Abs(x);
        
        float xVelocity = 0;
        float platformXVelocity = 0;
        float platformYVelocity = 0;
        
        Vector2 velocity = mRb.velocity;
        
        //무빙플랫폼 처리
        movingPlatform = GetMovingPlatform();
        if (movingPlatform)
        {
            platformXVelocity = movingPlatform.Velocity.x;
            platformYVelocity = movingPlatform.Velocity.y;
        }
        
        //점프하기로 하고 점프상태가 아니면 점프
        if (mDoJump && state != ActionState.JUMP)
        {
            SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
            
            //벽점프.
            if (state == ActionState.WALLSLIDE)
            {
                if ( PressingAgainstWall == false )
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
            
            velocity.y = (mJumpCount > 0 ? airJumpSpeed : jumpSpeed) + (platformYVelocity >= 0 ? platformYVelocity : 0);
            mJumpStartTime = Time.time;
            SetState( ActionState.JUMP );
            mDoJump = false;
            if (airJumpPrefab != null && mJumpCount > 0)
                Instantiate(airJumpPrefab, transform.position, Quaternion.identity);
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

            if (OnJump != null)
                OnJump(transform);

        }
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

            if (skeletonGhost != null)
                skeletonGhost.ghostingEnabled = true;
        }

        //ground logic
        if (state < ActionState.JUMP)
        {
            if (OnGround || movingPlatform)
            {
                mJumpCount = 0;
                upAttackUsed = false;
                if (absX > runThreshhold)
                {
                    xVelocity = runSpeed * Mathf.Sign(x);
                    velocity.x = Mathf.MoveTowards(velocity.x, xVelocity + platformXVelocity, Time.deltaTime * 15);
                    if (movingPlatform) velocity.y = platformYVelocity;
                    SetState( ActionState.RUN );
                    SetFriction(movingFriction);
                }
                else if (absX > deadZone)
                {
                    xVelocity = walkSpeed * Mathf.Sign(x);
                    velocity.x = Mathf.MoveTowards(velocity.x, xVelocity + platformXVelocity, Time.deltaTime * 25);
                    if (movingPlatform) velocity.y = platformYVelocity;
                    SetState( ActionState.WALK );
                    SetFriction(movingFriction);
                }
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
            else
            {
                SetFallState(true);
            }
            //air logic
        }
        else if (state == ActionState.JUMP)
        {
            float jumpTime = Time.time - mJumpStartTime;
            savedXVelocity = velocity.x;
            if (!mJumpPressed || jumpTime >= jumpDuration || downAttackRecovery)
            {
                mJumpStartTime -= jumpDuration;

                if (velocity.y > 0)
                    velocity.y = Mathf.MoveTowards(velocity.y, 0, Time.deltaTime * 30);

                if (velocity.y <= 0 || (jumpTime < jumpDuration && OnGround))
                {
                    if (!downAttackRecovery)
                    {
                        SetFallState(false);
                    }
                    else
                    {
                        downAttackRecovery = false;
                    }

                }
            }

            //fall logic
        }
        else if (state == ActionState.FALL)
        {

            if (OnGround)
            {
                SoundPalette.PlaySound(landSound, 1, 1, transform.position);
                if (absX > runThreshhold)
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
            //wall slide logic
        }
        else if (state == ActionState.WALLSLIDE)
        {
            mJumpCount = 0;
            if (OnGround && Mathf.Abs(velocity.x) < 0.1f && Time.time > (wallSlideStartTime + 0.2f))
            {
                SoundPalette.PlaySound(landSound, 1, 1, transform.position);
                SetState( ActionState.IDLE );
            }
            else if (!PressingAgainstWall)
            {
                if (!EnemyBounceCheck(ref velocity))
                {
                    if (y > -0.5f)
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

        //air control
        if (state == ActionState.JUMP || state == ActionState.FALL)
        {
            if (Time.time > airControlLockoutTime)
            {
                if (absX > runThreshhold)
                {
                    velocity.x = Mathf.MoveTowards(velocity.x, runSpeed * Mathf.Sign(x), Time.deltaTime * 8);
                }
                else if (absX > deadZone)
                {
                    velocity.x = Mathf.MoveTowards(velocity.x, walkSpeed * Mathf.Sign(x), Time.deltaTime * 8);
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
                    //cancel air control lockout if reverse joystick
                    if (absX > deadZone)
                    {
                        airControlLockoutTime = Time.time - 1;
                    }
                }
            }

            if (attackWasPressed && mAxis.y < -0.5f)
            {
                velocity.x = 0;
                velocity.y = 0;
                SetState( ActionState.DOWNATTACK );
                upAttackUsed = true;
                skeletonAnimation.AnimationName = downAttackAnim;
            }
            else if (attackWasPressed && mAxis.y > 0.5f)
            {
                if (!upAttackUsed)
                {
                    SetState( ActionState.UPATTACK );
                    skeletonAnimation.AnimationName = upAttackAnim;
                    upAttackUsed = true;
                    velocity.y = 1;
                }
            }

            if (state == ActionState.JUMP || state == ActionState.FALL)
            {
                if (Time.time != mJumpStartTime && PressingAgainstWall)
                {
                    if (Mathf.Abs(mRb.velocity.x) > 0.1f || (state == ActionState.FALL && absX > deadZone))
                    {
                        if (!wasWallJump && state == ActionState.JUMP)
                        {
                            //dont do anything if still going up
                        }
                        else
                        {
                            //start wall slide
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
                                wallSlideFlip = x > 0;
                            }
                        }


                    }

                }
            }
        }

        //falling and wallslide
        if (state == ActionState.FALL)
            velocity.y += fallGravity * Time.deltaTime;
        else if (state == ActionState.WALLSLIDE)
        {
            velocity.y = Mathf.Clamp(velocity.y, wallSlideSpeed, 0);
        }

        //slide control
        if (state == ActionState.SLIDE)
        {
            float slideTime = Time.time - slideStartTime;

            if (slideTime > slideDuration)
            {
                primaryCollider.transform.localScale = Vector3.one;
                IgnoreCharacterCollisions(false);
                if (skeletonGhost != null) skeletonGhost.ghostingEnabled = false;
                SetState( ActionState.IDLE );
            }
            else
            {
                x = Mathf.Sign(savedXVelocity);
                velocity.x = savedXVelocity + platformXVelocity;
                if (movingPlatform)
                    velocity.y = platformYVelocity;
            }

            if (!OnGround)
            {
                //Fell off edge while sliding
                primaryCollider.transform.localScale = Vector3.one;
                IgnoreCharacterCollisions(false);
                if (skeletonGhost != null)
                    skeletonGhost.ghostingEnabled = false;
                SetFallState(true);
            }
        }

        //attack control
        if (state == ActionState.ATTACK)
        {
            if (attackWasPressed)
            {
                attackWasPressed = false;

                //check if animation allows input now
                if (waitingForAttackInput)
                {
                    waitingForAttackInput = false;
                    skeletonAnimation.state.GetCurrent(0).TimeScale = 1;
                }
            }

            //apply some of the moving platform velocity
            velocity.x = Mathf.MoveTowards(velocity.x, platformXVelocity, Time.deltaTime * 8);
            if (movingPlatform)
                velocity.y = Mathf.MoveTowards(velocity.y, platformYVelocity, Time.deltaTime * 15);

            //combo is paused, set idle mode and run watchdog
            if (waitingForAttackInput)
            {
                SetFriction(idleFriction);
                attackWatchdog -= Time.deltaTime;
                //cancel combo
                if (attackWatchdog < 0) SetState( ActionState.IDLE );
            }
            else
            {
                //attacking, set moving mode
                SetFriction(movingFriction);
            }
        }

        //generic motion flipping control
        if (state < ActionState.ATTACK && state != ActionState.WALLSLIDE)
        {
            if (Time.time > airControlLockoutTime)
            {
                if (x > deadZone)
                    skeletonAnimation.Skeleton.FlipX = false;
                else if (x < -deadZone)
                    skeletonAnimation.Skeleton.FlipX = true;
            }
            else
            {
                if (velocity.x > deadZone)
                    skeletonAnimation.Skeleton.FlipX = false;
                else if (velocity.x < deadZone)
                    skeletonAnimation.Skeleton.FlipX = true;
            }

        }

        //down attack
        if (state == ActionState.DOWNATTACK)
        {
            //recovering from down attack
            if (downAttackRecovery)
            {
                //time elapsed, jump back to feet using JUMP state
                if (downAttackRecoveryTime <= 0)
                {
                    SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
                    velocity.y = jumpSpeed + (platformYVelocity >= 0 ? platformYVelocity : 0);
                    mJumpStartTime = Time.time;
                    SetState( ActionState.JUMP );
                    mDoJump = false;
                    mJumpPressed = false;
                }
                //wait for a bit
                else
                {
                    downAttackRecoveryTime -= Time.deltaTime;
                    velocity = Vector2.zero;
                    if (movingPlatform)
                        velocity = movingPlatform.Velocity;
                }
            }
            else
            {
                //Has impacted the ground, advance sub-state and recover
                if (OnGround)
                {
                    SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
                    downAttackRecoveryTime = 2f;  //hard coded value to add drama to recovery
                    downAttackRecovery = true;

                    //TODO: use set value
                    skeletonAnimation.skeleton.Data.FindAnimation(clearAttackAnim).Apply(skeletonAnimation.skeleton, 0, 1, false, null);
                    skeletonAnimation.state.GetCurrent(0).Time = (downAttackFrameSkip / 30f);

                    //spawn effect
                    if (downAttackPrefab)
                        Instantiate(downAttackPrefab, transform.position + new Vector3(0, 0.25f, 0), Quaternion.identity);

                    //adhere to moving platform
                    if (movingPlatform)
                        velocity = movingPlatform.Velocity;

                }
                else
                {
                    //TODO:  Watchdog and error case check
                }
            }

            //pause all movement, set by Pause event in animation for great dramatic posing.
            if (velocityLock)
                velocity = Vector2.zero;
        }

        mFlipped = skeletonAnimation.Skeleton.FlipX;
        mRb.velocity = velocity;
    }
    
    //Bounce off a player in an angry way
    bool EnemyBounceCheck(ref Vector2 velocity)
    {
        var character = OnTopOfCharacter();
        if (character != null)
        {
            SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
            character.SendMessage("Hit", 1, SendMessageOptions.DontRequireReceiver);
            velocity.y = headBounceSpeed;
            mJumpStartTime = Time.time;
            SetState( ActionState.JUMP );
            mDoJump = false;
            return true;
        }
        return false;
    }
}
