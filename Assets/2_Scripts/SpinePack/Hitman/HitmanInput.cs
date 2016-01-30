/*****************************************************************************
 * Spine Asset Pack License
 * Version 1.0
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use the Asset Pack and derivative works only as
 * incorporated and embedded components of your software applications and to
 * distribute such software applications. Any source code contained in the Asset
 * Pack may not be distributed in source form. You may otherwise not reproduce,
 * distribute, sublicense, rent, lease or lend the Asset Pack. It is emphasized
 * that you are not entitled to distribute or transfer the Asset Pack in any way
 * other way than as integrated components of your software applications.
 * 
 * THIS ASSET PACK IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS ASSET PACK, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

//#define IN_CONTROL
//#define REWIRED

using UnityEngine;
using System.Collections;
#if IN_CONTROL
using InControl;
#elif REWIRED
using Rewired;
#endif

public class HitmanInput : GameCharacterInput {

#if IN_CONTROL
	[Header("InControl")]
	[Tooltip("-1 to use ActiveDevice")]
	public int deviceIndex = -1;
#elif REWIRED
	[Header("Rewired")]
	public int playerId = 0;
#else
	[Header("Gamepad")]
	[InputAxis]
	public string xAxis;
	[InputAxis]
	public string yAxis;
	[InputAxis]
	public string jumpButton;
	[InputAxis]
	public string attackButton;
	[InputAxis]
	public string slideButton;
#endif
	[Header("Keyboard")]
	public KeyCode leftKey = KeyCode.A;
	public KeyCode rightKey = KeyCode.D;
	public KeyCode upKey = KeyCode.W;
	public KeyCode downKey = KeyCode.S;
	public KeyCode jumpKey = KeyCode.Space;
	public KeyCode attackKey = KeyCode.J;
	public KeyCode slideKey = KeyCode.K;


	void Start () {
#if IN_CONTROL
		if (InputManager.Devices.Count - 1 < deviceIndex)
			deviceIndex = -1;
#endif

		GetComponent<HitmanController>().HandleInput += HandleInput;
	}

	void HandleInput (HitmanController controller) {
		bool JUMP_isPressed = false;
		bool JUMP_wasPressed = false;
		bool SLIDE_wasPressed = false;
		bool ATTACK_wasPressed = false;
		Vector2 moveStick = Vector2.zero;

		if (useKeyboard) {
			moveStick.x = ((Input.GetKey(leftKey) ? -1 : 0) + (Input.GetKey(rightKey) ? 1 : 0));
			moveStick.y = ((Input.GetKey(downKey) ? -1 : 0) + (Input.GetKey(upKey) ? 1 : 0));
			JUMP_wasPressed = Input.GetKeyDown(jumpKey);
			JUMP_isPressed = Input.GetKey(jumpKey);
			SLIDE_wasPressed = Input.GetKeyDown(slideKey);
			ATTACK_wasPressed = Input.GetKeyDown(attackKey);
		} else {
#if IN_CONTROL
			var device = deviceIndex == -1 ? InputManager.ActiveDevice : InputManager.Devices[deviceIndex];
			moveStick.x = device.Direction.X;
			moveStick.y = device.Direction.Y;
			JUMP_wasPressed = device.Action1.WasPressed;
			JUMP_isPressed = device.Action1.IsPressed;
			SLIDE_wasPressed = device.Action2.WasPressed;
			ATTACK_wasPressed = device.Action3.WasPressed;
#elif REWIRED
			var player = ReInput.players.GetPlayer(playerId);
			moveStick.x = player.GetAxis("Move X");
			moveStick.y = player.GetAxis("Move Y");
			JUMP_wasPressed = player.GetButtonDown("Jump");
			JUMP_isPressed = player.GetButton("Jump");
			SLIDE_wasPressed = player.GetButtonDown("Slide");
			ATTACK_wasPressed = player.GetButtonDown("Attack");
#else
			moveStick.x = Input.GetAxis("Horizontal");
			moveStick.y = Input.GetAxis("Vertical");
			JUMP_wasPressed = Input.GetButtonDown(jumpButton);
			JUMP_isPressed = Input.GetButton(jumpButton);
			SLIDE_wasPressed = Input.GetButtonDown(slideButton);
			ATTACK_wasPressed = Input.GetButtonDown(attackButton);
#endif
		}

		controller.Input(moveStick, JUMP_isPressed, JUMP_wasPressed, SLIDE_wasPressed, ATTACK_wasPressed);
	}
}
