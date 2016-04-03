using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Spine;

namespace druggedcode.engine
{
    public delegate bool StateTransition();

    public class DEActor : MonoBehaviour
    {
        #region STATIC
        const string ANIM_EVENT_VX = "VX";
        const string ANIM_EVENT_VY = "VY";
        const string ANIM_EVENT_GRAVITY = "Gravity";
        const string ANIM_EVENT_GHOSTING = "Ghosting";
        const string ANIM_EVENT_STEP = "Step";
        const string ANIM_EVENT_SOUND = "Sound";
        const string ANIM_EVENT_EFFECT = "Effect";

		public const string ANIM_EVENT_WAITATTACK = "WaitAttack";
		public const string ANIM_EVENT_FIRE = "Fire";
		public const string ANIM_EVENT_EJECT_CASING = "EjectCasing";

        static public List<DEActor> AllActor = new List<DEActor>();
        static private void Register(DEActor ch)
        {
            if (AllActor.Contains(ch) == false) AllActor.Add(ch);
        }

        static private void Unregister(DEActor ch)
        {
            if (AllActor.Contains(ch)) AllActor.Remove(ch);
        }
        #endregion

		#region INSPECTOR
        public Transform graphic;

        public float idleFriction = 20;
        public float movingFriction = 0;

        [Header("Bone")]
        [SpineBone(dataField: "skeletonAnimation")]
        public string footEffectBone;

        //inspector
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

        [Header("Jump")]
        [SpineAnimation]
        public string jumpAnim;
        [SpineAnimation]
        public string fallAnim;
        public int jumpMax = 3;
        
        [Header("Hit")]
        [SpineAnimation]
        public string hitAnim;
		#endregion

		#region PROPERTY
        //----------------------------------------------------------------------------------------------------------
        // input
        //----------------------------------------------------------------------------------------------------------
        public Vector2 axis { get; set; }
		public Vector2 lastAxis { get; set; }
		public bool isRun { get; set; }

        //GET,SET
        public CharacterState State { get; protected set; }
        public DEController Controller { get; private set; }
        public HealthState Health { get; set; }
        public float CurrentSpeed { get; set; }
        public int JumpCount { get; set; }
        public DTSCharacter dts { get; set; }
        public Ladder CurrentLadder { get; set; }

        //events
        public UnityAction<DEActor> OnUpdateInput;
        public UnityAction<DEActor> OnDead;

        protected Transform mTr;
        protected int mFacing = 1;

        //state
        protected List<StateTransition> mStateTransitions;
        protected UnityAction mStateLoop;
        protected UnityAction mStateExit;

        //restriction
        protected bool mCanJump;
        protected bool mCanAttack;
        protected bool mCanMove;
        protected bool mCanFacing;

		protected bool mCanEscape;
		protected bool mCanDash;

        //
        protected float mJumpStartTime;
        protected float jumpElapsedTime { get { return Time.time - mJumpStartTime; } }

        //anim
        protected SkeletonAnimation mSkeletonAnimation;
        protected SkeletonGhost mGhost;
        protected SkeletonRagdoll2D mRagdoll;

        // weapon
        protected List<Weapon> mWeaponList;
        protected Weapon mCurrentWeapon;
		#endregion

        #region Initialize
        virtual protected void Awake()
        {
            mTr = transform;
            
            Controller = GetComponent<DEController>();
            Controller.OnJustGotGrounded += OnJustGotGrounded;
            
            mGhost = GetComponentInChildren<SkeletonGhost>();
            mSkeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            mRagdoll = GetComponentInChildren<SkeletonRagdoll2D>();
            
            Health = new HealthState(100);

            mStateTransitions = new List<StateTransition>();
            mStateLoop = delegate { };
            mStateExit = delegate { };
        }

