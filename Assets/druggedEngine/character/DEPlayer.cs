using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	public class DEPlayer : DECharacter
	{
		public LocationLinker currentManualLinker{ get;set; }
		public DialogueZone currentDialogueZone{ get;set; }

		protected bool mCanSlide;
		protected bool mCanDash;


		protected float mDashStartTime;

		public void Slide()
		{
			if( mCanSlide == false ) return;

			SetState( CharacterState.ESCAPE );
		}

		public void Dash()
		{
			if( mCanDash == false ) return;

			mCanMove = false;
			mCanDash = false;
			mCanFacingUpdate = false;
			controller.axisX = mFacing == Facing.LEFT ? -1f : 1f;
			SetState( CharacterState.DASH );

			mDashStartTime = Time.time;
			StartCoroutine( Boost(0.1f, new Vector2( mFacing == Facing.RIGHT ? 4 : -4,0f)) );

			/*
			// declarations	
			float _dashDirection;
			float _boostForce;

			// if the Dash action is enabled in the permissions, we continue, if not we do nothing
			if (!Permissions.DashEnabled || BehaviorState.IsDead)
				return;
			// if the character is not in a position where it can move freely, we do nothing.
			if (!BehaviorState.CanMoveFreely)
				return;


			// If the user presses the dash button and is not aiming down
			if (_verticalMove>-0.8) 
			{	
				// if the character is allowed to dash
				if (BehaviorState.CanDash)
				{
					// we set its dashing state to true
					BehaviorState.Dashing=true;

					// depending on its direction, we calculate the dash parameters to apply				
					if (_isFacingRight) { _dashDirection=1f; } else { _dashDirection = -1f; }
					_boostForce=_dashDirection*BehaviorParameters.DashForce;
					BehaviorState.CanDash = false;
					// we launch the boost corountine with the right parameters
					StartCoroutine( Boost(BehaviorParameters.DashDuration,_boostForce,0,"dash") );
				}			
			}
			// if the user presses the dash button and is aiming down
			if (_verticalMove<-0.8) 
			{
				_controller.CollisionsOn();
				// we start the dive coroutine
				StartCoroutine(Dive());
			}	
			*/
		}

		protected virtual IEnumerator Boost(float boostDuration, Vector2 force ) 
		{
			float time = 0f; 
			while(boostDuration > time) 
			{
				controller.AddForce( force );
				time+=Time.deltaTime;
				yield return null;
			}
//			// once the boost is complete, if we were dashing, we make it stop and start the dash cooldown
//			if (name=="dash")
//			{
//				BehaviorState.Dashing=false;
//				GravityActive(true);
//				yield return new WaitForSeconds(BehaviorParameters.DashCooldown); 
//				BehaviorState.CanDash = true; 
//			}	
//			if (name=="wallJump")
//			{
//				// so far we do nothing, but you could use it to trigger a sound or an effect when walljumping
//			}	
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

	}
}
