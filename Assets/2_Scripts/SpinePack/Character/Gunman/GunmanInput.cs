using UnityEngine;
using System.Collections;
#if IN_CONTROL
using InControl;
#elif REWIRED
using Rewired;
#endif

public class GunmanInput : MonoBehaviour {

	public bool useKeyboard;

	public bool singleStick;
	public bool singleStickAiming;
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
	public string xAxisMove;
	[InputAxis]
	public string yAxisMove;
	[InputAxis]
	public string xAxisAim;
	[InputAxis]
	public string yAxisAim;
	[InputAxis]
	public string jumpButton;
	[InputAxis]
	public string fireButton;
	[InputAxis]
	public string fireTrigger;
	[InputAxis]
	public string previousButton;
	[InputAxis]
	public string nextButton;

	float previousFireTriggerValue;
#endif

	[Header("Mouse & Keyboard")]
#if !REWIRED
	public KeyCode leftKey = KeyCode.A;
	public KeyCode rightKey = KeyCode.D;
	public KeyCode upKey = KeyCode.W;
	public KeyCode downKey = KeyCode.S;
	public KeyCode jumpKey = KeyCode.Space;
	public KeyCode attackKey = KeyCode.Mouse0;
	public KeyCode runKey = KeyCode.LeftShift;
	public KeyCode nextKey = KeyCode.E;
	public KeyCode previousKey = KeyCode.Q;
	public string weaponChangeAxis = "Mouse ScrollWheel";
#endif

	Plane mouseCastPlane;

	void Start () {
#if IN_CONTROL
		if (InputManager.Devices.Count - 1 < deviceIndex)
			deviceIndex = -1;
#endif
		mouseCastPlane = new Plane(Vector3.forward, transform.position);

		GetComponent<GunmanController>().HandleInput += HandleInput;
	}