        virtual protected void Start()
        {
            mSkeletonAnimation.state.Event += HandleEvent;
            mSkeletonAnimation.state.Complete += HandleComplete;

			Weapon[] weaponArr = GetComponents<Weapon>();
			mWeaponList = new List<Weapon>(weaponArr);
			foreach (Weapon w in mWeaponList)
			{
				w.Init (this, mSkeletonAnimation );
			}

			if (mWeaponList.Count > 0) EquipWeapon (mWeaponList [0]);

            Idle();
        }

        virtual protected void OnEnable()
        {
            Register(this);
        }

        virtual protected void OnDisable()
        {
            Unregister(this);
			CurrentLadder = null;
			OnDead = null;
        }

        public void ResetJump()
        {
            JumpCount = 0;
        }

        public void Spawn(Vector3 pos)
        {
            mTr.position = pos;
            SetFacing(1);
            ResetJump();
            gameObject.SetActive(true);
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
            Controller.AddForceX (mFacing * f );
        }

        virtual protected void OnAnimVY(int i, float f, string s)
        {
            Controller.AddForceY(f);
        }

        virtual protected void OnAnimGhosting(int i, float f, string s)
        {
            GhostMode (i == 1 ? true : false, s, f);
        }

        virtual protected void OnAnimStep(int i, float f, string s)
        {
			if (Controller.State.IsGrounded) PlatformSoundPlay ();
        }

        virtual protected void OnAnimSound(int i, float f, string s)
        {
            SoundManager.Instance.PlaySFX(s, 1f, 1, transform.position);
        }

        virtual protected void OnAnimEffect(int i, float f, string s)
        {
            //print("Effect : " + s);
        }

        public void PlayAnimation(string animName, bool loop = true, int trackIndex = 0)
        {
            if (string.IsNullOrEmpty(animName)) return;

            mSkeletonAnimation.state.SetAnimation(trackIndex, animName, loop);
        }

        protected void PlayAnimation(Spine.Animation animation, bool loop = true, int trackIndex = 0)
        {
            mSkeletonAnimation.state.SetAnimation(trackIndex, animation, loop);
        }

        protected bool HasAnim(string animName)
        {
            return mSkeletonAnimation.state.Data.SkeletonData.FindAnimation(animName) == null ? false : true;
        }

        protected TrackEntry GetCurrent(int trackIndex = 0)
        {
            return mSkeletonAnimation.state.GetCurrent(trackIndex);
        }

        protected float currentAnimationDuration
        {
            get
            {
                return GetCurrent(0).animation.Duration;
            }
        }

        protected void currentAnimationTimeScale(float timeScale)
        {
            GetCurrent(0).TimeScale = timeScale;
        }

        #endregion

        #region Loop
        void Update()
        {
            if (OnUpdateInput != null) OnUpdateInput(this);

            StateUpdate();

            if (mCanFacing) UpdateFacing();
            if (mCanMove) Move();

			lastAxis = axis;
        }

        void FixedUpdate()
        {
        }

        void OnJustGotGrounded()
        {
            ResetJump();
            PlatformSoundPlay();
            PlatformEffectSpawn();
        }
        #endregion

        #region SOUND & EFFECT
        protected void SpawnAtFoot(GameObject prefab, Quaternion rotation, Vector3 scale)
        {
            if (prefab == null) return;

            Vector3 pos = mTr.position;
            if (footEffectBone != null)
            {
                Bone bone = mSkeletonAnimation.Skeleton.FindBone(footEffectBone);
                if (bone != null) pos = graphic.TransformPoint(bone.WorldX, bone.WorldY, 0f);
            }

            FXManager.Instance.SpawnFX(prefab, pos, rotation, scale);
        }

        protected void PlatformSoundPlay()
        {
            Platform platform = Controller.State.StandingPlatform;
            if (platform != null) platform.PlaySound(mTr.position, 1f, 1f);
        }

        protected void PlatformEffectSpawn()
        {
            Platform platform = Controller.State.StandingPlatform;
            if (platform != null) platform.ShowEffect(mTr.position, new Vector3(mFacing * 1f, 1f, 1f));
        }
        #endregion

