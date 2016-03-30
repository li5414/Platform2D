using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	public class Hitman : DEPlayer
    {
        [Header("-- Hitman --")]
        
        [Header ("Crounch")]
		[SpineAnimation]
		public string crouchAnim;
		public float crouchSpeed = 1f;
        
        [Header("WallSlide")]
        [SpineAnimation]
        public string wallSlideAnim;
        public float wallSlideSpeed = -1f;
        
		[Header ("Ladder")]
		[SpineAnimation]
		public string ladderAnim;
		public float ladderSpeed = 2f;
		public float ladderClimbSpeed = 1f;

        [Header("ClearAttackCollider")]
        [SpineAnimation]
        public string clearAttackAnim;
        
		#region override DEFAULT BEHAVIOUR
        override protected void Idle()
        {
            base.Idle();

            AddTransition(Transition_Climb);
            AddTransition(TransitionGround_Crouch);
        }
        
        override protected void Walk()
        {
            base.Walk();

            AddTransition(Transition_Climb);
            AddTransition(TransitionGround_Crouch);
        }

        override protected void Run()
        {
            base.Run();

            AddTransition(Transition_Climb);
            AddTransition(TransitionGround_Crouch);
        }
        
        override protected void Fall(bool useJumpCount = true)
        {
            base.Fall(useJumpCount);

            AddTransition(TransitionAir_WallSLide);
            AddTransition(Transition_Climb);
        }
		#endregion
        
		#region CROUCH
        void Crouch()
        {
            SetState(CharacterState.CROUCH);

			SetRestrict( true,true,true,true,true,true );

            PlayAnimation(crouchAnim);
            CurrentSpeed = crouchSpeed;
            Controller.UpdateColliderSize(1f, 0.5f);

			if( -0.1f < axis.x && axis.x < 0.1f )
			{
				Controller.SetFriction(idleFriction);
				currentAnimationTimeScale(0f);
			}
			else
			{
				Controller.SetFriction(movingFriction);
			}

            AddTransition(TransitionGround_Fall);
            AddTransition(TransitionCrouch_Idle);

			mStateLoop += CrouchLoop;
        }

		void CrouchLoop()
		{
			if( lastAxis.x == axis.x ) return;

			if ( -0.1f < axis.x && axis.x < 0.1f )
			{
				Controller.SetFriction(idleFriction);
				currentAnimationTimeScale(0f);
			}
			else
			{
				Controller.SetFriction(movingFriction);
				currentAnimationTimeScale(1f);
			}
		}
		#endregion

		#region LADDER CLIMB
		protected void LadderClimb()
		{
			SetState(CharacterState.LADDER);

			SetRestrict( false,false,true,false,false,false );

			PlayAnimation(ladderAnim);

			Controller.Stop();
			Controller.State.ClearPlatform();
			GravityActive(false);
			ResetJump();

			AddTransition(TransitionLadder_Idle);

			mStateLoop += LadderClimbLoop;
			mStateExit += LadderClimbExit;
		}

		void LadderClimbLoop()
		{
			if (axis.y == 0f) currentAnimationTimeScale(0f);
			else currentAnimationTimeScale(1f);

			Controller.vy = axis.y * ladderClimbSpeed;
		}

		void LadderClimbExit()
		{
			Controller.Stop();
			GravityActive(true);
		}
		#endregion

		#region WALLSLIDE
		protected void WallSlide()
		{
			SetState(CharacterState.WALLSLIDE);

			SetRestrict( false,false,true,false,false,false );

			PlayAnimation(wallSlideAnim);
			Controller.Stop();
			ResetJump();

			Controller.LockVY(wallSlideSpeed);
			//AnimFilp = true;
			//BodyPosition(new Vector2(mFacing == Facing.LEFT ? -0.15f : 0.15f, 0f));

			AddTransition(TransitionAir_Idle);
			AddTransition(TransitionWallSlide_Fall);

			mStateExit += WallSlideExit;
		}

		void WallSlideExit()
		{
			Controller.UnLockVY();
			//AnimFilp = false;
			//BodyPosition(Vector2.zero);
		}
		#endregion

        #region JUMP
		override protected void Jump()
        {
			base.Jump();
			if (State == CharacterState.WALLSLIDE)
			{
				//					Controller.vx = mFacing == Facing.LEFT ? 4 : -4;
				//					Controller.LockMove (0.5f);
				SpawnAtFoot(jumpEffectPrefab, Quaternion.Euler(0, 0, mFacing * 90), new Vector3(mFacing * 1f, 1f, 1f));
			}
			else if (Controller.State.IsGrounded)
			{
				FXManager.Instance.SpawnFX(jumpEffectPrefab, mTr.position, new Vector3(mFacing * 1f, 1f, 1f));
			}

            AddTransition(TransitionAir_WallSLide);
            AddTransition(Transition_Climb);

			mStateLoop += JumpLoop;
        }

		void JumpLoop()
		{
			if (isJumpPressed == false)
			{
				float vy = Controller.vy;
				if (vy > 0) Controller.vy = Mathf.MoveTowards(vy, 0f, Time.deltaTime * 50f);
			}
		}
        #endregion
        
        protected virtual IEnumerator Dive()
        {
            yield break;
            //			// Shake parameters : intensity, duration (in seconds) and decay
            //			Vector3 ShakeParameters = new Vector3(1.5f,0.5f,1f);
            //			BehaviorState.Diving=true;
            //			// while the player is not grounded, we force it to go down fast
            //			while (!_controller.State.IsGrounded)
            //			{
            //				_controller.SetVerticalForce(-Mathf.Abs(_controller.Parameters.Gravity)*2);
            //				yield return 0; //go to next frame
            //			}
            //
            //			// once the player is grounded, we shake the camera, and restore the diving state to false
            //			_sceneCamera.Shake(ShakeParameters);		
            //			BehaviorState.Diving=false;

        }
        
        #region Transition
		protected bool TransitionLadder_Idle()
		{
			if (Controller.State.IsGrounded || CurrentLadder == null)
			{
				Idle();
				return true;
			}

			// 캐릭터가 사다리의 정상 바닥보다 y 위치가 올라간 경우 등반을 멈춘다.
			if (mTr.position.y > CurrentLadder.PlatformY)
			{
				Idle();
				return true;
			}

			return false;
		}

        bool TransitionAir_WallSLide()
        {
            if (jumpElapsedTime < 0.3f) return false;
            else if (IsPressAgainstWall == false) return false;

            WallSlide();
            return true;
        }

        bool TransitionWallSlide_Fall()
        {
            if (IsPressAgainstWall) return false;
            Fall();
            return true;
        }
        
        bool Transition_Climb()
        {
            if (CurrentLadder == null) return false;

			if (axis.y > 0.1f && CurrentLadder.PlatformY > mTr.position.y)
            {
                //사다리를 등반하며 점프하자마자 다시 붙는현상을 피하기위해 약간의 버퍼타임을 둔다. 
                if (Controller.State.IsGrounded == false && jumpElapsedTime < 0.2f) return false;
                mTr.position = new Vector2(CurrentLadder.transform.position.x, mTr.position.y + 0.1f);
                LadderClimb();
                return true;
            }
			else if (axis.y < -0.1f && CurrentLadder.PlatformY <= mTr.position.y)
            {
                mTr.position = new Vector2(CurrentLadder.transform.position.x, CurrentLadder.PlatformY - 0.1f);
                LadderClimb();
                return true;
            }

            return false;
        }
        
        bool TransitionCrouch_Idle()
        {
			if (axis.y >= -0.1f && Controller.IsCollidingHead == false)
            {
                Idle();
                return true;
            }
            return false;
        }
        
        bool TransitionGround_Crouch()
        {
			if (axis.y < -0.1f)
            {
                Crouch();
                return true;
            }

            return false;
        }
        #endregion
        
        
        //입력하지 않고 계산으로 알수있지 않을까
        public float downAttackFrameSkip;
        protected void StateLoopLogic()
        {
            base.StateUpdate();

            switch (State)
            {
                case CharacterState.ATTACK:
                    //---
                    /*
                    //recovering from down attack
                    if (downAttackRecovery)
                    {
                        //time elapsed, jump back to feet using JUMP state
                        if (downAttackRecoveryTime <= 0)
                        {
                            SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
                            velocity.y = jumpSpeed + (platformYVelocity >= 0 ? platformYVelocity : 0);
                            jumpStartTime = Time.time;
                            state = ActionState.JUMP;
                            doJump = false;
                            jumpPressed = false;
                        }
                        //wait for a bit
                        else
                        {
                            downAttackRecoveryTime -= Time.deltaTime;
                            velocity = Vector2.zero;
                            if (movingPlatform)
                                velocity = movingPlatform.Velocity;
                        }
                    }
                    else
                    {
                        //Has impacted the ground, advance sub-state and recover
                        if (OnGround)
                        {
                            SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
                            downAttackRecoveryTime = 2f;  //hard coded value to add drama to recovery
                            downAttackRecovery = true;

                            //TODO: use set value
                            skeletonAnimation.skeleton.Data.FindAnimation(clearAttackAnim).Apply(skeletonAnimation.skeleton, 0, 1, false, null);
                            skeletonAnimation.state.GetCurrent(0).Time = (downAttackFrameSkip / 30f);

                            //spawn effect
                            if (downAttackPrefab)
                                Instantiate(downAttackPrefab, transform.position + new Vector3(0, 0.25f, 0), Quaternion.identity);

                            //adhere to moving platform
                            if (movingPlatform)
                                velocity = movingPlatform.Velocity;

                        }
                        else
                        {
                            //TODO:  Watchdog and error case check
                        }
                        */
                    //--
                    break;
            }
        }
    }
}

