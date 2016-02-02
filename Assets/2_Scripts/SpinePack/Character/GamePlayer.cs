using UnityEngine;
using System.Collections;

public class GamePlayer : GameCharacter
{
	//input
	public InputData mInputData;

	public Vector2 inputedAxis;
	public bool inputJumpWasPressed;
	public bool inputJumpIsPressed;
	public bool inputAttackPressed;
	public bool inputSlidePressed;
	protected bool attackWasPressed;
	protected bool inputRun = false;
}
