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

		public float ladderClimbSpeed = 1f;

		public float dashDuration = 0.1f;
		public float dashSpeed = 1f;

		public float waitAttackDuration = 0.5f;

		float mSlideStartTime;


		override protected void Start()
		{
			base.Start();

			SetState(CharacterState.IDLE );
		}

		override protected void GroundAttack()
		{
			if( mWaitNextAttack == false )
			{
				if( verticalAxis > 0.1f )
				{
					PlayAnimation( upAttackAnim );
				}
				else
				{
					PlayAnimation( attackAnim );
					controller.AddForce( new Vector2(2f,0f));
				}

			}
			else
			{
				NextAttack();
			}
		}

		override protected void AirAttack()
		{
			//airattack
			//
			//			//----
			//			if (attackWasPressed && moveStick.y < -0.5f) {
			//				velocity.x = 0;
			//				velocity.y = 0;
			//				state = ActionState.DOWNATTACK;
			//				upAttackUsed = true;
			//				skeletonAnimation.AnimationName = downAttackAnim;
			//			} else if (attackWasPressed && moveStick.y > 0.5f) {
			//				if (!upAttackUsed) {
			//					state = ActionState.UPATTACK;
			//					skeletonAnimation.AnimationName = upAttackAnim;
			//					upAttackUsed = true;
			//					velocity.y = 1;
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
					mCanAttack = true;
					mCanFacingUpdate = true;

					controller.ResetColliderSize();
					break;

				case CharacterState.WALK:
					PlayAnimation( walkAnim );
					controller.CurrentSpeed = WalkSpeed;

					break;

				case CharacterState.RUN:
					PlayAnimation( runAnim );
					controller.CurrentSpeed = RunSpeed;
					break;

				case CharacterState.CROUCH:
					PlayAnimation( crouchAnim );
					controller.CurrentSpeed = CrouchSpeed;
					controller.UpdateColliderSize(1f,0.5f);
					break;

				case CharacterState.DASH:
					mCanDash = false;
					PlayAnimation( dashAnim );
					GravityActive( false );
					break;

				case CharacterState.LADDER:
					mCanMove = false;
					mCanSlide = false;
					mCanDash = false;
					mCanFacingUpdate = false;

					controller.state.ClearPlatform();
					PlayAnimation( ladderAnim );
					GravityActive(false);
					Stop();
					ResetJump();
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
					controller.UpdateColliderSize(1f,0.5f);

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


		bool CheckCrouch()
		{
			if ( verticalAxis < -0.1f)
			{
				SetState(CharacterState.CROUCH);
				return true;
			}

			return false;
		}

		bool CheckLadderClimb()
		{
			if ( currentLadder == null) return false;

			if ( verticalAxis > 0.1f && currentLadder.PlatformY > mTr.position.y)
			{
				//사다리를 등반하며 점프하자마자 다시 붙는현상을 피하기위해 약간의 버퍼타임을 둔다. 
				if ( controller.state.IsGrounded == false && jumpElapsedTime < 0.2f) return false;
				mTr.position = new Vector2( currentLadder.transform.position.x, mTr.position.y + 0.1f);
				SetState( CharacterState.LADDER );
				return true;
			}
			else if ( verticalAxis < -0.1f && currentLadder.PlatformY <= mTr.position.y)
			{
				mTr.position = new Vector2( currentLadder.transform.position.x, currentLadder.PlatformY - 0.1f );
				SetState( CharacterState.LADDER );
				return true;
			}

			return false;
		}

		//-----------------------dash
		bool CheckDashToIdle()
		{
			float dashElapsedTime = Time.time - mDashStartTime;
			if( dashElapsedTime < dashDuration ) return false;

			SetState( CharacterState.IDLE );
			return true;
		}

		//-----------------------crouch
		bool CheckCrouchToIdle()
		{
			if ( verticalAxis >= -0.1f && controller.IsCollidingHead == false)
			{
				SetState(CharacterState.IDLE);
				return true;
			}
			return false;
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

			if(controller.IsCollidingHead ) return false;

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
					if( CheckFall()) return;
					if (CheckLadderClimb()) return;
					if( CheckCrouch() ) return;
					if( CheckWalk()) return;

					break;

				case CharacterState.WALK:
					if( CheckFall()) return;
					if (CheckLadderClimb()) return;
					if( CheckCrouch() ) return;
					if( CheckIdle()) return;
					if( CheckRun()) return;
					break;

				case CharacterState.RUN:
					if( CheckFall()) return;
					if (CheckLadderClimb()) return;
					if( CheckCrouch() ) return;
					if( CheckRunStop()) return;

					break;

				case CharacterState.CROUCH:
					if( CheckFall()) return;
					if( CheckCrouchToIdle()) return;

					if( horizontalAxis == 0f ) currentAnimationTimeScale( 0f );
					else currentAnimationTimeScale( 1f );

					break;

				case CharacterState.LADDER:
					if ( controller.state.IsGrounded || currentLadder == null )
					{
						SetState(CharacterState.IDLE);
						return;
					}

					// 캐릭터가 사다리의 정상 바닥보다 y 위치가 올라간 경우 등반을 멈춘다.
					if ( mTr.position.y > currentLadder.PlatformY )
					{
						SetState(CharacterState.IDLE);
						return;
					}
					if( verticalAxis == 0f ) currentAnimationTimeScale( 0f );
					else currentAnimationTimeScale( 1f );

					controller.vy = verticalAxis * ladderClimbSpeed;
					break;

				case CharacterState.DASH:

					if( CheckDashToIdle()) return;
					controller.AddForce( new Vector2( mFacing == Facing.RIGHT ? dashSpeed : -dashSpeed,0f) );
					break;

				case CharacterState.JUMP:
					//de
//					if (CheckAirAttack()) return;
					if( CheckWallSlide()) return;
					if( CheckJumpFall()) return;
					if (CheckLadderClimb()) return;
					break;

				case CharacterState.FALL:
					//spine
//					if (CheckBounceCheck()) return;
//					if (CheckAirAttack()) return;
					if(CheckWallSlide()) return;
					if(CheckAirToGround()) return;
					if (CheckLadderClimb()) return;

					break;

				case CharacterState.WALLSLIDE:
//					if (CheckBounceCheck()) return;

					if(CheckAirToGround()) return;
					if(CheckWallSlideToFall()) return;

					break;

				case CharacterState.ESCAPE:
					if (CheckFall()) return;
					if (CheckSlideStop()) return;

					break;

				case CharacterState.ATTACK:
					if( mWaitNextAttack )
					{
						float waitElapsedTime = Time.time - mWaitNextAttackStartTime;
						if( waitElapsedTime > waitAttackDuration ) SetState( CharacterState.IDLE );
					}
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
					GhostMode( false );
					break;
				case CharacterState.DASH:
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

