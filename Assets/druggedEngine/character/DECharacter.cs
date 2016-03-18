using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Spine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace druggedcode.engine
{
    public delegate bool StateTransition();

    public class DECharacter : MonoBehaviour, IDamageable
    {
		#region static
        const string ANIM_EVENT_VX = "VX";
        const string ANIM_EVENT_VY = "VY";
        const string ANIM_EVENT_WAITATTACK = "WaitAttack";
        const string ANIM_EVENT_FIRE = "Fire";
        const string ANIM_EVENT_EJECT_CASING = "EjectCasing";
        const string ANIM_EVENT_GRAVITY = "Gravity";
        const string ANIM_EVENT_GHOSTING = "Ghosting";
        const string ANIM_EVENT_STEP = "Step";
        const string ANIM_EVENT_SOUND = "Sound";
        const string ANIM_EVENT_EFFECT = "Effect";

        static public List<DECharacter> All = new List<DECharacter>();
        static private void Register(DECharacter ch)
        {
            if (All.Contains(ch) == false) All.Add(ch);
        }

        static private void Unregister(DECharacter ch)
        {
            if (All.Contains(ch)) All.Remove(ch);
        }
		#endregion

        public Transform body;
        public AnimationType bodyType;

        [SpineBone(dataField: "skeletonAnimation")]
        public string footEffectBone;

        [Header("Move")]
        public bool smoothMovement = true;
        public float accelOnGround = 10f;
        public float accelOnAir = 3f;

        [Header("Idle")]
        [SpineAnimation]
        public string idleAnim;

        [Header("Walk")]
        [SpineAnimation]
        public string walkAnim;
        public float WalkSpeed = 4f;

        [Header("Run")]
        [SpineAnimation]
        public string runAnim;
        public float RunSpeed = 10f;

        [Header("Dash")]
        [SpineAnimation]
        public string dashAnim;
        public float dashSpeed = 1f;
        public float dashDuration = 0.1f;

        [Header("Crounch")]
        [SpineAnimation]
        public string crouchAnim;
        public float CrouchSpeed = 1f;

        [Header("Ladder")]
        [SpineAnimation]
        public string ladderAnim;
        public float LadderSpeed = 2f;
        public float ladderClimbSpeed = 1f;

        [Header("Escape")]
        [SpineAnimation]
        public string escapeAnim;
        public float escapeDuration = 1f;

        [Header("Jump")]
        [SpineAnimation]
        public string jumpAnim;
        [SpineAnimation]
        public string fallAnim;
        public float JumpHeight = 3f;
        public float JumpHeightOnAir = 2f;
        public int jumpMax = 3;

        [Header("AttackGround")]
        [SpineAnimation]
        public string attackGroundAnim;
        [SpineAnimation]
        public string attackUpAnim;
        [SpineAnimation]
        public string attackDownAnim;
        public float waitAttackDuration = 0.5f;

        [Header("AttackAir")]
        [SpineAnimation]
        public string attackAirAnim;
        [SpineAnimation]
        public string attackAirUpAnim;
        [SpineAnimation]
        public string attackAirDownAnim;

        [Header("Effect")]
        public GameObject jumpEffectPrefab;
        public GameObject airJumpEffectPrefab;

        //----------------------------------------------------------------------------------------------------------
        // event
        //----------------------------------------------------------------------------------------------------------
        public UnityAction<DECharacter> OnUpdateInput;

        //----------------------------------------------------------------------------------------------------------
        // public
        //----------------------------------------------------------------------------------------------------------
        public Ladder currentLadder { get; set; }

        public DEController controller { get; private set; }

        public int jumpCount { get; set; }

        public CharacterState state { get; protected set; }

        public float CurrentSpeed { get; set; }

        public DTSCharacter dts { get; set; }

        //----------------------------------------------------------------------------------------------------------
        // input
        //----------------------------------------------------------------------------------------------------------
        public float horizontalAxis { get; set; }

        public float verticalAxis { get; set; }

        public bool isRun { get; set; }

        //----------------------------------------------------------------------------------------------------------
        // private,protected
        //----------------------------------------------------------------------------------------------------------

        protected Transform mTr;

        protected float jumpStartTime;

        protected float jumpElapsedTime { get { return Time.time - jumpStartTime; } }

        protected float mDashStartTime;

        protected float mEscapeStartTime;
        protected List<StateTransition> mStateTransitions;

        protected float mWaitNextAttackEndTime;
        protected bool mWaitNextAttack;
        protected int mAttackIndex;

        //캐릭터의 중력을 활성화 하거나 비활성화 할때 이전 중력값을 기억하기 위한 용도
        protected float _originalGravity;
        protected Facing mFacing;

        protected SkeletonAnimation mSkeletonAnimation;
        protected SkeletonGhost mGhost;

		//----------------------------------------------------------------------------------------------------------
		// weapon
		//----------------------------------------------------------------------------------------------------------
		public List<Weapon> weapons;

        //----------------------------------------------------------------------------------------------------------
        // behaviour
        //----------------------------------------------------------------------------------------------------------
        protected bool mCanJump;
        protected bool mCanAttack;
        protected bool mCanEscape;
        protected bool mCanDash;

        protected UnityAction mStateLoop;
        protected UnityAction mStateExit;

		#region initialize

        virtual protected void Awake()
        {
            mTr = transform;
            controller = GetComponent<DEController>();
            mStateTransitions = new List<StateTransition>();

            mStateLoop = delegate { };
            mStateExit = delegate { };
        }        

        virtual protected void Start()
        {

            switch (bodyType)
            {
                case AnimationType.SPINE:

                    mGhost = GetComponentInChildren<SkeletonGhost>();

                    mSkeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
                    mSkeletonAnimation.state.Event += HandleEvent;
                    mSkeletonAnimation.state.Complete += HandleComplete;
                    break;
            }


			foreach (Weapon w in weapons)
			{
				w.Init( this, mSkeletonAnimation.skeleton.Data );
			}

			if( weapons.Count > 0 ) EquipWeapon(weapons[0]);

            Idle();
        }

		#endregion

		#region loop
        void Update()
        {
            if (OnUpdateInput != null) OnUpdateInput(this);

            StateUpdate();
        }

        void FixedUpdate()
        {
            if (controller.state.JustGotGrounded)
            {
                ResetJump();
                PlatformSoundPlay();
                PlatformEffectSpawn();
            }
        }

		#endregion

		#region ANIM HANDLE
		virtual protected void HandleComplete(Spine.AnimationState animState, int trackIndex, int loopCount)
		{
			var entry = animState.GetCurrent(trackIndex);
			if (entry.Animation.Name.IndexOf("Attack") == 0)
			{
				Idle();
			}
		}

		virtual protected void HandleEvent(Spine.AnimationState animState, int trackIndex, Spine.Event e)
		{
			switch (e.Data.name)
			{
				case ANIM_EVENT_VX:
					OnAnimVX(e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_VY:
					OnAnimVY(e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_WAITATTACK:
					OnAnimWaitAttack(e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_FIRE:
					OnFire(e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_EJECT_CASING:
					OnEjectCasing(e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_GRAVITY:
					OnAnimGravity(e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_GHOSTING:
					OnAnimGhosting(e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_STEP:
					OnAnimStep(e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_SOUND:
					OnAnimSound(e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_EFFECT:
					OnAnimEffect(e.Int, e.Float, e.String);
					break;
			}
		}

		virtual protected void OnAnimVX(int i, float f, string s)
		{
			controller.AddForceX(mFacing == Facing.RIGHT ? f : -f);
		}

		virtual protected void OnAnimVY(int i, float f, string s)
		{
			controller.AddForceY(f);
		}

		virtual protected void OnFire(int i, float f, string s)
		{
			FireWeapon();
		}

		virtual protected void OnEjectCasing(int i, float f, string s)
		{
			EjectCasing();
		}

		virtual protected void OnAnimWaitAttack(int i, float f, string s)
		{
			WaitNextAttack();
		}

		virtual protected void OnAnimGravity(int i, float f, string s)
		{
			controller.Stop();
			controller.gravityScale = f;
		}

		virtual protected void OnAnimGhosting(int i, float f, string s)
		{
			GhostMode(i == 1 ? true : false, s, f);
		}

		virtual protected void OnAnimStep(int i, float f, string s)
		{
			if (controller.state.IsGrounded) PlatformSoundPlay();
		}

		virtual protected void OnAnimSound(int i, float f, string s)
		{
			SoundManager.Instance.PlaySFX(s, 1f, 1, transform.position);
		}

		virtual protected void OnAnimEffect(int i, float f, string s)
		{
			//print("Effect : " + s);
		}
		#endregion

		virtual protected void OnEnable()
		{
			Register(this);
		}

		virtual protected void OnDisable()
		{
			Unregister(this);
		}


		#region FSM
        protected void SetState(CharacterState next)
        {
            if (state == next)
            {
                //Debug.Log("---------same state!!!!! " + state + " > " + next);
                return;
            }

            //Debug.Log (state + " > " + next);

            mStateExit();

            mStateExit = delegate { };
            mStateLoop = delegate { };
            mStateTransitions.Clear();

            state = next;
        }

        protected void StateUpdate()
        {
            if (StateTransition() == false) mStateLoop();
        }

        bool StateTransition()
        {
            int count = mStateTransitions.Count;
            StateTransition transition;
            for (int i = 0; i < count; ++i)
            {
                transition = mStateTransitions[i];
                if (transition()) return true;
            }
            return false;
        }

        protected void AddTransition(StateTransition transition)
        {
            mStateTransitions.Add(transition);
        }

        protected void RemoveTransition(StateTransition transition)
        {
            mStateTransitions.Remove(transition);
        }

        virtual protected void Idle()
        {
            SetState(CharacterState.IDLE);

            mCanEscape = true;
            mCanDash = true;
            mCanJump = true;
            mCanAttack = true;

            PlayAnimation(idleAnim);
            CurrentSpeed = 0f;

            AddTransition(TransitionGround_Fall);
            AddTransition(TransitionIdle_Move);

            mStateLoop += Move;
        }

        virtual protected void Walk()
        {
            SetState(CharacterState.WALK);

            PlayAnimation(walkAnim);
            CurrentSpeed = WalkSpeed;

            AddTransition(TransitionGround_Fall);
            AddTransition(TransitionWalk_IdleOrRun);

            mStateLoop += Move;
        }

        virtual protected void Run()
        {
            SetState(CharacterState.RUN);

            PlayAnimation(runAnim);
            CurrentSpeed = RunSpeed;

            AddTransition(TransitionGround_Fall);
            AddTransition(TransitionRun_StopOrWalk);

            mStateLoop += Move;
        }

		#endregion

		#region Action
		protected void DoDash()
		{
			if (mCanDash == false) return;
			SetState(CharacterState.DASH);

			mCanDash = false;
			mCanJump = false;
			mCanEscape = false;
			mCanAttack = true;

			mDashStartTime = Time.time;

			PlayAnimation(dashAnim);
			GravityActive(false);
			Stop();
			controller.vx = mFacing == Facing.RIGHT ? dashSpeed : -dashSpeed;

			AddTransition(TransitionDash_Idle);

			mStateExit += delegate
			{
				GravityActive(true);
				Stop();
			};
		}

		protected void DoEscape()
		{
			if (mCanEscape == false) return;

			SetState(CharacterState.ESCAPE);

			mCanDash = true;
			mCanJump = true;
			mCanEscape = false;
			mCanAttack = false;

			mEscapeStartTime = Time.time;
			PlayAnimation(escapeAnim);
			controller.UpdateColliderSize(1f, 0.5f);
			Stop();
			controller.vx = mFacing == Facing.RIGHT ? RunSpeed : -RunSpeed;

			AddTransition(TransitionGround_Fall);
			AddTransition(TransitionEscape_Idle);

			mStateExit += delegate
			{
				controller.ResetColliderSize();
				GhostMode(false);
			};
		}

		virtual protected void DoAttack()
		{
			if (mCanAttack == false) return;

			SetState(CharacterState.ATTACK);

			mCanDash = false;
			mCanJump = false;
			mCanEscape = true;
			mCanAttack = false;

			if (mWaitNextAttack)
			{
				NextAttack();
				return;
			}

			if (controller.state.IsGrounded)
			{
				GroundAttack();
			}
			else
			{
				AirAttack();
			}

			mStateExit += delegate
			{
				mWaitNextAttack = false;
			};
		}

		virtual protected void GroundAttack()
		{
			mAttackIndex = 0;
			Stop();

			if (verticalAxis > 0.1f && string.IsNullOrEmpty(attackUpAnim) == false)
			{
				PlayAnimation(attackUpAnim);
			}
			else if (verticalAxis < -0.1f && string.IsNullOrEmpty(attackDownAnim) == false)
			{
				PlayAnimation(attackDownAnim);
			}
			else if (string.IsNullOrEmpty(attackGroundAnim) == false)
			{
				PlayAnimation(attackGroundAnim);
			}

			AddTransition(TransitionAttack_Idle);
			AddTransition(TransitionGround_Fall);
		}

		protected void NextAttack()
		{
			mWaitNextAttack = false;
			mAttackIndex++;

			if (mAttackIndex == 3)
			{
				RemoveTransition(TransitionGround_Fall);
			}

			switch (bodyType)
			{
				case AnimationType.SPINE:
					GetCurrent(0).TimeScale = 1;
					break;
			}
		}

		void WaitNextAttack()
		{
			mCanAttack = true;
			mWaitNextAttack = true;
			mWaitNextAttackEndTime = Time.time + waitAttackDuration;
			currentAnimationTimeScale(0f);
		}

		void StopWaitNextAttack()
		{
			mWaitNextAttack = false;
			Idle();
		}

		virtual protected void AirAttack()
		{
			if (verticalAxis > 0.1f && string.IsNullOrEmpty(attackAirUpAnim) == false)
			{
				SetState(CharacterState.ATTACK);
				PlayAnimation(attackAirUpAnim);
			}
			else if (verticalAxis < -0.1f && string.IsNullOrEmpty(attackAirDownAnim) == false)
			{
				SetState(CharacterState.ATTACK);
				PlayAnimation(attackAirDownAnim);
			}
			else if (string.IsNullOrEmpty(attackAirAnim) == false)
			{
				SetState(CharacterState.ATTACK);
				PlayAnimation(attackAirAnim);
			}

		}

        virtual protected void DoJump()
        {
            if (mCanJump == false) return;
            if (jumpCount >= jumpMax) return;

            SetState(CharacterState.JUMP);

            mCanDash = true;
            mCanJump = true;
            mCanEscape = false;
            mCanAttack = true;

            bool wallJump = false;
            float jumpPower;
            GameObject effect;

            //firstJump
            if (jumpCount == 0)
            {
                controller.state.ClearPlatform();
                PlatformSoundPlay();
                PlatformEffectSpawn();

                if (state == CharacterState.WALLSLIDE)
                {
                    controller.vx = mFacing == Facing.LEFT ? 4 : -4;
                    controller.LockMove(0.5f);
                    wallJump = true;
                }
                else if (controller.state.IsGrounded)
                {

                }

                PlayAnimation(jumpAnim);
                jumpPower = Mathf.Sqrt(2f * JumpHeight * Mathf.Abs(controller.Gravity));
                effect = jumpEffectPrefab;
            }
            //airJump
            else
            {
                PlayAnimation(jumpAnim);
                jumpPower = Mathf.Sqrt(2f * JumpHeightOnAir * Mathf.Abs(controller.Gravity));
                effect = airJumpEffectPrefab;
            }

            CurrentSpeed = isRun ? RunSpeed : WalkSpeed;
            controller.vy = jumpPower;
            jumpStartTime = Time.time;
            jumpCount++;

            if (wallJump)
            {
                SpawnAtFoot(effect, Quaternion.Euler(0, 0, mFacing == Facing.RIGHT ? 90 : -90), new Vector3(mFacing == Facing.RIGHT ? 1f : -1f, 1f, 1f));
            }
            else
            {
                FXManager.Instance.SpawnFX(effect, mTr.position, new Vector3(mFacing == Facing.RIGHT ? 1f : -1f, 1f, 1f));
            }

            AddTransition(TransitionJump_Fall);

            mStateLoop += Move;
        }

        protected void DoJumpBelow()
        {
            if (mCanJump == false) return;

            if (controller.state.IsOnOneway)
            {
                PassOneway();
                return;
            }
            else if (state == CharacterState.LADDER)
            {
                Fall();
                return;
            }
            else
            {
                DoJump();
            }
        }
		#endregion

        protected void PassOneway()
        {
            mTr.position = new Vector2(mTr.position.x, mTr.position.y - 0.1f);
            controller.state.ClearPlatform();
            controller.PassThroughOneway();
            Fall();
        }

        virtual protected void Fall(bool useJumpCount = true)
        {
            SetState(CharacterState.ESCAPE);

            mCanDash = true;
            mCanJump = true;
            mCanEscape = false;
            mCanAttack = true;

            if (useJumpCount) jumpCount++;
            PlayAnimation(fallAnim);

            mStateLoop += Move;

            AddTransition(TransitionAir_Idle);
        }

        protected void PlatformSoundPlay()
        {
            Platform platform = controller.state.StandingPlatform;
            if (platform != null) platform.PlaySound(mTr.position, 1f, 1f);
        }

        protected void PlatformEffectSpawn()
        {
            Platform platform = controller.state.StandingPlatform;
            if (platform != null) platform.ShowEffect(mTr.position, new Vector3(mFacing == Facing.RIGHT ? 1f : -1f, 1f, 1f));
        }

        protected void SpawnAtFoot(GameObject prefab, Quaternion rotation, Vector3 scale)
        {
            if (prefab == null) return;

            Vector3 pos = mTr.position;
            if (footEffectBone != null)
            {
                Bone bone = mSkeletonAnimation.Skeleton.FindBone(footEffectBone);
                if (bone != null) pos = body.transform.TransformPoint(bone.WorldX, bone.WorldY, 0f);
            }

            FXManager.Instance.SpawnFX(prefab, pos, rotation, scale);
        }

        protected void GhostMode(bool use, string str = "", float f = 0f)
        {
            if (mGhost == null) return;

            mGhost.ghostingEnabled = use;

            if (use == false) return;

            if (f != 0f)
            {
                //mGhost.spawnRate = f;
                //or
                //mGhost.fadeSpeed = f;
            }

            if (string.IsNullOrEmpty(str) == false)
            {
                mGhost.color = ColorUtil.HexToColor(str);
            }
        }

        public void Pause()
        {
            controller.Stop();
            controller.enabled = false;

            CurrentSpeed = 0f;
            horizontalAxis = 0f;
            currentLadder = null;
            enabled = false;

            /* reset state
            InDialogueZone = false;
            CurrentDialogueZone = null;
            */

            Idle();
        }

        public void Active()
        {
            controller.enabled = true;
            controller.CollisionsOn();

            enabled = true;
            ResetJump();

            gameObject.SetActive(true);
        }

        public void Kill()
        {
            //GravityActive(true);
            //_state.TriggerDead = true;
            //todo. respawn after dead motion.
        }

        protected void Move()
        {
            if (mFacing == Facing.LEFT && horizontalAxis > 0.1f)
            {
                SetFacing(Facing.RIGHT);
            }
            else if (mFacing == Facing.RIGHT && horizontalAxis < -0.1f)
            {
                SetFacing(Facing.LEFT);
            }

            float targetVX = horizontalAxis * CurrentSpeed;
            if (targetVX != 0f && smoothMovement)
            {
                float moveFactor = controller.state.IsGrounded ? accelOnGround : accelOnAir;
                targetVX = Mathf.Lerp(controller.vx, targetVX, Time.deltaTime * moveFactor);
            }

            controller.vx = targetVX;
        }

        public void Stop()
        {
            controller.Stop();
        }

        public void Spawn(Vector3 pos)
        {
            mTr.position = pos;
            SetFacing(Facing.RIGHT);

            Active();
        }

        public void ResetJump()
        {
            jumpCount = 0;
        }

		#region WEAPON

		protected Weapon currentWeapon;
		void EquipWeapon(Weapon weapon)
		{
			if( currentWeapon == weapon ) return;

			currentWeapon = weapon;
			currentWeapon.Setup();

			PlayAnimation(weapon.idleAnim, true, 1);
		}

		virtual protected void FireWeapon()
		{
			// currentWeapon.Fire();
			// if (this.state == ActionState.JETPACK)
			// {
			//     doRecoil = true;
			// }
		}

		void Shoot()
		{
			//조준하고,
			if (currentWeapon.reloadLock == false &&
				currentWeapon.clip > 0 &&
				Time.time >= currentWeapon.nextFireTime)
			{
				PlayAnimation(currentWeapon.fireAnim, false, 1);
				currentWeapon.nextFireTime = Time.time + currentWeapon.refireRate;
			}
			else if (currentWeapon.reloadLock == false &&
				Time.time >= currentWeapon.nextFireTime)
			{
				if (currentWeapon.ammo > 0 && currentWeapon.clip < currentWeapon.clipSize)
				{
					PlayAnimation(currentWeapon.reloadAnim, false, 1);
					currentWeapon.reloadLock = true;
				}
			}

			TrackEntry entry = GetCurrent(1);
			//리로드 가 아닌 경우 aiming 
			if( currentWeapon.reloadLock == false )
			{
				if( entry == null ||
					entry.Animation != currentWeapon.FireAnim && entry.Animation != currentWeapon.AimAnim )
				{
					PlayAnimation( currentWeapon.aimAnim,true,1);
				}

				float angle = 45f;
			}
			//리로드 중인 경우
			else
			{
				if( currentWeapon.reloadLock == false &&
					( entry == null || entry.Animation != currentWeapon.FireAnim && entry.Animation != currentWeapon.IdleAnim ))
				{
					PlayAnimation( currentWeapon.idleAnim, true, 1 );
				}
			}
		}

		virtual protected void EjectCasing()
		{
			// Instantiate(currentWeapon.casingPrefab, currentWeapon.casingEjectPoint.position, Quaternion.LookRotation(Vector3.forward, currentWeapon.casingEjectPoint.up));
		}

		#endregion
        //----------------------------------------------------------------------------------------------------------
        // interface
        //----------------------------------------------------------------------------------------------------------

        public void TakeDamage(int damage, GameObject attacker)
        {
            //hit sound play
            //hit effect instantiate

            // 일정시간동안 레이어12 와 13 과의 충돌을 제거한다 (탄,적)
            //Physics2D.IgnoreLayerCollision(9, 12, true);
            //Physics2D.IgnoreLayerCollision(9, 13, true);

            //StartCoroutine(ResetLayerCollision(0.5f));

            // 캐릭터의 sprite 를 반짝거리게 만든다.
            //			if (GetComponent<Renderer>() != null)
            //			{
            //				Color flickerColor = new Color32(255, 20, 20, 255);
            //				StartCoroutine(Flicker(_initialColor, flickerColor, 0.05f));
            //			}

            //데미지만큼 hp 감소

            //            Health -= damage;
            //            if (Health<=0)
            //            {
            //                LevelManager.Instance.KillPlayer();
            //            }
        }

        //----------------------------------------------------------------------------------------------------------
        // physics
        //----------------------------------------------------------------------------------------------------------

        //캐릭터의 중력을 활성화 하거나 비활성화한다.
        public void GravityActive(bool state)
        {
            if (state)
            {
                if (controller.gravityScale == 0)
                {
                    controller.gravityScale = _originalGravity;
                }
            }
            else
            {
                if (controller.gravityScale != 0)
                {
                    _originalGravity = controller.gravityScale;
                }
                controller.gravityScale = 0;
            }
        }

        public void UpdatePhysicInfo(PhysicInfo physicInfo)
        {
            controller.SetPhysicsSpace(physicInfo);
        }

        public void ResetPhysicInfo()
        {
            controller.ResetPhysicInfo();
        }

        //----------------------------------------------------------------------------------------------------------
        // body Controll
        //----------------------------------------------------------------------------------------------------------

        public void BodyPosition(Vector2 translate)
        {
            body.transform.localPosition = translate;
        }

        void SetFacing(Facing facing)
        {
            mFacing = facing;

            switch (mFacing)
            {
                case Facing.RIGHT:
                    body.localScale = new Vector3(1f, body.localScale.y, body.localScale.z);
                    break;

                case Facing.LEFT:
                    body.localScale = new Vector3(-1f, body.localScale.y, body.localScale.z);
                    break;
            }
        }

        protected bool AnimFilp
        {
            set
            {
                switch (bodyType)
                {
                    case AnimationType.SPINE:
                        mSkeletonAnimation.Skeleton.FlipX = value;
                        break;
                }
            }
        }

        protected TrackEntry GetCurrent(int trackIndex = 0)
        {
            return mSkeletonAnimation.state.GetCurrent(trackIndex);
        }

        protected float currentAnimationDuration
        {
            get
            {
                switch (bodyType)
                {
                    case AnimationType.SPINE:
                        return GetCurrent(0).animation.Duration;
                }

                return 0f;
            }
        }

        protected void currentAnimationTimeScale(float timeScale)
        {
            switch (bodyType)
            {
                case AnimationType.SPINE:
                    GetCurrent(0).TimeScale = timeScale;
                    break;
            }
        }

        protected void PlayAnimation(string animName, bool loop = true, int trackIndex = 0)
        {
            switch (bodyType)
            {
                case AnimationType.SPINE:
                    mSkeletonAnimation.state.SetAnimation(trackIndex, animName, loop);
                    break;
            }
        }

        protected void PlayAnimation(Spine.Animation animation, bool loop = true, int trackIndex = 0)
        {
            mSkeletonAnimation.state.SetAnimation(trackIndex, animation, loop);
        }

        protected bool HasAnim(string animName)
        {
            switch (bodyType)
            {
                case AnimationType.SPINE:
                    return mSkeletonAnimation.state.Data.SkeletonData.FindAnimation(animName) == null ? false : true;
            }

            return false;
        }

        //--------------------------------------------------------------------------------------------
        // state transition
        //--------------------------------------------------------------------------------------------
        protected bool TransitionGround_Fall()
        {
            if (controller.state.IsGrounded) return false;

            Fall();
            return true;
        }

        protected bool TransitionWalk_IdleOrRun()
        {
            if (horizontalAxis == 0f)
            {
                Idle();
                return true;
            }
            if (isRun)
            {
                Run();
                return true;
            }

            return false;
        }

        protected bool TransitionIdle_Move()
        {
            if (horizontalAxis == 0f) return false;

            if (isRun) Run();
            else Walk();
            return true;
        }

        protected bool TransitionRun_StopOrWalk()
        {
            if (isRun) return false;

            if (horizontalAxis != 0f) Walk();
            else Idle();
            return true;
        }

        protected bool TransitionDash_Idle()
        {
            float dashElapsedTime = Time.time - mDashStartTime;
            if (dashElapsedTime < dashDuration) return false;

            Idle();
            return true;
        }

        protected bool TransitionAttack_Idle()
        {
            if (mWaitNextAttack == false) return false;
            if (Time.time < mWaitNextAttackEndTime) return false;
            StopWaitNextAttack();
            return true;
        }

        protected bool TransitionEscape_Idle()
        {
            float slideElapsedTime = Time.time - mEscapeStartTime;
            if (slideElapsedTime < escapeDuration) return false;
            if (controller.IsCollidingHead) return false;

            Idle();
            return true;
        }

        protected bool TransitionJump_Fall()
        {
            if (controller.vy > 0) return false;
            Fall(false);
            return true;
        }

        protected bool TransitionAir_Idle()
        {
            if (controller.state.IsGrounded == false) return false;
            Idle();
            return true;
        }

        //----------------------------------------------------------------------------------------------------------
        // get;set;
        //----------------------------------------------------------------------------------------------------------
        //추후 wall 이 아니라 특정 오브젝트를 밀고 있는지를 알 수 있는 메소드로 변경하자
        public bool IsPressAgainstWall
        {
            get
            {
                if (controller.state.CollidingSide == null) return false;

                Wall wall = controller.state.CollidingSide.GetComponent<Wall>();

                if (wall == null) return false;
                else if (wall.slideWay == WallSlideWay.NOTHING) return false;
                else if ((wall.slideWay == WallSlideWay.LEFT || wall.slideWay == WallSlideWay.BOTH) && mFacing == Facing.RIGHT && horizontalAxis > 0f) return true;
                else if ((wall.slideWay == WallSlideWay.RIGHT || wall.slideWay == WallSlideWay.BOTH) && mFacing == Facing.LEFT && horizontalAxis < 0f) return true;
                return false;
            }
        }


#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Handles.Label(mTr.position + new Vector3(0, 1.2f, 0), state.ToString());
        }
#endif
    }
}


