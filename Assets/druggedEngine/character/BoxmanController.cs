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


#if UNITY_EDITOR
using UnityEditor;
#endif

public class HitmanController : GameCharacter {

	//for some reason Vector2.down doesn't exist in most Unity versions
	static Vector2 _v2down = new Vector2(0, -1);

	public enum ActionState { IDLE, WALK, RUN, JUMP, FALL, WALLSLIDE, SLIDE, ATTACK, DOWNATTACK, UPATTACK }

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

	[Header("Raycasting")]
	public LayerMask characterMask;
	[HideInInspector]
	public LayerMask currentMask;
	public LayerMask groundMask;
	public LayerMask passThroughMask;

	[Header("Speeds & Timings")]
	public float walkSpeed = 1;
	public float runSpeed = 5;
	/// <summary>
	/// Initial jump velocity
	/// </summary>
	public float jumpSpeed = 12;
	/// <summary>
	/// Double and beyond jump velocity
	/// </summary>
	public float airJumpSpeed = 10;
	/// <summary>
	/// Jump speed when goomba stomping something
	/// </summary>
	public float headBounceSpeed = 16;
	/// <summary>
	/// How long you can hold onto the jump button to increase jump height
	/// </summary>
	public float jumpDuration = 0.5f;
	/// <summary>
	/// Horizontal speed when wall jumping
	/// </summary>
	public float wallJumpXSpeed = 3;
	/// <summary>
	/// How long a slide lasts
	/// </summary>
	public float slideDuration = 0.5f;
	/// <summary>
	/// How fast a slide is
	/// </summary>
	public float slideVelocity = 6;
	/// <summary>
	/// Y Scale of the Primary Collider during a slide
	/// </summary>
	public float slideSquish = 0.6f;
	/// <summary>
	/// How much velocity to apply during a punch animation when XVelocity Event is fired.
	/// </summary>
	public float punchVelocity = 7;
	/// <summary>
	/// How much velocity to apply during an uppercut when YVelocity Event is fired
	/// </summary>
	public float uppercutVelocity = 5;
	/// <summary>
	/// Downward velocity for duration of Down Attack
	/// </summary>
	public float downAttackVelocity = 20;
	/// <summary>
	/// How long it takes for a punch combo to timeout and go back to idle
	/// </summary>
	public float attackWatchdogDuration = 0.5f;
	/// <summary>
	/// How long to wait before falling off of a wall if no input
	/// </summary>
	public float wallSlideWatchdogDuration = 10f;
	/// <summary>
	/// Fall speed limit when wall sliding
	/// </summary>
	public float wallSlideSpeed = -2;
	/// <summary>
	/// How many times you can jump (1 = no double jumps)
	/// </summary>
	[Range(1, 3)]
	public int maxJumps = 1;

	[Header("Physics")]
	/// <summary>
	/// Additional fall gravity to feel more platformy
	/// </summary>
	public float fallGravity = -4;
	/// <summary>
	/// Friction applied when idle
	/// </summary>
	public float idleFriction = 20;
	/// <summary>
	/// Friction applied when moving
	/// </summary>
	public float movingFriction = 0;


	[Header("Animations")]
	[SpineAnimation]
	public string walkAnim;
	[SpineAnimation]
	public string runAnim;
	[SpineAnimation]
	public string idleAnim;
	[SpineAnimation]
	public string balanceBackward;
	[SpineAnimation]
	public string balanceForward;
	[SpineAnimation]
	public string jumpAnim;
	[SpineAnimation]
	public string fallAnim;
	[SpineAnimation]
	public string wallSlideAnim;
	[SpineAnimation]
	public string slideAnim;
	[SpineAnimation]
	public string attackAnim;
	[SpineAnimation]
	public string downAttackAnim;
	//the frame to skip to when a Down Attack impacts a solid ground
	public float downAttackFrameSkip;
	[SpineAnimation]
	public string upAttackAnim;
	//this animation is used strictly to turn off all attacking bounding box attachment based hitboxes
	[SpineAnimation]
	public string clearAttackAnim;

	[Header("Sounds")]
	public string footstepSound;
	public string landSound;
	public string jumpSound;
	public string slideSound;

	[Header("References")]
	public PolygonCollider2D primaryCollider;
	public SkeletonAnimation skeletonAnimation;
	public SkeletonGhost skeletonGhost;
	public GameObject groundJumpPrefab;
	public GameObject airJumpPrefab;
	[SpineBone(dataField: "skeletonAnimation")]
	public string footEffectBone;
	public GameObject downAttackPrefab;

