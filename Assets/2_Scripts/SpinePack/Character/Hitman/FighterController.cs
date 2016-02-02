using UnityEngine;
using System.Collections;

public class FighterController : GamePlayer
{
	public float wallJumpXSpeed = 3;

	//------------------------------------------------------------------------------
	// slide
	//------------------------------------------------------------------------------
	public float slideDuration = 0.5f;
	public float slideVelocity = 6;
	public float slideSquish = 0.6f;


	//------------------------------------------------------------------------------
	// wallSlide
	//------------------------------------------------------------------------------
	public float wallSlideSpeed = -2;
	//아무런 입력이 없이 슬라이딩이 지속될 시간
	public float wallSlideWatchdogDuration = 10f;

	//------------------------------------------------------------------------------
	// anim events
	//------------------------------------------------------------------------------
	// 펀치 공격 혹은 어퍼컷에서 XVelocity 이벤트가 발생할 경우 x축에 영향을 끼칠 속도.
	public float punchVelocity = 7;
	//어퍼컷공격 시 YVelocity 이벤트가 발생한 경우 y 축에 영향을 끼칠 속도.
	public float uppercutVelocity = 5;
	//downattack 에서 YVelocity 이벤트가 발생한 경우 y 축에 영향을 끼칠 속도.
	public float downAttackVelocity = 20;
	//공격 시 Pause 이벤트가 발생한 경우. 공격 콤보를 위한 다음 입력을 기다릴 시간.
	public float attackWatchdogDuration = 0.5f;


	//------------------------------------------------------------------------------
	// anim animation names
	//------------------------------------------------------------------------------
	[Header ("Animations")]
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

	//------------------------------------------------------------------------------
	// sound
	//------------------------------------------------------------------------------
	[Header ("Sounds")]
	public string footstepSound;
	public string landSound;
	public string jumpSound;
	public string slideSound;

	//------------------------------------------------------------------------------
	// effectPrefab
	//------------------------------------------------------------------------------
	public GameObject downAttackPrefab;

	//------------------------------------------------------------------------------
	// private
	//------------------------------------------------------------------------------

	//behavior trigger
	bool doSlide;

	//slide
	float slideStartTime;

	//콤보 검사
	bool waitingForAttackInput;
	float attackWatchdog;
	//업어택을 했냐 안했냐
	bool upAttackUsed;
	//다운어택
	bool downAttackRecovery = false;
	float downAttackRecoveryTime = 0;
	bool velocityLock;
	//다운어택중 극적인 효과를 위해 속도를 고정

	//wall slide
	float wallSlideWatchdog;
	float wallSlideStartTime;
	bool wallSlideFlip;
	bool wasWallJump;

	//slide시 적용, slide 동안 x 적용
	//jump loop, fall loop 점프 후 땅에 착지했을때 실적용
	float savedXVelocity;
	float airControlLockoutTime = 0;
	//벽점프 시 현재보다 조금 뒤로. x이동과 플립에 관여.

	override protected void Start ()
	{
		base.Start ();
	}

	override protected void HandleComplete (Spine.AnimationState state, int trackIndex, int loopCount)
	{
		var entry = state.GetCurrent (trackIndex);
		if (entry.Animation.Name == attackAnim || entry.Animation.Name == upAttackAnim)
		{
			//attack complete
			skeletonAnimation.AnimationName = idleAnim;
			SetState (ActionState.IDLE);
		}
	}

	override protected void HandleEvent (Spine.AnimationState state, int trackIndex, Spine.Event e)
	{
		var entry = state.GetCurrent (trackIndex);
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
			} else if (entry.Animation.Name == downAttackAnim)
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
						OnFootstep (transform);
					break;
				case "Sound":
					SoundPalette.PlaySound (e.String, 1f, 1, transform.position);
					break;
				case "Effect":
					switch (e.String)
					{
						case "GroundJump":
							if (groundJumpPrefab && OnGround)
								SpawnAtFoot (groundJumpPrefab, Quaternion.identity, new Vector3 (mFlipped ? -1 : 1, 1, 1));
							break;
					}
					break;
			}
		}
	}

	override protected void Update ()
	{
//		if( mIsActive == false ) return;
//		if (_state.IsDead) return;
//
//		mFsm.Update();
//		_state.Reset();
		//----------------------------------------------

		if (HandleInput != null)
			HandleInput (this);
//
//		ProcessInput();
//
//		UpdateAnim();
	}

	override protected void StateExit ()
	{

	}

	override protected void StateEnter ()
	{

	}

	override protected void StateUpdate ()
	{
		switch (state)
		{
			case ActionState.IDLE:
				if (CheckJump ())
					return;
				break;

			case ActionState.WALK:
				if (CheckJump ())
					return;
				break;

			case ActionState.RUN:
				if (CheckJump ())
					return;
				break;

			case ActionState.JUMP:
				break;

			case ActionState.FALL:
				if (CheckJump ())
					return;
				break;

			case ActionState.WALLSLIDE:
				break;

			case ActionState.SLIDE:
				break;

			case ActionState.ATTACK:
				break;

			case ActionState.DOWNATTACK:
				break;

			case ActionState.UPATTACK:
				break;
		}
	}

	bool CheckJump ()
	{
		//남은 점프 수 확인
		//아래이고 원웨이면 아래점프
		//아님 점프 
		return false;
	}



	override protected void FixedUpdate ()
	{
		// * DEController
		//현재 속도 측정( 전달받은 속도를 바탕으로 중력, 지면 마찰 등 적용 )
		//특정 지역에 있다면 ( 물, 우주, 회오리 바람 ) 적용
		//속도를 바탕으로 움직여야할 벡터 측정.
		//지난 프레임 저장, 리셋
		//충돌영역 업데이트
		//충돌검사( 아래,옆,위 ). 충돌로 인해 변경된 벡터로 속도재설정. 여기서 밟고 있는 바닥, 경사를 판단.
		//부딛히고 밀수있다면 민다.
		//지상에 막 닿은건지 아닌지를 판단.
		//떨어지고 있는지 아닌지를 판단.
		//실제 캐릭터 이동

		// * DECharacter
		//지상에 막 닿았따면 점프 수를 초기화 한다.
		//지상에 막 닿았고 밟은 플랫폼의 착지 이펙트가 있따면 생성

		//HandlePhysics();
	}
}
