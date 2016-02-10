using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//PassOneway 와 Platform 의 trigger 과 동시에 실행되면서 중첩된다
namespace druggedcode.engine
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DECharacterController : MonoBehaviour
    {
        //--------------------------------------------------------------------------
        // Editor
        //--------------------------------------------------------------------------

        public LayerMask currentMask;

        [Header("Physics")]
        public Collider2D primaryCollider;
        public float fallGravity = -4;
        public float raySafetyDis = 0.1f;

        [Header("Move")]
        public UpdateType updateType;
        public SmoothType smoothType;


        //--------------------------------------------------------------------------
        // private protected
        //--------------------------------------------------------------------------

        Rigidbody2D mRb;

        Transform mTr;

        Vector3 mCastOriginWall;
        float mCastOriginWallDistance;

        PhysicsMaterial2D mPhysicsMaterial;

        //통합 과정 시 추가한 것들 & 임시

        public Rigidbody2D StompableObject { get; private set; }

        public DECharacterControllerState state { get; private set; }

        //보통 검사 시 movingPlatform 과 같이 검사한다. 통합 고려하자.
        //ground

        //side

        public bool IsPressAgainstWall { get; private set; }

        public float addedGravity { get; protected set; }

        Vector2 mMovingPlatformVelocity;

        Facing mFacaing;

        public float axisX { get; set; }

        List<Platform> mPlatformList;
        List<bool> mGroundList;
        List<float> mAngleList;
        List<Vector3> mOriginList;

        //--------------------------------------------------------------------------
        // get;set
        //--------------------------------------------------------------------------
        float mPassedVX = 0f;
        public Vector2 velocity { get { return mRb.velocity; } }
        public float vx { get { return mRb.velocity.x; } set { mPassedVX = value; } }
        public float vy { get { return mRb.velocity.y; } set { mRb.velocity = new Vector2(mRb.velocity.x, value); } }

        void Awake()
        {
            mRb = GetComponent<Rigidbody2D>();

            mRb.angularDrag = 0f;
            mRb.constraints = RigidbodyConstraints2D.FreezeRotation;

            mTr = GetComponent<Transform>();

            state = new DECharacterControllerState();

            mPlatformList = new List<Platform>(new Platform[] { null, null, null });
            mAngleList = new List<float>(new float[] { 0f, 0f, 0f });
            mGroundList = new List<bool>(new bool[] { false, false, false });
            mOriginList = new List<Vector3>(new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero });

            CalculateRayBounds(primaryCollider);

            if (primaryCollider.sharedMaterial == null)
                mPhysicsMaterial = new PhysicsMaterial2D("CharacterColliderMaterial");
            else
                mPhysicsMaterial = Instantiate(primaryCollider.sharedMaterial);

            primaryCollider.sharedMaterial = mPhysicsMaterial;
        }

        void Start()
        {
            currentMask = DruggedEngine.MASK_ALL_GROUND;
        }

        void CalculateRayBounds(Collider2D coll)
        {
            Bounds b = coll.bounds;
            Vector3 min = transform.InverseTransformPoint(b.min);
            Vector3 center = transform.InverseTransformPoint(b.center);
            Vector3 max = transform.InverseTransformPoint(b.max);

            Vector3 forwadGround = new Vector3();
            Vector3 centerGround = new Vector3();
            Vector3 backGround = new Vector3();

            forwadGround.x = max.x;
            forwadGround.y = min.y + raySafetyDis;

            centerGround.x = center.x;
            centerGround.y = min.y + raySafetyDis;

            backGround.x = min.x;
            backGround.y = min.y + raySafetyDis;


            mOriginList[0] = forwadGround;
            mOriginList[1] = centerGround;
            mOriginList[2] = backGround;

            mCastOriginWall = center;
            mCastOriginWallDistance = b.extents.x + raySafetyDis;
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

        public void PassThroughPlatform()
        {
            state.StandingPlatform.PassThough( this );
        }
        
        public void IgnoreCollision( Collider2D collider, bool ignore )
        {
            print("ignore");
            Physics2D.IgnoreCollision( primaryCollider, collider, ignore );
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

            if (state.OnGround)
            {
                mPassedVX += mMovingPlatformVelocity.x;
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
                    current.x = Mathf.MoveTowards(current.x, mPassedVX, delta * movementFactor);
                    break;
                case SmoothType.Lerp:
                    current.x = Mathf.Lerp(current.x, mPassedVX, delta * movementFactor);
                    break;
            }


            mRb.velocity = current;
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

            state.Reset();

            //detect
            DetectOnGround();
            DetectPressAgainstWall();

            //위를 체크 하긴 해야함. 앉아 있다가 일어서려고 할 때 하기만 하면 될까?
            DetectStompObject();//Stompable.cs 참고. 밟히는 쪽에 설치하는게 올바른 것 같다.

            //save
            state.Save();
        }

        void DetectOnGround()
        {
            if (mRb.velocity.y > 0f)
            {
                return;
            }

            state.ClearPlatform();

            GroundCast(0);
            GroundCast(1);
            GroundCast(2);

            state.OnForwardGround = mGroundList[0];
            state.OnCenterGround = mGroundList[1];
            state.OnBackGround = mGroundList[2];

            state.ForwardPlatform = mPlatformList[0];
            state.CenterPlatofrm = mPlatformList[1];
            state.BackPlatform = mPlatformList[2];

            //			state.Slope = hit.normal.y;

            //            // if (standingPlatform == null) mMovingPlatformVelocity = Vector2.zero;
            //            // else mMovingPlatformVelocity = standingPlatform.velocity;
        }

        void GroundCast(int idx)
        {
            Vector3 origin = mTr.TransformPoint(mOriginList[idx]);
            float rayLength = 0.15f;
            RaycastHit2D hit = PhysicsUtil.DrawRayCast(origin, Vector2.down, rayLength, currentMask, Color.red);
            Platform platform = null;
            float angle = 0f;
            bool isOn = false;

            if (hit && hit.collider != null && hit.collider.isTrigger == false)
            {
                isOn = true;
                platform = hit.collider.GetComponent<Platform>();
                angle = hit.normal.y;
                //angle = Vector2.Angle( hit.normal, Vector2.up );
                //_state.SlopeAngle = _groundAngles[closestIndex];

                //특정 각도 범위가 아니면 지면이 아니라고 판단.
                //지면이지만 특정 각도 범위면 경사에 있다고 판단.

                // if (hit.normal.y < 0.4f)
                //     return false;
                // else if (hit.normal.y < 0.95f)
                //     state.IsOnSlope = true;

                // platform = hit.collider.GetComponent<Platform>();

                // return true;
            }

            mGroundList[idx] = isOn;
            mPlatformList[idx] = platform;
            mAngleList[idx] = angle;
        }

        //see if character is on a one-way platform
        protected Platform PlatformCast(Vector3 origin)
        {
            Platform platform = null;
            RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(origin), Vector2.down, 0.5f, currentMask);
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                platform = hit.collider.GetComponent<Platform>();
            }

            return platform;
        }

        void DetectStompObject()
        {
            StompableObject = GetRelevantCharacterCast(mOriginList[0], 0.15f);
            if (StompableObject == null) StompableObject = GetRelevantCharacterCast(mOriginList[1], 0.15f);
            if (StompableObject == null) StompableObject = GetRelevantCharacterCast(mOriginList[2], 0.15f);
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
                                   currentMask
                               );

            if (hit.collider != null && !hit.collider.isTrigger)
            {
                if (hit.collider.GetComponent<Platform>())
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

            if (state.OnGround)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.grey;

            Gizmos.DrawWireSphere(transform.TransformPoint(mOriginList[0]), 0.07f);
            Gizmos.DrawWireSphere(transform.TransformPoint(mOriginList[1]), 0.07f);
            Gizmos.DrawWireSphere(transform.TransformPoint(mOriginList[2]), 0.07f);
        }
