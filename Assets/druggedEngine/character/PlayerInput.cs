using UnityEngine;
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
		public KeyCode escapeKey = KeyCode.Q;
		public KeyCode dashKey = KeyCode.E;
        
        [InputAxis]
        public string attackKey;

        DEActor mPlayer;

		void Start ()
		{
			mPlayer = GetComponent<DEActor>();
			mPlayer.OnUpdateInput += OnUpdateInput;
		}

		void OnUpdateInput ( DEActor ch )
		{
			if( GameManager.Instance.playerControllable == false ) return;

			var axisX = ((Input.GetKey (leftKey) ? -1 : 0) + (Input.GetKey (rightKey) ? 1 : 0));
			var axisY = ((Input.GetKey (downKey) ? -1 : 0) + (Input.GetKey (upKey) ? 1 : 0));

			mPlayer.axis = new Vector2(axisX,axisY);
			mPlayer.isRun = Input.GetKey (runKey);
			mPlayer.isJumpPressed = Input.GetKey( jumpKey );

			if( Input.GetKeyDown (jumpKey))
			{
				if( axisY < -0.1f ) mPlayer.DoJumpBelow();
				else mPlayer.DoJump();
			}
			// if( Input.GetKeyDown (escapeKey)) mPlayer.OrderEscape();
			// if( Input.GetKeyDown (dashKey)) mPlayer.OrderDash();
			// if (Input.GetButtonDown( attackKey )) mPlayer.OrderAttack();
		}
	}
}