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
					mCanJump = true;
					break;

				case ActionState.WALK:
					PlayAnimation( walkAnim );
					mCanJump = true;
					break;

				case ActionState.RUN:
					PlayAnimation( runAnim );
					currentVX = RunSpeed;
					mCanJump = true;
					break;

				case ActionState.JUMP:
					//DECharacter의 jump 메소드에 위임
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

		bool CheckFall()
		{
			if( controller.state.IsGrounded ) return false;

			Fall();
			return true;
		}

		//----------------------ground
		bool CheckIdle()
		{
			if (input.axisX != 0f) return false;
			SetState(ActionState.IDLE);
			return true;
		}

		bool CheckWalk()
		{
			if (input.axisX == 0f) return false;
			SetState(ActionState.WALK);
			return true;
		}

		bool CheckRun()
		{
			if (input.inputRun == false) return false;
			SetState(ActionState.RUN);
			return true;
		}

		bool CheckRunStop()
		{
			if (input.inputRun) return false;

			if (input.axisX != 0f) SetState(ActionState.WALK);
			else SetState(ActionState.IDLE);
			return true;
		}

		//-----------------------jumpfall
		bool CheckJumpFall()
		{
			if (controller.vy >= 0) return false;
			Fall( false );
			return true;
		}

		bool CheckJumpToGround()
		{
			if (controller.state.IsGrounded == false) return false;
			SetState(ActionState.IDLE);
			return true;
		}

		override protected void StateUpdate ()
		{
			switch (state)
			{
				//모든 땅에서의 움직임은 Idle에서 시작한다.
				case ActionState.IDLE:
					//-------------------------------de
//					if (CheckLadderClimb()) return;
//					if (CheckLookUp()) return;
//					if (CheckCrouch()) return;
//					if (CheckSlide()) return;
//					if (CheckJump()) return;

					if( CheckFall()) return;
					if( CheckWalk()) return;
						
					Move();

					break;

				case ActionState.WALK:
//					if (CheckLadderClimb()) return;
//					if (CheckLookUp()) return;
//					if (CheckCrouch()) return;
//					if (CheckSlide()) return;

					if( CheckFall()) return;
					if( CheckIdle()) return;
					if( CheckRun()) return;

					Move();
					break;

				case ActionState.RUN:
//					if (CheckLadderClimb()) return;
//					if (CheckCrouch()) return;
//					if (CheckSlide()) return;
					if (CheckFall()) return;
					if (CheckRunStop()) return;
					Move();
					break;

				case ActionState.JUMP:
					//de
//					if (CheckLadderClimb()) return;
//					if (CheckWallClinging()) return;
//					if (CheckAirAttack()) return;
//					if (CheckWallSlide()) return;

					print("jumpupdate");

					if( CheckJumpFall()) return;

					Move();

					break;

				case ActionState.FALL:

					print("fallupdate");
					//spine
//					if (CheckBounceCheck()) return;
//					if (CheckAirAttack()) return;
//					if (CheckWallSlide()) return;
//					if (CheckLadderClimb()) return;
//					if (CheckWallClinging()) return;

					if (CheckJumpToGround()) return;

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

