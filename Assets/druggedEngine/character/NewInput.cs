using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    public class NewInput : MonoBehaviour
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

		NewCharacter mPlayer;

		void Start ()
		{
			mPlayer = GetComponent<NewCharacter>();
			mPlayer.OnUpdateInput += OnUpdateInput;
		}

		void OnUpdateInput ( NewCharacter ch )
		{
			//if( GameManager.Instance.playerControllable == false ) return;

			var axisX = ((Input.GetKey (leftKey) ? -1 : 0) + (Input.GetKey (rightKey) ? 1 : 0));
			var axisY = ((Input.GetKey (downKey) ? -1 : 0) + (Input.GetKey (upKey) ? 1 : 0));

			mPlayer.axis = new Vector2(axisX,axisY);
			mPlayer.isRun = Input.GetKey (runKey);

			// if( Input.GetKeyDown (jumpKey)) mPlayer.OrderJump();
			// if( Input.GetKeyDown (escapeKey)) mPlayer.OrderEscape();
			// if( Input.GetKeyDown (dashKey)) mPlayer.OrderDash();
			// if (Input.GetButtonDown( attackKey )) mPlayer.OrderAttack();
		}
    }
}
