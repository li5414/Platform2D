using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Spine;

namespace druggedcode.engine
{
	public delegate bool StateTransition ();

	public class NewCharacter : MonoBehaviour
	{
		#region STATIC
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

		static public List<DECharacter> All = new List<DECharacter> ();
		static private void Register (DECharacter ch)
		{
			if (All.Contains (ch) == false) All.Add (ch);
		}

		static private void Unregister (DECharacter ch)
		{
			if (All.Contains (ch)) All.Remove (ch);
		}
		#endregion

		public Transform graphic;

		[Header ("Bone")]
		[SpineBone (dataField: "skeletonAnimation")]
		public string footEffectBone;

		//inspector
		[Header ("Idle")]
		[SpineAnimation]
		public string idleAnim;

		[Header ("Walk")]
		[SpineAnimation]
		public string walkAnim;
		public float WalkSpeed = 4f;

		[Header ("Run")]
		[SpineAnimation]
		public string runAnim;
		public float RunSpeed = 10f;

		[Header ("Jump")]
		[SpineAnimation]
		public string jumpAnim;
		[SpineAnimation]
		public string fallAnim;
		public float jumpSpeed = 3f;
		public float airJumpSpeed = 2f;
		public int jumpMax = 3;

        //----------------------------------------------------------------------------------------------------------
		// input
		//----------------------------------------------------------------------------------------------------------
		public Vector2 axis{ get;set; }
		public bool isRun { get; set; }


		//GET,SET
		public CharacterState State { get; protected set; }
		public NewController Controller {get; private set;}
		public HealthState Health{ get; set; }
		public float CurrentSpeed { get; set; }
		public int JumpCount { get; set; }

        public UnityAction<NewCharacter> OnUpdateInput;
        
		protected Transform mTr;
		protected int mFacing = 1;

		//state
		protected List<StateTransition> mStateTransitions;
		protected UnityAction mStateLoop;
		protected UnityAction mStateExit;

		//restriction
		protected bool mCanJump;
		protected bool mCanAttack;
		protected bool mCanEscape;
		protected bool mCanDash;
		protected bool mCanMove;
		protected bool mCanFacing;

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

		#region Initialize
		virtual protected void Awake()
		{
			mTr = transform;
			Controller = GetComponent<NewController>();
			Controller.OnJustGotGrounded += OnJustGotGrounded;
			Health = new HealthState (100);

			mStateTransitions = new List<StateTransition>();
			mStateLoop = delegate{};
			mStateExit = delegate{};
		}

		virtual protected void Start()
		{
			//initAnim
			mGhost = GetComponentInChildren<SkeletonGhost> ();
			mSkeletonAnimation = GetComponentInChildren<SkeletonAnimation> ();
			mSkeletonAnimation.state.Event += HandleEvent;
			mSkeletonAnimation.state.Complete += HandleComplete;
			mRagdoll = GetComponentInChildren<SkeletonRagdoll2D> ();

			//initWeapon
			Weapon[] weaponArr = GetComponents<Weapon> ();
			mWeaponList = new List<Weapon> (weaponArr);

			foreach (Weapon w in mWeaponList)
			{
				//w.Init (this, mSkeletonAnimation.skeleton);
			}

			//if (mWeaponList.Count > 0) EquipWeapon (mWeaponList [0]);

			Idle();
		}
		#endregion

		#region ANIM HANDLE
		virtual protected void HandleComplete (Spine.AnimationState animState, int trackIndex, int loopCount)
		{
			var entry = animState.GetCurrent (trackIndex);
			if (entry.Animation.Name.IndexOf ("Attack") == 0)
			{
				Idle ();
			}
		}