	public Rigidbody2D RB {
		get {
			return this.rb;
		}
	}

	//input injection
	public System.Action<HitmanController> HandleInput;

	//events
	public System.Action<Transform> OnFootstep;
	public System.Action<Transform> OnJump;

	Rigidbody2D rb;
	PhysicsMaterial2D characterColliderMaterial;
	Vector2 moveStick;
	bool doJump;
	bool jumpPressed;
	float jumpStartTime;
	bool doSlide;
	float slideStartTime;
	bool doPassthrough;
	OneWayPlatform passThroughPlatform;
	MovingPlatform movingPlatform;
	bool onIncline = false;

	bool attackWasPressed;
	bool waitingForAttackInput;
	float attackWatchdog;
	float airControlLockoutTime = 0;
	float wallSlideWatchdog;
	float wallSlideStartTime;
	bool wallSlideFlip;
	bool wasWallJump;
	int jumpCount = 0;
	bool upAttackUsed;
	bool downAttackRecovery = false;
	float downAttackRecoveryTime = 0;


	bool flipped;
	bool velocityLock;

	Vector3 backGroundCastOrigin;
	Vector3 centerGroundCastOrigin;
	Vector3 forwardGroundCastOrigin;
	Vector3 wallCastOrigin;
	float wallCastDistance;

	//box 2d workarounds
	float savedXVelocity;

	void OnEnable () {
		//Add to global GameCharacter list
		Register();
	}

	void OnDisable () {
		//Remove from global GameCharacter list
		Unregister();
	}

	public override void IgnoreCollision (Collider2D collider, bool ignore) {
		Physics2D.IgnoreCollision(primaryCollider, collider, ignore);
	}

	void Start () {
		if (skeletonGhost == null)
			skeletonGhost = skeletonAnimation.GetComponent<SkeletonGhost>();

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

		wallCastOrigin = center;
		wallCastDistance = b.extents.x + 0.1f;
	}

	//Handle Spine Animation Complete callbacks (only to clear attack states)
	void HandleComplete (Spine.AnimationState state, int trackIndex, int loopCount) {
		var entry = state.GetCurrent(trackIndex);
		if (entry.Animation.Name == attackAnim || entry.Animation.Name == upAttackAnim) {
			//attack complete
			skeletonAnimation.AnimationName = idleAnim;
			this.state = ActionState.IDLE;
		}
	}