        #region Special
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

        void RagdollMode(Vector2 force, float time = 0f)
        {
            if (mRagdoll == null) return;

            mRagdoll.Apply();
            mRagdoll.RootRigidbody.velocity = force * 1f;

            //			var agent = ragdoll.RootRigidbody.gameObject.AddComponent<MovingPlatformAgent>();
            //			var rootCollider = ragdoll.RootRigidbody.GetComponent<Collider2D>();
            //			agent.platformMask = platformMask;
            //			agent.castRadius = rootCollider.GetType() == typeof(CircleCollider2D) ? ((CircleCollider2D)rootCollider).radius * 8f : rootCollider.bounds.size.y;
            //			agent.useCircleMode = true;

            var rbs = mRagdoll.RootRigidbody.transform.parent.GetComponentsInChildren<Rigidbody2D>();
            foreach (var r in rbs)
            {
                r.gameObject.AddComponent<RagdollImpactEffector>();
            }

            //			remove rigidbody2d and primaryCollider;

            StartCoroutine(WaitUntilStopped());
        }

        IEnumerator WaitUntilStopped()
        {
            yield return new WaitForSeconds(0.5f);

            float t = 0;
            while (t < 0.5f)
            {
                if (mRagdoll.RootRigidbody.velocity.magnitude > 0.09f)
                    t = 0;
                else
                    t += Time.deltaTime;

                yield return null;
            }

            //StartCoroutine (RestoreRagdoll ());
        }

        IEnumerator RestoreRagdoll()
        {
            float restoreDuration = 0.5f;
            Vector3 estimatedPos = mRagdoll.EstimatedSkeletonPosition;
            Vector3 rbPosition = mRagdoll.RootRigidbody.position;

            Vector3 skeletonPoint = estimatedPos;
            RaycastHit2D hit = Physics2D.Raycast((Vector2)rbPosition, (Vector2)(estimatedPos - rbPosition), Vector3.Distance(estimatedPos, rbPosition), DruggedEngine.MASK_ALL_GROUND);
            if (hit.collider != null) skeletonPoint = hit.point;


            mRagdoll.RootRigidbody.isKinematic = true;
            mRagdoll.SetSkeletonPosition(skeletonPoint);

            yield return mRagdoll.SmoothMix(0, restoreDuration);
            mRagdoll.Remove();

            //add rigidbody2d and primaryCollider;
        }
        #endregion

        #region Controller Handle
        protected void UpdateFacing()
        {
            if (mFacing == 1 && axis.x < -0.1f) SetFacing(-1);
            else if (mFacing == -1 && axis.x > 0.1f) SetFacing(1);
        }

        protected void SetFacing(int facing)
        {
            mFacing = facing;
            Controller.Facing = mFacing;

            if (mFacing == 1) mSkeletonAnimation.Skeleton.FlipX = false;
            else mSkeletonAnimation.Skeleton.FlipX = true;
        }

        protected void Move()
        {
            float speed = CurrentSpeed * axis.x;
            Controller.Axis = axis;
            Controller.TargetVX = speed;
        }

        public bool IsPressAgainstWall
        {
            get
            {
                return false;
                /*
                if (Controller.state.CollidingSide == null) return false;

                Wall wall = Controller.state.CollidingSide.GetComponent<Wall>();

                if (wall == null) return false;
                else if (wall.slideWay == WallSlideWay.NOTHING) return false;
                else if ((wall.slideWay == WallSlideWay.LEFT || wall.slideWay == WallSlideWay.BOTH) && mFacing == Facing.RIGHT && horizontalAxis > 0f) return true;
                else if ((wall.slideWay == WallSlideWay.RIGHT || wall.slideWay == WallSlideWay.BOTH) && mFacing == Facing.LEFT && horizontalAxis < 0f) return true;
                return false;
                */
            }
        }
        #endregion

