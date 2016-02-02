using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour
{

	[Header ("Keyboard")]
	public KeyCode leftKey = KeyCode.A;
	public KeyCode rightKey = KeyCode.D;
	public KeyCode upKey = KeyCode.W;
	public KeyCode downKey = KeyCode.S;

	public KeyCode jumpKey = KeyCode.Space;
	public KeyCode attackKey = KeyCode.J;
	public KeyCode specialKey = KeyCode.K;

	GamePlayer mPlayer;

	void Start ()
	{
		mPlayer = GetComponent<GamePlayer>();
		mPlayer.HandleInput += HandleInput;
	}

	void HandleInput ( GameCharacter gamecontroller )
	{
		InputData data = new InputData();
		data.axisX = ((Input.GetKey (leftKey) ? -1 : 0) + (Input.GetKey (rightKey) ? 1 : 0));
		data.axisY = ((Input.GetKey (downKey) ? -1 : 0) + (Input.GetKey (upKey) ? 1 : 0));
		data.jumpTrigger = Input.GetKeyDown (jumpKey);
		data.jumpPressed = Input.GetKey (jumpKey);
		data.attackTrigger = Input.GetKey (attackKey);
		data.specailATrigger = Input.GetKey (specialKey);
        
		mPlayer.input = data;
	}
}