	void HandleInput (TempGameCharacter gamecontroller) {

		GunmanController controller = gamecontroller as GunmanController;
		Vector2 moveStick = Vector2.zero;
		Vector2 aimStick = Vector2.zero;
		bool JUMP_wasPressed = false;
		bool JUMP_isPressed = false;
		bool FIRE_wasPressed = false;
		bool NEXT_wasPressed = false;
		bool PREVIOUS_wasPressed = false;

		//move handling
		if (useKeyboard) {
#if REWIRED
			var player = ReInput.players.GetPlayer(playerId);
			bool runPressed = player.GetButton("Run");
			if (controller.state != ActionState.JETPACK) {
				moveStick.x = player.GetAxis("Move X") * (runPressed ? 1 : 0.5f);
			} else {
				moveStick.x = player.GetAxis("Move X");
			}

			moveStick.y = player.GetAxis("Move Y");

			aimStick.x = player.GetAxis("Aim X");
			aimStick.y = player.GetAxis("Aim Y");
			JUMP_wasPressed = player.GetButtonDown("Jump");
			JUMP_isPressed = player.GetButton("Jump");
			FIRE_wasPressed = player.GetButtonDown("Attack");
			NEXT_wasPressed = player.GetButtonDown("Next");
			PREVIOUS_wasPressed = player.GetButtonDown("Previous");
			
#else
			//movement
			bool runPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			if (controller.state != ActionState.JETPACK)
				moveStick.x = ((Input.GetKey(leftKey) ? -1 : 0) + (Input.GetKey(rightKey) ? 1 : 0)) * (runPressed ? 1 : 0.5f);
			else
				moveStick.x = ((Input.GetKey(leftKey) ? -1 : 0) + (Input.GetKey(rightKey) ? 1 : 0));

			moveStick.y = ((Input.GetKey(downKey) ? -1 : 0) + (Input.GetKey(upKey) ? 1 : 0));

			JUMP_wasPressed = Input.GetKeyDown(jumpKey);
			JUMP_isPressed = Input.GetKey(jumpKey);

			//weapon change

			float scroll = Input.GetAxis(weaponChangeAxis);
			if (Input.GetKeyDown(nextKey))
				scroll = 1;
			else if (Input.GetKeyDown(previousKey))
				scroll = -1;

			if (scroll > 0) {
				NEXT_wasPressed = true;
				PREVIOUS_wasPressed = false;
			} else if (scroll < 0) {
				PREVIOUS_wasPressed = true;
				NEXT_wasPressed = false;
			} else {
				NEXT_wasPressed = false;
				PREVIOUS_wasPressed = false;
			}

			FIRE_wasPressed = Input.GetKeyDown(attackKey);
#endif


			//aiming
			if (!runPressed || controller.state == ActionState.JETPACK) {
				float dist = 0;
				var aimRay = Camera.main.ScreenPointToRay(Input.mousePosition);

				if (mouseCastPlane.Raycast(aimRay, out dist)) {
					Vector2 aimPivotPos = controller.aimPivotBone.transform.position;
					Vector2 targetPos = aimRay.GetPoint(dist);
					aimStick = (targetPos - aimPivotPos).normalized;
				}
			} else {
				aimStick = Vector2.zero;
			}
		} else {
#if IN_CONTROL
			var device = deviceIndex == -1 ? InputManager.ActiveDevice : InputManager.Devices[deviceIndex];
			moveStick = device.Direction.Vector;
			if (singleStick) {
				
					if (moveStick.sqrMagnitude > 0) {
						if(singleStickAiming){
							//aimStick = moveStick;
							if (moveStick.x > 0)
								aimStick.x = 1;
							else if (moveStick.x < 0)
								aimStick.x = -1;
							else
								aimStick.x = controller.Flipped ? -1 : 1;

							if (moveStick.y > 0.5f) {
								aimStick.y = 1;
							} else if (moveStick.y < -0.5f) {
								aimStick.y = -1;
							} else {
								aimStick.y = 0;
								
							}
								

						}
						else {
							//aimStick.x = Mathf.Approximately(moveStick.x, 0) ? (controller.Flipped ? -1 : 1) : moveStick.x;
							if (moveStick.x > 0)
								aimStick.x = 1;
							else if (moveStick.x < 0)
								aimStick.x = -1;
							else
								aimStick.x = controller.Flipped ? -1 : 1;
							aimStick.y = 0;
						}
					} else {
						aimStick.x = controller.Flipped ? -1 : 1;
						aimStick.y = 0;
					}
				
				
			} else {
				aimStick = device.RightStick.Vector;
			}

			JUMP_wasPressed = device.Action1.WasPressed || device.LeftTrigger.WasPressed;
			JUMP_isPressed = device.Action1.IsPressed || device.LeftTrigger.IsPressed;
			PREVIOUS_wasPressed = device.LeftBumper.WasPressed;
			NEXT_wasPressed = device.RightBumper.WasPressed;
			FIRE_wasPressed = device.RightTrigger.WasPressed;
#elif REWIRED
			var player = ReInput.players.GetPlayer(playerId);
			moveStick.x = player.GetAxis("Move X");
			moveStick.y = player.GetAxis("Move Y");

				if (singleStick) {
				
					if (moveStick.sqrMagnitude > 0) {
						if(singleStickAiming){
							if (moveStick.x > 0)
								aimStick.x = 1;
							else if (moveStick.x < 0)
								aimStick.x = -1;
							else
								aimStick.x = controller.Flipped ? -1 : 1;

							if (moveStick.y > 0.5f) {
								aimStick.y = 1;
							} else if (moveStick.y < -0.5f) {
								aimStick.y = -1;
							} else {
								aimStick.y = 0;
								
							}
						}
						else {
							if (moveStick.x > 0)
								aimStick.x = 1;
							else if (moveStick.x < 0)
								aimStick.x = -1;
							else
								aimStick.x = controller.Flipped ? -1 : 1;
							aimStick.y = 0;
						}
					} else {
						aimStick.x = controller.Flipped ? -1 : 1;
						aimStick.y = 0;
					}
			} else {
				aimStick.x = player.GetAxis("Aim X");
				aimStick.y = player.GetAxis("Aim Y");
			}

			JUMP_wasPressed = player.GetButtonDown("Jump");
			JUMP_isPressed = player.GetButton("Jump");
			FIRE_wasPressed = player.GetButtonDown("Attack");
			NEXT_wasPressed = player.GetButtonDown("Next");
			PREVIOUS_wasPressed = player.GetButtonDown("Previous");
#else
			moveStick = new Vector2(Input.GetAxis(xAxisMove), Input.GetAxis(yAxisMove));

			if (singleStick) {

				if (moveStick.sqrMagnitude > 0) {
					if (singleStickAiming) {
						if (moveStick.x > 0)
							aimStick.x = 1;
						else if (moveStick.x < 0)
							aimStick.x = -1;
						else
							aimStick.x = controller.Flipped ? -1 : 1;

						if (moveStick.y > 0.5f) {
							aimStick.y = 1;
						} else if (moveStick.y < -0.5f) {
							aimStick.y = -1;
						} else {
							aimStick.y = 0;

						}
					} else {
						if (moveStick.x > 0)
							aimStick.x = 1;
						else if (moveStick.x < 0)
							aimStick.x = -1;
						else
							aimStick.x = controller.Flipped ? -1 : 1;
						aimStick.y = 0;
					}
				} else {
					aimStick.x = controller.Flipped ? -1 : 1;
					aimStick.y = 0;
				}


			} else {
				aimStick = new Vector2(Input.GetAxis(xAxisAim), Input.GetAxis(yAxisAim));
			}

			JUMP_wasPressed = Input.GetButtonDown(jumpButton);
			JUMP_isPressed = Input.GetButton(jumpButton);
			if (previousButton != "")
				PREVIOUS_wasPressed = Input.GetButtonDown(previousButton);
			if (nextButton != "")
				NEXT_wasPressed = Input.GetButtonDown(nextButton);

			if (fireButton != "")
				FIRE_wasPressed = Input.GetButtonDown(fireButton);

			if (fireTrigger != "") {
				//TODO: threshhold?
				if (previousFireTriggerValue < 0.05f && Input.GetAxis(fireTrigger) > 0)
					FIRE_wasPressed = true;

				previousFireTriggerValue = Input.GetAxis(fireTrigger);
			}

#endif
		}

		controller.Input(moveStick, aimStick, JUMP_isPressed, JUMP_wasPressed, FIRE_wasPressed, PREVIOUS_wasPressed, NEXT_wasPressed);
	}
}