#endif
    }

    public class DECharacterControllerState
    {
        //
        public bool WasOnGroundLastFrame { get; set; }
        public bool JustGotGrounded { get; set; }

        //slope
        public bool IsOnSlope { get; set; }
        public float Slope { get; set; }
        public float ForwardAngle { get; set; }
        public float CenterAngle { get; set; }
        public float BackAngle { get; set; }
        
        //ground
        public bool OnGround { get { return OnCenterGround || OnBackGround || OnForwardGround; } }
        public bool OnForwardGround { get; set; }
        public bool OnCenterGround { get; set; }
        public bool OnBackGround { get; set; }
        
        public bool OnOneway
        {
            get
            {
                if( OnGround == false ) return false;
                if( StandingPlatform == false ) return false;
                if( StandingPlatform.oneway == false ) return false;
                return true;
            }
        }

        //platform
        public Platform StandingPlatform
        {
            get
            {
                if (CenterPlatofrm != null) return CenterPlatofrm;
                else if (ForwardPlatform != null) return ForwardPlatform;
                else if (BackPlatform != null) return BackPlatform;
                return null;
            }
        }

        public Platform ForwardPlatform { get; set; }
        public Platform CenterPlatofrm { get; set; }
        public Platform BackPlatform { get; set; }

        public DECharacterControllerState()
        {

        }

        public void Reset()
        {
            JustGotGrounded = false;
            // IsFalling = false;           
            // SlopeAngle = 0;
            // StandingPlatfom = null;
            // HittedClingWall = null;
        }

        public void ClearPlatform()
        {
            OnForwardGround = false;
            OnCenterGround = false;
            OnBackGround = false;

            ForwardPlatform = null;
            CenterPlatofrm = null;
            BackPlatform = null;

            ForwardAngle = 0f;
            CenterAngle = 0f;
            BackAngle = 0f;
        }

        public void Save()
        {
            WasOnGroundLastFrame = OnGround;
        }
    }
}
