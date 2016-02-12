using UnityEngine;
using UnityEngine.Events;
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
        public SmoothType smoothType;
        UpdateType updateType;


		//--------------------------------------------------------------------------
		// event
		//--------------------------------------------------------------------------
		public UnityAction OnJustGotGround;

        //--------------------------------------------------------------------------
        // private protected
        //--------------------------------------------------------------------------

        Rigidbody2D mRb;

        Transform mTr;

        Vector3 mCastOriginWall;
        float mCastOriginWallDistance;
		float mCastGroundRayLength;

        PhysicsMaterial2D mPhysicsMaterial;

        Vector2 mMovingPlatformVelocity;

        List<Platform> mPlatformResults;
        List<bool> mIsGroundResults;
        List<float> mSlopeAngleResults;
        List<Vector3> mOriginList;

        bool mMoveLocked;
        Coroutine mLockMoveRoutine;

        //--------------------------------------------------------------------------
        // get;set
        //--------------------------------------------------------------------------
        public Rigidbody2D StompableObject { get; private set; }
        public DECharacterControllerState state { get; private set; }
        public float addedGravity { get; protected set; }
        public float axisX { get; set; }
        public float targetSpeed { get; set; }
        public Vector2 velocity { get { return mRb.velocity; } }
        public float vx { get { return mRb.velocity.x; } set { mRb.velocity = new Vector2(value, mRb.velocity.y); } }
        public float vy { get { return mRb.velocity.y; } set { mRb.velocity = new Vector2(mRb.velocity.x, value); } }

        void Awake()
        {
            mRb = GetComponent<Rigidbody2D>();

            mRb.angularDrag = 0f;
            mRb.constraints = RigidbodyConstraints2D.FreezeRotation;

            mTr = GetComponent<Transform>();

            state = new DECharacterControllerState();

            mPlatformResults = new List<Platform>(new Platform[] { null, null, null });
            mSlopeAngleResults = new List<float>(new float[] { 0f, 0f, 0f });
            mIsGroundResults = new List<bool>(new bool[] { false, false, false });
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
            updateType = DruggedEngine.MOVE_CHARACTER;
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
			forwadGround.y = center.y;

            centerGround.x = center.x;
			centerGround.y = center.y;

            backGround.x = min.x;
			backGround.y = center.y;


            mOriginList[0] = forwadGround;
            mOriginList[1] = centerGround;
            mOriginList[2] = backGround;

            mCastOriginWall = center;
            mCastOriginWallDistance = b.extents.x + raySafetyDis;
			mCastGroundRayLength = b.extents.y + raySafetyDis;
        }

        //--------------------------------------------------------------------------
        // behaviour
        //--------------------------------------------------------------------------

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
            state.StandingPlatform.PassThough(this);
        }

        public void IgnoreCollision(Collider2D collider, bool ignore)
        {
            print("ignore");
            Physics2D.IgnoreCollision(primaryCollider, collider, ignore);
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
        
        public void LockMove(float duration)
        {
            if( mLockMoveRoutine != null ) StopCoroutine( mLockMoveRoutine );
            mLockMoveRoutine = StartCoroutine(LockMoveRoutine(duration));
        }

        IEnumerator LockMoveRoutine(float duration )
        {
            mMoveLocked = true;
            yield return new WaitForRealSeconds(duration);
            mMoveLocked = false;
        }

        //character에서 호출된다.
        public void Move(float delta)
        {
            Vector2 current = mRb.velocity;

            //x
            if (mMoveLocked == false)
            {
                float targetVX = axisX * targetSpeed;
                targetVX += mMovingPlatformVelocity.x;
                float movementFactor = 25f;//15,25 //지상과 공중의 차이가 있어야 한다. 공중에서 급격한 방향 전환이 어렵도록.var movementFactor = _controller.State.IsGrounded ? AccelOnGround : AccelOnAir;
                switch (smoothType)
                {
                    case SmoothType.MoveTowards:
                        current.x = Mathf.MoveTowards(current.x, targetVX, delta * movementFactor);
                        break;
                    case SmoothType.Lerp:
                        current.x = Mathf.Lerp(current.x, targetVX, delta * movementFactor);
                        break;
                }
            }

            //y
            current.y += mMovingPlatformVelocity.y;
            if (state.IsGround == false)
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
            if (updateType == UpdateType.FixedUpdate)
            {
                print("d");
                Move(Time.fixedDeltaTime);
            }

            state.Reset();

            //detect
            DetectGround();
            DetectFront();

            //위를 체크 하긴 해야함. 앉아 있다가 일어서려고 할 때 하기만 하면 될까?
            DetectStompObject();//Stompable.cs 참고. 밟히는 쪽에 설치하는게 올바른 것 같다.

			if( state.WasOnGroundLastFrame == false && state.IsGround )
			{
				if( OnJustGotGround != null ) OnJustGotGround();
			}
				
            //save
            state.Save();
        }

        void DetectGround()
        {
            //if (mRb.velocity.y > 0f) return;

            GroundCast(0);
            GroundCast(1);
            GroundCast(2);

			state.IsGroundForward = mIsGroundResults[0];
            state.IsGroundCenter = mIsGroundResults[1];
            state.IsGroundBack = mIsGroundResults[2];

			state.ForwardPlatform = mPlatformResults[0];
            state.CenterPlatofrm = mPlatformResults[1];
            state.BackPlatform = mPlatformResults[2];

			state.ForwardAngle = mSlopeAngleResults[0];
			state.CenterAngle = mSlopeAngleResults[1];
			state.BackAngle = mSlopeAngleResults[2];

            //			state.Slope = hit.normal.y;

            //            // if (standingPlatform == null) mMovingPlatformVelocity = Vector2.zero;
            //            // else mMovingPlatformVelocity = standingPlatform.velocity;
        }

        void GroundCast(int idx)
        {
            Vector3 origin = mTr.TransformPoint(mOriginList[idx]);
			RaycastHit2D hit = PhysicsUtil.DrawRayCast(origin, Vector2.down, mCastGroundRayLength, currentMask, Color.red);
            Platform platform = null;
            float angle = 0f;
			bool isGrounded = false;

            if (hit && hit.collider != null && hit.collider.isTrigger == false)
            {
                isGrounded = true;
                platform = hit.collider.GetComponent<Platform>();
                //angle = hit.normal.y;
                angle = Vector2.Angle( hit.normal, Vector2.up );
            }

            mIsGroundResults[idx] = isGrounded;
			mPlatformResults[idx] = platform;
			mSlopeAngleResults[idx] = angle;
        }

        void DetectStompObject()
        {
            StompableObject = GetRelevantCharacterCast(mOriginList[0], 0.15f);
            if (StompableObject == null) StompableObject = GetRelevantCharacterCast(mOriginList[1], 0.15f);
            if (StompableObject == null) StompableObject = GetRelevantCharacterCast(mOriginList[2], 0.15f);
        }

        void DetectFront()
        {
            float x = mRb.velocity.x;
            if (x == 0f) return;
            Vector3 origin = mTr.TransformPoint(mCastOriginWall);
            Vector2 direction = Vector2.right * axisX;
            float rayLength = mCastOriginWallDistance + x * Time.deltaTime;
            RaycastHit2D hit = PhysicsUtil.DrawRayCast(origin, direction, rayLength, currentMask, Color.red);

            if (hit && hit.collider != null && hit.collider.isTrigger == false)
            {
                state.CollidingFront = hit.collider;

                //각도도 검사해야 할 수도?
            }
        }

        public bool IsPressAgainstWall
        {
            get
            {
                if (state.IsCollidingFront == false) return false;
                Wall wall = state.CollidingFront.GetComponent<Wall>();
                if (wall == null) return false;

                if (wall.slideWay == Wall.WallSlideWay.NOTHING) return false;
                else if (wall.slideWay == Wall.WallSlideWay.BOTH) return true;
                else if (wall.slideWay == Wall.WallSlideWay.LEFT && axisX > 0) return true;
                else if (wall.slideWay == Wall.WallSlideWay.RIGHT && axisX < 0) return true;
                return false;
            }
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

            if (state.IsGround)
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

		//side
        public bool IsCollidingFront { get { return CollidingFront != null; } }
        public Collider2D CollidingFront { get; set; }

        //slope
		public bool IsOnSlope { get{ return Mathf.Abs( SlopeAngle ) > 5; }}
		public float SlopeAngle { get{ return ForwardAngle; }}
        public float ForwardAngle { get; set; }
        public float CenterAngle { get; set; }
        public float BackAngle { get; set; }

        //ground
        public bool IsGround { get { return IsGroundCenter || IsGroundBack || IsGroundForward; } }
        public bool IsGroundForward { get; set; }
        public bool IsGroundCenter { get; set; }
        public bool IsGroundBack { get; set; }

		//platform
        public bool IsOnOneway
        {
            get
            {
                if (IsGround == false) return false;
                if (StandingPlatform == false) return false;
                if (StandingPlatform.oneway == false) return false;
                return true;
            }
        }

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
            CollidingFront = null;

            ClearPlatform();
        }

        public void ClearPlatform()
        {
            IsGroundForward = false;
            IsGroundCenter = false;
            IsGroundBack = false;

            ForwardPlatform = null;
            CenterPlatofrm = null;
            BackPlatform = null;

            ForwardAngle = 0f;
            CenterAngle = 0f;
            BackAngle = 0f;
        }

        public void Save()
        {
            WasOnGroundLastFrame = IsGround;
        }
    }
}