	//Handle Spine Event callback
	//Used to apply various keyed effects, velocity augmentations, sounds, so on.
	void HandleEvent (Spine.AnimationState state, int trackIndex, Spine.Event e) {
		var entry = state.GetCurrent(trackIndex);
		if (entry != null) {
			if (entry.Animation.Name == attackAnim || entry.Animation.Name == upAttackAnim) {
				switch (e.Data.Name) {
					case "XVelocity":
						Vector2 velocity = rb.velocity;
						velocity.x = flipped ? -punchVelocity * e.Float : punchVelocity * e.Float;
						rb.velocity = velocity;
						break;
					case "YVelocity":
						velocity = rb.velocity;
						velocity.y = uppercutVelocity * e.Float;
						if (movingPlatform)
							velocity.y += movingPlatform.Velocity.y;
						rb.velocity = velocity;
						break;
					case "Pause":
						attackWatchdog = attackWatchdogDuration;
						waitingForAttackInput = true;
						entry.TimeScale = 0;
						break;
				}
			} else if (entry.Animation.Name == downAttackAnim) {
				switch (e.Data.Name) {
					case "YVelocity":
						Vector2 velocity = rb.velocity;
						velocity.y = downAttackVelocity * e.Float;
						rb.velocity = velocity;
						break;
					case "Pause":
						velocityLock = e.Int == 1 ? true : false;
						break;
				}
			}

			switch (e.Data.Name) {
				case "Footstep":
					if (OnFootstep != null)
						OnFootstep(transform);
					break;
				case "Sound":
					SoundPalette.PlaySound(e.String, 1f, 1, transform.position);
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

	public void Input (Vector2 moveStick, bool JUMP_isPressed, bool JUMP_wasPressed, bool SLIDE_wasPressed, bool ATTACK_wasPressed) {
		if (((OnGround || movingPlatform) && state < ActionState.JUMP) || (state == ActionState.FALL && jumpCount < maxJumps)) {
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

			if (!doJump && !doSlide) {
				if (state < ActionState.JUMP) {
					doSlide = SLIDE_wasPressed;
				}
			}
		} else if (OnGround && state == ActionState.SLIDE) {
			doJump = JUMP_wasPressed;
			if (doJump) {
				if (skeletonGhost != null)
					skeletonGhost.ghostingEnabled = false;
				jumpPressed = true;
			}
		} else if (state == ActionState.WALLSLIDE) {
			doJump = JUMP_wasPressed;
			if (doJump) {
				jumpPressed = true;
			}
		}

		this.moveStick = moveStick;
		attackWasPressed = ATTACK_wasPressed;
		jumpPressed = JUMP_isPressed;
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

	public bool OnGround {
		get {
			return CenterOnGround || BackOnGround || ForwardOnGround;
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

	//check to see if pressing against a wall
	public bool PressingAgainstWall {
		get {
			float x = rb.velocity.x;
			bool usingVelocity = true;
			if (Mathf.Abs(x) < 0.1f) {
				x = moveStick.x;
				if (Mathf.Abs(x) <= deadZone) {
					return false;
				} else {
					usingVelocity = false;
				}
			}

			RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(wallCastOrigin), new Vector2(x, 0).normalized, wallCastDistance + (usingVelocity ? x * Time.deltaTime : 0), currentMask);
			if (hit.collider != null && !hit.collider.isTrigger) {
				if (hit.collider.GetComponent<OneWayPlatform>())
					return false;

				return true;
			}

			return false;
		}
	}

	//cache x velocity to ensure speed restores after heavy impact that results in physics penalties
	void HandlePhysics () {
		onIncline = false;

		float axisX = moveStick.x;
		float axisY = moveStick.y;
		float absAxisX = Mathf.Abs(axisX);

		float xSpeed = 0;
		Vector2 nextVelocity = rb.velocity;

		if ("점프가 아닌데 점프를 눌렀어")
        {
			if ("벽타는 중이면" )
            {
				if ( PressingAgainstWall == false ) 
                {
                     //벽을 미는 상태가 아닌 경우만 공중 조작 제한을 건다.
					nextVelocity.x = wallJumpXSpeed * (flipped ? -1 : 1) * 2;
				} else {
					nextVelocity.x = wallJumpXSpeed * (flipped ? -1 : 1);
				}
			}

			float jumpPower = 1f;
			nextVelocity.y = jumpPower + state.PlatformVelocity.y;
			
		} else if ("슬라이딩 시작하면")
        {
			nextVelocity.x = flipped ? -slideVelocity : slideVelocity;
			savedXVelocity = nextVelocity.x;
			nextVelocity.x += state.PlatformVelocity.x;
		}

		//ground logic
		if ("땅")
        {
			if ("움직일때(axisX 가 0이 아닐때)")
			{
				xSpeed = runSpeed * Mathf.Sign(axisX);
				nextVelocity.x = Mathf.MoveTowards(nextVelocity.x, xSpeed + platformXVelocity, Time.deltaTime * 15);
				nextVelocity.y = platformYVelocity;
				SetFriction(movingFriction);
			} 
			else if("움직이지 않을때( axisX가 0 )")
			{
				if( "밟은 땅이 움직이고 있는 경우")
				{
					nextVelocity.x = platformXVelocity;
					SetFriction( movingFriction );
				}
				else if("걍 땅임")
				{
					nextVelocity.x = Mathf.MoveTowards(nextVelocity.x, 0, Time.deltaTime * 10);
					SetFriction( movingFriction );
				}

				nextVelocity.y = platformYVelocity;
			}

			if ("슬라이딩중")
			{
				axisX = Mathf.Sign(savedXVelocity);
				nextVelocity.x = savedXVelocity + platformXVelocity;
				if (movingPlatform)	nextVelocity.y = platformYVelocity;
			}

			if ("공격")
			{
				nextVelocity.x = Mathf.MoveTowards(nextVelocity.x, platformXVelocity, Time.deltaTime * 8);
				if (movingPlatform)	nextVelocity.y = Mathf.MoveTowards(nextVelocity.y, platformYVelocity, Time.deltaTime * 15);
			}
		}
		else if ("점프 혹은 낙하 중이라면")
		{
			if( "공중 조작 제한")
			{
				//키 조작을 한다면 공중 조작 제한 풀자. 속도는 변하지 않게( 변경 해야 할 수도 ?)
			} else {
				nextVelocity.x = Mathf.MoveTowards(nextVelocity.x, walkSpeed * Mathf.Sign(axisX), Time.deltaTime * 8);
			}

			if ("점프라면")
			{
				savedXVelocity = nextVelocity.x;
				if ( jumpPressed ==false || //점프버튼 누른걸 땠거나 
					jumpTime >= jumpDuration ||
					downAttackRecovery ) //하늘이긴 한대 낙하공격 회복중인 경우
				{
					jumpStartTime -= jumpDuration;

					//상승중이였다면 y를 0으로 얼른 변화시키자
					if (nextVelocity.y > 0)	nextVelocity.y = Mathf.MoveTowards(nextVelocity.y, 0, Time.deltaTime * 30);
				}
			} else if ("낙하중이라면")
			{
				if ("땅에 닿았다")
				{
					nextVelocity.x = savedXVelocity;//낙하 도중의 기억시킨 X 를 설정
				}
				{
					EnemyBounceCheck(ref nextVelocity);//무엇인가를 밟았는지 검사(velocity 변화됨 )
					savedXVelocity = nextVelocity.x;
				}

				//falling and wallslide
				if ("낙하중" )
				{
					nextVelocity.y += fallGravity * Time.deltaTime;
				}
				else if ("벽타기")
				{
					nextVelocity.y = Mathf.Clamp(nextVelocity.y, wallSlideSpeed, 0);
				}
			}
		}

		if ("IDLE, WALK, RUN, JUMP, FALL,SLIDE")
        {
			if (Time.time > airControlLockoutTime)
            {
				if (axisX > 0.1f) skeletonAnimation.Skeleton.FlipX = false;
				else if (axisX < -0.1f)	skeletonAnimation.Skeleton.FlipX = true;
			} else
            {
				if (nextVelocity.x > 0.1f) skeletonAnimation.Skeleton.FlipX = false;
				else if (nextVelocity.x < -0.1f) skeletonAnimation.Skeleton.FlipX = true;
			}
		}
        
        //down attack
		if (state == ActionState.DOWNATTACK)
        {
			if ("다운어택 회복중이였다면")
            {
				if ("일정 시간이 흐른 후")
                {
					nextVelocity.y = jumpSpeed + (platformYVelocity >= 0 ? platformYVelocity : 0);// > jump
				}
				else
                {
					nextVelocity = Vector2.zero;
					if (movingPlatform) nextVelocity = movingPlatform.Velocity;
				}
			} else if("땅에 닿앗다" )
            {
				if (movingPlatform)	nextVelocity = movingPlatform.Velocity;
			}
		}
		flipped = skeletonAnimation.Skeleton.FlipX;
		rb.velocity = nextVelocity;
	}

	//Bounce off a player in an angry way
	bool EnemyBounceCheck (ref Vector2 velocity) {
		var character = OnTopOfCharacter();
		if (character != null) {
			SoundPalette.PlaySound(jumpSound, 1, 1, transform.position);
			character.SendMessage("Hit", 1, SendMessageOptions.DontRequireReceiver);
			velocity.y = headBounceSpeed;
			jumpStartTime = Time.time;
			state = ActionState.JUMP;
			doJump = false;
			return true;
		}
		return false;
	}

	//Enter the fall state, optionally using a jump counter.  IE: to prevent jumping after slipping off a platform
	void SetFallState (bool useJump) {
		if (useJump)
			jumpCount++;

		state = ActionState.FALL;
	}

	//Sync the Spine Animation with the current ActionState. Handle a few "edge" cases to do with inclines and uh... edges.
	void UpdateAnim () {
		switch (state) {
			case ActionState.IDLE:
				if (CenterOnGround) {
					skeletonAnimation.AnimationName = idleAnim;
				} else {
					if (onIncline)
						skeletonAnimation.AnimationName = idleAnim;
					else if (BackOnGround) {
						skeletonAnimation.AnimationName = balanceForward;
					} else if (ForwardOnGround) {
						skeletonAnimation.AnimationName = balanceBackward;
					}
				}
				break;
			case ActionState.WALK:
				skeletonAnimation.AnimationName = walkAnim;
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
			case ActionState.WALLSLIDE:
				skeletonAnimation.AnimationName = wallSlideAnim;
				break;
			case ActionState.SLIDE:
				skeletonAnimation.AnimationName = slideAnim;
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

	//Special effects helper function
	void SpawnAtFoot (GameObject prefab, Quaternion rotation, Vector3 scale) {
		var bone = skeletonAnimation.Skeleton.FindBone(footEffectBone);
		Vector3 pos = skeletonAnimation.transform.TransformPoint(bone.WorldX, bone.WorldY, 0);
		((GameObject)Instantiate(prefab, pos, rotation)).transform.localScale = scale;
	}
}
