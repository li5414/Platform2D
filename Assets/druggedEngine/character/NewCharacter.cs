﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
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

        //----------------------------------------------------------------------------------------------------------
		// input
		//----------------------------------------------------------------------------------------------------------
		public Vector2 axis{ get;set; }
		public bool isRun { get; set; }

		public CharacterState State { get; protected set; }
		public NewController Controller {get; private set;}
		public HealthState Health{ get; set; }
		public float CurrentSpeed { get; set; }

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
			if (Controller.State.JustGotGrounded)
			{
//				ResetJump ();
//				PlatformSoundPlay ();
//				PlatformEffectSpawn ();
			}
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

			//Debug.Log (state + " > " + next);

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
		#endregion

		#region DEFAULT BEHAVIOUR
		virtual protected void Idle()
		{
			SetState (CharacterState.IDLE);

			mCanEscape = true;
			mCanDash = true;
			mCanJump = true;
			mCanAttack = true;
			mCanMove = true;
			mCanFacing = true;

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
			SetState (CharacterState.ESCAPE);

			mCanDash = true;
			mCanJump = true;
			mCanEscape = false;
			mCanAttack = true;
			mCanMove = true;
			mCanFacing = true;

			if (useJumpCount) JumpCount++;
			PlayAnimation (fallAnim);

			mStateLoop += Move;

			AddTransition (TransitionAir_Idle);
		}

		#endregion

		#region STATE TRANSITION
		protected bool TransitionIdle_Move ()
		{
			print( axis.x );

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
		#endregion

	}
}