        #region STATE CONTROLL
        protected void SetState(CharacterState next)
        {
            if (State == next)
            {
                //Debug.Log("---------same state!!!!! " + state + " > " + next);
                return;
            }

            //Debug.Log (State + " > " + next);

            mStateExit();
            mStateExit = delegate { };
            mStateLoop = delegate { };

            mStateTransitions.Clear();

            State = next;
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

        virtual protected void SetRestrict(bool move, bool facing, bool jump, bool attack, bool dash, bool escape)
        {
            mCanMove = move;
            mCanFacing = facing;
            mCanJump = jump;
            mCanAttack = attack;
            mCanDash = dash;
            mCanEscape = escape;
        }
        #endregion

        #region DEFAULT BEHAVIOUR
        virtual protected void Idle()
        {
            SetState(CharacterState.IDLE);

            SetRestrict(true, true, true, true, true, true);

            PlayAnimation(idleAnim);
            CurrentSpeed = 0f;

			if( Controller.State.PlatformVelocity == Vector2.zero ) Controller.SetFriction( idleFriction );
			else Controller.SetFriction( movingFriction );

            AddTransition(TransitionGround_Fall);
            AddTransition(TransitionIdle_Move);
        }

        virtual protected void Walk()
        {
            SetState(CharacterState.WALK);

            PlayAnimation(walkAnim);
            CurrentSpeed = WalkSpeed;
            Controller.SetFriction(movingFriction);

            AddTransition(TransitionGround_Fall);
            AddTransition(TransitionWalk_IdleOrRun);
        }

        virtual protected void Run()
        {
            SetState(CharacterState.RUN);

            PlayAnimation(runAnim);
            CurrentSpeed = RunSpeed;
            Controller.SetFriction(movingFriction);

            AddTransition(TransitionGround_Fall);
            AddTransition(TransitionRun_IdleOrWalk);
        }

        virtual protected void Fall(bool useJumpCount = true)
        {
            SetState(CharacterState.FALL);
            CurrentSpeed = isRun ? RunSpeed : WalkSpeed;
            SetRestrict(true, true, true, true, true, false);

            if (useJumpCount) JumpCount++;
            PlayAnimation(fallAnim);

            AddTransition(TransitionAir_Idle);
        }

        public void Hit(HitData hitdata)
        {
            float damage = hitdata.damage;

            Health.Damaged(damage);

            if (Health.IsDead)
            {
                Dead();

                RagdollMode(hitdata.force);
            }
            else
            {
                Controller.AddForce(hitdata.force);
                PlayAnimation(hitAnim, false, 1);

                // 일정시간동안 레이어12 와 13 과의 충돌을 제거한다 (탄,적)
                //Physics2D.IgnoreLayerCollision(9, 12, true);
                //Physics2D.IgnoreLayerCollision(9, 13, true);
                //StartCoroutine(ResetLayerCollision(0.5f));

                //			if (GetComponent<Renderer>() != null)
                //			{
                //				Color flickerColor = new Color32(255, 20, 20, 255);
                //				StartCoroutine(Flicker(_initialColor, flickerColor, 0.05f));
                //			}
            }
        }

        virtual public void Dead()
        {
            /*
			SetState (CharacterState.DEAD);

			enabled = false;

			Controller.Stop ();
			Controller.enabled = false;

			PlayAnimation( deadAnim );

//			InDialogueZone = false;
//			CurrentDialogueZone = null;
			CurrentLadder = null;
            */
            if (OnDead != null) OnDead(this);
        }

        #endregion

        #region Action

        public void DoJump()
        {
            if (mCanJump == false) return;
            if (JumpCount >= jumpMax) return;

			if (JumpCount == 0) Jump();
			else AirJump();
        }

		virtual protected void Jump()
		{
			SetState(CharacterState.JUMP);
			SetRestrict(true, true, true, true, true, false);

			PlatformSoundPlay();
			PlatformEffectSpawn();

			PlayAnimation(jumpAnim);
			CurrentSpeed = isRun ? RunSpeed : WalkSpeed;

			Controller.Jump();
			mJumpStartTime = Time.time;
			JumpCount++;

			AddTransition(TransitionJump_Fall);
		}

		virtual protected void AirJump()
		{
			PlayAnimation(jumpAnim);

			Controller.Jump();
			mJumpStartTime = Time.time;
			JumpCount++;
		}

        virtual public void DoJumpBelow()
        {
            if (mCanJump == false) return;

            if (Controller.State.IsOnOneway)
            {
                PassOneway();
                return;
            }
            else if (State == CharacterState.LADDER)
            {
                Fall();
                return;
            }
            else
            {
                DoJump();
            }
        }

        protected void PassOneway()
        {
            Controller.PassOneway();
            Fall();
        }

        virtual public void DoDash()
        {
			
        }

        virtual public void DoEscape()
        {
           
        }
        #endregion

		#region WEAPON
		void EquipWeapon(Weapon weapon)
		{
			if (mCurrentWeapon == weapon) return;
			if (mCurrentWeapon != null ) mCurrentWeapon.Reset();
			mCurrentWeapon = weapon;
			mCurrentWeapon.Equip();
		}
		#endregion

        #region ATTACK
        virtual public void DoAttack()
        {
			if( mCanAttack == false ) return;
			if( mCurrentWeapon == null ) return;
			if( mCurrentWeapon.IsReady() == false ) return;
			if( mCurrentWeapon.Attack())
			{
				SetState(CharacterState.ATTACK);
				SetRestrict( false, false, false,true,false,false );
			}


            if (mWaitNextAttack)
            {
                NextAttack();
                return;
            }

            if (Controller.state.IsGrounded)
            {
                GroundAttack();
            }
            else
            {
                AirAttack();
            }

            
        }

        virtual protected void GroundAttack()
        {
            /*
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
            */
        }

        protected void NextAttack()
        {
            /*
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
            */
        }

        void WaitNextAttack()
        {
            /*
            mCanAttack = true;
            mWaitNextAttack = true;
            mWaitNextAttackEndTime = Time.time + waitAttackDuration;
            currentAnimationTimeScale(0f);
            */
        }

        void StopWaitNextAttack()
        {
            /*
            mWaitNextAttack = false;
            Idle();
            */
        }

        virtual protected void AirAttack()
        {
            /*
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
            */
        }

		protected bool TransitionAttack_Idle ()
		{
//			if (mWaitNextAttack == false) return false;
//			if (Time.time < mWaitNextAttackEndTime) return false;
//			StopWaitNextAttack ();
//			return true;
		}
        #endregion

        #region STATE TRANSITION
        protected bool TransitionIdle_Move()
        {
            if (axis.x == 0f) return false;

            if (isRun) Run();
            else Walk();
            return true;
        }

        protected bool TransitionWalk_IdleOrRun()
        {
            if (axis.x == 0f)
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

        protected bool TransitionRun_IdleOrWalk()
        {
            if (axis.x == 0f || isRun == false)
            {
                Idle();
                return true;
            }

            return false;
        }

        protected bool TransitionGround_Fall()
        {
            if (Controller.State.IsGrounded) return false;

            Fall();
            return true;
        }

        protected bool TransitionAir_Idle()
        {
            if (Controller.State.IsGrounded == false) return false;
            Idle();
            return true;
        }

        protected bool TransitionJump_Fall()
        {
            if (Controller.vy > 0) return false;
            Fall(false);
            return true;
        }
        #endregion

		#region COLLISION
		virtual protected void OnTriggerEnter2D( Collider2D other )
		{
			
		}

		virtual protected void OnTriggerExit2D( Collider2D other )
		{
			
		}
		#endregion
    }
}
