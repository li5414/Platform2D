using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine;

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

        public List<Weapon> weapons;

        public LocationLinker currentManualLinker { get; set; }
        public DialogueZone currentDialogueZone { get; set; }

        override protected void Start()
        {
            base.Start();

            foreach (Weapon w in weapons)
            {
                w.CacheSpineAnimations( mSkeletonAnimation.skeleton.Data );
            }
            
            if( weapons.Count > 0 ) EquipWeapon(weapons[0]);
        }

        //--------------------------------------------------------------------
        // Order
        //--------------------------------------------------------------------

        public void OrderEscape()
        {
            DoEscape();
        }

        public void OrderDash()
        {
            DoDash();
        }

        public void OrderAttack()
        {
            DoAttack();
        }

        virtual public void OrderJump()
        {
            if (verticalAxis < -0.1f) DoJumpBelow();
            else DoJump();
        }

        protected Weapon currentWeapon;
        void EquipWeapon(Weapon weapon)
        {
            Skeleton skeleton = mSkeletonAnimation.skeleton;
            weapon.SetupAnim.Apply(skeleton, 0, 1, false, null);

            PlayAnimation(weapon.idleAnim, true, 1);

            currentWeapon = weapon;
            currentWeapon.Setup();
        }

        void Shoot()
        {
            //조준하고,
            if (currentWeapon.reloadLock == false &&
                currentWeapon.clip > 0 &&
                Time.time >= currentWeapon.nextFireTime)
            {
                PlayAnimation(currentWeapon.fireAnim, false, 1);
                currentWeapon.nextFireTime = Time.time + currentWeapon.refireRate;
            }
            else if (currentWeapon.reloadLock == false &&
                    Time.time >= currentWeapon.nextFireTime)
            {
                if (currentWeapon.ammo > 0 && currentWeapon.clip < currentWeapon.clipSize)
                {
                    PlayAnimation(currentWeapon.reloadAnim, false, 1);
                    currentWeapon.reloadLock = true;
                }
            }

            TrackEntry entry = GetCurrent(1);
            //리로드 가 아닌 경우 aiming 
            if( currentWeapon.reloadLock == false )
            {
                if( entry == null ||
                    entry.Animation != currentWeapon.FireAnim && entry.Animation != currentWeapon.AimAnim )
                {
                    PlayAnimation( currentWeapon.aimAnim,true,1);
                }
                
                float angle = 45f;
            }
            //리로드 중인 경우
            else
            {
                if( currentWeapon.reloadLock == false &&
                ( entry == null || entry.Animation != currentWeapon.FireAnim && entry.Animation != currentWeapon.IdleAnim ))
                {
                    PlayAnimation( currentWeapon.idleAnim, true, 1 );
                }
            }
        }
        
        override protected void FireWeapon()
        {
            currentWeapon.Fire();
            // if (this.state == ActionState.JETPACK)
            // {
            //     doRecoil = true;
            // }
        }
        
        override protected void EjectCasing()
        {
            // Instantiate(currentWeapon.casingPrefab, currentWeapon.casingEjectPoint.position, Quaternion.LookRotation(Vector3.forward, currentWeapon.casingEjectPoint.up));
        }
        
        //--------------------------------------------------------------------
        // Override
        //--------------------------------------------------------------------

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

        protected void Chrouch()
        {
            SetState(CharacterState.CROUCH);

            PlayAnimation(crouchAnim);
            CurrentSpeed = CrouchSpeed;
            controller.UpdateColliderSize(1f, 0.5f);

            mStateLoop += Move;
            mStateLoop += delegate
            {
                if (horizontalAxis == 0f) currentAnimationTimeScale(0f);
                else currentAnimationTimeScale(1f);
            };

            AddTransition(TransitionGround_Fall);
            AddTransition(TransitionCrouch_Idle);
        }

        override protected void DoJump()
        {
            base.DoJump();

            AddTransition(TransitionAir_WallSLide);
            AddTransition(Transition_Climb);
        }

        override protected void Fall(bool useJumpCount = true)
        {
            base.Fall(useJumpCount);

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

            controller.state.ClearPlatform();
            PlayAnimation(ladderAnim);
            GravityActive(false);
            Stop();
            ResetJump();

            AddTransition(TransitionLadder_Idle);

            mStateLoop += delegate
            {
                if (verticalAxis == 0f) currentAnimationTimeScale(0f);
                else currentAnimationTimeScale(1f);
                controller.vy = verticalAxis * ladderClimbSpeed;
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
            controller.LockVY(wallSlideSpeed);
            BodyPosition(new Vector2(mFacing == Facing.LEFT ? -0.15f : 0.15f, 0f));
            Stop();
            ResetJump();

            AddTransition(TransitionAir_Idle);
            AddTransition(TransitionWallSlide_Fall);

            mStateExit += delegate
            {
                AnimFilp = false;
                controller.UnLockVY();
                BodyPosition(Vector2.zero);
            };
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

        protected bool TransitionLadder_Idle()
        {
            if (controller.state.IsGrounded || currentLadder == null)
            {
                Idle();
                return true;
            }

            // 캐릭터가 사다리의 정상 바닥보다 y 위치가 올라간 경우 등반을 멈춘다.
            if (mTr.position.y > currentLadder.PlatformY)
            {
                Idle();
                return true;
            }

            return false;
        }

        protected bool TransitionCrouch_Idle()
        {
            if (verticalAxis >= -0.1f && controller.IsCollidingHead == false)
            {
                Idle();
                return true;
            }
            return false;
        }

        protected bool Transition_Climb()
        {
            if (currentLadder == null) return false;

            if (verticalAxis > 0.1f && currentLadder.PlatformY > mTr.position.y)
            {
                //사다리를 등반하며 점프하자마자 다시 붙는현상을 피하기위해 약간의 버퍼타임을 둔다. 
                if (controller.state.IsGrounded == false && jumpElapsedTime < 0.2f) return false;
                mTr.position = new Vector2(currentLadder.transform.position.x, mTr.position.y + 0.1f);
                LadderClimb();
                return true;
            }
            else if (verticalAxis < -0.1f && currentLadder.PlatformY <= mTr.position.y)
            {
                mTr.position = new Vector2(currentLadder.transform.position.x, currentLadder.PlatformY - 0.1f);
                LadderClimb();
                return true;
            }

            return false;
        }

        protected bool TransitionGround_Crouch()
        {
            if (verticalAxis < -0.1f)
            {
                Chrouch();
                return true;
            }

            return false;
        }
    }
}
