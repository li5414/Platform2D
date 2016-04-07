using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	public class Hitman : DEPlayer
    {
		#region INSPECTOR
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

		[Header ("Dash")]
		[SpineAnimation]
		public string dashAnim;
		public float dashSpeed = 1f;
		public float dashDuration = 0.1f;

		[Header ("Escape")]
		[SpineAnimation]
		public string escapeAnim;
		public float escapeDuration = 1f;

        [Header("ClearAttackCollider")]
        [SpineAnimation]
        public string clearAttackAnim;
		#endregion

		protected float mDashStartTime;
		protected float mEscapeStartTime;

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
            Controller.UpdateColliderSize(1f, 0.65f);

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

			mStateLoop += delegate
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
			};

			mStateExit += delegate
			{
				Controller.ResetColliderSize();
			};
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
			Controller.GravityActive(false);

			Controller.ExceptOneway( CurrentLadder.ladderPlatform );
			ResetJump();

			AddTransition(TransitionLadder_Idle);

			mStateLoop += delegate
			{
				if (axis.y == 0f) currentAnimationTimeScale(0f);
				else currentAnimationTimeScale(1f);

				Controller.vy = axis.y * ladderClimbSpeed;
			};

			mStateExit += delegate
			{
				Controller.Stop();
				Controller.GravityActive(true);
			};
		}
		#endregion

		#region WALLSLIDE
		protected void WallSlide()
		{
			SetState(CharacterState.WALLSLIDE);

			SetRestrict(false,false,true,false,false,false );

			PlayAnimation(wallSlideAnim);
			Controller.Stop();
			ResetJump();

			Controller.LockVY(wallSlideSpeed);
			AnimFlip( true );
			//BodyPosition(new Vector2(mFacing == Facing.LEFT ? -0.15f : 0.15f, 0f));

			AddTransition(TransitionAir_Idle);
			AddTransition(TransitionWallSlide_Fall);

			mStateExit += delegate
			{
				Controller.UnLockVY();
				AnimFlip( false );
				//BodyPosition(Vector2.zero);
			};

			/*
			state = ActionState.WALLSLIDE;
			jumpCount = 0;
			wallSlideWatchdog = wallSlideWatchdogDuration;
			wallSlideStartTime = Time.time;
			upAttackUsed = false;
			if (Mathf.Abs(rb.velocity.x) > 0.1) {
				wallSlideFlip = rb.velocity.x > 0;
			} else {
				wallSlideFlip = x > 0;
			}
			*/
		}
		#endregion

        #region JUMP
		override protected void Jump()
        {
			base.Jump();
			if (State == CharacterState.WALLSLIDE)
			{
				//벽을 밀지 않으면 반대로 뛴다.
				//벽을 밀고 있으면 조금밀고 올라간다 (오리 참고 )
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

			mStateLoop += delegate
			{
				if (isJumpPressed == false)
				{
					float vy = Controller.vy;
					if (vy > 0) Controller.vy = Mathf.MoveTowards(vy, 0f, Time.deltaTime * 50f);
				}
			};
        }
        #endregion

		#region DASH
		override public void DoDash()
		{
            if (mCanDash == false) return;

			SetState (CharacterState.DASH);

			SetRestrict( false,false,false,false,false,false );

			mDashStartTime = Time.time;

			PlayAnimation (dashAnim);
			Controller.GravityActive (false);
			Controller.vy = 0f;
			Controller.vx = mFacing * dashSpeed;

			AddTransition (TransitionDash_Idle);

			mStateExit += delegate
			{
				Controller.GravityActive (true);
				Controller.Stop();
			};
		}
		#endregion

		#region ESCAPE
		override public void DoEscape()
		{
            if (mCanEscape == false) return;

			SetState (CharacterState.ESCAPE);

			SetRestrict( false,false,true,false,true,false );

			mEscapeStartTime = Time.time;
			PlayAnimation (escapeAnim);
			Controller.UpdateColliderSize (1f, 0.5f);
			Controller.SetFriction( movingFriction );
			Controller.vy = 0f;
			Controller.vx = mFacing * RunSpeed;

			AddTransition (TransitionGround_Fall);
			AddTransition (TransitionEscape_Idle);

			mStateExit += delegate
			{
				Controller.ResetColliderSize ();
				GhostMode (false);
			};
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
        
		public bool IsPressAgainstWall
		{
			get
			{
				GameObject front = Controller.State.FrontGameObject;
				if( front == null ) return false;

				Wall wall = front.GetComponent<Wall>();
				if( wall == null ) return false;

				if( wall.slideWay == WallSlideWay.NOTHING ) return false;
				else if(( wall.slideWay == WallSlideWay.LEFT || wall.slideWay == WallSlideWay.BOTH ) && mFacing == 1 && axis.x > 0.5f ) return true;
				else if(( wall.slideWay == WallSlideWay.RIGHT || wall.slideWay == WallSlideWay.BOTH ) && mFacing == -1 && axis.x < -0.5f ) return true;
				else return false;
			}
		}

        #region Transition
		protected bool TransitionLadder_Idle()
		{
			if (Controller.State.IsGrounded || CurrentLadder == null)
			{
				print("tpye1 " + Controller.State.IsGrounded + ", " + CurrentLadder );
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

		protected bool TransitionDash_Idle ()
		{
			float dashElapsedTime = Time.time - mDashStartTime;
			if (dashElapsedTime < dashDuration) return false;

			Idle ();
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

