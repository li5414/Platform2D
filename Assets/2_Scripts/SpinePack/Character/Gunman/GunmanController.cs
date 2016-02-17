using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using Spine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GunmanController : TempGameCharacter
{
    /*
    ActionState[] ableList = 
    {
        ActionState.IDLE,
        ActionState.WALK,
        ActionState.RUN,
        ActionState.JUMP,
        ActionState.FALL,
        ActionState.JETPACK
    };
    */
    
    public float aimDeadZone = 0.05f;

    public float jetpackStartSpeed = 5;
    public float jetpackThrust = 5;
    public float jetpackDampener = 0.8f;
    public float jetpackDescentSpeed = -2;
    public float jetpackDuration = 2;
    public float jetpackFuel = 2;
    public float jetpackRecovery = 1;


    [Header("Animations")]
    [SpineAnimation]
    public string walkAnim;
    [SpineAnimation]
    public string walkBackwardAnim;
    [SpineAnimation]
    public string runAnim;
    [SpineAnimation]
    public string idleAnim;
    [SpineAnimation]
    public string jumpAnim;
    [SpineAnimation]
    public string fallAnim;
    [SpineAnimation]
    public string jetpackNeutralAnim;
    [SpineAnimation]
    public string jetpackForwardAnim;
    [SpineAnimation]
    public string jetpackBackwardAnim;

    [Header("Sounds")]
    public string footstepSound;
    public string landSound;
    public string jumpSound;

    public Transform graphicsRoot;
    public SkeletonUtilityBone aimPivotBone;
    public Thruster thruster;

    [Header("Weapons")]
    public bool allowRunAim = false;
    //public Weapon[] weapons;
    public List<GunmanWeapon> weapons;

    bool aiming;
    Vector2 aimStick;

    GunmanWeapon currentWeapon;

    override protected void Start()
    {
        base.Start();

        foreach (GunmanWeapon w in weapons)
            w.CacheSpineAnimations(skeletonAnimation.skeleton.Data);

        EquipWeapon(weapons[0]);
    }

    void EquipWeapon(GunmanWeapon weapon)
    {
        var skeleton = skeletonAnimation.skeleton;
        weapon.SetupAnim.Apply(skeleton, 0, 1, false, null);
        skeletonAnimation.state.SetAnimation(1, weapon.IdleAnim, true);
        currentWeapon = weapon;
        currentWeapon.Setup();
    }

    override protected void HandleComplete(Spine.AnimationState state, int trackIndex, int loopCount)
    {
        var entry = state.GetCurrent(trackIndex);
        if (entry.Animation == currentWeapon.ReloadAnim)
        {
            currentWeapon.Reload();
            currentWeapon.reloadLock = false;
        }
    }

    bool doRecoil;

    override protected void HandleEvent(Spine.AnimationState state, int trackIndex, Spine.Event e)
    {
        var entry = state.GetCurrent(trackIndex);
        if (entry != null)
        {
            switch (e.Data.Name)
            {
                case "Fire":
                    currentWeapon.Fire();
                    if (this.state == CharacterState.JETPACK)
                    {
                        doRecoil = true;
                    }

                    break;
                case "Sound":
                    SoundPalette.PlaySound(e.String, 1f, 1, transform.position);
                    break;
                case "EjectCasing":
                    Instantiate(currentWeapon.casingPrefab, currentWeapon.casingEjectPoint.position, Quaternion.LookRotation(Vector3.forward, currentWeapon.casingEjectPoint.up));
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

        UpdateAnim();
    }

    public void Input(
        Vector2 moveStick, Vector2 aimStick,
        bool JUMP_isPressed, bool JUMP_wasPressed,
        bool FIRE_wasPressed, bool PREVIOUS_wasPressed, bool NEXT_wasPressed)
    {
        bool useKeyboard = false;

        if (((OnGround || movingPlatform) && state < CharacterState.JUMP) || (state == CharacterState.FALL/* Jetpack fuel here*/))
        {
            if (mJumpPressed == false )
            {

                if (JUMP_wasPressed && this.mPassThroughPlatform == null && moveStick.y < -0.25f)
                {
                    var platform = PlatformCast(mCastOriginCenterGround);
                    if (platform != null)
                        DoPassThrough(platform);
                    else
                    {
                        mDoJump = true;
                        mJumpPressed = true;
                    }
                }
                else
                {
                    mDoJump = JUMP_wasPressed;
                    if (mDoJump)
                    {
                        mJumpPressed = true;
                    }
                }
            }
        }
        else
        {
            if (state == CharacterState.JUMP || state == CharacterState.JETPACK)
            {
                if (mJumpCount >= maxJumps)
                {
                    //do jetpack
                    if (JUMP_wasPressed)
                    {
                        mJumpPressed = true;
                        mDoJump = true;
                    }
                }
            }
        }

        mJumpPressed = JUMP_isPressed;


        //gun handling
        bool flip = false;
        if (moveStick.x > deadZone)
        {
            //do nothing
        }
        else if (moveStick.x < -deadZone)
        {
            flip = true;
        }

        if (aiming && !currentWeapon.reloadLock && FIRE_wasPressed && currentWeapon.clip > 0 && Time.time >= currentWeapon.nextFireTime)
        {
            skeletonAnimation.state.SetAnimation(1, currentWeapon.FireAnim, false);
            currentWeapon.nextFireTime = Time.time + currentWeapon.refireRate;
        }
        else if (!currentWeapon.reloadLock && Time.time >= currentWeapon.nextFireTime && FIRE_wasPressed)
        {
            if (currentWeapon.ammo > 0 && currentWeapon.clip < currentWeapon.clipSize)
            {
                skeletonAnimation.state.SetAnimation(1, currentWeapon.ReloadAnim, false);
                currentWeapon.reloadLock = true;
            }
        }

        var entry = skeletonAnimation.state.GetCurrent(1);
        if (!currentWeapon.reloadLock && (aimStick.magnitude > aimDeadZone || (useKeyboard && !mIsRun)))
        {
            aiming = true;
            if (entry == null || entry.Animation != currentWeapon.FireAnim && entry.Animation != currentWeapon.AimAnim)
            {
                skeletonAnimation.state.SetAnimation(1, currentWeapon.AimAnim, true);
            }

            float a = Mathf.Atan2(aimStick.y, aimStick.x) * Mathf.Rad2Deg;
            if (a < 0)
                a += 360;

            if (a < 270 && a > 90)
                flip = true;
            else
                flip = false;

            mFlipped = flip;

            float minAngle = currentWeapon.minAngle;
            float maxAngle = currentWeapon.maxAngle;

            a = flip ? 180 + Mathf.Clamp(Mathf.DeltaAngle(0, a - 180), -maxAngle, -minAngle) : Mathf.Clamp(Mathf.DeltaAngle(0, a), minAngle, maxAngle);

            aimPivotBone.transform.localRotation = Quaternion.RotateTowards(aimPivotBone.transform.localRotation, Quaternion.AngleAxis(flip ? 180 - a : a, Vector3.forward), 300 * Time.deltaTime);
        }
        else
        {
            aiming = false;
            aimPivotBone.transform.localRotation = Quaternion.Slerp(aimPivotBone.transform.localRotation, Quaternion.AngleAxis(0, Vector3.forward), 10 * Time.deltaTime);

            //TODO: automatic revert to aiming if firing without holding aim stick
            if (!currentWeapon.reloadLock && (entry == null || entry.Animation != currentWeapon.FireAnim && entry.Animation != currentWeapon.IdleAnim))
            {
                skeletonAnimation.state.SetAnimation(1, currentWeapon.IdleAnim, true);
            }

            if (moveStick.magnitude > deadZone)
                mFlipped = flip;
        }


        if (NEXT_wasPressed && !currentWeapon.reloadLock && currentWeapon.nextFireTime < Time.time)
        {
            int idx = weapons.IndexOf(currentWeapon);
            idx++;
            if (idx == weapons.Count)
                idx = 0;

            EquipWeapon(weapons[idx]);
        }
        else if (PREVIOUS_wasPressed && !currentWeapon.reloadLock && currentWeapon.nextFireTime < Time.time)
        {
            int idx = weapons.IndexOf(currentWeapon);
            idx--;
            if (idx < 0)
                idx = weapons.Count - 1;

            EquipWeapon(weapons[idx]);
        }

        this.mAxis = moveStick;
        this.aimStick = aimStick;

        graphicsRoot.localRotation = Quaternion.Euler(0, mFlipped ? 180 : 0, 0);
    }

    //cache x velocity to ensure speed restores after heavy impact that results in physics penalties
    float savedXVelocity;
    bool jetpackLatch;

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
			platformXVelocity = movingPlatform.velocity.x;
			platformYVelocity = movingPlatform.velocity.y;
        }

        if (mDoJump && mJumpCount >= maxJumps)
        {
            if (state == CharacterState.JETPACK)
            {
                //stop jetpacking
                SetState( velocity.y > 0 ? CharacterState.JUMP : CharacterState.FALL );
                mDoJump = false;
                thruster.goalThrust = 0;
            }
            else
            {
                if (jetpackFuel > 0)
                {
                    //start jetpacking
                    SetState( CharacterState.JETPACK );
                    mJumpStartTime = Time.time;
                    mDoJump = false;
                    velocity.y = jetpackStartSpeed;
                    thruster.goalThrust = 1;
                    SetFriction(movingFriction);
                    jetpackLatch = false;
                }
                else
                {
                    mDoJump = false;
                }

            }

        }
        else if (mDoJump && state != CharacterState.JUMP)
        {
            SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);

            velocity.y = (mJumpCount > 0 ? airJumpSpeed : jumpSpeed) + (platformYVelocity >= 0 ? platformYVelocity : 0);
            mJumpStartTime = Time.time;
            SetState( CharacterState.JUMP );
            mDoJump = false;
            if (airJumpPrefab != null && mJumpCount > 0)
                Instantiate(airJumpPrefab, transform.position, Quaternion.identity);
            else if (groundJumpPrefab != null && mJumpCount == 0)
            {
                SpawnAtFoot(groundJumpPrefab, Quaternion.identity, new Vector3(mFlipped ? -1 : 1, 1, 1));
            }
            mJumpCount++;



            if (OnJump != null) OnJump(transform);
        }

        //ground logic
        if (state < CharacterState.JUMP)
        {
            if (OnGround || movingPlatform)
            {
                jetpackFuel = Mathf.MoveTowards(jetpackFuel, jetpackDuration, Time.deltaTime * jetpackRecovery);
                mJumpCount = 0;
                if (absX > runThreshold && (!aiming || allowRunAim))
                {
                    xVelocity = runSpeed * Mathf.Sign(x);
                    velocity.x = Mathf.MoveTowards(velocity.x, xVelocity + platformXVelocity, Time.deltaTime * 15);
                    if (movingPlatform) velocity.y = platformYVelocity;
                    SetState( CharacterState.RUN );
                    SetFriction(movingFriction);
                }
                else if (absX > deadZone)
                {
                    xVelocity = walkSpeed * Mathf.Sign(x);
                    velocity.x = Mathf.MoveTowards(velocity.x, xVelocity + platformXVelocity, Time.deltaTime * 25);
                    if (movingPlatform) velocity.y = platformYVelocity;
                    SetState( CharacterState.WALK );
                    SetFriction(movingFriction);
                }
                else
                {
                    velocity.x = movingPlatform ? platformXVelocity : Mathf.MoveTowards(velocity.x, 0, Time.deltaTime * 10);
                    if (movingPlatform) velocity.y = platformYVelocity;
                    SetState( CharacterState.IDLE );
                    SetFriction(movingPlatform ? movingFriction : idleFriction);
                }
            }
            else
            {
                SetFallState(true);
            }
            //air logic
        }
        else if (state == CharacterState.JUMP)
        {
            float jumpTime = Time.time - mJumpStartTime;
            savedXVelocity = velocity.x;
            if (!mJumpPressed || jumpTime >= jumpDuration)
            {
                mJumpStartTime -= jumpDuration;

                if (velocity.y > 0)
                    velocity.y = Mathf.MoveTowards(velocity.y, 0, Time.deltaTime * 30);

                if (velocity.y <= 0 || (jumpTime < jumpDuration && OnGround))
                {
                    SetFallState(false);
                }
            }

            //fall logic
        }
        else if (state == CharacterState.FALL)
        {

            if (OnGround)
            {
                SoundPalette.PlaySound(landSound, 1, 1, transform.position);
                if (absX > runThreshold)
                {
                    velocity.x = savedXVelocity;
                    SetState( CharacterState.RUN );
                }
                else if (absX > deadZone)
                {
                    velocity.x = savedXVelocity;
                    SetState( CharacterState.WALK );
                }
                else
                {
                    velocity.x = savedXVelocity;
                    SetState( CharacterState.IDLE );
                }
            }
            else
            {
                EnemyBounceCheck(ref velocity);
                savedXVelocity = velocity.x;

            }

        }

        //air control
        if (state == CharacterState.JUMP || state == CharacterState.FALL)
        {
            if (absX > runThreshold)
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

            if (state == CharacterState.JUMP || state == CharacterState.FALL)
            {

            }
        }
        else if (state == CharacterState.JETPACK)
        {
            float jetpackMultiplier = jetpackFuel > 0 ? 1 : 0.2f;
            velocity.x = Mathf.MoveTowards(velocity.x, mAxis.x * jetpackThrust * jetpackMultiplier, Time.deltaTime * 10);

            if (velocity.y > jetpackDescentSpeed)
                velocity.y += (-Physics2D.gravity.y * mRb.gravityScale) * Time.deltaTime * jetpackDampener; //offset gravity
            else
            {
                velocity.y += (-Physics2D.gravity.y * mRb.gravityScale) * Time.deltaTime * 1.2f; //offset gravity stronger
            }


            float fuelSpend = (Mathf.Abs(mAxis.x) + Mathf.Clamp01(mAxis.y) * 1.5f) / 2;
            if (!jetpackLatch && mJumpPressed)
                fuelSpend = Mathf.Clamp(fuelSpend, 0.75f, 10);

            jetpackFuel -= fuelSpend * Time.deltaTime;
            jetpackFuel = Mathf.Clamp(jetpackFuel, 0, jetpackDuration);
            if (jetpackFuel > 0)
                thruster.goalThrust = Mathf.Lerp(0.15f, 1f, fuelSpend);
            else
                thruster.goalThrust = 0.1f;
            if (Mathf.Abs(mAxis.y) < deadZone)
            {
                //nothin
                if (!jetpackLatch)
                {
                    if (!mJumpPressed)
                        jetpackLatch = true;
                    else if (jetpackFuel > 0)
                        velocity.y = Mathf.MoveTowards(velocity.y, jetpackThrust, Time.deltaTime * 15);
                }
            }
            else
            {
                float jetpackY = y;
                if (mJumpPressed)
                    jetpackY = 1;

                if (jetpackFuel == 0 && jetpackY > 0)
                {
                    jetpackY = 0;
                    thruster.goalThrust = 0.15f;

                }
                else
                {
                    velocity.y = Mathf.MoveTowards(velocity.y, jetpackY * jetpackThrust, Time.deltaTime * 15);
                }
            }

            if (doRecoil)
            {
                var recoil = currentWeapon.GetRecoil();
                velocity.x += recoil.x;
                velocity.y += recoil.y;
                doRecoil = false;
            }

            if (Time.time > mJumpStartTime + 0.25f && OnGround)
            {
                if (velocity.y < 2f)
                {
                    SetState( CharacterState.IDLE );
                    thruster.goalThrust = 0;
                }
            }
        }

        //falling and wallslide
        if (state == CharacterState.FALL)
            velocity.y += fallGravity * Time.deltaTime;

        //generic motion flipping control
        /*  handled in ProcessInput for this character due to aiming */

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
            SetState( CharacterState.JUMP );
            mDoJump = false;
            SetFriction(movingFriction);
            return true;
        }
        return false;
    }

    void UpdateAnim()
    {
        switch (state)
        {
            case CharacterState.IDLE:
                if (CenterOnGround)
                {
                    skeletonAnimation.AnimationName = idleAnim;
                }
                else
                {
                    //TODO:  deal with edge animations for this character rig
                    if (mIsOnSlope)
                        skeletonAnimation.AnimationName = idleAnim;
                    else if (BackOnGround)
                    {
                        skeletonAnimation.AnimationName = idleAnim;
                    }
                    else if (ForwardOnGround)
                    {
                        skeletonAnimation.AnimationName = idleAnim;
                    }
                }
                break;
            case CharacterState.WALK:
                if (aiming)
                {
                    if (Mathf.Sign(aimStick.x) != Mathf.Sign(mAxis.x))
                        skeletonAnimation.AnimationName = walkBackwardAnim;
                    else
                        skeletonAnimation.AnimationName = walkAnim;
                }
                else
                {
                    skeletonAnimation.AnimationName = walkAnim;
                }

                break;
            case CharacterState.RUN:
                skeletonAnimation.AnimationName = runAnim;
                break;
            case CharacterState.JUMP:
                skeletonAnimation.AnimationName = jumpAnim;
                break;
            case CharacterState.FALL:
                skeletonAnimation.AnimationName = fallAnim;
                break;
            case CharacterState.JETPACK:
                if (mAxis.x > deadZone)
                    skeletonAnimation.AnimationName = mFlipped ? jetpackBackwardAnim : jetpackForwardAnim;
                else if (mAxis.x < -deadZone)
                    skeletonAnimation.AnimationName = mFlipped ? jetpackForwardAnim : jetpackBackwardAnim;
                else
                    skeletonAnimation.AnimationName = jetpackNeutralAnim;
                break;
        }
    }
}
