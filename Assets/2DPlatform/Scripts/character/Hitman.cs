using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    public class Hitman : DEPlayer
    {
        [Header("-- Hitman --")]
        [Header("SpecialAnim")]
        [SpineAnimation]
        public string downAttackAnim;
        [SpineAnimation]
        public string upAttackAnim;

        //입력하지 않고 계산으로 알수있지 않을까
        public float downAttackFrameSkip;

        override protected void GroundAttack()
        {
            if (verticalAxis > 0.1f)
            {
                PlayAnimation(upAttackAnim);
            }
            else
            {
                PlayAnimation(groundAttackAnim);
            }
        }

        override protected void AirAttack()
        {
            if (verticalAxis > 0.1f)
            {
                PlayAnimation(upAttackAnim);
            }
            else if (verticalAxis < -0.1f)
            {
                PlayAnimation(downAttackAnim);
            }
            else
            {
                PlayAnimation(airAttackAnim);
            }
        }

        protected void StateUpdate()
        {
            base.StateUpdate();

            switch (state)
            {
                case CharacterState.WALK:
                    if (TransitionGroundToFall()) return;
                    if (CheckLadderClimb()) return;
                    if (CheckCrouch()) return;
                    if (CheckIdle()) return;
                    if (CheckRun()) return;
                    break;

                case CharacterState.RUN:
                    if (TransitionGroundToFall()) return;
                    if (CheckLadderClimb()) return;
                    if (CheckCrouch()) return;
                    if (CheckRunStop()) return;

                    break;

                case CharacterState.CROUCH:
                    if (TransitionGroundToFall()) return;
                    if (CheckCrouchToIdle()) return;

                    if (horizontalAxis == 0f) currentAnimationTimeScale(0f);
                    else currentAnimationTimeScale(1f);

                    break;

                case CharacterState.LADDER:
                    if (controller.state.IsGrounded || currentLadder == null)
                    {
                        Idle();
                        return;
                    }

                    // 캐릭터가 사다리의 정상 바닥보다 y 위치가 올라간 경우 등반을 멈춘다.
                    if (mTr.position.y > currentLadder.PlatformY)
                    {
                        Idle();
                        return;
                    }
                    if (verticalAxis == 0f) currentAnimationTimeScale(0f);
                    else currentAnimationTimeScale(1f);

                    controller.vy = verticalAxis * ladderClimbSpeed;
                    break;

                case CharacterState.DASH:
                    if (CheckDashToIdle()) return;
                    break;

                case CharacterState.ATTACK_GROUND:
                    if (mWaitNextAttack)
                    {
                        if (Time.time - mWaitNextAttackStartTime > waitAttackDuration) StopWaitNextAttack();
                    }
                    break;

                case CharacterState.ATTACK_AIR:
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

                case CharacterState.JUMP:
                    //de
                    //					if (CheckAirAttack()) return;
                    if (CheckWallSlide()) return;
                    if (CheckJumpFall()) return;
                    if (CheckLadderClimb()) return;
                    break;

                case CharacterState.FALL:
                    //spine
                    //					if (CheckBounceCheck()) return;
                    //					if (CheckAirAttack()) return;
                    if (CheckWallSlide()) return;
                    if (CheckAirToGround()) return;
                    if (CheckLadderClimb()) return;

                    break;

                case CharacterState.WALLSLIDE:
                    //					if (CheckBounceCheck()) return;

                    if (CheckAirToGround()) return;
                    if (CheckWallSlideToFall()) return;

                    break;

                case CharacterState.ESCAPE:
                    if (TransitionGroundToFall()) return;
                    if (CheckSlideStop()) return;
                    break;
            }
        }

        protected void StateExit()
        {
            switch (state)
            {
                case CharacterState.WALLSLIDE:

                    AnimFilp = false;
                    controller.UnLockVY();
                    BodyPosition(Vector2.zero);
                    break;

                case CharacterState.ESCAPE:
                    GhostMode(false);
                    break;
                case CharacterState.DASH:
                    Stop();
                    break;
            }
        }

        //--------------------------------------------------------------------------------------------
        // state transition
        //--------------------------------------------------------------------------------------------

        //-----------------------dash
        bool CheckDashToIdle()
        {
            float dashElapsedTime = Time.time - mDashStartTime;
            if (dashElapsedTime < dashDuration) return false;

            Idle();
            return true;
        }

        //-----------------------crouch
        bool CheckCrouchToIdle()
        {
            if (verticalAxis >= -0.1f && controller.IsCollidingHead == false)
            {
                Idle();
                return true;
            }
            return false;
        }

        //-----------------------jumpfall
        bool CheckJumpFall()
        {
            if (controller.vy > 0) return false;
            Fall(false);
            return true;
        }

        bool CheckAirToGround()
        {
            if (controller.state.IsGrounded == false) return false;
            Idle();
            return true;
        }

        bool CheckWallSlide()
        {
            if (jumpElapsedTime < 0.3f) return false;
            else if (IsPressAgainstWall == false) return false;

            WallSlideEnter();
            return true;
        }

        //-----------------------wallslide
        bool CheckWallSlideToFall()
        {
            if (IsPressAgainstWall) return false;

            Fall();
            return true;
        }

        //-----------------------Slide
        bool CheckSlideStop()
        {
            float slideElapsedTime = Time.time - mEscapeStartTime;
            if (slideElapsedTime < escapeDuration) return false;

            if (controller.IsCollidingHead) return false;

            Idle();
            return true;

        }
    }
}

