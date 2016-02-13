using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	public class Hitman : DEPlayer
	{
		[Header ("< Hitman >")]
		[Header ("SpecialAnimations")]
		[SpineAnimation]
		public string balanceBackward;
		[SpineAnimation]
		public string balanceForward;
		[SpineAnimation]
		public string wallSlideAnim;
		[SpineAnimation]
		public string slideAnim;
		[SpineAnimation]
		public string downAttackAnim;
		[SpineAnimation]
		public string upAttackAnim;
		//this animation is used strictly to turn off all attacking bounding box attachment based hitboxes
		[SpineAnimation]
		public string clearAttackAnim;


		//입력하지 않고 계산으로 알수있지 않을까
		public float downAttackFrameSkip;

		override protected void Start()
		{
			base.Start();

			controllable = true;
			SetState(ActionState.IDLE );
		}

		override protected void HandleComplete (Spine.AnimationState state, int trackIndex, int loopCount)
		{
//			var entry = state.GetCurrent(trackIndex);
//			if (entry.Animation.Name == attackAnim || entry.Animation.Name == upAttackAnim)
//			{
//				SetState(ActionState.IDLE);
//			}
		}

		override protected void HandleEvent (Spine.AnimationState state, int trackIndex, Spine.Event e)
		{
//			var entry = state.GetCurrent(trackIndex);
//			if (entry != null)
//			{
//				if (entry.Animation.Name == attackAnim || entry.Animation.Name == upAttackAnim)
//				{
//					switch (e.Data.Name)
//					{
//						case "XVelocity":
//							//mVelocity.x = mFlipped ? -punchVelocity * e.Float : punchVelocity * e.Float;
//							//공격하면서 앞으로 전진하자 .
//							break;
//						case "YVelocity":
//							//업어택하면서 위로 올라갖.
//							//mVelocity.y = uppercutVelocity * e.Float;
//							break;
//						case "Pause":
//							attackWatchdog = attackWatchdogDuration;
//							waitingForAttackInput = true;
//							entry.TimeScale = 0;
//							break;
//					}
//				}
//				else if (entry.Animation.Name == downAttackAnim)
//				{
//					switch (e.Data.Name)
//					{
//						case "YVelocity":
//							//down attack 하면서 아래로ㅅ슈욱 가자
//							//mVelocity.y = downAttackVelocity * e.Float;
//							break;
//						case "Pause":
//							velocityLock = e.Int == 1 ? true : false;
//							break;
//					}
//				}
//
//				switch (e.Data.Name)
//				{
//					case "Footstep":
//						if (OnFootstep != null) OnFootstep(transform);
//						break;
//					case "Sound":
//						SoundPalette.PlaySound(e.String, 1f, 1, transform.position);
//						break;
//					case "Effect":
//						switch (e.String)
//						{
//							case "GroundJump":
//								if (groundJumpPrefab && controller.state.IsGround)
//								{
//									SpawnAtFoot(groundJumpPrefab, Quaternion.identity );
//								}
//								break;
//						}
//						break;
//				}
//			}
		}

		override protected void StateEnter ()
		{
			switch (state)
			{
				//모든 땅에서의 움직임은 Idle에서 시작한다.
				case ActionState.IDLE:
					PlayAnimation( idleAnim );
					GravityActive( true );
					currentVX = WalkSpeed;
					break;

				case ActionState.WALK:
					PlayAnimation( walkAnim );
					break;

				case ActionState.RUN:
					PlayAnimation( runAnim );
					currentVX = RunSpeed;
					break;

				case ActionState.JUMP:
					//spine
//					if (JumpCount == 0)
//					{
//						controller.state.ClearPlatform();
//						PlayAnimation(jumpAnim);
//						SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
//						controller.vy = jumpSpeed;
//						// controller.AddForceVertical(mJumpCount > 0 ? airJumpSpeed : jumpSpeed);
//						if (groundJumpPrefab != null)
//						{
//							if (wasWallJump) SpawnAtFoot(groundJumpPrefab, Quaternion.Euler(0, 0, controller.vx >= 0 ? -90 : 90));
//							else SpawnAtFoot(groundJumpPrefab, Quaternion.identity );
//						}
//
//						wasWallJump = false;
//					}
//					else
//					{
//						PlayAnimation(jumpAnim);
//						SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
//						controller.vy = airJumpSpeed;
//						if (airJumpPrefab != null) Instantiate(airJumpPrefab, transform.position, Quaternion.identity);
//					}
//
//					mJumpStartTime = Time.time;
//
//					JumpCount++;
//					if (OnJump != null) OnJump(transform);

					//-----------------------de
					GravityActive( true );
					PlayAnimation( jumpAnim );

					break;

				case ActionState.FALL:
					//PlayAnimation (fallAnim);
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

		override protected void StateUpdate ()
		{
			switch (state)
			{
				//모든 땅에서의 움직임은 Idle에서 시작한다.
				case ActionState.IDLE:
					//-------------------------------de
//					if (CheckFall()) return;
//					if (CheckLadderClimb()) return;
//					if (CheckLookUp()) return;
//					if (CheckCrouch()) return;
//
//					if (_character.horizontalAxis != 0f) SetState(PlayerState.WALK);
//					else _character.Move();

					//----------------------------spine
//					if (CheckJump()) return;
//					if (CheckSlide()) return;
//					if (CheckGroundFall()) return;
//					if (CheckWalk()) return;
//					Move();

					if( input.axisX != 0f ) SetState( ActionState.WALK );
					else Move();

					break;

				case ActionState.WALK:
					//-------------------------------de
//					if (CheckFall()) return;
//					if (CheckLadderClimb()) return;
//					if (CheckLookUp()) return;
//					if (CheckCrouch()) return;
//
//					if (_character.horizontalAxis == 0f)
//					{
//						SetState(PlayerState.IDLE);
//					}
//					else if (_characterState.IsRun)
//					{
//						SetState(PlayerState.RUN);
//					}
//					else
//					{
//						_character.Move();
//					}

					//--------------------------spine
//					if (CheckJump()) return;
//					if (CheckSlide()) return;
//					if (CheckGroundFall()) return;
//					if (CheckIdle()) return;
//					if (CheckRun()) return;
//					Move();

					if( input.axisX == 0f ) SetState( ActionState.IDLE );
					else if( input.inputRun ) SetState( ActionState.RUN );
					else Move();
					break;

				case ActionState.RUN:

					//de
//					if (CheckFall()) return;
//					if (CheckLadderClimb()) return;
//					if (CheckCrouch()) return;
//
//					if (_characterState.IsRun == false || _character.horizontalAxis == 0f)
//					{
//						SetState(PlayerState.IDLE);
//					}
//					else
//					{
//						_character.Move();
//					}

					//spine
//					if (CheckJump()) return;
//					if (CheckSlide()) return;
//					if (CheckGroundFall()) return;
//					if (CheckRunStop()) return;
//					Move();

					if( horizontalAxis == 0f )
					{
						SetState( ActionState.IDLE );
					}
					else if( input.inputRun == false )
					{
						SetState( ActionState.WALK );
					}
					else Move();
					break;

				case ActionState.JUMP:
					//de
//					if (CheckAirToGround()) return;
//					if (CheckLadderClimb()) return;
//					if (CheckWallClinging()) return;
//					if (_controllerState.IsFalling)
//					{
//						SetState(PlayerState.FALL);
//					}
//					else
//					{
//						_character.Move();
//					}
					//spine
//					if (CheckJump()) return;
//					if (CheckJumpFall()) return;
//					if (CheckAirAttack()) return;
//					if (CheckWallSlide()) return;
//					Move();

					Move();

					break;

				case ActionState.FALL:
					//spine
//					if (CheckJump()) return;
//					if (CheckBounceCheck()) return;
//					if (CheckAirAttack()) return;
//					if (CheckWallSlide()) return;
//					if (CheckFallToGround()) return;
//					Move();

					//de
//					if (CheckAirToGround()) return;
//					if (CheckLadderClimb()) return;
//					if (CheckWallClinging()) return;
//
//					_character.Move();

					//-----------spine

					Move();

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

		override  protected void StateExit ()
		{
			switch (state)
			{
				//모든 땅에서의 움직임은 Idle에서 시작한다.
				case ActionState.IDLE:
					break;

				case ActionState.WALK:
					break;

				case ActionState.RUN:
					break;

				case ActionState.JUMP:
					break;

				case ActionState.FALL:
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
	}
}

