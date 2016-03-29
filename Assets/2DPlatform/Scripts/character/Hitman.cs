﻿using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    public class Hitman : DEActor
    {
        [Header("-- Hitman --")]
        
        [Header ("Crounch")]
		[SpineAnimation]
		public string crouchAnim;
		public float CrouchSpeed = 1f;
        
        [Header("WallSlide")]
        [SpineAnimation]
        public string wallSlideAnim;
        public float wallSlideSpeed = -1f;
        
        [Header("ClearAttackCollider")]
        [SpineAnimation]
        public string clearAttackAnim;
        
        
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
        
        void Chrouch()
        {
            SetState(CharacterState.CROUCH);

            PlayAnimation(crouchAnim);
            CurrentSpeed = CrouchSpeed;
            Controller.UpdateColliderSize(1f, 0.5f);

            mStateLoop += Move;
            mStateLoop += delegate
            {
                if (horizontalAxis == 0f) currentAnimationTimeScale(0f);
                else currentAnimationTimeScale(1f);
            };

            AddTransition(TransitionGround_Fall);
            AddTransition(TransitionCrouch_Idle);
        }
        
        #region Action
        override public void DoJump()
        {
            base.DoJump();

            AddTransition(TransitionAir_WallSLide);
            AddTransition(Transition_Climb);
        }
        
        
        protected void LadderClimb()
        {
            SetState(CharacterState.LADDER);

            mCanDash = false;
            mCanJump = true;
            mCanEscape = false;
            mCanAttack = false;

            Controller.state.ClearPlatform();
            PlayAnimation(ladderAnim);
            GravityActive(false);
            Stop();
            ResetJump();

            AddTransition(TransitionLadder_Idle);

            mStateLoop += delegate
            {
                if (verticalAxis == 0f) currentAnimationTimeScale(0f);
                else currentAnimationTimeScale(1f);
                Controller.vy = verticalAxis * ladderClimbSpeed;
            };

            mStateExit += delegate
            {
                Stop();
                GravityActive(true);
            };
        }

        protected void WallSlide()
        {
            SetState(CharacterState.WALLSLIDE);

            mCanDash = false;
            mCanJump = true;
            mCanEscape = false;
            mCanAttack = false;

            PlayAnimation(wallSlideAnim);
            AnimFilp = true;
            Controller.LockVY(wallSlideSpeed);
            BodyPosition(new Vector2(mFacing == Facing.LEFT ? -0.15f : 0.15f, 0f));
            Stop();
            ResetJump();

            AddTransition(TransitionAir_Idle);
            AddTransition(TransitionWallSlide_Fall);

            mStateExit += delegate
            {
                AnimFilp = false;
                Controller.UnLockVY();
                BodyPosition(Vector2.zero);
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
        
        #region Transition
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

            if (verticalAxis > 0.1f && CurrentLadder.PlatformY > mTr.position.y)
            {
                //사다리를 등반하며 점프하자마자 다시 붙는현상을 피하기위해 약간의 버퍼타임을 둔다. 
                if (Controller.state.IsGrounded == false && jumpElapsedTime < 0.2f) return false;
                mTr.position = new Vector2(CurrentLadder.transform.position.x, mTr.position.y + 0.1f);
                LadderClimb();
                return true;
            }
            else if (verticalAxis < -0.1f && CurrentLadder.PlatformY <= mTr.position.y)
            {
                mTr.position = new Vector2(CurrentLadder.transform.position.x, CurrentLadder.PlatformY - 0.1f);
                LadderClimb();
                return true;
            }

            return false;
        }
        
        bool TransitionCrouch_Idle()
        {
            if (verticalAxis >= -0.1f && Controller.IsCollidingHead == false)
            {
                Idle();
                return true;
            }
            return false;
        }
        
        bool TransitionGround_Crouch()
        {
            if (verticalAxis < -0.1f)
            {
                Chrouch();
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

