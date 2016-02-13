using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Spine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace druggedcode.engine
{
	public class DECharacter : MonoBehaviour, IDamageable
	{
		public enum Facing
		{
			RIGHT,
			LEFT
		}

		//----------------------------------------------------------------------------------------------------------
		// Inspector
		//----------------------------------------------------------------------------------------------------------

		public Transform body;
		public AnimationType bodyType;
		public float AccelOnGround = 10f;
		public float AccelOnAir = 3f;

		[Header("Speed")]
		public float CrouchSpeed = 1f;
		public float WalkSpeed = 4f;
		public float RunSpeed = 10f;
		public float LadderSpeed = 2f;

		[Header("Jump")]
		public float JumpHeight = 3f;
		public float JumpHeightOnAir = 2f;
		public int jumpMax = 3;

		[Header("Animations")]
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
		public UnityAction<DECharacter> OnUpdateInput;

		//----------------------------------------------------------------------------------------------------------
		// public
		//----------------------------------------------------------------------------------------------------------
		public Ladder currentLadder{ get;set; }
		public DEController controller{get; private set;}
		public int jumpCount{get;set;}
		public float horizontalAxis { get; set;}
		public float verticalAxis { get; set; }
		public float currentVX { get; set; }
		public bool controllable{get;set;}
		public InputData input{get;set;}
		public ActionState state { get; protected set; }

		//----------------------------------------------------------------------------------------------------------
		// private,protected
		//----------------------------------------------------------------------------------------------------------

		Transform mTr;
		protected int mJumpCount;
		protected float jumStartTime;
		protected float jumpElapsedTime{ get{ return Time.time - jumStartTime; }} 

		//캐릭터의 중력을 활성화 하거나 비활성화 할때 이전 중력값을 기억하기 위한 용도
		protected float _originalGravity;
		protected Facing mFacaing;

		SkeletonAnimation mSkeletonAnimation;
		protected SkeletonGhost mGhost;

		bool mMoveLocked;

		virtual protected void Awake()
		{
			mTr = transform;

			controller = GetComponent<DEController>();

		}

		virtual protected void Start()
		{
			switch (bodyType)
			{
				case AnimationType.SPINE:
					mSkeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
					mSkeletonAnimation.state.Event += HandleEvent;
					mSkeletonAnimation.state.Complete += HandleComplete;
					//mSkeletonAnimation.state.Start += OnStart;
					//mSkeletonAnimation.state.End += OnEnd;
					break;
			}
		}

		virtual protected void HandleEvent( Spine.AnimationState state, int trackIndex, Spine.Event e )
		{

		}

		virtual protected void HandleComplete( Spine.AnimationState state, int trackIndex, int loopCount )
		{

		}

		public void SetState(ActionState next)
		{
			if (state == next)
			{
				Debug.Log("---------same state!!!!! " + state + " > " + next);
				//return;
			}
			else
			{
				StateExit();
				Debug.Log(state + " > " + next);
				state = next;
			}

			StateEnter();
		}

		void Update()
		{
			if( controllable && OnUpdateInput != null ) OnUpdateInput( this );

			StateUpdate();
		}

		virtual protected void StateExit()
		{

		}

		virtual protected void StateUpdate()
		{

		}

		virtual protected void StateEnter()
		{

		}

		void LateUpdate()
		{
			if( controller.state.JustGotGrounded )
			{
				ResetJump();
				//if (_effectAndSound.TouchTheGroundEffect != null) Instantiate(_effectAndSound.TouchTheGroundEffect, transform.position, transform.rotation);
			}
		}

		protected void Move()
		{
			if( mMoveLocked ) return;

			if ( mFacaing == Facing.LEFT && input.axisX > 0.1f)
			{
				SetFacing(Facing.RIGHT);
			}
			else if (mFacaing == Facing.RIGHT && input.axisX < -0.1f)
			{
				SetFacing(Facing.LEFT);
			}

			if ( horizontalAxis == 0)
			{
				controller.vx = 0f;
			}
			else
			{
				float vx = horizontalAxis * currentVX;
				var movementFactor = controller.state.IsGrounded ? AccelOnGround : AccelOnAir;
				controller.vx = Mathf.Lerp(controller.vx, vx, Time.deltaTime * movementFactor);
				// _controller.vx = vx;
			}

			//spine
//			controller.axisX = input.axisX;
//
//			if (mFacaing == Facing.LEFT && controller.axisX > 0.1f)
//			{
//				SetFacing(Facing.RIGHT);
//			}
//			else if (mFacaing == Facing.RIGHT && controller.axisX < -0.1f)
//			{
//				SetFacing(Facing.LEFT);
//			}
		}

		public void DeActive()
		{
			gameObject.SetActive(false);
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

		public void Active()
		{
			gameObject.SetActive(true);
			//origin was player
//			if( mIsActive ) return;
//
//			Controllable( true );
//
//			_controller.enabled = true;
//
//			mIsActive = true;
		}

		public void ResetJump()
		{
			mJumpCount = 0;
		}

		public void Kill()
		{
			//GravityActive(true);
			//_state.TriggerDead = true;
			//todo. respawn after dead motion.
		}

		public void Spawn( CheckPoint cp )
		{
			mTr.position = cp.transform.position;

			controller.CollisionsOn();
			SetFacing(Facing.RIGHT);
			ResetJump();

			/* reset state
            InDialogueZone = false;
            CurrentDialogueZone = null;
            */

			gameObject.SetActive(true);
		}

		public void Stop()
		{
			controller.Stop();
		}

		//----------------------------------------------------------------------------------------------------------
		// ientface
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
		// state
		//----------------------------------------------------------------------------------------------------------

		//캐릭터의 중력을 활성화 하거나 비활성화한다.
		public void GravityActive(bool state)
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

		public void UpdatePhysicInfo(PhysicInfo physicInfo)
		{
			controller.SetPhysicsSpace(physicInfo);
		}

		public void ResetPhysicInfo()
		{
			controller.ResetPhysicInfo();
		}

		//----------------------------------------------------------------------------------------------------------
		// bodyHandle
		//----------------------------------------------------------------------------------------------------------

		void SetFacing(Facing facing)
		{
			mFacaing = facing;

			switch (mFacaing)
			{
				case Facing.RIGHT:
					body.localScale = new Vector3(1f, body.localScale.y, body.localScale.z);
					break;

				case Facing.LEFT:
					body.localScale = new Vector3(-1f, body.localScale.y, body.localScale.z);
					break;
			}
		}

		public bool AnimFilp
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


		public float currentAnimationDuration
		{
			get
			{
				switch (bodyType)
				{
					case AnimationType.SPINE:
						return mSkeletonAnimation.state.GetCurrent(0).animation.Duration;
						break;
				}

				return 0f;
			}
		}


		protected void PlayAnimation(string animName, bool loop = true, int trackIndex = 0 )
		{
			switch (bodyType)
			{
				case AnimationType.SPINE:
					mSkeletonAnimation.state.SetAnimation( trackIndex, animName, loop );
					break;
			}
		}

		protected bool HasAnim( string animName )
		{
			switch (bodyType)
			{
				case AnimationType.SPINE:
					return mSkeletonAnimation.state.Data.SkeletonData.FindAnimation(animName) == null ? false : true;
					break;
			}

			return false;
		}

		protected void NextAttack()
		{
			mSkeletonAnimation.state.GetCurrent(0).TimeScale = 1;
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


