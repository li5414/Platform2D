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
	public class DECharacterOld : MonoBehaviour, IDamageable
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

		static public List<DECharacterOld> All = new List<DECharacterOld> ();

		static private void Register (DECharacterOld ch)
		{
			if (All.Contains (ch) == false) All.Add (ch);
		}

		static private void Unregister (DECharacterOld ch)
		{
			if (All.Contains (ch)) All.Remove (ch);
		}

		#endregion

		public Transform graphic;
		public AnimationType bodyType;

		[Header ("Stats")]
		public float hp;

		[Header ("Bone")]
		[SpineBone (dataField: "skeletonAnimation")]
		public string footEffectBone;

		[Header ("Move")]
		public bool smoothMovement = true;
		public float accelOnGround = 10f;
		public float accelOnAir = 3f;

		[Header ("Hit")]
		[SpineAnimation (startsWith: "Hit")]
		public string hitAnim;

		[Header ("Dead")]
		[SpineAnimation]
		public string deadAnim;

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

		[Header ("Dash")]
		[SpineAnimation]
		public string dashAnim;
		public float dashSpeed = 1f;
		public float dashDuration = 0.1f;

		[Header ("Crounch")]
		[SpineAnimation]
		public string crouchAnim;
		public float CrouchSpeed = 1f;

		[Header ("Ladder")]
		[SpineAnimation]
		public string ladderAnim;
		public float LadderSpeed = 2f;
		public float ladderClimbSpeed = 1f;

		[Header ("Escape")]
		[SpineAnimation]
		public string escapeAnim;
		public float escapeDuration = 1f;

		[Header ("Jump")]
		[SpineAnimation]
		public string jumpAnim;
		[SpineAnimation]
		public string fallAnim;
		public float JumpHeight = 3f;
		public float JumpHeightOnAir = 2f;
		public int jumpMax = 3;

		[Header ("AttackGround")]
		[SpineAnimation]
		public string attackGroundAnim;
		[SpineAnimation]
		public string attackUpAnim;
		[SpineAnimation]
		public string attackDownAnim;
		public float waitAttackDuration = 0.5f;

		[Header ("AttackAir")]
		[SpineAnimation]
		public string attackAirAnim;
		[SpineAnimation]
		public string attackAirUpAnim;
		[SpineAnimation]
		public string attackAirDownAnim;

		[Header ("Effect")]
		public GameObject jumpEffectPrefab;
		public GameObject airJumpEffectPrefab;

		//----------------------------------------------------------------------------------------------------------
		// event
		//----------------------------------------------------------------------------------------------------------
		public UnityAction<DECharacterOld> OnUpdateInput;
		public UnityAction<DECharacterOld> OnDead;

		//----------------------------------------------------------------------------------------------------------
		// public
		//----------------------------------------------------------------------------------------------------------
		public Ladder CurrentLadder { get; set; }

		public DEControllerOld Controller { get; private set; }

		public int JumpCount { get; set; }

		public CharacterState State { get; protected set; }

		public float CurrentSpeed { get; set; }

		public DTSCharacter dts { get; set; }

		public HealthState Health{ get; set; }

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

		protected float mJumpStartTime;
		protected float jumpElapsedTime { get { return Time.time - mJumpStartTime; } }

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
		protected SkeletonRagdoll2D mRagdoll;

		//----------------------------------------------------------------------------------------------------------
		// weapon
		//----------------------------------------------------------------------------------------------------------
		protected List<Weapon> mWeaponList;
		protected Weapon mCurrentWeapon;

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

		virtual protected void Awake ()
		{
			mTr = transform;
			Controller = GetComponent<DEControllerOld> ();

			Health = new HealthState (hp);

			mStateTransitions = new List<StateTransition> ();

			mStateLoop = delegate
			{
			};
			mStateExit = delegate
			{
			};
		}

		virtual protected void Start ()
		{
			switch (bodyType)
			{
				case AnimationType.SPINE:

					mGhost = GetComponentInChildren<SkeletonGhost> ();

					mSkeletonAnimation = GetComponentInChildren<SkeletonAnimation> ();
					mSkeletonAnimation.state.Event += HandleEvent;
					mSkeletonAnimation.state.Complete += HandleComplete;
					break;
			}

			mRagdoll = GetComponentInChildren<SkeletonRagdoll2D> ();

			Weapon[] weaponArr = GetComponents<Weapon> ();
			mWeaponList = new List<Weapon> (weaponArr);

			foreach (Weapon w in mWeaponList)
			{
				//w.Init (this, mSkeletonAnimation.skeleton);
			}

			if (mWeaponList.Count > 0) EquipWeapon (mWeaponList [0]);

			Idle ();
		}

		#endregion

		#region loop

		void Update ()
		{
			if (OnUpdateInput != null) OnUpdateInput (this);

			StateUpdate ();
		}

		void FixedUpdate ()
		{
			if (Controller.state.JustGotGrounded)
			{
				ResetJump ();
				PlatformSoundPlay ();
				PlatformEffectSpawn ();
			}
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

				case ANIM_EVENT_GRAVITY:
					OnAnimGravity (e.Int, e.Float, e.String);
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
			Controller.AddForceX (mFacing == Facing.RIGHT ? f : -f);
		}

		virtual protected void OnAnimVY (int i, float f, string s)
		{
			Controller.AddForceY (f);
		}

		virtual protected void OnFire (int i, float f, string s)
		{
			FireWeapon ();
		}

		virtual protected void OnEjectCasing (int i, float f, string s)
		{
			EjectCasing ();
		}

		virtual protected void OnAnimWaitAttack (int i, float f, string s)
		{
			WaitNextAttack ();
		}

		virtual protected void OnAnimGravity (int i, float f, string s)
		{
			Controller.Stop ();
			Controller.gravityScale = f;
		}

		virtual protected void OnAnimGhosting (int i, float f, string s)
		{
			GhostMode (i == 1 ? true : false, s, f);
		}

		virtual protected void OnAnimStep (int i, float f, string s)
		{
			if (Controller.state.IsGrounded) PlatformSoundPlay ();
		}

		virtual protected void OnAnimSound (int i, float f, string s)
		{
			SoundManager.Instance.PlaySFX (s, 1f, 1, transform.position);
		}

		virtual protected void OnAnimEffect (int i, float f, string s)
		{
			//print("Effect : " + s);
		}

		#endregion

		virtual protected void OnEnable ()
		{
			Register (this);
		}

		virtual protected void OnDisable ()
		{
			Unregister (this);
		}


		#region FSM

		protected void SetState (CharacterState next)
		{
			if (State == next)
			{
				//Debug.Log("---------same state!!!!! " + state + " > " + next);
				return;
			}

			//Debug.Log (state + " > " + next);

			mStateExit ();

			mStateExit = delegate
			{
			};
			mStateLoop = delegate
			{
			};
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

		virtual protected void Idle ()
		{
			SetState (CharacterState.IDLE);

			mCanEscape = true;
			mCanDash = true;
			mCanJump = true;
			mCanAttack = true;

			PlayAnimation (idleAnim);
			CurrentSpeed = 0f;

			AddTransition (TransitionGround_Fall);
			AddTransition (TransitionIdle_Move);

			mStateLoop += Move;
		}

		virtual protected void Walk ()
		{
			SetState (CharacterState.WALK);

			PlayAnimation (walkAnim);
			CurrentSpeed = WalkSpeed;

			AddTransition (TransitionGround_Fall);
			AddTransition (TransitionWalk_IdleOrRun);

			mStateLoop += Move;
		}

		virtual protected void Run ()
		{
			SetState (CharacterState.RUN);

			PlayAnimation (runAnim);
			CurrentSpeed = RunSpeed;

			AddTransition (TransitionGround_Fall);
			AddTransition (TransitionRun_IdleOrWalk);

			mStateLoop += Move;
		}

		#endregion

		#region FSM Transition

		protected bool TransitionGround_Fall ()
		{
			if (Controller.state.IsGrounded) return false;

			Fall ();
			return true;
		}

		protected bool TransitionWalk_IdleOrRun ()
		{
			if (horizontalAxis == 0f)
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

		protected bool TransitionIdle_Move ()
		{
			if (horizontalAxis == 0f) return false;

			if (isRun) Run ();
			else Walk ();
			return true;
		}

		protected bool TransitionRun_IdleOrWalk ()
		{
			if (horizontalAxis == 0f || isRun == false )
			{
				Idle();
				return true;
			}

			return false;
		}

		protected bool TransitionDash_Idle ()
		{
			float dashElapsedTime = Time.time - mDashStartTime;
			if (dashElapsedTime < dashDuration) return false;

			Idle ();
			return true;
		}

		protected bool TransitionAttack_Idle ()
		{
			if (mWaitNextAttack == false) return false;
			if (Time.time < mWaitNextAttackEndTime) return false;
			StopWaitNextAttack ();
			return true;
		}

		protected bool TransitionEscape_Idle ()
		{
			float slideElapsedTime = Time.time - mEscapeStartTime;
			if (slideElapsedTime < escapeDuration) return false;
			if (Controller.IsCollidingHead) return false;

			Idle ();
			return true;
		}

		protected bool TransitionJump_Fall ()
		{
			if (Controller.vy > 0) return false;
			Fall (false);
			return true;
		}

		protected bool TransitionAir_Idle ()
		{
			if (Controller.state.IsGrounded == false) return false;
			Idle ();
			return true;
		}

		#endregion

		#region Action

		protected void DoDash ()
		{
			if (mCanDash == false) return;
			SetState (CharacterState.DASH);

			mCanDash = false;
			mCanJump = false;
			mCanEscape = false;
			mCanAttack = true;

			mDashStartTime = Time.time;

			PlayAnimation (dashAnim);
			GravityActive (false);
			Stop ();
			Controller.vx = mFacing == Facing.RIGHT ? dashSpeed : -dashSpeed;

			AddTransition (TransitionDash_Idle);

			mStateExit += delegate
			{
				GravityActive (true);
				Stop ();
			};
		}

		protected void DoEscape ()
		{
			if (mCanEscape == false) return;

			SetState (CharacterState.ESCAPE);

			mCanDash = true;
			mCanJump = true;
			mCanEscape = false;
			mCanAttack = false;

			mEscapeStartTime = Time.time;
			PlayAnimation (escapeAnim);
			Controller.UpdateColliderSize (1f, 0.5f);
			Stop ();
			Controller.vx = mFacing == Facing.RIGHT ? RunSpeed : -RunSpeed;

			AddTransition (TransitionGround_Fall);
			AddTransition (TransitionEscape_Idle);

			mStateExit += delegate
			{
				Controller.ResetColliderSize ();
				GhostMode (false);
			};
		}

		virtual protected void DoJump ()
		{
			if (mCanJump == false) return;
			if (JumpCount >= jumpMax) return;

			SetState (CharacterState.JUMP);

			mCanDash = true;
			mCanJump = true;
			mCanEscape = false;
			mCanAttack = true;

			bool wallJump = false;
			float jumpPower;
			GameObject effect;

			//firstJump
			if (JumpCount == 0)
			{
				Controller.state.ClearPlatform ();
				PlatformSoundPlay ();
				PlatformEffectSpawn ();

				if (State == CharacterState.WALLSLIDE)
				{
					Controller.vx = mFacing == Facing.LEFT ? 4 : -4;
					Controller.LockMove (0.5f);
					wallJump = true;
				}
				else if (Controller.state.IsGrounded)
				{

				}

				PlayAnimation (jumpAnim);
				jumpPower = Mathf.Sqrt (2f * JumpHeight * Mathf.Abs (Controller.Gravity));
				effect = jumpEffectPrefab;
			}
            //airJump
            else
			{
				PlayAnimation (jumpAnim);
				jumpPower = Mathf.Sqrt (2f * JumpHeightOnAir * Mathf.Abs (Controller.Gravity));
				effect = airJumpEffectPrefab;
			}

			CurrentSpeed = isRun ? RunSpeed : WalkSpeed;
			Controller.vy = jumpPower;
			mJumpStartTime = Time.time;
			JumpCount++;

			if (wallJump)
			{
				SpawnAtFoot (effect, Quaternion.Euler (0, 0, mFacing == Facing.RIGHT ? 90 : -90), new Vector3 (mFacing == Facing.RIGHT ? 1f : -1f, 1f, 1f));
			}
			else
			{
				FXManager.Instance.SpawnFX (effect, mTr.position, new Vector3 (mFacing == Facing.RIGHT ? 1f : -1f, 1f, 1f));
			}

			AddTransition (TransitionJump_Fall);

			mStateLoop += Move;
		}

		protected void DoJumpBelow ()
		{
			if (mCanJump == false) return;

			if (Controller.state.IsOnOneway)
			{
				PassOneway ();
				return;
			}
			else if (State == CharacterState.LADDER)
			{
				Fall ();
				return;
			}
			else
			{
				DoJump ();
			}
		}

		#endregion

		protected void PassOneway ()
		{
			mTr.position = new Vector2 (mTr.position.x, mTr.position.y - 0.1f);
			Controller.state.ClearPlatform ();
			Controller.PassThroughOneway ();
			Fall ();
		}

		virtual protected void Fall (bool useJumpCount = true)
		{
			SetState (CharacterState.FALL);

			mCanDash = true;
			mCanJump = true;
			mCanEscape = false;
			mCanAttack = true;

			if (useJumpCount) JumpCount++;
			PlayAnimation (fallAnim);

			mStateLoop += Move;

			AddTransition (TransitionAir_Idle);
		}

		protected void PlatformSoundPlay ()
		{
			Platform platform = Controller.state.StandingPlatform;
			if (platform != null) platform.PlaySound (mTr.position, 1f, 1f);
		}

		protected void PlatformEffectSpawn ()
		{
			Platform platform = Controller.state.StandingPlatform;
			if (platform != null) platform.ShowEffect (mTr.position, new Vector3 (mFacing == Facing.RIGHT ? 1f : -1f, 1f, 1f));
		}

		protected void SpawnAtFoot (GameObject prefab, Quaternion rotation, Vector3 scale)
		{
			if (prefab == null) return;

			Vector3 pos = mTr.position;
			if (footEffectBone != null)
			{
				Bone bone = mSkeletonAnimation.Skeleton.FindBone (footEffectBone);
				if (bone != null) pos = graphic.transform.TransformPoint (bone.WorldX, bone.WorldY, 0f);
			}

			FXManager.Instance.SpawnFX (prefab, pos, rotation, scale);
		}

		protected void GhostMode (bool use, string str = "", float f = 0f)
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

			if (string.IsNullOrEmpty (str) == false)
			{
				mGhost.color = ColorUtil.HexToColor (str);
			}
		}

		public void Pause ()
		{
			Controller.Stop ();
			Controller.enabled = false;

			CurrentSpeed = 0f;
			horizontalAxis = 0f;
			CurrentLadder = null;
			enabled = false;

			Idle ();
		}

		public void Resume ()
		{
			Controller.enabled = true;
			Controller.CollisionsOn ();

			enabled = true;
			ResetJump ();

			gameObject.SetActive (true);
		}

		public void Spawn (Vector3 pos)
		{
			mTr.position = pos;
			SetFacing (Facing.RIGHT);

			Controller.enabled = true;
			Controller.CollisionsOn ();

			enabled = true;
			ResetJump ();

			gameObject.SetActive (true);
		}

		virtual public void Dead ()
		{
			SetState (CharacterState.DEAD);

			enabled = false;

			Controller.Stop ();
			Controller.enabled = false;

			PlayAnimation( deadAnim );

//			InDialogueZone = false;
//			CurrentDialogueZone = null;
			CurrentLadder = null;

			if (OnDead != null) OnDead (this);
		}

		protected void Move ()
		{
			if (mFacing == Facing.LEFT && horizontalAxis > 0.1f)
			{
				SetFacing (Facing.RIGHT);
			}
			else if (mFacing == Facing.RIGHT && horizontalAxis < -0.1f)
			{
				SetFacing (Facing.LEFT);
			}

			float targetVX = horizontalAxis * CurrentSpeed;
			if (targetVX != 0f && smoothMovement)
			{
				float moveFactor = Controller.state.IsGrounded ? accelOnGround : accelOnAir;
				targetVX = Mathf.Lerp (Controller.vx, targetVX, Time.deltaTime * moveFactor);
			}

			Controller.vx = targetVX;
		}

		public void Stop ()
		{
			Controller.Stop ();
		}

		public void ResetJump ()
		{
			JumpCount = 0;
		}

		#region WEAPON

		void EquipWeapon (Weapon weapon)
		{
			if (mCurrentWeapon == weapon) return;

			mCurrentWeapon = weapon;
			mCurrentWeapon.Setup ();
		}

		virtual protected void DoAttack ()
		{
			if (mCanAttack == false) return;
//			if( mCurrentWeapon == null ) return;

			SetState (CharacterState.ATTACK);

			mCanDash = false;
			mCanJump = false;
			mCanEscape = true;
			mCanAttack = false;

			if (mWaitNextAttack)
			{
				NextAttack ();
				return;
			}

			if (Controller.state.IsGrounded)
			{
				GroundAttack ();
			}
			else
			{
				AirAttack ();
			}

			mStateExit += delegate
			{
				mWaitNextAttack = false;
			};
		}

		virtual protected void GroundAttack ()
		{
			mAttackIndex = 0;
			Stop ();

			if (verticalAxis > 0.1f && string.IsNullOrEmpty (attackUpAnim) == false)
			{
				PlayAnimation (attackUpAnim);
			}
			else if (verticalAxis < -0.1f && string.IsNullOrEmpty (attackDownAnim) == false)
			{
				PlayAnimation (attackDownAnim);
			}
			else if (string.IsNullOrEmpty (attackGroundAnim) == false)
			{
				PlayAnimation (attackGroundAnim);
			}

			AddTransition (TransitionAttack_Idle);
			AddTransition (TransitionGround_Fall);
		}

		protected void NextAttack ()
		{
			mWaitNextAttack = false;
			mAttackIndex++;

			if (mAttackIndex == 3)
			{
				RemoveTransition (TransitionGround_Fall);
			}

			switch (bodyType)
			{
				case AnimationType.SPINE:
					GetCurrent (0).TimeScale = 1;
					break;
			}
		}

		void WaitNextAttack ()
		{
			mCanAttack = true;
			mWaitNextAttack = true;
			mWaitNextAttackEndTime = Time.time + waitAttackDuration;
			currentAnimationTimeScale (0f);
		}

		void StopWaitNextAttack ()
		{
			mWaitNextAttack = false;
			Idle ();
		}

		virtual protected void AirAttack ()
		{
			if (verticalAxis > 0.1f && string.IsNullOrEmpty (attackAirUpAnim) == false)
			{
				SetState (CharacterState.ATTACK);
				PlayAnimation (attackAirUpAnim);
			}
			else if (verticalAxis < -0.1f && string.IsNullOrEmpty (attackAirDownAnim) == false)
			{
				SetState (CharacterState.ATTACK);
				PlayAnimation (attackAirDownAnim);
			}
			else if (string.IsNullOrEmpty (attackAirAnim) == false)
			{
				SetState (CharacterState.ATTACK);
				PlayAnimation (attackAirAnim);
			}
		}

		virtual protected void FireWeapon ()
		{
			// currentWeapon.Fire();
			// if (this.state == ActionState.JETPACK)
			// {
			//     doRecoil = true;
			// }
		}

		virtual protected void EjectCasing ()
		{
			// Instantiate(currentWeapon.casingPrefab, currentWeapon.casingEjectPoint.position, Quaternion.LookRotation(Vector3.forward, currentWeapon.casingEjectPoint.up));
		}

		#endregion


		#region Hit

		public void Hit( float damage )
		{
			Hit( new HitData( damage, Vector2.zero ));
		}

		public void Hit (HitData hitdata)
		{
			float damage = hitdata.damage;

			Health.Damaged (damage);

			if (Health.IsDead)
			{
				Dead ();

				RagdollMode (hitdata.force);
			}
			else
			{
				Controller.AddForce (hitdata.force);
				PlayAnimation (hitAnim, false, 1);

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

		#endregion

		void RagdollMode (Vector2 force, float time = 0f)
		{
			if (mRagdoll == null) return;

			mRagdoll.Apply ();
			mRagdoll.RootRigidbody.velocity = force * 1f;

//			var agent = ragdoll.RootRigidbody.gameObject.AddComponent<MovingPlatformAgent>();
//			var rootCollider = ragdoll.RootRigidbody.GetComponent<Collider2D>();
//			agent.platformMask = platformMask;
//			agent.castRadius = rootCollider.GetType() == typeof(CircleCollider2D) ? ((CircleCollider2D)rootCollider).radius * 8f : rootCollider.bounds.size.y;
//			agent.useCircleMode = true;

			var rbs = mRagdoll.RootRigidbody.transform.parent.GetComponentsInChildren<Rigidbody2D> ();
			foreach (var r in rbs)
			{
				r.gameObject.AddComponent<RagdollImpactEffector> ();
			}

//			remove rigidbody2d and primaryCollider;

			StartCoroutine (WaitUntilStopped ());
		}

		IEnumerator WaitUntilStopped ()
		{
			yield return new WaitForSeconds (0.5f);
			
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

		IEnumerator RestoreRagdoll ()
		{
			float restoreDuration = 0.5f;
			Vector3 estimatedPos = mRagdoll.EstimatedSkeletonPosition;
			Vector3 rbPosition = mRagdoll.RootRigidbody.position;

			Vector3 skeletonPoint = estimatedPos;
			RaycastHit2D hit = Physics2D.Raycast((Vector2)rbPosition, (Vector2)(estimatedPos - rbPosition), Vector3.Distance(estimatedPos, rbPosition), DruggedEngine.MASK_ALL_GROUND );
			if (hit.collider != null) skeletonPoint = hit.point;


			mRagdoll.RootRigidbody.isKinematic = true;
			mRagdoll.SetSkeletonPosition(skeletonPoint);

			yield return mRagdoll.SmoothMix(0, restoreDuration);
			mRagdoll.Remove();

			//add rigidbody2d and primaryCollider;
		}
		#region physics

		//캐릭터의 중력을 활성화 하거나 비활성화한다.
		public void GravityActive (bool state)
		{
			if (state)
			{
				if (Controller.gravityScale == 0)
				{
					Controller.gravityScale = _originalGravity;
				}
			}
			else
			{
				if (Controller.gravityScale != 0)
				{
					_originalGravity = Controller.gravityScale;
				}
				Controller.gravityScale = 0;
			}
		}

		public void UpdatePhysicInfo (PhysicsData physicInfo)
		{
			Controller.SetPhysicsSpace (physicInfo);
		}

		public void ResetPhysicInfo ()
		{
			Controller.ResetPhysicInfo ();
		}

		#endregion

		#region body controll

		public void BodyPosition (Vector2 translate)
		{
			graphic.transform.localPosition = translate;
		}

		void SetFacing (Facing facing)
		{
			mFacing = facing;

			switch (mFacing)
			{
				case Facing.RIGHT:
					graphic.localRotation = Quaternion.Euler( 0f,0f,0f );
					break;

				case Facing.LEFT:
					graphic.localRotation = Quaternion.Euler( 0f,180f,0f );
					break;
			}
		}

		protected bool AnimFilp {
			set {
				switch (bodyType)
				{
					case AnimationType.SPINE:
						mSkeletonAnimation.Skeleton.FlipX = value;
						break;
				}
			}
		}

		#endregion

		protected TrackEntry GetCurrent (int trackIndex = 0)
		{
			return mSkeletonAnimation.state.GetCurrent (trackIndex);
		}

		protected float currentAnimationDuration {
			get {
				switch (bodyType)
				{
					case AnimationType.SPINE:
						return GetCurrent (0).animation.Duration;
				}

				return 0f;
			}
		}

		protected void currentAnimationTimeScale (float timeScale)
		{
			switch (bodyType)
			{
				case AnimationType.SPINE:
					GetCurrent (0).TimeScale = timeScale;
					break;
			}
		}

		public void PlayAnimation (string animName, bool loop = true, int trackIndex = 0)
		{
			if (string.IsNullOrEmpty (animName)) return;

			switch (bodyType)
			{
				case AnimationType.SPINE:
					mSkeletonAnimation.state.SetAnimation (trackIndex, animName, loop);
					break;
			}
		}

		protected void PlayAnimation (Spine.Animation animation, bool loop = true, int trackIndex = 0)
		{
			mSkeletonAnimation.state.SetAnimation (trackIndex, animation, loop);
		}

		protected bool HasAnim (string animName)
		{
			switch (bodyType)
			{
				case AnimationType.SPINE:
					return mSkeletonAnimation.state.Data.SkeletonData.FindAnimation (animName) == null ? false : true;
			}

			return false;
		}

		//----------------------------------------------------------------------------------------------------------
		// get;set;
		//----------------------------------------------------------------------------------------------------------
		//추후 wall 이 아니라 특정 오브젝트를 밀고 있는지를 알 수 있는 메소드로 변경하자
		public bool IsPressAgainstWall {
			get {
				if (Controller.state.CollidingSide == null) return false;

				Wall wall = Controller.state.CollidingSide.GetComponent<Wall> ();

				if (wall == null) return false;
				else if (wall.slideWay == WallSlideWay.NOTHING) return false;
				else if ((wall.slideWay == WallSlideWay.LEFT || wall.slideWay == WallSlideWay.BOTH) && mFacing == Facing.RIGHT && horizontalAxis > 0f) return true;
				else if ((wall.slideWay == WallSlideWay.RIGHT || wall.slideWay == WallSlideWay.BOTH) && mFacing == Facing.LEFT && horizontalAxis < 0f) return true;
				return false;
			}
		}


		#if UNITY_EDITOR
		void OnDrawGizmos ()
		{
			if (!Application.isPlaying) return;

			Handles.Label (mTr.position + new Vector3 (0, 1.2f, 0), State.ToString ());
		}
		#endif
	}
}


