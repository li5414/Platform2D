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

using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using Spine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GunmanController : GameCharacter {
	static Vector2 _v2down = new Vector2(0, -1);

	public enum ActionState { IDLE, WALK, RUN, JUMP, FALL, JETPACK }

	public ActionState state {
		get {
			return _state;
		}
		set {
			if (_state != value) {
				_state = value;
			}
		}
	}
	private ActionState _state;

	[Header("Input")]
	public float deadZone = 0.05f;
	public float runThreshhold = 0.5f;
	public float aimDeadZone = 0.05f;

	[Header("Raycasting")]
	public LayerMask characterMask;
	[HideInInspector]
	public LayerMask currentMask;
	public LayerMask groundMask;
	public LayerMask passThroughMask;

	[Header("Speeds & Timings")]
	public float walkSpeed;
	public float runSpeed;
	public float jumpSpeed;
	public float airJumpSpeed;
	public float enemyBounceSpeed;
	public float jumpDuration = 0.5f;
	[Range(1, 3)]
	public int maxJumps = 1;
	public float jetpackStartSpeed = 5;
	public float jetpackThrust = 5;
	public float jetpackDampener = 0.8f;
	public float jetpackDescentSpeed = -2;
	public float jetpackDuration = 2;
	public float jetpackFuel = 2;
	public float jetpackRecovery = 1;

	[Header("Physics")]
	//additional fall gravity to feel more platformy
	public float fallGravity = -4;
	public float idleFriction = 4;
	public float movingFriction = 0;

	[Header("Animations")]
	[SpineAnimation]
	public string walkAnim;
	[SpineAnimation]
	public string walkBackwardAnim;
	[SpineAnimation]
	public string runAnim;
	[SpineAnimation]
	public string idleAnim;
	[SpineAnimation]
	public string jumpAnim;
	[SpineAnimation]
	public string fallAnim;
	[SpineAnimation]
	public string jetpackNeutralAnim;
	[SpineAnimation]
	public string jetpackForwardAnim;
	[SpineAnimation]
	public string jetpackBackwardAnim;

	[Header("Sounds")]
	public string footstepSound;
	public string landSound;
	public string jumpSound;

	[Header("References")]
	public PolygonCollider2D primaryCollider;
	public SkeletonAnimation skeletonAnimation;
	public GameObject groundJumpPrefab;
	public GameObject airJumpPrefab;
	[SpineBone(dataField: "skeletonAnimation")]
	public string footEffectBone;
	public Transform graphicsRoot;
	public SkeletonUtilityBone aimPivotBone;
	public Thruster thruster;

	[Header("Weapons")]
	public bool allowRunAim = false;
	//public Weapon[] weapons;
	public List<GunmanWeapon> weapons;

	public Rigidbody2D RB {
		get {
			return this.rb;
		}
	}

	public bool Flipped {
		get {
			return this.flipped;
		}
	}

	//injection
	public System.Action<GunmanController> HandleInput;

	//events
	public System.Action<Transform> OnFootstep;
	public System.Action<Transform> OnJump;

	Rigidbody2D rb;
	PhysicsMaterial2D characterColliderMaterial;
	Vector2 moveStick;
	Vector2 aimStick;
	bool doJump;
	bool jumpPressed;
	float jumpStartTime;
	bool doPassthrough;
	bool runPressed = false;
	OneWayPlatform passThroughPlatform;
	OneWayPlatform insidePlatform;
	MovingPlatform movingPlatform;
	bool onIncline;
	int jumpCount = 0;
	bool flipped;
	bool aiming;

	Vector3 backGroundCastOrigin;
	Vector3 centerGroundCastOrigin;
	Vector3 forwardGroundCastOrigin;

	GunmanWeapon currentWeapon;
	
	public override void IgnoreCollision (Collider2D collider, bool ignore) {
		Physics2D.IgnoreCollision(primaryCollider, collider, ignore);
	}

	void Start () {
		rb = GetComponent<Rigidbody2D>();

		skeletonAnimation.state.Event += HandleEvent;
		skeletonAnimation.state.Complete += HandleComplete;

		CalculateRayBounds(primaryCollider);

		if (primaryCollider.sharedMaterial == null)
			characterColliderMaterial = new PhysicsMaterial2D("CharacterColliderMaterial");
		else
			characterColliderMaterial = Instantiate(primaryCollider.sharedMaterial);

		primaryCollider.sharedMaterial = characterColliderMaterial;

		currentMask = groundMask;

		foreach (GunmanWeapon w in weapons)
			w.CacheSpineAnimations(skeletonAnimation.skeleton.Data);

		EquipWeapon(weapons[0]);


	}

	void EquipWeapon (GunmanWeapon weapon) {
		var skeleton = skeletonAnimation.skeleton;
		weapon.SetupAnim.Apply(skeleton, 0, 1, false, null);
		skeletonAnimation.state.SetAnimation(1, weapon.IdleAnim, true);
		currentWeapon = weapon;
		currentWeapon.Setup();
	}

	//Calculate where the collision rays should be based on a polygon collider
	void CalculateRayBounds (PolygonCollider2D coll) {
		Bounds b = coll.bounds;
		Vector3 min = transform.InverseTransformPoint(b.min);
		Vector3 center = transform.InverseTransformPoint(b.center);
		Vector3 max = transform.InverseTransformPoint(b.max);

		backGroundCastOrigin.x = min.x;
		backGroundCastOrigin.y = min.y + 0.1f;

		centerGroundCastOrigin.x = center.x;
		centerGroundCastOrigin.y = min.y + 0.1f;

		forwardGroundCastOrigin.x = max.x;
		forwardGroundCastOrigin.y = min.y + 0.1f;
	}

	//Handle Spine Animation Complete callbacks
	void HandleComplete (Spine.AnimationState state, int trackIndex, int loopCount) {
		var entry = state.GetCurrent(trackIndex);
		if (entry.Animation == currentWeapon.ReloadAnim) {
			currentWeapon.Reload();
			currentWeapon.reloadLock = false;
		}
	}

	//Handle Spine Event callback
	//Used to apply various keyed effects, velocity augmentations, sounds, so on.
	bool doRecoil;
	void HandleEvent (Spine.AnimationState state, int trackIndex, Spine.Event e) {
		var entry = state.GetCurrent(trackIndex);
		if (entry != null) {
			switch (e.Data.Name) {
				case "Fire":
					currentWeapon.Fire();
					if (this.state == ActionState.JETPACK) {
						doRecoil = true;
					}

					break;
				case "Sound":
					SoundPalette.PlaySound(e.String, 1f, 1, transform.position);
					break;
				case "EjectCasing":
					Instantiate(currentWeapon.casingPrefab, currentWeapon.casingEjectPoint.position, Quaternion.LookRotation(Vector3.forward, currentWeapon.casingEjectPoint.up));
					break;
				case "Effect":
					switch (e.String) {
						case "GroundJump":
							if (groundJumpPrefab && OnGround)
								SpawnAtFoot(groundJumpPrefab, Quaternion.identity, new Vector3(flipped ? -1 : 1, 1, 1));
							break;
					}
					break;
			}
		}
	}

	//Graphics loop
	void Update () {
		if (HandleInput != null)
			HandleInput(this);

		UpdateAnim();
	}

	//Physics loop
	void FixedUpdate () {
		HandlePhysics();
	}

	void DoPassThrough (OneWayPlatform platform) {
		StartCoroutine(PassthroughRoutine(platform));
	}

	IEnumerator PassthroughRoutine (OneWayPlatform platform) {
		currentMask = passThroughMask;
		Physics2D.IgnoreCollision(primaryCollider, platform.collider, true);
		passThroughPlatform = platform;
		yield return new WaitForSeconds(0.5f);
		Physics2D.IgnoreCollision(primaryCollider, platform.collider, false);
		currentMask = groundMask;
		passThroughPlatform = null;
	}

	public void Input (Vector2 moveStick, Vector2 aimStick, bool JUMP_isPressed, bool JUMP_wasPressed, bool FIRE_wasPressed, bool PREVIOUS_wasPressed, bool NEXT_wasPressed) {
		bool useKeyboard = false;

		if (((OnGround || movingPlatform) && state < ActionState.JUMP) || (state == ActionState.FALL/* Jetpack fuel here*/)) {
			if (!jumpPressed) {

				if (JUMP_wasPressed && this.passThroughPlatform == null && moveStick.y < -0.25f) {
					var platform = PlatformCast(centerGroundCastOrigin);
					if (platform != null)
						DoPassThrough(platform);
					else {
						doJump = true;
						jumpPressed = true;
					}
				} else {
					doJump = JUMP_wasPressed;
					if (doJump) {
						jumpPressed = true;
					}
				}
			}
		} else {
			if (state == ActionState.JUMP || state == ActionState.JETPACK) {
				if (jumpCount >= maxJumps) {
					//do jetpack
					if (JUMP_wasPressed) {
						jumpPressed = true;
						doJump = true;
					}
				}
			}
		}

		jumpPressed = JUMP_isPressed;


		//gun handling
		bool flip = false;
		if (moveStick.x > deadZone) {
			//do nothing
		} else if (moveStick.x < -deadZone) {
			flip = true;
		}

		if (aiming && !currentWeapon.reloadLock && FIRE_wasPressed && currentWeapon.clip > 0 && Time.time >= currentWeapon.nextFireTime) {
			skeletonAnimation.state.SetAnimation(1, currentWeapon.FireAnim, false);
			currentWeapon.nextFireTime = Time.time + currentWeapon.refireRate;
		} else if (!currentWeapon.reloadLock && Time.time >= currentWeapon.nextFireTime && FIRE_wasPressed) {
			if (currentWeapon.ammo > 0 && currentWeapon.clip < currentWeapon.clipSize) {
				skeletonAnimation.state.SetAnimation(1, currentWeapon.ReloadAnim, false);
				currentWeapon.reloadLock = true;
			}
		}

		var entry = skeletonAnimation.state.GetCurrent(1);
		if (!currentWeapon.reloadLock && (aimStick.magnitude > aimDeadZone || (useKeyboard && !runPressed))) {
			aiming = true;
			if (entry == null || entry.Animation != currentWeapon.FireAnim && entry.Animation != currentWeapon.AimAnim) {
				skeletonAnimation.state.SetAnimation(1, currentWeapon.AimAnim, true);
			}

			float a = Mathf.Atan2(aimStick.y, aimStick.x) * Mathf.Rad2Deg;
			if (a < 0)
				a += 360;

			if (a < 270 && a > 90)
				flip = true;
			else
				flip = false;

			flipped = flip;

			float minAngle = currentWeapon.minAngle;
			float maxAngle = currentWeapon.maxAngle;

			a = flip ? 180 + Mathf.Clamp(Mathf.DeltaAngle(0, a - 180), -maxAngle, -minAngle) : Mathf.Clamp(Mathf.DeltaAngle(0, a), minAngle, maxAngle);

			aimPivotBone.transform.localRotation = Quaternion.RotateTowards(aimPivotBone.transform.localRotation, Quaternion.AngleAxis(flip ? 180 - a : a, Vector3.forward), 300 * Time.deltaTime);
		} else {
			aiming = false;
			aimPivotBone.transform.localRotation = Quaternion.Slerp(aimPivotBone.transform.localRotation, Quaternion.AngleAxis(0, Vector3.forward), 10 * Time.deltaTime);

			//TODO: automatic revert to aiming if firing without holding aim stick
			if (!currentWeapon.reloadLock && (entry == null || entry.Animation != currentWeapon.FireAnim && entry.Animation != currentWeapon.IdleAnim) ) {
				skeletonAnimation.state.SetAnimation(1, currentWeapon.IdleAnim, true);
			}

			if (moveStick.magnitude > deadZone)
				flipped = flip;
		}


		if (NEXT_wasPressed && !currentWeapon.reloadLock && currentWeapon.nextFireTime < Time.time) {
			int idx = weapons.IndexOf(currentWeapon);
			idx++;
			if (idx == weapons.Count)
				idx = 0;

			EquipWeapon(weapons[idx]);
		} else if (PREVIOUS_wasPressed && !currentWeapon.reloadLock && currentWeapon.nextFireTime < Time.time) {
			int idx = weapons.IndexOf(currentWeapon);
			idx--;
			if (idx < 0)
				idx = weapons.Count - 1;

			EquipWeapon(weapons[idx]);
		}

		this.moveStick = moveStick;
		this.aimStick = aimStick;

		graphicsRoot.localRotation = Quaternion.Euler(0, flipped ? 180 : 0, 0);
	}

#if UNITY_EDITOR
	void OnDrawGizmos () {
		Handles.Label(transform.position, state.ToString());
		if (!Application.isPlaying)
			return;

		if (OnGround)
			Gizmos.color = Color.green;
		else
			Gizmos.color = Color.grey;

		Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));

		Gizmos.DrawWireSphere(transform.TransformPoint(centerGroundCastOrigin), 0.07f);
		Gizmos.DrawWireSphere(transform.TransformPoint(backGroundCastOrigin), 0.07f);
		Gizmos.DrawWireSphere(transform.TransformPoint(forwardGroundCastOrigin), 0.07f);

		Gizmos.DrawLine(transform.TransformPoint(centerGroundCastOrigin), transform.TransformPoint(centerGroundCastOrigin + new Vector3(0, -0.15f, 0)));
		Gizmos.DrawLine(transform.TransformPoint(backGroundCastOrigin), transform.TransformPoint(backGroundCastOrigin + new Vector3(0, -0.15f, 0)));
		Gizmos.DrawLine(transform.TransformPoint(forwardGroundCastOrigin), transform.TransformPoint(forwardGroundCastOrigin + new Vector3(0, -0.15f, 0)));


	}
