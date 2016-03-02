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
	public enum CharacterState
	{
		NULL,
		IDLE,
		WALK,
		RUN,
		DASH,
		ESCAPE,
		CROUCH,
		LADDER,
		LOOKUP,
		JUMP,
		FALL,
		WALLSLIDE,
		JETPACK,
		ATTACK_GROUND,
		ATTACK_AIR,
		DEAD
	}

	public class DECharacter : MonoBehaviour, IDamageable
	{
		const string ANIM_EVENT_VX = "VX";
		const string ANIM_EVENT_VY = "VY";
		const string ANIM_EVENT_WAITATTACK = "WaitAttack";
		const string ANIM_EVENT_GRAVITY = "Gravity";
		const string ANIM_EVENT_GHOSTING = "Ghosting";
		const string ANIM_EVENT_STEP = "Step";
		const string ANIM_EVENT_SOUND = "Sound";
		const string ANIM_EVENT_EFFECT = "Effect";

		public static List<DECharacter> All = new List<DECharacter> ();

		//----------------------------------------------------------------------------------------------------------
		// Inspector
		//----------------------------------------------------------------------------------------------------------

		public Transform body;
		public AnimationType bodyType;

		[SpineBone (dataField: "skeletonAnimation")]
		public string footEffectBone;

		[Header ("Move")]
		public bool smoothMovement = true;
		public float accelOnGround = 10f;
		public float accelOnAir = 3f;

		[Header ("Speed")]
		public float CrouchSpeed = 1f;
		public float WalkSpeed = 4f;
		public float RunSpeed = 10f;
		public float LadderSpeed = 2f;

		[Header ("Jump")]
		public float JumpHeight = 3f;
		public float JumpHeightOnAir = 2f;
		public int jumpMax = 3;

		[Header ("Attack")]
		public float waitAttackDuration = 0.5f;

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
		public string groundAttackAnim;
		[SpineAnimation]
		public string airAttackAnim;

		[Header ("Effect")]
		public GameObject jumpEffectPrefab;
		public GameObject airJumpEffectPrefab;


		//----------------------------------------------------------------------------------------------------------
		// event
		//----------------------------------------------------------------------------------------------------------
		public UnityAction<DECharacter> OnUpdateInput;
		public UnityAction<DECharacter> OnFootStep;

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

		protected float mWaitNextAttackStartTime;
		protected bool mWaitNextAttack;


		//캐릭터의 중력을 활성화 하거나 비활성화 할때 이전 중력값을 기억하기 위한 용도
		protected float _originalGravity;
		protected Facing mFacing;

		SkeletonAnimation mSkeletonAnimation;
		protected SkeletonGhost mGhost;

		//----------------------------------------------------------------------------------------------------------
		// behaviour
		//----------------------------------------------------------------------------------------------------------
		protected bool mCanJump;
		protected bool mCanMove;
		protected bool mCanFacingUpdate;
		protected bool mCanAttack;
		protected bool mCanEscape;
		protected bool mCanDash;


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
					break;
			}
		}

		virtual protected void HandleComplete (Spine.AnimationState animState, int trackIndex, int loopCount)
		{
			var entry = animState.GetCurrent (trackIndex);
			if (entry.Animation.Name.IndexOf ("Attack") == 0)
			{
				SetState (CharacterState.IDLE);
			}
		}

		virtual protected void HandleEvent (Spine.AnimationState animState, int trackIndex, Spine.Event e)
		{
			switch (e.Data.name)
			{
				case ANIM_EVENT_VX:
					controller.AddForceX (mFacing == Facing.RIGHT ? e.Float : -e.Float);
					break;

				case ANIM_EVENT_VY:
					controller.AddForceY (e.Float);
					break;

				case ANIM_EVENT_WAITATTACK:
					WaitNextAttack ();
					break;

				case ANIM_EVENT_GRAVITY:
					controller.Stop ();
					controller.gravityScale = e.Float;
					break;

				case ANIM_EVENT_GHOSTING:
					GhostMode (e.Int == 1 ? true : false, e.String, e.Float);
					break;

				case ANIM_EVENT_STEP:
					AnimStep ();
					break;

				case ANIM_EVENT_SOUND:
					SoundManager.Instance.PlaySound (e.String, 1f, 1, transform.position);
					break;

				case ANIM_EVENT_EFFECT:
					AnimEffect (e.String, e.Float, e.Int);
					break;
			}
		}

		virtual protected void AnimEffect (string str, float f, int i)
		{
			print ("Effect : " + str);
		}

		virtual protected void AnimStep ()
		{
			if (controller.state.IsGrounded)
			{
				PlatformSoundPlay ();
			}

			if (OnFootStep != null) OnFootStep (this);
		}

		protected void PlatformSoundPlay()
		{
			Platform platform = controller.state.StandingPlatform;
			if (platform != null) platform.PlaySound( mTr.position,1f,1f );
		}

		protected void PlatformEffectSpawn()
		{
			Platform platform = controller.state.StandingPlatform;
			if( platform != null ) platform.ShowEffect( mTr.position, new Vector3( mFacing == Facing.RIGHT ? 1f : -1f ,1f,1f)  );
		}

		protected void SpawnAtFoot (GameObject prefab, Quaternion rotation, Vector3 scale)
		{
			if( prefab == null ) return;

			Vector3 pos = mTr.position;
			if (footEffectBone != null)
			{
				Bone bone = mSkeletonAnimation.Skeleton.FindBone (footEffectBone);
				if (bone != null) pos = body.transform.TransformPoint( bone.WorldX, bone.WorldY, 0f);
			}

			FXManager.Instance.SpawnFX( prefab, pos, rotation, scale );
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

		public void DeActive ()
		{
			controller.Stop ();
			controller.enabled = false;

			CurrentSpeed = 0f;
			horizontalAxis = 0f;
			currentLadder = null;
			enabled = false;

			/* reset state
            InDialogueZone = false;
            CurrentDialogueZone = null;
            */
		}

		public void Active ()
		{
			controller.enabled = true;
			controller.CollisionsOn ();

			enabled = true;
			ResetJump ();

			gameObject.SetActive (true);
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

		void Update ()
		{
			if (OnUpdateInput != null) OnUpdateInput (this);

			TimesUpdate ();
			StateUpdate ();
			FacingUpdate ();
			Move ();
		}

		void LateUpdate ()
		{
			if (controller.state.JustGotGrounded)
			{
				ResetJump ();
				PlatformSoundPlay();
				PlatformEffectSpawn();
			}
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

		void Move ()
		{
			if (mCanMove == false) return;

			float targetVX = horizontalAxis * CurrentSpeed;
			if (targetVX != 0f && smoothMovement)
			{
				float moveFactor = controller.state.IsGrounded ? accelOnGround : accelOnAir;
				targetVX = Mathf.Lerp (controller.vx, targetVX, Time.deltaTime * moveFactor);
			}

			controller.vx = targetVX;
		}

		public void Stop ()
		{
			controller.Stop ();
		}

		void TimesUpdate ()
		{
			if (mWaitNextAttack)
			{
				if (Time.time - mWaitNextAttackStartTime > waitAttackDuration) StopWaitNextAttack ();
			}
		}

		public void SetState (CharacterState next)
		{
			if (state == next)
			{
				//Debug.Log("---------same state!!!!! " + state + " > " + next);
				return;
			}
			else
			{
				StateExit ();
				//Debug.Log (state + " > " + next);
				state = next;
			}

			StateEnter ();
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

		public void Kill ()
		{
			//GravityActive(true);
			//_state.TriggerDead = true;
			//todo. respawn after dead motion.
		}

		public void Spawn (Vector3 pos)
		{
			mTr.position = pos;
			SetFacing (Facing.RIGHT);

			Active ();
		}

		public void Escape ()
		{
			if (mCanEscape == false) return;

			SetState (CharacterState.ESCAPE);
		}

		public void Dash ()
		{
			if (mCanDash == false) return;

			mCanMove = false;
			mCanDash = false;
			mCanFacingUpdate = false;
			SetState (CharacterState.DASH);

			mDashStartTime = Time.time;
		}

		public void Attack ()
		{
			if (mCanAttack == false) return;

			mCanMove = false;
			mCanFacingUpdate = false;
			mCanAttack = false;

			Stop ();

			if (mWaitNextAttack)
			{
				NextAttack ();
			}
			else if (controller.state.IsGrounded)
			{
				SetState (CharacterState.ATTACK_GROUND);
				GroundAttack ();
			}
			else
			{
				SetState (CharacterState.ATTACK_AIR);
				AirAttack ();
			}
		}

		virtual protected void GroundAttack ()
		{
			PlayAnimation (groundAttackAnim);
		}

		virtual protected void AirAttack ()
		{
			PlayAnimation (airAttackAnim);
		}

		void WaitNextAttack ()
		{
			if (state == CharacterState.ATTACK_GROUND == false) return;

			mCanAttack = true;
			mWaitNextAttack = true;
			mWaitNextAttackStartTime = Time.time;
			currentAnimationTimeScale (0f);
		}

		void StopWaitNextAttack ()
		{
			mWaitNextAttack = false;
			SetState (CharacterState.IDLE);
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
			mCanEscape = false;

			bool wallJump = false;

			Platform platform = controller.state.StandingPlatform;

			GameObject effect = jumpEffectPrefab;

			//아래 점프 체크도 한다
			if (IsAblePassOneWay ())
			{
				PassOneway ();
				return;
			}
            //사다리타는 상황이고 아래를 눌렀다면
            else if (state == CharacterState.LADDER && verticalAxis < -0.1f)
			{
				Fall ();
				return;
			}
            //벽을 타고 있다면
            else if (state == CharacterState.WALLSLIDE)
			{
				controller.vx = mFacing == Facing.LEFT ? 4 : -4;
				controller.LockMove (0.5f);

				wallJump = true;
			}

			CurrentSpeed = isRun ? RunSpeed : WalkSpeed;

			float jumpPower;
			if (jumpCount == 0)
			{
				GravityActive (true);
				PlayAnimation (jumpAnim);

				PlatformSoundPlay();
				PlatformEffectSpawn();
				controller.state.ClearPlatform ();
				jumpPower = Mathf.Sqrt (2f * JumpHeight * Mathf.Abs (controller.Gravity));
			}
			else
			{
				PlayAnimation (jumpAnim);
				jumpPower = Mathf.Sqrt (2f * JumpHeightOnAir * Mathf.Abs (controller.Gravity));

				effect = airJumpEffectPrefab;
			}

			controller.vy = jumpPower;
			jumpStartTime = Time.time;
			jumpCount++;

			if( wallJump )
			{
				SpawnAtFoot(effect, Quaternion.Euler(0, 0, mFacing == Facing.RIGHT ? 90 : -90), new Vector3( mFacing == Facing.RIGHT ? 1f : -1f ,1f,1f) );
			}
			else
			{
				FXManager.Instance.SpawnFX( effect,mTr.position,new Vector3( mFacing == Facing.RIGHT ? 1f : -1f ,1f,1f));
			}

			SetState (CharacterState.JUMP);
		}

		protected void PassOneway ()
		{
			mTr.position = new Vector2 (mTr.position.x, mTr.position.y - 0.1f);
			controller.state.ClearPlatform ();
			controller.PassThroughOneway ();
			Fall ();
		}

		//ActionState.FALL은 직접적으로 호출 하지 말도록 하자. 점프 후 떨어지는 것과 지면에서 갑자기 떨어지는 것은 차이가 있다.
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
		// interface
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
		// physics
		//----------------------------------------------------------------------------------------------------------

		//캐릭터의 중력을 활성화 하거나 비활성화한다.
		public void GravityActive (bool state)
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

		public void UpdatePhysicInfo (PhysicInfo physicInfo)
		{
			controller.SetPhysicsSpace (physicInfo);
		}

		public void ResetPhysicInfo ()
		{
			controller.ResetPhysicInfo ();
		}

		//----------------------------------------------------------------------------------------------------------
		// body Controll
		//----------------------------------------------------------------------------------------------------------

		public void BodyPosition (Vector2 translate)
		{
			body.transform.localPosition = translate;
		}

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
			}

			return false;
		}


		//----------------------------------------------------------------------------------------------------------
		// get;set;
		//----------------------------------------------------------------------------------------------------------
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


		#if UNITY_EDITOR
		void OnDrawGizmos ()
		{
			if (!Application.isPlaying) return;

			Handles.Label (mTr.position + new Vector3 (0, 1.2f, 0), state.ToString ());
		}
		#endif
	}
}


