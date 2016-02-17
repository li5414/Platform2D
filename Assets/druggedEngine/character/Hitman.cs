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
		public string dashAnim;
		[SpineAnimation]
		public string ladderAnim;
		[SpineAnimation]
		public string crouchAnim;
		[SpineAnimation]
		public string escapeAnim;
		[SpineAnimation]
		public string downAttackAnim;
		[SpineAnimation]
		public string upAttackAnim;
		//this animation is used strictly to turn off all attacking bounding box attachment based hitboxes
		[SpineAnimation]
		public string clearAttackAnim;


		//입력하지 않고 계산으로 알수있지 않을까
		public float downAttackFrameSkip;
		public float wallSlideSpeed = -1f;
		public float slideDuration = 1f;

		float mSlideStartTime;

		override protected void Start()
		{
			base.Start();

			controllable = true;
			SetState(CharacterState.IDLE );
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
				case CharacterState.IDLE:
					PlayAnimation( idleAnim );
					GravityActive( true );
					controller.CurrentSpeed = WalkSpeed;

					mCanSlide = true;
					mCanDash = true;
					mCanJump = true;
					mCanMove = true;
					mCanFacingUpdate = true;
					break;

				case CharacterState.WALK:
					PlayAnimation( walkAnim );
					controller.CurrentSpeed = WalkSpeed;

					break;

				case CharacterState.RUN:
					PlayAnimation( runAnim );
					controller.CurrentSpeed = RunSpeed;
					break;

				case CharacterState.DASH:
					mCanDash = false;
					PlayAnimation( dashAnim );
					break;

				case CharacterState.LADDER:
					PlayAnimation( ladderAnim );
					break;

				case CharacterState.JUMP:
					//DECharacter의 jump 메소드에서 처리
					break;

				case CharacterState.FALL:
					//DECharacter의 Fall 메소드에서 처리
					break;

				case CharacterState.WALLSLIDE:
					//spine
//					wallSlideStartTime = Time.time;
//					controller.LockVY(wallSlideSpeed);

					AnimFilp = true;
					mCanJump = true;
					mCanMove = true;
					mCanFacingUpdate = true;

					controller.LockVY(wallSlideSpeed);
					BodyPosition( new Vector2( mFacing == Facing.LEFT ? -0.15f : 0.15f ,0f));
					PlayAnimation( wallSlideAnim );
					Stop();
					ResetJump();
					break;

				case CharacterState.ESCAPE:
					PlayAnimation( escapeAnim );

					controller.CurrentSpeed = RunSpeed;
					controller.axisX = mFacing == Facing.LEFT ? -1f : 1f;
//					controller.vx = RunSpeed;
					controller.UpdateColliderSize(1,0.5f);

					mCanSlide = false;
					mCanMove = false;
					mCanFacingUpdate = false;

					mSlideStartTime = Time.time;
					GhostMode( true );
					//SoundPalette.PlaySound(slideSound, 1, 1, transform.position);
					break;

				case CharacterState.ATTACK:
					break;

				case CharacterState.DOWNATTACK:
					break;

				case CharacterState.UPATTACK:
					break;
			}
		}


		//----------------------ground
		bool CheckFall()
		{
			if( controller.state.IsGrounded ) return false;

			Fall();
			return true;
		}

		bool CheckIdle()
		{
			if (horizontalAxis != 0f) return false;
			SetState(CharacterState.IDLE);
			return true;
		}

		bool CheckWalk()
		{
			if (horizontalAxis == 0f) return false;
			SetState(CharacterState.WALK);
			return true;
		}

		bool CheckRun()
		{
			if ( isRun == false) return false;
			SetState(CharacterState.RUN);
			return true;
		}

		bool CheckRunStop()
		{
			if (isRun) return false;

			if ( horizontalAxis != 0f) SetState(CharacterState.WALK);
			else SetState(CharacterState.IDLE);
			return true;
		}

		//-----------------------jumpfall
		bool CheckJumpFall()
		{
			if (controller.vy > 0) return false;
			Fall( false );
			return true;
		}

		bool CheckAirToGround()
		{
			if (controller.state.IsGrounded == false) return false;
			SetState(CharacterState.IDLE);
			return true;
		}

		bool CheckWallSlide()
		{
			if( jumpElapsedTime < 0.3f ) return false;
			else if( IsPressAgainstWall == false ) return false;

			SetState( CharacterState.WALLSLIDE );
			return true;
		}

		//-----------------------wallslide
		bool CheckWallSlideToFall()
		{
			if( IsPressAgainstWall ) return false;

			Fall();
			return true;
		}

		//-----------------------Slide
		bool CheckSlideStop()
		{
			float slideElapsedTime = Time.time - mSlideStartTime;
			if( slideElapsedTime < slideDuration ) return false;

			SetState( CharacterState.IDLE );
			return true;

		}

		override protected void StateUpdate ()
		{
			switch (state)
			{
				//모든 땅에서의 움직임은 Idle에서 시작한다.
				case CharacterState.IDLE:
					//-------------------------------de
//					if (CheckLadderClimb()) return;
//					if (CheckLookUp()) return;
//					if (CheckCrouch()) return;
//					if (CheckSlide()) return;
//					if (CheckJump()) return;

					if( CheckFall()) return;
					if( CheckWalk()) return;

					break;

				case CharacterState.WALK:
//					if (CheckLadderClimb()) return;
//					if (CheckLookUp()) return;
//					if (CheckCrouch()) return;
//					if (CheckSlide()) return;

					if( CheckFall()) return;
					if( CheckIdle()) return;
					if( CheckRun()) return;
					break;

				case CharacterState.RUN:
//					if (CheckLadderClimb()) return;
//					if (CheckCrouch()) return;
//					if (CheckSlide()) return;
					if (CheckFall()) return;
					if (CheckRunStop()) return;

					break;

				case CharacterState.DASH:

					float dashElapsedTime = Time.time - mDashStartTime;
					if( dashElapsedTime < 0.1f ) return;

					SetState( CharacterState.IDLE );
//					return true;
//					mDashStartTime = Time.time;
					//controller.AddForce( new Vector2(4f,0f) );controller.AddForce( new Vector2(4f,0f) );
					break;

				case CharacterState.JUMP:
					//de
//					if (CheckLadderClimb()) return;
//					if (CheckWallClinging()) return;
//					if (CheckAirAttack()) return;
//					if (CheckWallSlide()) return;

					if( CheckWallSlide()) return;
					if( CheckJumpFall()) return;
					break;

				case CharacterState.FALL:

					//spine
//					if (CheckBounceCheck()) return;
//					if (CheckAirAttack()) return;
//					if (CheckLadderClimb()) return;

					if( CheckWallSlide()) return;
					if (CheckAirToGround()) return;

					break;

				case CharacterState.WALLSLIDE:
					//spine
//					if (CheckWallJump()) return;
//					if (CheckBounceCheck()) return;

					if(CheckAirToGround()) return;
					if(CheckWallSlideToFall()) return;

					break;

				case CharacterState.ESCAPE:
					if (CheckFall()) return;
					if (CheckSlideStop()) return;

					break;

				case CharacterState.ATTACK:
					break;

				case CharacterState.DOWNATTACK:
					break;

				case CharacterState.UPATTACK:
					break;
			}
		}

		override  protected void StateExit ()
		{
			switch (state)
			{
				//모든 땅에서의 움직임은 Idle에서 시작한다.
				case CharacterState.IDLE:
					break;

				case CharacterState.WALK:
					break;

				case CharacterState.RUN:
					break;

				case CharacterState.JUMP:
					break;

				case CharacterState.FALL:
					break;

				case CharacterState.WALLSLIDE:

					AnimFilp = false;
					controller.UnLockVY();
					BodyPosition( Vector2.zero );
					break;

				case CharacterState.ESCAPE:
					controller.ResetColliderSize();
					//IgnoreCharacterCollisions(false);
					GhostMode( false );
					break;

				case CharacterState.ATTACK:
					break;

				case CharacterState.DOWNATTACK:
					break;

				case CharacterState.UPATTACK:
					break;
			}
		}
	}
}

