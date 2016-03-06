using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    public class Hitman : DEPlayer
    {
        [Header("-- Hitman --")]
        [Header("SpecialAnim")]

        //입력하지 않고 계산으로 알수있지 않을까
        public float downAttackFrameSkip;

        protected void StateUpdate()
        {
            base.StateUpdate();

            switch (state)
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

