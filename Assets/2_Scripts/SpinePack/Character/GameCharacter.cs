using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ActionState
{
    IDLE,
    WALK,
    RUN,
    JUMP,
    FALL,
    WALLSLIDE,
    JETPACK,
    SLIDE,
    ATTACK,
    DOWNATTACK,
    UPATTACK,
    DANCE,
    DEAD
}

[RequireComponent(typeof(Rigidbody2D))]
public class GameCharacter : MonoBehaviour
{
    static List<GameCharacter> All = new List<GameCharacter>();

    public enum AnimationType
    {
        SPINE,
        ANIMATION,
        SPRITE,
        NONE
    }

    protected ActionState mState;

    public ActionState state
    {
        get{ return mState; }
    }
    
    public void SetState( ActionState next )
    {
        if( mState == next ) return;

		StateExit();

        mState = next;

		StateEnter();
    }

	virtual protected void StateExit()
	{

	}

	virtual protected void StateEnter()
	{

	}

    public AnimationType bodyType;

    [Header("Physics")]
    public Collider2D primaryCollider;
    public float fallGravity = -4;
    public float idleFriction = 20;//gunman 4
    public float movingFriction = 0;
    
    [Header("Speeds & Timings")]
    public float walkSpeed = 1;
    public float runSpeed = 5;
    public float jumpSpeed = 12;
    public float airJumpSpeed = 10;
    public float headBounceSpeed = 16;//캐릭터를 밟았을 때 vy
    public float jumpDuration = 0.5f;
    
    [Range(1, 3)]
    public int maxJumps = 1;
    
    

    [Header("References")]
    public SkeletonAnimation skeletonAnimation;
    public SkeletonGhost skeletonGhost;
    public GameObject groundJumpPrefab;
    public GameObject airJumpPrefab;

    [Header("Input")]
    public float deadZone = 0.05f;
    public float runThreshold = 0.5f;

    //raycasting
    [Header("Raycasting")]
    public LayerMask characterMask;
    [HideInInspector]
    public LayerMask currentMask;
    public LayerMask groundMask;
    public LayerMask passThroughMask;

    [SpineBone(dataField: "skeletonAnimation")]
    public string footEffectBone;

    //--------------------------------------------------------------------------
    // Inspector에 표시되지 않는 public 
    //--------------------------------------------------------------------------
    //input injection
    public System.Action<GameCharacter> HandleInput;
    //events
    public System.Action<Transform> OnFootstep;
    public System.Action<Transform> OnJump;


    //--------------------------------------------------------------------------
    // protected, private
    //--------------------------------------------------------------------------

	//physics
    protected Rigidbody2D mRb;
    
	protected Vector3 mCastOrginBackGround;
    protected Vector3 mCastOriginCenterGround;
    protected Vector3 mCastOriginForwardGround;
    protected Vector3 mCastOriginWall;
    protected float mCastOriginWallDistance;

    protected PhysicsMaterial2D mPhysicsMaterial;

    protected OneWayPlatform mPassThroughPlatform;
	protected MovingPlatform movingPlatform; //handlePhysics 에서 결정

	protected float mSlope;
    protected bool mIsOnSlope = false;

	//character state
    protected int mJumpCount = 0;
    
	protected bool mFlipped;

    protected Vector2 mAxis;
    protected bool mJumpPressed;
    protected float mJumpStartTime;
    protected bool mIsRun = false;

	//trigger
	protected bool mDoJump;
    
    public bool Flipped
    {
        get
        {
            return this.mFlipped;
        }
    }

