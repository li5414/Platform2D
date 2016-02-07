using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class DECharacterController : MonoBehaviour
{
    //--------------------------------------------------------------------------
    // Editor
    //--------------------------------------------------------------------------

    public UpdateType updateType;
    public SmoothType smoothType;

    [Header("Physics")]
    public Collider2D primaryCollider;
    public float fallGravity = -4;

    //--------------------------------------------------------------------------
    // private protected
    //--------------------------------------------------------------------------

    Rigidbody2D mRb;

    Transform mTr;

    //physics
    LayerMask mCurrentMask;

    Vector3 mCastOrginBackGround;
    Vector3 mCastOriginCenterGround;
    Vector3 mCastOriginForwardGround;
    Vector3 mCastOriginWall;
    float mCastOriginWallDistance;

    PhysicsMaterial2D mPhysicsMaterial;

    OneWayPlatform mPassThroughPlatform;



    float mSlope;

    public bool IsOnSlope { get; private set; }

    //통합 과정 시 추가한 것들 & 임시

    public OneWayPlatform oneWayPlatform { get; protected set; }

    public MovingPlatform standingPlatform { get; protected set; }

    public Rigidbody2D StompableObject { get; private set; }

    public bool BackOnGround { get; private set; }

    public bool ForwardOnGround { get; private set; }

    public bool CenterOnGround { get; private set; }

    public bool IsPressAgainstWall { get; private set; }
    public float addedGravity { get; protected set; }

    Vector2 mMovingPlatformVelocity;

    public float CurrentSpeed { get; set; }

    Facing mFacaing;

    public float axisX { get; set; }


    //--------------------------------------------------------------------------
    // get;set
    //--------------------------------------------------------------------------
    public Vector2 velocity
    {
        get { return mRb.velocity; }
    }

    public float vx
    {
        get { return mRb.velocity.x; }
        set { mRb.velocity = new Vector2(value, mRb.velocity.y); }
    }

    public float vy
    {
        get { return mRb.velocity.y; }
        set { mRb.velocity = new Vector2(mRb.velocity.x, value); }
    }

    void Awake()
    {
        mRb = GetComponent<Rigidbody2D>();

        mRb.angularDrag = 0f;
        mRb.constraints = RigidbodyConstraints2D.FreezeRotation;

        mTr = GetComponent<Transform>();

        CalculateRayBounds(primaryCollider);

        if (primaryCollider.sharedMaterial == null)
            mPhysicsMaterial = new PhysicsMaterial2D("CharacterColliderMaterial");
        else
            mPhysicsMaterial = Instantiate(primaryCollider.sharedMaterial);

        primaryCollider.sharedMaterial = mPhysicsMaterial;
    }

    void Start()
    {
        mCurrentMask = DruggedEngine.MASK_ALL_PLATFORM;
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

    //--------------------------------------------------------------------------
    // behaviour
    //--------------------------------------------------------------------------

    //todo 플립 적용
    public void SetFacing(Facing facing)
    {
        mFacaing = facing;
        switch (mFacaing)
        {
            case Facing.RIGHT:
                mTr.localRotation = Quaternion.Euler(0f, 0f, 0f);
                break;

            case Facing.LEFT:
                mTr.localRotation = Quaternion.Euler(0f, 180f, 0f);
                break;
        }
    }

    //work-around for Box2D not updating friction values at runtime...
    public void SetFriction(float friction)
    {
        if (friction != mPhysicsMaterial.friction)
        {
            mPhysicsMaterial.friction = friction;
            primaryCollider.gameObject.SetActive(false);
            primaryCollider.gameObject.SetActive(true);
        }
    }

    public void PassOneway()
    {
        StartCoroutine(PassOnewayRoutine(oneWayPlatform));
        ClearPlatform();
    }

    IEnumerator PassOnewayRoutine(OneWayPlatform passableOneway)
    {
        mCurrentMask = DruggedEngine.MASK_ENVIRONMENT;
        Physics2D.IgnoreCollision(primaryCollider, passableOneway.collider, true);
        mPassThroughPlatform = passableOneway;
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(primaryCollider, passableOneway.collider, false);
        mCurrentMask = DruggedEngine.MASK_ALL_PLATFORM;
        mPassThroughPlatform = null;
    }

    //TODO
    public void ClearPlatform()
    {
        oneWayPlatform = null;
        standingPlatform = null;
    }

    public void UpdateColliderSize(float xScale, float yScale)
    {
        UpdateColliderSize(new Vector3(xScale, yScale, 1f));
    }

    public void ResetColliderSize()
    {
        UpdateColliderSize(Vector3.one);
    }

    void UpdateColliderSize(Vector3 size)
    {
        primaryCollider.transform.localScale = size;

        //todo 아래는 DE의 코드이다. BoxCollider 에만 해당한다. Spine은 PolygonCollider로 경사를 처리하는거 같다.
        //경사 문제가 해결되면 아래의 코드 적용을 고려하자.
        // _collider.size = size;
        // _collider.offset = new Vector2(0f, _collider.size.y * 0.5f);
    }

    public void AddForceHorizontal(float x)
    {
        AddForce(new Vector2(x, 0f));
    }

    public void AddForceVertical(float y)
    {
        AddForce(new Vector2(0f, y));
    }


    public void AddForce(Vector2 force)
    {
        mRb.AddForce(force, ForceMode2D.Impulse);
    }

    public void Stop()
    {
        mRb.velocity = Vector2.zero;
    }

    void Update()
    {
        if (updateType == UpdateType.Update) Move(Time.deltaTime);
    }

    void LateUpdate()
    {
        if (updateType == UpdateType.LateUpdate) Move(Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (updateType == UpdateType.FixedUpdate) Move(Time.deltaTime);

        DetectOnGround();
        DetectPlatform();
        DetectStompObject();
        DetectPressAgainstWall();
    }

    float mLockedVY;
    bool mIsLockVY;

    public void LockVY(float wantVY)
    {
        mIsLockVY = true;
        mLockedVY = wantVY;
    }

    public void UnLockVY()
    {
        mIsLockVY = false;
        mLockedVY = 0f;
    }

    //character에서 호출된다.
    public void Move(float delta)
    {
        float movementFactor = 25f;//15,25 //지상과 공중의 차이가 있어야 한다. 공중에서 급격한 방향 전환이 어렵도록.var movementFactor = _controller.State.IsGrounded ? AccelOnGround : AccelOnAir;

        Vector2 current = mRb.velocity;

        //x move
        float vx = mFacaing == Facing.RIGHT ? CurrentSpeed : -CurrentSpeed;

        if (OnGround)
        {
            vx += mMovingPlatformVelocity.x;
            //print("더하자 : " + mMovingPlatformVelocity.x);
            current.y += mMovingPlatformVelocity.y;
        }
        else
        {
            if (mIsLockVY)
            {
                current.y = mLockedVY;
            }
            else
            {
                current.y += fallGravity * delta;
            }
        }

        //무빙 플랫폼 처리

        //점프 하는 동안 Mathf.MoveTowards(velocity.y, 0, Time.deltaTime * 30);
        //fall 인 경우 velocity.y += fallGravity * Time.deltaTime;
        //wallSlide 인 경우 velocity.y = Mathf.Clamp(velocity.y, wallSlideSpeed, 0);

        //apply
        switch (smoothType)
        {
            case SmoothType.MoveTowards:
                current.x = Mathf.MoveTowards(current.x, vx, delta * movementFactor);
                break;
            case SmoothType.Lerp:
                current.x = Mathf.Lerp(current.x, vx, delta * movementFactor);
                break;
        }


        mRb.velocity = current;
    }

    void DetectOnGround()
    {
        BackOnGround = GroundCast(mCastOrginBackGround);
        ForwardOnGround = GroundCast(mCastOriginForwardGround);
        CenterOnGround = GroundCast(mCastOriginCenterGround);
    }

    void DetectPlatform()
    {
        standingPlatform = MovingPlatformCast(mCastOriginCenterGround);
        if (standingPlatform == null) standingPlatform = MovingPlatformCast(mCastOrginBackGround);
        if (standingPlatform == null) standingPlatform = MovingPlatformCast(mCastOriginForwardGround);

        oneWayPlatform = PlatformCast(mCastOriginCenterGround);
        if (oneWayPlatform == null) oneWayPlatform = PlatformCast(mCastOrginBackGround);
        if (oneWayPlatform == null) oneWayPlatform = PlatformCast(mCastOriginForwardGround);

        if (standingPlatform == null) mMovingPlatformVelocity = Vector2.zero;
        else mMovingPlatformVelocity = standingPlatform.Velocity;

        //mIsOnSlope 처리해야한다.
    }

    void DetectStompObject()
    {
        StompableObject = GetRelevantCharacterCast(mCastOriginCenterGround, 0.15f);
        if (StompableObject == null) StompableObject = GetRelevantCharacterCast(mCastOrginBackGround, 0.15f);
        if (StompableObject == null) StompableObject = GetRelevantCharacterCast(mCastOriginForwardGround, 0.15f);
    }

    void DetectPressAgainstWall()
    {
        float x = mRb.velocity.x;
        bool usingVelocity = true;

        if (Mathf.Abs(x) < 0.1f)
        {
            x = axisX;
            if (x == 0f)
            {
                IsPressAgainstWall = false;
                return;
            }
            else
            {
                usingVelocity = false;
            }
        }

        RaycastHit2D hit = Physics2D.Raycast
        (
                               transform.TransformPoint(mCastOriginWall),
                               new Vector2(x, 0).normalized,
                               mCastOriginWallDistance + (usingVelocity ? x * Time.deltaTime : 0),
                               mCurrentMask
                           );

        if (hit.collider != null && !hit.collider.isTrigger)
        {
            if (hit.collider.GetComponent<OneWayPlatform>())
            {
                IsPressAgainstWall = false;
                return;
            }

            IsPressAgainstWall = true;
            return;
        }

        IsPressAgainstWall = false;
    }

    //--------------------------------------------------------------------------------
    // check Physics
    //--------------------------------------------------------------------------------

    //see if character is on a one-way platform
    protected OneWayPlatform PlatformCast(Vector3 origin)
    {
        OneWayPlatform platform = null;
        RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(origin), Vector2.down, 0.5f, mCurrentMask);
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
        RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(origin), Vector2.down, 0.5f, mCurrentMask);
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            platform = hit.collider.GetComponent<MovingPlatform>();
        }

        return platform;
    }

    //de참고
    protected bool GroundCast(Vector3 origin)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(origin), Vector2.down, 0.15f, mCurrentMask);
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            if (hit.normal.y < 0.4f)
                return false;
            else if (hit.normal.y < 0.95f)
                IsOnSlope = true;

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

    public bool OnOneway
    {
        get
        {
            if (oneWayPlatform == null)
                return false;
            else
                return true;
        }
    }

    //Raycasting stuff
    protected Rigidbody2D GetRelevantCharacterCast(Vector3 origin, float dist)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.TransformPoint(origin), Vector2.down, dist, DruggedEngine.MASK_ENEMY);
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

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        if (OnGround)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.grey;

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