#endif

	bool OnGround {
		get {
			return BackOnGround || CenterOnGround || ForwardOnGround;
		}

	}

	bool BackOnGround {
		get {
			return GroundCast(flipped ? forwardGroundCastOrigin : backGroundCastOrigin);
		}
	}

	bool ForwardOnGround {
		get {
			return GroundCast(flipped ? backGroundCastOrigin : forwardGroundCastOrigin);
		}
	}

	bool CenterOnGround {
		get {
			return GroundCast(centerGroundCastOrigin);
		}
	}

	//Detect being on top of a characters's head
	Rigidbody2D OnTopOfCharacter () {
		Rigidbody2D character = GetRelevantCharacterCast(centerGroundCastOrigin, 0.15f);
		if (character == null)
			character = GetRelevantCharacterCast(backGroundCastOrigin, 0.15f);
		if (character == null)
			character = GetRelevantCharacterCast(forwardGroundCastOrigin, 0.15f);

		return character;
	}

	//Raycasting stuff
	Rigidbody2D GetRelevantCharacterCast (Vector3 origin, float dist) {
		RaycastHit2D[] hits = Physics2D.RaycastAll(transform.TransformPoint(origin), _v2down, dist, characterMask);
		if (hits.Length > 0) {
			int index = 0;

			if (hits[0].rigidbody == rb) {
				if (hits.Length == 1)
					return null;

				index = 1;
			}
			if (hits[index].rigidbody == rb)
				return null;

			var hit = hits[index];
			if (hit.collider != null && hit.collider.attachedRigidbody != null) {
				return hit.collider.attachedRigidbody;
			}
		}

		return null;
	}

	//see if character is on the ground
	//throw onIncline flag
	bool GroundCast (Vector3 origin) {
		RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(origin), _v2down, 0.15f, currentMask);
		if (hit.collider != null && !hit.collider.isTrigger) {
			if (hit.normal.y < 0.4f)
				return false;
			else if (hit.normal.y < 0.95f)
				onIncline = true;


			return true;
		}


		return false;
	}

	//see if character is on a one-way platform
	OneWayPlatform PlatformCast (Vector3 origin) {
		OneWayPlatform platform = null;
		RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(origin), _v2down, 0.5f, currentMask);
		if (hit.collider != null && !hit.collider.isTrigger) {
			platform = hit.collider.GetComponent<OneWayPlatform>();
		}


		return platform;
	}

	//see if character is on a moving platform
	MovingPlatform MovingPlatformCast (Vector3 origin) {
		MovingPlatform platform = null;
		RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(origin), _v2down, 0.5f, currentMask);
		if (hit.collider != null && !hit.collider.isTrigger) {
			platform = hit.collider.GetComponent<MovingPlatform>();
		}


		return platform;
	}

	//cache x velocity to ensure speed restores after heavy impact that results in physics penalties
	float savedXVelocity;
	bool jetpackLatch;
	void HandlePhysics () {
		onIncline = false;

		float x = moveStick.x;
		float y = moveStick.y;

		float absX = Mathf.Abs(x);
		float xVelocity = 0;
		float platformXVelocity = 0;
		float platformYVelocity = 0;
		Vector2 velocity = rb.velocity;

		movingPlatform = MovingPlatformCast(centerGroundCastOrigin);

		if (movingPlatform) {
			platformXVelocity = movingPlatform.Velocity.x;
			platformYVelocity = movingPlatform.Velocity.y;
		}

		if (doJump && jumpCount >= maxJumps) {
			if (state == ActionState.JETPACK) {
				//stop jetpacking
				state = velocity.y > 0 ? ActionState.JUMP : ActionState.FALL;
				doJump = false;
				thruster.goalThrust = 0;
			} else {
				if (jetpackFuel > 0) {
					//start jetpacking
					state = ActionState.JETPACK;
					jumpStartTime = Time.time;
					doJump = false;
					velocity.y = jetpackStartSpeed;
					thruster.goalThrust = 1;
					SetFriction(movingFriction);
					jetpackLatch = false;
				} else {
					doJump = false;
				}

			}

		} else if (doJump && state != ActionState.JUMP) {
			SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);


			velocity.y = (jumpCount > 0 ? airJumpSpeed : jumpSpeed) + (platformYVelocity >= 0 ? platformYVelocity : 0);
			jumpStartTime = Time.time;
			state = ActionState.JUMP;
			doJump = false;
			if (airJumpPrefab != null && jumpCount > 0)
				Instantiate(airJumpPrefab, transform.position, Quaternion.identity);
			else if (groundJumpPrefab != null && jumpCount == 0) {
				SpawnAtFoot(groundJumpPrefab, Quaternion.identity, new Vector3(flipped ? -1 : 1, 1, 1));
			}
			jumpCount++;



			if (OnJump != null)
				OnJump(transform);
		}

		//ground logic
		if (state < ActionState.JUMP) {
			if (OnGround || movingPlatform) {
				jetpackFuel = Mathf.MoveTowards(jetpackFuel, jetpackDuration, Time.deltaTime * jetpackRecovery);
				jumpCount = 0;
				if (absX > runThreshhold && (!aiming || allowRunAim)) {
					xVelocity = runSpeed * Mathf.Sign(x);
					velocity.x = Mathf.MoveTowards(velocity.x, xVelocity + platformXVelocity, Time.deltaTime * 15);
					if (movingPlatform)
						velocity.y = platformYVelocity;
					state = ActionState.RUN;
					SetFriction(movingFriction);
				} else if (absX > deadZone) {
					xVelocity = walkSpeed * Mathf.Sign(x);
					velocity.x = Mathf.MoveTowards(velocity.x, xVelocity + platformXVelocity, Time.deltaTime * 25);
					if (movingPlatform)
						velocity.y = platformYVelocity;
					state = ActionState.WALK;
					SetFriction(movingFriction);
				} else {
					velocity.x = movingPlatform ? platformXVelocity : Mathf.MoveTowards(velocity.x, 0, Time.deltaTime * 10);
					if (movingPlatform)
						velocity.y = platformYVelocity;
					state = ActionState.IDLE;
					SetFriction(movingPlatform ? movingFriction : idleFriction);
				}
			} else {
				SetFallState(true);
			}
			//air logic
		} else if (state == ActionState.JUMP) {
			float jumpTime = Time.time - jumpStartTime;
			savedXVelocity = velocity.x;
			if (!jumpPressed || jumpTime >= jumpDuration) {
				jumpStartTime -= jumpDuration;

				if (velocity.y > 0)
					velocity.y = Mathf.MoveTowards(velocity.y, 0, Time.deltaTime * 30);

				if (velocity.y <= 0 || (jumpTime < jumpDuration && OnGround)) {
					SetFallState(false);
				}
			}

			//fall logic
		} else if (state == ActionState.FALL) {

			if (OnGround) {
				SoundPalette.PlaySound(landSound, 1, 1, transform.position);
				if (absX > runThreshhold) {
					velocity.x = savedXVelocity;
					state = ActionState.RUN;
				} else if (absX > deadZone) {
					velocity.x = savedXVelocity;
					state = ActionState.WALK;
				} else {
					velocity.x = savedXVelocity;
					state = ActionState.IDLE;
				}
			} else {
				EnemyBounceCheck(ref velocity);
				savedXVelocity = velocity.x;

			}

		}

		//air control
		if (state == ActionState.JUMP || state == ActionState.FALL) {
			if (absX > runThreshhold) {
				velocity.x = Mathf.MoveTowards(velocity.x, runSpeed * Mathf.Sign(x), Time.deltaTime * 8);
			} else if (absX > deadZone) {
				velocity.x = Mathf.MoveTowards(velocity.x, walkSpeed * Mathf.Sign(x), Time.deltaTime * 8);
			} else {
				velocity.x = Mathf.MoveTowards(velocity.x, 0, Time.deltaTime * 8);
			}

			if (state == ActionState.JUMP || state == ActionState.FALL) {

			}
		} else if (state == ActionState.JETPACK) {
			float jetpackMultiplier = jetpackFuel > 0 ? 1 : 0.2f;
			velocity.x = Mathf.MoveTowards(velocity.x, moveStick.x * jetpackThrust * jetpackMultiplier, Time.deltaTime * 10);

			if (velocity.y > jetpackDescentSpeed)
				velocity.y += (-Physics2D.gravity.y * rb.gravityScale) * Time.deltaTime * jetpackDampener; //offset gravity
			else {
				velocity.y += (-Physics2D.gravity.y * rb.gravityScale) * Time.deltaTime * 1.2f; //offset gravity stronger
			}


			float fuelSpend = (Mathf.Abs(moveStick.x) + Mathf.Clamp01(moveStick.y) * 1.5f) / 2;
			if (!jetpackLatch && jumpPressed)
				fuelSpend = Mathf.Clamp(fuelSpend, 0.75f, 10);

			jetpackFuel -= fuelSpend * Time.deltaTime;
			jetpackFuel = Mathf.Clamp(jetpackFuel, 0, jetpackDuration);
			if (jetpackFuel > 0)
				thruster.goalThrust = Mathf.Lerp(0.15f, 1f, fuelSpend);
			else
				thruster.goalThrust = 0.1f;
			if (Mathf.Abs(moveStick.y) < deadZone) {
				//nothin
				if (!jetpackLatch) {
					if (!jumpPressed)
						jetpackLatch = true;
					else if (jetpackFuel > 0)
						velocity.y = Mathf.MoveTowards(velocity.y, jetpackThrust, Time.deltaTime * 15);
				}
			} else {
				float jetpackY = y;
				if (jumpPressed)
					jetpackY = 1;

				if (jetpackFuel == 0 && jetpackY > 0) {
					jetpackY = 0;
					thruster.goalThrust = 0.15f;

				} else {
					velocity.y = Mathf.MoveTowards(velocity.y, jetpackY * jetpackThrust, Time.deltaTime * 15);
				}
			}

			if (doRecoil) {
				var recoil = currentWeapon.GetRecoil();
				velocity.x += recoil.x;
				velocity.y += recoil.y;
				doRecoil = false;
			}

			if (Time.time > jumpStartTime + 0.25f && OnGround) {
				if (velocity.y < 2f) {
					state = ActionState.IDLE;
					thruster.goalThrust = 0;
				}
			}
		}

		//falling and wallslide
		if (state == ActionState.FALL)
			velocity.y += fallGravity * Time.deltaTime;

		//generic motion flipping control
		/*  handled in ProcessInput for this character due to aiming */

		rb.velocity = velocity;
	}

	//Bounce off a player in an angry way
	bool EnemyBounceCheck (ref Vector2 velocity) {
		var character = OnTopOfCharacter();
		if (character != null) {
			SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
			character.SendMessage("Hit", 1, SendMessageOptions.DontRequireReceiver);
			velocity.y = enemyBounceSpeed;
			jumpStartTime = Time.time;
			state = ActionState.JUMP;
			doJump = false;
			SetFriction(movingFriction);
			return true;
		}
		return false;
	}

	//Enter the fall state, optionally using a jump counter.  IE: to prevent jumping after slipping off a platform
	void SetFallState (bool useJump) {
		if (useJump)
			jumpCount = 1;

		state = ActionState.FALL;
	}

	//Sync the Spine Animation with the current ActionState. Handle a few "edge" cases to do with inclines and uh... edges.
	void UpdateAnim () {
		switch (state) {
			case ActionState.IDLE:
				if (CenterOnGround) {
					skeletonAnimation.AnimationName = idleAnim;
				} else {
					//TODO:  deal with edge animations for this character rig
					if (onIncline)
						skeletonAnimation.AnimationName = idleAnim;
					else if (BackOnGround) {
						skeletonAnimation.AnimationName = idleAnim;
					} else if (ForwardOnGround) {
						skeletonAnimation.AnimationName = idleAnim;
					}
				}
				break;
			case ActionState.WALK:
				if (aiming) {
					if (Mathf.Sign(aimStick.x) != Mathf.Sign(moveStick.x))
						skeletonAnimation.AnimationName = walkBackwardAnim;
					else
						skeletonAnimation.AnimationName = walkAnim;
				} else {
					skeletonAnimation.AnimationName = walkAnim;
				}

				break;
			case ActionState.RUN:
				skeletonAnimation.AnimationName = runAnim;
				break;
			case ActionState.JUMP:
				skeletonAnimation.AnimationName = jumpAnim;
				break;
			case ActionState.FALL:
				skeletonAnimation.AnimationName = fallAnim;
				break;
			case ActionState.JETPACK:
				if (moveStick.x > deadZone)
					skeletonAnimation.AnimationName = flipped ? jetpackBackwardAnim : jetpackForwardAnim;
				else if (moveStick.x < -deadZone)
					skeletonAnimation.AnimationName = flipped ? jetpackForwardAnim : jetpackBackwardAnim;
				else
					skeletonAnimation.AnimationName = jetpackNeutralAnim;
				break;
		}
	}

	//work-around for Box2D not updating friction values at runtime...
	void SetFriction (float friction) {
		if (friction != characterColliderMaterial.friction) {
			characterColliderMaterial.friction = friction;
			primaryCollider.gameObject.SetActive(false);
			primaryCollider.gameObject.SetActive(true);
		}
	}


	//TODO:  deal with SetFriction workaround breaking ignore pairs.........
	void IgnoreCharacterCollisions (bool ignore) {
		foreach (GameCharacter gc in All)
			if (gc == this)
				continue;
			else {
				gc.IgnoreCollision(primaryCollider, ignore);
			}
	}

	void SpawnAtFoot (GameObject prefab, Quaternion rotation, Vector3 scale) {
		var bone = skeletonAnimation.Skeleton.FindBone(footEffectBone);
		Vector3 pos = skeletonAnimation.transform.TransformPoint(bone.WorldX, bone.WorldY, 0);
		((GameObject)Instantiate(prefab, pos, rotation)).transform.localScale = scale;
	}
}