    virtual protected void Awake()
    {
        mRb = GetComponent<Rigidbody2D>();

        mRb.angularDrag = 0f;
        mRb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    virtual protected void Start()
    {
        switch (bodyType)
        {
            case AnimationType.SPINE:
                skeletonAnimation.state.Event += HandleEvent;
                skeletonAnimation.state.Complete += HandleComplete;
                break;
        }

        CalculateRayBounds(primaryCollider);

        if (primaryCollider.sharedMaterial == null)
            mPhysicsMaterial = new PhysicsMaterial2D("CharacterColliderMaterial");
        else
            mPhysicsMaterial = Instantiate(primaryCollider.sharedMaterial);

        primaryCollider.sharedMaterial = mPhysicsMaterial;

        currentMask = groundMask;
    }


    virtual protected void OnEnable()
    {
        Register();
    }

    virtual protected void OnDisable()
    {
        Unregister();
    }

    void Register()
    {
        if (All.Contains(this) == false) All.Add(this);
    }

    void Unregister()
    {
        if (All.Contains(this)) All.Remove(this);
    }


    //Calculate where the collision rays should be based on a polygon collider
    void CalculateRayBounds(Collider2D coll)
    {
        Bounds b = coll.bounds;
        Vector3 min = transform.InverseTransformPoint(b.min);
        Vector3 center = transform.InverseTransformPoint(b.center);
        Vector3 max = transform.InverseTransformPoint(b.max);

        mCastOrginBackGround.x = min.x;
        mCastOrginBackGround.y = min.y + 0.1f;

        mCastOriginCenterGround.x = center.x;
        mCastOriginCenterGround.y = min.y + 0.1f;

        mCastOriginForwardGround.x = max.x;
        mCastOriginForwardGround.y = min.y + 0.1f;

        mCastOriginWall = center;
        mCastOriginWallDistance = b.extents.x + 0.1f;
    }

    virtual protected void HandleEvent(Spine.AnimationState state, int trackIndex, Spine.Event e)
    {
        //handle spine anim event
    }

    virtual protected void HandleComplete(Spine.AnimationState state, int trackIndex, int loopCount)
    {
        //Handle Spine Animation Complete callbacks
    }
    
    //--------------------------------------------------------------------------
    // behaviour
    //--------------------------------------------------------------------------
    
    protected void DoPassThrough(OneWayPlatform platform)
    {
        StartCoroutine(PassthroughRoutine(platform));
    }

    IEnumerator PassthroughRoutine(OneWayPlatform platform)
    {
        currentMask = passThroughMask;
        Physics2D.IgnoreCollision(primaryCollider, platform.collider, true);
        mPassThroughPlatform = platform;
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(primaryCollider, platform.collider, false);
        currentMask = groundMask;
        mPassThroughPlatform = null;
    }
    
    //--------------------------------------------------------------------------
    // Graphics
    //--------------------------------------------------------------------------

    virtual protected void Update()
    {
        // * DECharacter
        //input
        //fsm update.
        
        if (HandleInput != null) HandleInput(this);

        ProcessInput();

		StateUpdate();

        UpdateAnim();
    }

	virtual protected void StateUpdate()
	{

	}

    virtual protected void ProcessInput()
    {

    }

    //Sync the Spine Animation with the current ActionState. Handle a few "edge" cases to do with inclines and uh... edges.
    virtual protected void UpdateAnim()
    {

    }
    
    //--------------------------------------------------------------------------
    // Physics
    //--------------------------------------------------------------------------
    virtual protected void FixedUpdate()
    {
        // * DEController
        //현재 속도 측정( 전달받은 속도를 바탕으로 중력, 지면 마찰 등 적용 )
        //특정 지역에 있다면 ( 물, 우주, 회오리 바람 ) 적용
        //속도를 바탕으로 움직여야할 벡터 측정.
        //지난 프레임 저장, 리셋
        //충돌영역 업데이트
        //충돌검사( 아래,옆,위 ). 충돌로 인해 변경된 벡터로 속도재설정. 여기서 밟고 있는 바닥, 경사를 판단.
        //부딛히고 밀수있다면 민다.
        //지상에 막 닿은건지 아닌지를 판단.
        //떨어지고 있는지 아닌지를 판단.
        //실제 캐릭터 이동
        
        // * DECharacter
        //지상에 막 닿았따면 점프 수를 초기화 한다.
        //지상에 막 닿았고 밟은 플랫폼의 착지 이펙트가 있따면 생성
        
        HandlePhysics();
    }

    virtual protected void HandlePhysics()
    {

    }
    
    protected MovingPlatform GetMovingPlatform()
    {
        MovingPlatform mp = MovingPlatformCast(mCastOriginCenterGround);
        if (mp == null) mp = MovingPlatformCast(mCastOrginBackGround);
        if (mp == null) mp = MovingPlatformCast(mCastOriginForwardGround);
        return mp;
    }
    
    //Detect being on top of a characters's head
    protected Rigidbody2D OnTopOfCharacter()
    {
        Rigidbody2D character = GetRelevantCharacterCast(mCastOriginCenterGround, 0.15f);
        if (character == null) character = GetRelevantCharacterCast(mCastOrginBackGround, 0.15f);
        if (character == null) character = GetRelevantCharacterCast(mCastOriginForwardGround, 0.15f);

        return character;
    }

    //Raycasting stuff
    Rigidbody2D GetRelevantCharacterCast(Vector3 origin, float dist)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.TransformPoint(origin), Vector2.down, dist, characterMask);
        if (hits.Length > 0)
        {
            int index = 0;

            if (hits[0].rigidbody == mRb)
            {
                if (hits.Length == 1)
                    return null;

                index = 1;
            }
            if (hits[index].rigidbody == mRb)
                return null;

            var hit = hits[index];
            if (hit.collider != null && hit.collider.attachedRigidbody != null)
            {
                return hit.collider.attachedRigidbody;
            }
        }

