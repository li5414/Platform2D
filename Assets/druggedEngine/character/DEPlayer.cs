using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    public class DEPlayer : DECharacter
    {
        [Header("-- Player --")]
        [Header("WallSlide")]
        [SpineAnimation]
        public string wallSlideAnim;
        public float wallSlideSpeed = -1f;

        [Header("ClearAttackCollider")]
        [SpineAnimation]
        public string clearAttackAnim;

        public LocationLinker currentManualLinker { get; set; }
        public DialogueZone currentDialogueZone { get; set; }
        
        
        public void DoEscape()
        {
            if (mCanEscape == false) return;
            
            Escape();
        }

        public void DoDash()
        {
            if (mCanDash == false) return;
            Dash();
        }

        public void DoAttack()
        {
            if (mCanAttack == false) return;

            mCanAttack = false;

            Stop();

            if (mWaitNextAttack)
            {
                NextAttack();
            }
            else if (controller.state.IsGrounded)
            {
                SetState(CharacterState.ATTACK_GROUND);
                GroundAttack();
            }
            else
            {
                SetState(CharacterState.ATTACK_AIR);
                AirAttack();
            }
        }
        
        virtual public void DoJump()
        {
            if (mCanJump == false) return;
            if (jumpCount == jumpMax) return;

            mCanEscape = false;

            bool wallJump = false;

            Platform platform = controller.state.StandingPlatform;

            GameObject effect = jumpEffectPrefab;

            //아래 점프 체크도 한다
            if (IsAblePassOneWay())
            {
                PassOneway();
                return;
            }
            //사다리타는 상황이고 아래를 눌렀다면
            else if (state == CharacterState.LADDER && verticalAxis < -0.1f)
            {
                Fall();
                return;
            }
            //벽을 타고 있다면
            else if (state == CharacterState.WALLSLIDE)
            {
                controller.vx = mFacing == Facing.LEFT ? 4 : -4;
                controller.LockMove(0.5f);

                wallJump = true;
            }

            CurrentSpeed = isRun ? RunSpeed : WalkSpeed;

            float jumpPower;
            if (jumpCount == 0)
            {
                GravityActive(true);
                PlayAnimation(jumpAnim);

                PlatformSoundPlay();
                PlatformEffectSpawn();
                controller.state.ClearPlatform();
                jumpPower = Mathf.Sqrt(2f * JumpHeight * Mathf.Abs(controller.Gravity));
            }
            else
            {
                PlayAnimation(jumpAnim);
                jumpPower = Mathf.Sqrt(2f * JumpHeightOnAir * Mathf.Abs(controller.Gravity));

                effect = airJumpEffectPrefab;
            }

            controller.vy = jumpPower;
            jumpStartTime = Time.time;
            jumpCount++;

            if (wallJump)
            {
                SpawnAtFoot(effect, Quaternion.Euler(0, 0, mFacing == Facing.RIGHT ? 90 : -90), new Vector3(mFacing == Facing.RIGHT ? 1f : -1f, 1f, 1f));
            }
            else
            {
                FXManager.Instance.SpawnFX(effect, mTr.position, new Vector3(mFacing == Facing.RIGHT ? 1f : -1f, 1f, 1f));
            }

            SetState(CharacterState.JUMP);
        }
        
        
        override protected void Idle()
        {
            base.Idle();
            
            AddTransition(CheckLadderClimb);
            AddTransition(CheckCrouch);
        }

        protected void ChrouchEnter()
        {
            SetState(CharacterState.CROUCH);

            PlayAnimation(crouchAnim);
            CurrentSpeed = CrouchSpeed;
            controller.UpdateColliderSize(1f, 0.5f);
        }

        protected void LadderEnter()
        {
            SetState(CharacterState.LADDER);

            mCanEscape = false;
            mCanDash = false;

            controller.state.ClearPlatform();
            PlayAnimation(ladderAnim);
            GravityActive(false);
            Stop();
            ResetJump();
        }

        protected void WallSlideEnter()
        {
            SetState(CharacterState.WALLSLIDE);
            
            //spine
            //					wallSlideStartTime = Time.time;
            //					controller.LockVY(wallSlideSpeed);

            AnimFilp = true;
            mCanJump = true;

            controller.LockVY(wallSlideSpeed);
            BodyPosition(new Vector2(mFacing == Facing.LEFT ? -0.15f : 0.15f, 0f));
            PlayAnimation(wallSlideAnim);
            Stop();
            ResetJump();
        }

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

        protected void LookUp()
        {
            //			_character.GravityActive(true);
            //			_character.SetAnimation("lookup");
            //_sceneCamera.LookUp();
        }
        
        
        protected bool CheckLadderClimb()
        {
            if (currentLadder == null) return false;

            if (verticalAxis > 0.1f && currentLadder.PlatformY > mTr.position.y)
            {
                //사다리를 등반하며 점프하자마자 다시 붙는현상을 피하기위해 약간의 버퍼타임을 둔다. 
                if (controller.state.IsGrounded == false && jumpElapsedTime < 0.2f) return false;
                mTr.position = new Vector2(currentLadder.transform.position.x, mTr.position.y + 0.1f);
                LadderEnter();
                return true;
            }
            else if (verticalAxis < -0.1f && currentLadder.PlatformY <= mTr.position.y)
            {
                mTr.position = new Vector2(currentLadder.transform.position.x, currentLadder.PlatformY - 0.1f);
                LadderEnter();
                return true;
            }

            return false;
        }
        
        protected bool CheckCrouch()
        {
            if (verticalAxis < -0.1f)
            {
                ChrouchEnter();
                return true;
            }

            return false;
        }
    }
}
