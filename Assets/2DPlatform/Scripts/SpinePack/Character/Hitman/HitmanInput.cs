using UnityEngine;
using System.Collections;

public class HitmanInput : MonoBehaviour
{
	[Header ("Keyboard")]
	public KeyCode leftKey = KeyCode.A;
	public KeyCode rightKey = KeyCode.D;
	public KeyCode upKey = KeyCode.W;
	public KeyCode downKey = KeyCode.S;

	public KeyCode jumpKey = KeyCode.Space;
	public KeyCode attackKey = KeyCode.J;
	public KeyCode specialKey = KeyCode.K;

	HitmanController mPlayer;

	void Start ()
	{
		mPlayer = GetComponent<HitmanController>();
		mPlayer.HandleInput += HandleInput;
	}

	void HandleInput ( TempGameCharacter gamecontroller )
	{
		Vector2 axis = Vector2.zero;
		axis.x = ((Input.GetKey (leftKey) ? -1 : 0) + (Input.GetKey (rightKey) ? 1 : 0));
		axis.y = ((Input.GetKey (downKey) ? -1 : 0) + (Input.GetKey (upKey) ? 1 : 0));

		mPlayer.inputedAxis = axis;
		mPlayer.inputJumpWasPressed = Input.GetKeyDown (jumpKey);
		mPlayer.inputJumpIsPressed = Input.GetKey (jumpKey);

		mPlayer.inputAttackPressed = Input.GetKeyDown (attackKey);
		mPlayer.inputSlidePressed = Input.GetKeyDown (specialKey);
	}
}