		virtual protected void HandleEvent (Spine.AnimationState animState, int trackIndex, Spine.Event e)
		{
			switch (e.Data.name)
			{
				case ANIM_EVENT_VX:
					OnAnimVX (e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_VY:
					OnAnimVY (e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_WAITATTACK:
					OnAnimWaitAttack (e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_FIRE:
					OnFire (e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_EJECT_CASING:
					OnEjectCasing (e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_GHOSTING:
					OnAnimGhosting (e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_STEP:
					OnAnimStep (e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_SOUND:
					OnAnimSound (e.Int, e.Float, e.String);
					break;

				case ANIM_EVENT_EFFECT:
					OnAnimEffect (e.Int, e.Float, e.String);
					break;
			}
		}

		virtual protected void OnAnimVX (int i, float f, string s)
		{
			//Controller.AddForceX (mFacing == Facing.RIGHT ? f : -f);
		}

		virtual protected void OnAnimVY (int i, float f, string s)
		{
			Controller.AddForceY (f);
		}

		virtual protected void OnFire (int i, float f, string s)
		{
			//FireWeapon ();
		}

		virtual protected void OnEjectCasing (int i, float f, string s)
		{
			//EjectCasing ();
		}

		virtual protected void OnAnimWaitAttack (int i, float f, string s)
		{
			//WaitNextAttack ();
		}

		virtual protected void OnAnimGhosting (int i, float f, string s)
		{
			//GhostMode (i == 1 ? true : false, s, f);
		}

		virtual protected void OnAnimStep (int i, float f, string s)
		{
			//if (Controller.state.IsGrounded) PlatformSoundPlay ();
		}

		virtual protected void OnAnimSound (int i, float f, string s)
		{
			SoundManager.Instance.PlaySFX (s, 1f, 1, transform.position);
		}

		virtual protected void OnAnimEffect (int i, float f, string s)
		{
			//print("Effect : " + s);
		}

		public void PlayAnimation (string animName, bool loop = true, int trackIndex = 0)
		{
			if (string.IsNullOrEmpty (animName)) return;

			mSkeletonAnimation.state.SetAnimation (trackIndex, animName, loop);
		}

		protected void PlayAnimation (Spine.Animation animation, bool loop = true, int trackIndex = 0)
		{
			mSkeletonAnimation.state.SetAnimation (trackIndex, animation, loop);
		}

		protected bool HasAnim (string animName)
		{
			return mSkeletonAnimation.state.Data.SkeletonData.FindAnimation (animName) == null ? false : true;
		}

		#endregion

		#region Loop
        void Update()
        {
            if (OnUpdateInput != null) OnUpdateInput (this);

			StateUpdate ();

			if( mCanFacing ) UpdateFacing();
			if( mCanMove ) Move();
        }

		void FixedUpdate ()
		{
		}

		void OnJustGotGrounded()
		{
			ResetJump ();
			PlatformSoundPlay ();
			PlatformEffectSpawn ();
		}
		#endregion

		#region Controller Handle

		protected void UpdateFacing()
		{
			if( mFacing == 1 && axis.x < -0.1f)
			{
				mFacing = -1;
				Controller.Facing = -1;

			}else if( mFacing == -1 && axis.x > 0.1f )
			{
				mFacing = 1;
				Controller.Facing = 1;
			}

			if( mFacing == 1 ) mSkeletonAnimation.Skeleton.FlipX = false;
			else mSkeletonAnimation.Skeleton.FlipX = true;
		}

		protected void Move ()
		{
			float speed = CurrentSpeed * Mathf.Sign( axis.x );

			Controller.Axis = axis;
			Controller.SetSpeed( speed );
		}
		#endregion

		#region STATE CONTROLL
		protected void SetState (CharacterState next)
		{
			if (State == next)
			{
				//Debug.Log("---------same state!!!!! " + state + " > " + next);
				return;
			}

			//Debug.Log (State + " > " + next);

			mStateExit();
			mStateExit = delegate{};
			mStateLoop = delegate{};

			mStateTransitions.Clear ();

			State = next;
		}

		protected void StateUpdate ()
		{
			if (StateTransition () == false) mStateLoop ();
		}

		bool StateTransition ()
		{
			int count = mStateTransitions.Count;

			StateTransition transition;
			for (int i = 0; i < count; ++i)
			{
				transition = mStateTransitions [i];
				if (transition ()) return true;
			}
			return false;
		}

		protected void AddTransition (StateTransition transition)
		{
			mStateTransitions.Add (transition);
		}

		protected void RemoveTransition (StateTransition transition)
		{
			mStateTransitions.Remove (transition);
		}

		virtual protected void SetRestrict( bool move, bool facing, bool jump, bool attack, bool dash, bool escape )
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
			SetState (CharacterState.IDLE);

			SetRestrict( true,true,true,true,true,true );

			PlayAnimation (idleAnim);
			CurrentSpeed = 0f;

//			AddTransition (TransitionGround_Fall);
			AddTransition (TransitionIdle_Move);
		}

		virtual protected void Walk()
		{
			SetState (CharacterState.WALK);

			PlayAnimation (walkAnim);
			CurrentSpeed = WalkSpeed;

			//AddTransition (TransitionGround_Fall);
			AddTransition (TransitionWalk_IdleOrRun);
		}

		virtual protected void Run()
		{
			SetState (CharacterState.RUN);

			PlayAnimation (runAnim);
			CurrentSpeed = RunSpeed;

			//AddTransition (TransitionGround_Fall);
			AddTransition (TransitionRun_IdleOrWalk);
		}

		virtual protected void Fall (bool useJumpCount = true)
		{
			SetState (CharacterState.FALL);

			SetRestrict( true,true,true,true,true,false );

			if (useJumpCount) JumpCount++;
			PlayAnimation (fallAnim);

			mStateLoop += Move;

			AddTransition (TransitionAir_Idle);
		}

		#endregion

		#region Action
		virtual public void DoJump ()
		{
			if (mCanJump == false) return;
			if (JumpCount >= jumpMax) return;

			print("jump");

			SetState (CharacterState.JUMP);

			SetRestrict( true, true,true,true,true, false );

			bool wallJump = false;
			float jumpPower = 0f;
			GameObject jumpEffect = null;

			if (JumpCount == 0)
			{
				PlatformSoundPlay ();
				PlatformEffectSpawn ();

				if (State == CharacterState.WALLSLIDE)
				{
//					Controller.vx = mFacing == Facing.LEFT ? 4 : -4;
//					Controller.LockMove (0.5f);
//					wallJump = true;
				}
				else if (Controller.State.IsGrounded)
				{

				}

				PlayAnimation (jumpAnim);

				jumpPower = jumpSpeed;
				//jumpPower = Mathf.Sqrt (2f * JumpHeight * Mathf.Abs (Controller.Gravity));
				//jumpEffect = jumpEffectPrefab;
			}
			//airJump
			else
			{
				PlayAnimation (jumpAnim);

				jumpPower = airJumpSpeed;
				//jumpPower = Mathf.Sqrt (2f * JumpHeightOnAir * Mathf.Abs (Controller.Gravity));
				//jumpEffect = airJumpEffectPrefab;
			}

			CurrentSpeed = isRun ? RunSpeed : WalkSpeed;
			Controller.Vy = jumpPower + Controller.State.PlatformVelocity.y;

			print( "jumpPower: "+ jumpPower + ", vy : " + Controller.Vy );

			mJumpStartTime = Time.time;
			JumpCount++;

			if (wallJump)
			{
				SpawnAtFoot (jumpEffect, Quaternion.Euler (0, 0, mFacing * 90 ), new Vector3 (mFacing * 1f , 1f, 1f));
			}
			else
			{
				FXManager.Instance.SpawnFX (jumpEffect, mTr.position, new Vector3 ( mFacing * 1f, 1f, 1f));
			}

			AddTransition (TransitionJump_Fall);
		}

		#endregion

		#region STATE TRANSITION
		protected bool TransitionIdle_Move ()
		{
			if (axis.x == 0f) return false;

			if (isRun) Run ();
			else Walk ();
			return true;
		}

		protected bool TransitionWalk_IdleOrRun ()
		{
			if ( axis.x == 0f)
			{
				Idle ();
				return true;
			}

			if (isRun)
			{
				Run ();
				return true;
			}

			return false;
		}

		protected bool TransitionRun_IdleOrWalk ()
		{
			if (axis.x == 0f || isRun == false )
			{
				Idle();
				return true;
			}

			return false;
		}

		protected bool TransitionAir_Idle ()
		{
			if (Controller.State.IsGrounded == false) return false;
			Idle ();
			return true;
		}

		protected bool TransitionJump_Fall ()
		{
			if (Controller.Vy > 0) return false;
			Fall (false);
			return true;
		}
		#endregion

		#region ETC
		public void ResetJump ()
		{
			JumpCount = 0;
		}

		protected void SpawnAtFoot (GameObject prefab, Quaternion rotation, Vector3 scale)
		{
			if (prefab == null) return;

			Vector3 pos = mTr.position;
			if (footEffectBone != null)
			{
				Bone bone = mSkeletonAnimation.Skeleton.FindBone (footEffectBone);
				if (bone != null) pos = graphic.TransformPoint (bone.WorldX, bone.WorldY, 0f);
			}

			FXManager.Instance.SpawnFX (prefab, pos, rotation, scale);
		}

		protected void PlatformSoundPlay ()
		{
			Platform platform = Controller.State.StandingPlatform;
			if (platform != null) platform.PlaySound (mTr.position, 1f, 1f);
		}

		protected void PlatformEffectSpawn ()
		{
			Platform platform = Controller.State.StandingPlatform;
			if (platform != null) platform.ShowEffect (mTr.position, new Vector3 (mFacing * 1f, 1f, 1f));
		}
		#endregion

	}
}
