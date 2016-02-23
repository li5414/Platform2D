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
	public class DECharacter : MonoBehaviour, IDamageable
	{
		static List<DECharacter> All = new List<DECharacter> ();

		//----------------------------------------------------------------------------------------------------------
		// Inspector
		//----------------------------------------------------------------------------------------------------------

		public Transform body;
		public AnimationType bodyType;

		[Header ("Speed")]
		public float CrouchSpeed = 1f;
		public float WalkSpeed = 4f;
		public float RunSpeed = 10f;
		public float LadderSpeed = 2f;

		[Header ("Jump")]
		public float JumpHeight = 3f;
		public float JumpHeightOnAir = 2f;
		public int jumpMax = 3;

		[Header ("Animations")]
		[SpineAnimation]
		public string idleAnim;
		[SpineAnimation]
		public string walkAnim;
		[SpineAnimation]
		public string runAnim;
		[SpineAnimation]
		public string jumpAnim;
		[SpineAnimation]
		public string fallAnim;
		[SpineAnimation]
		public string attackAnim;

		//----------------------------------------------------------------------------------------------------------
		// event
		//----------------------------------------------------------------------------------------------------------
		public UnityAction OnUpdateInput;

		//----------------------------------------------------------------------------------------------------------
		// public
		//----------------------------------------------------------------------------------------------------------
		public Ladder currentLadder{ get; set; }

		public DEController controller{ get; private set; }

		public int jumpCount{ get; set; }

		public bool controllable{ get; set; }

		public CharacterState state { get; protected set; }

		//----------------------------------------------------------------------------------------------------------
		// input
		//----------------------------------------------------------------------------------------------------------
		public float horizontalAxis { get; set; }

		public float verticalAxis { get; set; }

		public bool isRun{ get; set; }

		//----------------------------------------------------------------------------------------------------------
		// private,protected
		//----------------------------------------------------------------------------------------------------------

		protected Transform mTr;
		protected float jumpStartTime;

		protected float jumpElapsedTime{ get { return Time.time - jumpStartTime; } }

		//캐릭터의 중력을 활성화 하거나 비활성화 할때 이전 중력값을 기억하기 위한 용도
		protected float _originalGravity;
		protected Facing mFacing;

		SkeletonAnimation mSkeletonAnimation;
		protected SkeletonGhost mGhost;

		protected float mWaitNextAttackStartTime;
		protected bool mWaitNextAttack;

		//----------------------------------------------------------------------------------------------------------
		// behaviour
		//----------------------------------------------------------------------------------------------------------
		protected bool mCanJump;
		protected bool mCanMove;
		protected bool mCanFacingUpdate;
		protected bool mCanAttack;

		virtual protected void Awake ()
		{
			mTr = transform;

			controller = GetComponent<DEController> ();

		}

		virtual protected void OnEnable ()
		{
			Register ();
		}

		virtual protected void OnDisable ()
		{
			Unregister ();
		}

		void Register ()
		{
			if (All.Contains (this) == false) All.Add (this);
		}

		void Unregister ()
		{
			if (All.Contains (this)) All.Remove (this);
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
					//mSkeletonAnimation.state.Start += OnStart;
					//mSkeletonAnimation.state.End += OnEnd;
					break;
			}
		}

		virtual protected void HandleComplete (Spine.AnimationState state, int trackIndex, int loopCount)
		{
			var entry = state.GetCurrent (trackIndex);
			if (entry.Animation.Name.IndexOf ("Attack") == 0)
			{
				SetState (CharacterState.IDLE);
			}
		}

		virtual protected void HandleEvent (Spine.AnimationState state, int trackIndex, Spine.Event e)
		{
			var entry = state.GetCurrent (trackIndex);
			string name = entry.Animation.Name;
			switch (e.Data.name)
			{
				case "XVelocity":
					//punch
					//mVelocity.x = mFlipped ? -punchVelocity * e.Float : punchVelocity * e.Float;
					break;
				case "YVelocity":
					//업어택하면서 위로 올라갖.
					//mVelocity.y = uppercutVelocity * e.Float;
					break;

				case "Pause":
					mWaitNextAttackStartTime = Time.time;
					mWaitNextAttack = true;
					entry.TimeScale = 0f;

					//downattack
					//							velocityLock = e.Int == 1 ? true : false;
					break;

				case "Ghosting":
					GhostMode( e.Int == 1 ? true : false );
					break;



				case "Footstep":
					//if (OnFootstep != null) OnFootstep(transform);
					break;
				case "Sound":
					SoundPalette.PlaySound(e.String, 1f, 1, transform.position);
					break;
				case "Effect":
					switch (e.String)
					{
						case "GroundJump":
							//								if (groundJumpPrefab && controller.state.IsGround)
							//								{
							//									SpawnAtFoot(groundJumpPrefab, Quaternion.identity );
							//								}
							break;
					}
					break;
			}
		}

		protected void GhostMode (bool use)
		{
			if (mGhost == null) return;
			mGhost.ghostingEnabled = use;
		}

		protected void IgnoreCharacterCollisions (bool ignore)
		{
//			foreach (GameCharacter gc in All)
//			{
//				if (gc == this) continue;
//				gc.IgnoreCollision(controller.primaryCollider, ignore);
//			}
		}

		void IgnoreCollision (Collider2D tgCollider, bool ignore)
		{
//			Physics2D.IgnoreCollision(controller.primaryCollider, tgCollider, ignore);
		}

		public void SetState (CharacterState next)
		{
			if (state == next)
			{
				Debug.Log ("---------same state!!!!! " + state + " > " + next);
				return;
			}
			else
			{
				if (state == CharacterState.WALLSLIDE && next == CharacterState.FALL)
				{
					float a = 1f;
				}

				StateExit ();
				Debug.Log (state + " > " + next);
				state = next;
			}

			StateEnter ();
		}

		void Update ()
		{
			if (controllable && OnUpdateInput != null) OnUpdateInput ();

			StateUpdate ();

			FacingUpdate ();
			Move ();
		}

		void Move ()
		{
			if (mCanMove == false) return;
			controller.axisX = horizontalAxis;
		}

		void FacingUpdate ()
		{
			if (mCanFacingUpdate == false) return;

			if (mFacing == Facing.LEFT && horizontalAxis > 0.1f)
			{
				SetFacing (Facing.RIGHT);
			}
			else if (mFacing == Facing.RIGHT && horizontalAxis < -0.1f)
			{
				SetFacing (Facing.LEFT);
			}
		}

		virtual protected void StateExit ()
		{

		}

		virtual protected void StateUpdate ()
		{

		}

		virtual protected void StateEnter ()
		{

		}

		void LateUpdate ()
		{
			if (controller.state.JustGotGrounded)
			{
				ResetJump ();
				//SoundPalette.PlaySound(landSound, 1, 1, transform.position);
				//if (_effectAndSound.TouchTheGroundEffect != null) Instantiate(_effectAndSound.TouchTheGroundEffect, transform.position, transform.rotation);
			}
		}

		public void DeActive ()
		{
			gameObject.SetActive (false);
			//origin was player
//			if( mIsActive == false ) return;
//
//			Stop();
//			Controllable( false );
//
//			_controller.enabled = false;
//
//			mIsActive = false;
		}

		public void Active ()
		{
			gameObject.SetActive (true);
			//origin was player
//			if( mIsActive ) return;
//
//			Controllable( true );
//
//			_controller.enabled = true;
//
//			mIsActive = true;
		}

		public void Kill ()
		{
			//GravityActive(true);
			//_state.TriggerDead = true;
			//todo. respawn after dead motion.
		}

		public void Spawn ( Vector3 pos )
		{
			mTr.position = pos;

			controller.CollisionsOn ();
			SetFacing (Facing.RIGHT);
			ResetJump ();

			/* reset state
            InDialogueZone = false;
            CurrentDialogueZone = null;
            */

			gameObject.SetActive (true);
		}

		public void Stop ()
		{
			controller.Stop ();
		}

		public void Attack ()
		{
			if( mCanAttack == false ) return;

			if( state != CharacterState.ATTACK )
			{
				SetState( CharacterState.ATTACK );

				mCanMove = false;
				mCanFacingUpdate = false;
				mWaitNextAttack = false;
				Stop();
			}

			if( controller.state.IsGrounded == false )
			{
				AirAttack();
			}
			else
			{
				GroundAttack();
			}

		}

		virtual protected void GroundAttack()
		{
			PlayAnimation( attackAnim );
		}

		virtual protected void AirAttack()
		{

		}

		protected void NextAttack ()
		{
			mWaitNextAttack = false;
			switch (bodyType)
			{
				case AnimationType.SPINE:
					mSkeletonAnimation.state.GetCurrent (0).TimeScale = 1;
					break;
			}

		}


		public void ResetJump ()
		{
			jumpCount = 0;
		}

		//추후 wall 이 아니라 특정 오브젝트를 밀고 있는지를 알 수 있는 메소드로 변경하자
		public bool IsPressAgainstWall {
			get {
				if (controller.state.CollidingSide == null) return false;

				Wall wall = controller.state.CollidingSide.GetComponent<Wall> ();

				if (wall == null) return false;
				else if (wall.slideWay == WallSlideWay.NOTHING) return false;
				else if ((wall.slideWay == WallSlideWay.LEFT || wall.slideWay == WallSlideWay.BOTH) && mFacing == Facing.RIGHT && horizontalAxis > 0f) return true;
				else if ((wall.slideWay == WallSlideWay.RIGHT || wall.slideWay == WallSlideWay.BOTH) && mFacing == Facing.LEFT && horizontalAxis < 0f) return true;
				return false;
			}
		}

		bool IsAblePassOneWay ()
		{
			if (verticalAxis >= 0f) return false;
			if (controller.state.IsOnOneway == false) return false;

			return true;
		}

		virtual public void Jump ()
		{
			if (mCanJump == false) return;
			if (jumpCount == jumpMax) return;

			mCanMove = true;
			mCanFacingUpdate = true;

			var wasWall = false;

//			else if( _characterState.IsLadderClimb == true )
//			{
//				_character.DoJumpBelow();
//				SetState(PlayerState.FALL);
//			}

			//아래 점프 체크도 한다
			if (IsAblePassOneWay ())
			{
				mTr.position = new Vector2 (mTr.position.x, mTr.position.y - 0.1f);
				controller.state.ClearPlatform ();
				controller.PassThroughOneway ();
				return;
			}
			//사다리타는 상황이고 아래를 눌렀다면
			else if (state == CharacterState.LADDER && verticalAxis < -0.1f)
			{
				Fall ();
				return;
			}
			else if (state == CharacterState.WALLSLIDE)
			{
				wasWall = true;
				controller.vx = mFacing == Facing.LEFT ? 4 : -4;
				controller.LockMove (0.5f);
			}

			float jumpPower;
			if (jumpCount == 0)
			{
				controller.state.ClearPlatform ();
				GravityActive (true);
				PlayAnimation (jumpAnim);

				jumpPower = Mathf.Sqrt (2f * JumpHeight * Mathf.Abs (controller.gravity));//min 3 , max 100000

				//			if (groundJumpPrefab != null)
				//			{
				//				if (wasWallJump) SpawnAtFoot(groundJumpPrefab, Quaternion.Euler(0, 0, controller.vx >= 0 ? -90 : 90));
				//				else SpawnAtFoot(groundJumpPrefab, Quaternion.identity );
				//			}

//				if (wasWallJump) SpawnAtFoot(groundJumpPrefab, Quaternion.Euler(0, 0, controller.vx >= 0 ? -90 : 90));
//				else SpawnAtFoot(groundJumpPrefab, Quaternion.identity );
			}
			else
			{
				PlayAnimation (jumpAnim);
				jumpPower = Mathf.Sqrt (2f * JumpHeightOnAir * Mathf.Abs (controller.gravity));

				//if (airJumpPrefab != null) Instantiate(airJumpPrefab, transform.position, Quaternion.identity);
			}

			controller.vy = jumpPower;
			jumpStartTime = Time.time;


//			SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
			//if (_effectAndSound.JumpSfx != null) SoundManager.Instance.PlaySound(_effectAndSound.JumpSfx, transform.position);

			jumpCount++;

			SetState (CharacterState.JUMP);
		}

		protected void DoJumpBelow ()
		{
			//if can't try jump
			Fall ();
		}

		//ActionState.FALL은 직접적으로 호출 하지 말도록 하자. 점프 후 떨어지는 것과 지면에서 갑자기 떨어지는 것은 차이가 있따.
		//이 차이는 점프 수를 소비하냐 아니냐의 차이이다.
		//가령 이미 점프 중이였다면 fall 상태로 가면서 점프 수는 변동이 되지 않지만 갑자기 지면에서 떨어진 경우 점프를 소비 시켜야 한다.
		protected void Fall (bool useJump = true)
		{
			if (useJump) jumpCount++;

			GravityActive (true);
			PlayAnimation (fallAnim);
			SetState (CharacterState.FALL);
		}

		//----------------------------------------------------------------------------------------------------------
		// body controll
		//----------------------------------------------------------------------------------------------------------

		public void BodyPosition (Vector2 translate)
		{
			body.transform.localPosition = translate;
		}


		//----------------------------------------------------------------------------------------------------------
		// ientface
		//----------------------------------------------------------------------------------------------------------

		public void TakeDamage (int damage, GameObject attacker)
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
		// state
		//----------------------------------------------------------------------------------------------------------

		//캐릭터의 중력을 활성화 하거나 비활성화한다.
		public void GravityActive (bool state)
		{
			if (state)
			{
				if (controller.GravityScale == 0)
				{
					controller.GravityScale = _originalGravity;
				}
			}
			else
			{
				if (controller.GravityScale != 0)
				{
					_originalGravity = controller.GravityScale;
				}
				controller.GravityScale = 0;
			}
		}

		public void UpdatePhysicInfo (PhysicInfo physicInfo)
		{
			controller.SetPhysicsSpace (physicInfo);
		}

		public void ResetPhysicInfo ()
		{
			controller.ResetPhysicInfo ();
		}

		//----------------------------------------------------------------------------------------------------------
		// bodyHandle
		//----------------------------------------------------------------------------------------------------------

		void SetFacing (Facing facing)
		{
			mFacing = facing;

			switch (mFacing)
			{
				case Facing.RIGHT:
					body.localScale = new Vector3 (1f, body.localScale.y, body.localScale.z);
					break;

				case Facing.LEFT:
					body.localScale = new Vector3 (-1f, body.localScale.y, body.localScale.z);
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


		protected float currentAnimationDuration {
			get {
				switch (bodyType)
				{
					case AnimationType.SPINE:
						return mSkeletonAnimation.state.GetCurrent (0).animation.Duration;
						break;
				}

				return 0f;
			}
		}

		protected void currentAnimationTimeScale (float timeScale)
		{
			switch (bodyType)
			{
				case AnimationType.SPINE:
					mSkeletonAnimation.state.GetCurrent (0).TimeScale = timeScale;
					break;
			}
		}

		protected void PlayAnimation (string animName, bool loop = true, int trackIndex = 0)
		{
			switch (bodyType)
			{
				case AnimationType.SPINE:
					mSkeletonAnimation.state.SetAnimation (trackIndex, animName, loop);
					break;
			}
		}

		protected bool HasAnim (string animName)
		{
			switch (bodyType)
			{
				case AnimationType.SPINE:
					return mSkeletonAnimation.state.Data.SkeletonData.FindAnimation (animName) == null ? false : true;
					break;
			}

			return false;
		}


		#if UNITY_EDITOR
		void OnDrawGizmos ()
		{
			if (!Application.isPlaying) return;

			Handles.Label (mTr.position + new Vector3 (0, 1.2f, 0), state.ToString ());
		}
		#endif
	}
}