        return null;
    }

    //Enter the fall state, optionally using a jump counter.  IE: to prevent jumping after slipping off a platform
    protected void SetFallState(bool useJump)
    {
        if (useJump) mJumpCount++;
        SetState( ActionState.FALL );
    }

    //work-around for Box2D not updating friction values at runtime...
    protected void SetFriction(float friction)
    {
        if (friction != mPhysicsMaterial.friction)
        {
            mPhysicsMaterial.friction = friction;
            primaryCollider.gameObject.SetActive(false);
            primaryCollider.gameObject.SetActive(true);
        }
    }

    //TODO:  deal with SetFriction workaround breaking ignore pairs.........
    protected void IgnoreCharacterCollisions(bool ignore)
    {
        foreach (GameCharacter gc in All)
		{
			if (gc == this)	continue;
			gc.IgnoreCollision(primaryCollider, ignore);
		}
    }

	void IgnoreCollision(Collider2D collider, bool ignore)
	{
		if (primaryCollider == null) Physics2D.IgnoreCollision(GetComponentInChildren<Collider2D>(), collider, ignore);
		else Physics2D.IgnoreCollision(primaryCollider, collider, ignore);
	}

    //Special effects helper function
    protected void SpawnAtFoot(GameObject prefab, Quaternion rotation, Vector3 scale)
    {
        var bone = skeletonAnimation.Skeleton.FindBone(footEffectBone);
        Vector3 pos = skeletonAnimation.transform.TransformPoint(bone.WorldX, bone.WorldY, 0);
        ((GameObject)Instantiate(prefab, pos, rotation)).transform.localScale = scale;
    }

    //--------------------------------------------------------------------------------
    // check Physics
    //--------------------------------------------------------------------------------
    
	//see if character is on a one-way platform
	protected OneWayPlatform PlatformCast(Vector3 origin)
	{
		OneWayPlatform platform = null;
		RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(origin), Vector2.down, 0.5f, currentMask);
		if (hit.collider != null && !hit.collider.isTrigger)
		{
			platform = hit.collider.GetComponent<OneWayPlatform>();
		}

		return platform;
	}

	//see if character is on a moving platform
	protected MovingPlatform MovingPlatformCast(Vector3 origin)
	{
		MovingPlatform platform = null;
		RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(origin), Vector2.down, 0.5f, currentMask);
		if (hit.collider != null && !hit.collider.isTrigger)
		{
			platform = hit.collider.GetComponent<MovingPlatform>();
		}

		return platform;
	}

	//see if character is on the ground
	//throw onIncline flag
	bool GroundCast(Vector3 origin)
	{
		RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(origin), Vector2.down, 0.15f, currentMask);
		if (hit.collider != null && !hit.collider.isTrigger)
		{
			if (hit.normal.y < 0.4f)
				return false;
			else if (hit.normal.y < 0.95f)
				mIsOnSlope = true;

			return true;
		}

		return false;
	}
    
    //보통 검사 시 movingPlatform 과 같이 검사한다. 통합 고려하자.
	public bool OnGround
    {
        get
        {
            return CenterOnGround || BackOnGround || ForwardOnGround;
        }
    }

    protected bool BackOnGround
    {
        get
        {
            return GroundCast(mFlipped ? mCastOriginForwardGround : mCastOrginBackGround);
        }
    }

    protected bool ForwardOnGround
    {
        get
        {
            return GroundCast(mFlipped ? mCastOrginBackGround : mCastOriginForwardGround);
        }
    }

    protected bool CenterOnGround
    {
        get
        {
            return GroundCast(mCastOriginCenterGround);
        }
    }
    
    //벽을 밀고 있는지 검사한다.
    protected bool IsPressingAgainstWall
    {
        get
        {
            float x = mRb.velocity.x;
            bool usingVelocity = true;
            if (Mathf.Abs(x) < 0.1f)
            {
                x = mAxis.x;
                if (Mathf.Abs(x) <= deadZone)
                {
                    return false;
                }
                else
                {
                    usingVelocity = false;
                }
            }
            
            RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(mCastOriginWall), new Vector2(x, 0).normalized, mCastOriginWallDistance + (usingVelocity ? x * Time.deltaTime : 0), currentMask);
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                if (hit.collider.GetComponent<OneWayPlatform>()) return false;

                return true;
            }

            return false;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Handles.Label(transform.position, state.ToString());

        if (OnGround) Gizmos.color = Color.green;
        else Gizmos.color = Color.grey;

        Gizmos.DrawWireSphere(transform.TransformPoint(mCastOriginCenterGround), 0.07f);
        Gizmos.DrawWireSphere(transform.TransformPoint(mCastOrginBackGround), 0.07f);
        Gizmos.DrawWireSphere(transform.TransformPoint(mCastOriginForwardGround), 0.07f);

        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.TransformPoint(mCastOriginCenterGround), transform.TransformPoint(mCastOriginCenterGround + new Vector3(0, -0.15f, 0)));
        Gizmos.DrawLine(transform.TransformPoint(mCastOrginBackGround), transform.TransformPoint(mCastOrginBackGround + new Vector3(0, -0.15f, 0)));
        Gizmos.DrawLine(transform.TransformPoint(mCastOriginForwardGround), transform.TransformPoint(mCastOriginForwardGround + new Vector3(0, -0.15f, 0)));
    }
#endif
}
