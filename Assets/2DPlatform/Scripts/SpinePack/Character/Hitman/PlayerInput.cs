﻿using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	public class PlayerInput : MonoBehaviour
	{

		[Header ("Keyboard")]
		public KeyCode leftKey = KeyCode.A;
		public KeyCode rightKey = KeyCode.D;
		public KeyCode upKey = KeyCode.W;
		public KeyCode downKey = KeyCode.S;
		public KeyCode runKey = KeyCode.LeftShift;

		public KeyCode jumpKey = KeyCode.Space;
		public KeyCode slideKey = KeyCode.Q;
		public KeyCode dashKey = KeyCode.E;

		DEPlayer mPlayer;

		void Start ()
		{
			mPlayer = GetComponent<DEPlayer>();
			mPlayer.OnUpdateInput += OnUpdateInput;
		}

		void OnUpdateInput ()
		{
			mPlayer.horizontalAxis = ((Input.GetKey (leftKey) ? -1 : 0) + (Input.GetKey (rightKey) ? 1 : 0));
			mPlayer.verticalAxis = ((Input.GetKey (downKey) ? -1 : 0) + (Input.GetKey (upKey) ? 1 : 0));
			mPlayer.isRun = Input.GetKey (runKey);

			if( Input.GetKeyDown (jumpKey)) mPlayer.Jump();
			if( Input.GetKeyDown (slideKey)) mPlayer.Slide();
			if( Input.GetKeyDown (dashKey)) mPlayer.Dash();

			if (Input.GetButtonDown("Fire1")) mPlayer.Attack();
		}

		void Update ()
		{
			//Debug.Log( (string) Input.inputString );
		}

		void UpdateTemp()
		{
			/*
            //대화존에 캐릭터가 들어가 있는 경우 점프를 누르면 대화창을 재생
			if ( _player.BehaviorState.InDialogueZone &&
				_player.BehaviorState.CurrentDialogueZone!=null &&
				_player.BehaviorState.IsDead == false &&
				_controller.State.IsGrounded &&
				_player.BehaviorState.Dashing == false )
			{
				if (CrossPlatformInputManager.GetButtonDown ("Jump")) 
				{
					_player.BehaviorState.CurrentDialogueZone.StartDialogue();
				}
			}
			*/

			float h = Input.GetAxisRaw("Horizontal");
			float v = Input.GetAxisRaw("Vertical");

			if( mPlayer.currentManualLinker != null &&
				mPlayer.controller.state.IsGrounded &&
				v > 0.5f )
			{
				mPlayer.currentManualLinker.In( mPlayer );
			}

			mPlayer.horizontalAxis = h;
			mPlayer.verticalAxis = v;


			//            if (_character.verticalAxis != 0f)
			//            {
			//                SetState(PlayerState.LADDER_CLIMB_MOVE);
			//            }

			//run
			//			if (Input.GetButtonDown("Run")) mPlayer.Run();
			//			if (Input.GetButtonUp("Run")) mPlayer.StopRun();

			//대쉬    
			//            if ( CrossPlatformInputManager.GetButtonDown("Dash") )
			//                _player.Dash();

			//점프시작      
			//if (Input.GetButtonDown("Jump")) mPlayer.Jump();

			//근접공격
			if (Input.GetButtonDown("Fire1"))
			{
				//mPlayer.Attack();
				//_player.GetComponent<CharacterMelee>().Melee();
			}

			if (Input.GetButtonDown("Fire2"))
			{
				//_player.GetComponent<CharacterMelee>().Melee();
			}
			/*
            //발사
			if ( _player.GetComponent<CharacterShoot>() != null) 
			{
				_player.GetComponent<CharacterShoot>().SetHorizontalAxis(CrossPlatformInputManager.GetAxis ("Horizontal"));
				_player.GetComponent<CharacterShoot>().SetVerticalAxis(CrossPlatformInputManager.GetAxis ("Vertical"));

				if (CrossPlatformInputManager.GetButtonDown("Fire"))
					_player.GetComponent<CharacterShoot>().ShootOnce();         

				if (CrossPlatformInputManager.GetButton("Fire")) 
					_player.GetComponent<CharacterShoot>().ShootStart();

				if (CrossPlatformInputManager.GetButtonUp("Fire"))
					_player.GetComponent<CharacterShoot>().ShootStop();

			}

			//제트팩
			if ( _player.GetComponent<CharacterJetpack>() != null )
			{
				if ( (CrossPlatformInputManager.GetButtonDown("Jetpack")||CrossPlatformInputManager.GetButton("Jetpack")) )
					_player.GetComponent<CharacterJetpack>().JetpackStart();

				if (CrossPlatformInputManager.GetButtonUp("Jetpack"))
					_player.GetComponent<CharacterJetpack>().JetpackStop();
			}
			*/
		}
	}
}
