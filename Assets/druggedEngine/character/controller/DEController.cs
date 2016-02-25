using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace druggedcode.engine
{
	[RequireComponent (typeof(Collider2D), typeof(Rigidbody2D))]
	public class DEController : MonoBehaviour
	{
		//----------------------------------------------------------------------------------------------------------
		// helper 상수들
		//----------------------------------------------------------------------------------------------------------
		const float LARGE_VALUE = 500000f;
		const float SMALL_VALUE = 0.0001f;
		public const int VERTICAL_RAY_NUM = 3;
		const float GROUND_RAY_LENGTH = 0.4f;

		//----------------------------------------------------------------------------------------------------------
		// Inspector
		//----------------------------------------------------------------------------------------------------------


		[Header ("Collision")]
		public BoxCollider2D headChecker;

		[Header ("Move")]
		public float AccelOnGround = 10f;
		public float AccelOnAir = 3f;

		[Header ("Parameters")]

		public float GravityScale = 1;
		//넘을 수 있는 바닥 높이 비율 ( 충돌 BoxCollider2D 의 크기에 비례 )
		[Range (0, 1)]
		public float ToleranceHeightRatio = 0.2f;
		/// 캐릭터가 걸을 수 있는 최고 앵글( degree )
		[Range (0, 80)]
		public float MaximumSlopeAngle = 30f;

		//충돌감지를 위한 레이캐스팅 설정
		[Header ("RayCasting")]
		[Range (2, 10)]
		public int RayHorizontalCount = 3;
		public Vector2 RaySafetyDis = new Vector2(0.1f,0.01f);

		//----------------------------------------------------------------------------------------------------------
		//get;set;
		//----------------------------------------------------------------------------------------------------------
		public DEControllerState state { get; private set; }

		public float CurrentSpeed { get; set; }

		public Vector2 Velocity {
			get { return _speed; }
			set
			{
				_speed = value;
				_externalForce = value;
			}
		}

		public float vx {
			get { return _speed.x; }
			set
			{
				_speed.x = value;
				_externalForce.x = value;
			}
		}

		public float vy {
			get { return _speed.y; }
			set
			{
				_speed.y = value;
				_externalForce.y = value;
			}
		}

		public float gravity { get { return DruggedEngine.Gravity + _physicSpaceInfo.Gravity; } }

//		public float PlatformFriction{ get { return state.StandingOn == null ? 1f : state.StandingOn.friction; } }
//		public Vector2 PlatformVelocity{ get { return state.StandingOn == null ? Vector2.zero : state.StandingOn.velocity; } }

		public float PlatformFriction{ get { return 1f;} }
		public Vector2 PlatformVelocity{ get { return Vector2.zero; }}

		//----------------------------------------------------------------------------------------------------------
		// 속도, 움직임. y가 마이너스인 경우가 낙하상태이다.
		//----------------------------------------------------------------------------------------------------------

		Vector2 _externalForce;

		Vector2 _speed;
		Vector2 mMoveDirection = Vector2.right;


		LayerMask mDefaultPlatformMask;
		Vector2 mTranslateVector;

		PhysicInfo _physicSpaceInfo;

		float mLockedVY;

		bool mMoveLocked;
		Coroutine mLockMoveRoutine;

		//----------------------------------------------------------------------------------------------------------
		// caching components
		//----------------------------------------------------------------------------------------------------------
		Transform mTr;
		BoxCollider2D mCollider;

		Vector3 mHeadCheckerPos;
		Vector3 mHeadCheckerExtents = Vector3.zero;

		bool mCheckCollisions;

		Vector2 _colliderDefaultSize;
		float _toleranceHeight;

		int mHitCount;
		//충돌 횟수
		ColliderInfo mColliderBound;
		RaycastHit2D mHit2D;
		List<Rigidbody2D> _sideHittedPushableObject;


		void Awake ()
		{
			GetComponent<Rigidbody2D> ().isKinematic = true;

			if (headChecker != null)
			{
				mHeadCheckerPos = headChecker.transform.localPosition;
				mHeadCheckerExtents = headChecker.bounds.extents;
				headChecker.enabled = false;
			}

			mCollider = GetComponent<BoxCollider2D> ();

			mTr = transform;

			_colliderDefaultSize = mCollider.size;
			_toleranceHeight = mCollider.bounds.size.y * ToleranceHeightRatio;

			_sideHittedPushableObject = new List<Rigidbody2D> ();
			state = new DEControllerState ();

			mCheckCollisions = true;

			ResetPhysicInfo ();
		}

		void Start ()
		{
			mDefaultPlatformMask = DruggedEngine.MASK_ALL_GROUND;
			UpdateBound ();
		}

		//----------------------------------------------------------------------------------------------------------
		// 속도관련
		//----------------------------------------------------------------------------------------------------------

		public void AddForce (Vector2 force)
		{
			_speed += force;
			_externalForce += force;
		}

		public void LockMove (float duration)
		{
			if (mLockMoveRoutine != null) StopCoroutine (mLockMoveRoutine);
			mLockMoveRoutine = StartCoroutine (LockMoveRoutine (duration));
		}

		IEnumerator LockMoveRoutine (float duration)
		{
			mMoveLocked = true;
			yield return new WaitForRealSeconds (duration);
			mMoveLocked = false;
		}
			
		public void Stop ()
		{
			_speed = Vector2.zero;
		}

		//----------------------------------------------------------------------------------------------------------
		// core logic. 충돌처리. Update 에서 설정한 값, 상황을  LateUpdate 에서 실제로 처리한다.
		//----------------------------------------------------------------------------------------------------------

		public void LockVY (float lockvy)
		{
			mLockedVY = lockvy;
		}

		public void UnLockVY ()
		{
			mLockedVY = 0f;
		}

		//void FixedUpdate ()
		void LateUpdate ()
		{
			float delta: af
			_sideHittedPushableObject.Clear ();

			mTranslateVector = _speed * Time.deltaTime;

			//y speed
			if (mLockedVY != 0f)
			{
				_speed.y = mLockedVY;
			}
			else
			{
				_speed.y += gravity * GravityScale * delta;
			}

			state.SaveLastStateAndReset ();
			UpdateBound ();

			CheckMoveDirection ();

			CheckCollisions ();

			mTr.Translate (mTranslateVector, Space.World);

			_externalForce = Vector2.zero;
		}

		void CheckMoveDirection ()
		{
			if ( mMoveDirection.x == 1 && _speed.x < 0)
			{
				mMoveDirection = new Vector2 (-1, 0);
			}
			else if (mMoveDirection.x == -1 && _speed.x > 0)
			{
				mMoveDirection = new Vector2 (1, 0);
			}
		}

		//충돌검사. 충돌 상황에 따라 _translateVector 가 변경 될 수 있다.
		void CheckCollisions ()
		{
			if (mCheckCollisions == false) return;

			CastRaysBelow ();
			CastRaysSide ();
			CastRaysAbove ();

			if (Time.deltaTime > 0)	_speed = mTranslateVector / Time.deltaTime; //충돌로 인해 변경된 벡터를 바탕으로 속도 재설정.

			if (state.HasCollisions) PushHittedObject (); //밀수 있는 것들은 민다.

			//지상에 막 닿은건지 아닌지를 판단한다.
			if (state.WasColldingBelowLastFrame == false && state.IsGrounded) state.JustGotGrounded = true;
		}

		void CastRaysBelow ()
		{
			if ( mTranslateVector.y > 0) return;

			float rayLength = mColliderBound.hHalf + GROUND_RAY_LENGTH;
			if( mTranslateVector.y < 0f ) rayLength += Mathf.Abs (mTranslateVector.y);

			int rayIndex, rayIndexIncrease;
			if (mMoveDirection.x == 1)
			{
				rayIndex = VERTICAL_RAY_NUM - 1;
				rayIndexIncrease = -1;
			}
			else
			{
				rayIndex = 0;
				rayIndexIncrease = 1;
			}

			Vector2 verticalRayCastFromLeft = new Vector2 (mColliderBound.xLeft + mTranslateVector.x, mColliderBound.yCenter);
			Vector2 verticalRayCastToRight = new Vector2 (mColliderBound.xRight + mTranslateVector.x, mColliderBound.yCenter);

			mHitCount = 0;

			float sumY = 0f;
			float fowardY = 0;

			//float tolerance = mColliderBound.yBottom + _toleranceHeight;

			RaycastHit2D closestHit = new RaycastHit2D();
			float closestY = -LARGE_VALUE;

			for (int i = 0; i < VERTICAL_RAY_NUM; i++)
			{
				float hitY;
				Color rayColor = Color.red;

				#if UNITY_EDITOR
				if( i == 1 ) rayColor = Color.grey;
				else if( i == 2 ) rayColor = Color.blue;
				#endif

				Vector2 rayOriginPoint = Vector2.Lerp (verticalRayCastFromLeft, verticalRayCastToRight, (float)rayIndex / (float)(VERTICAL_RAY_NUM - 1));

				if ( mTranslateVector.y > 0f && state.WasColldingBelowLastFrame == false )
					mHit2D = PhysicsUtil.DrawRayCast (rayOriginPoint, -Vector2.up, rayLength, DruggedEngine.MASK_EXCEPT_ONEWAY_GROUND, rayColor );
				else
					mHit2D = PhysicsUtil.DrawRayCast (rayOriginPoint, -Vector2.up, rayLength, mDefaultPlatformMask, rayColor );


				if (mHit2D)
				{
					hitY = mHit2D.point.y;
					if (i == 0) fowardY = hitY;

					if ( hitY > mColliderBound.yBottom &&
						state.WasColldingBelowLastFrame == false &&
						mHit2D.collider.gameObject.layer == DruggedEngine.LAYER_ONEWAY )
					{
						//print( "hitY: " +hitY + ", yBottom: " + mColliderBound.yBottom );
						state.IsGroundedInfo [i] = false;
					}
					else
					{
						++mHitCount;
						sumY += hitY;
						//print( "i : " + i + ", hitY : " + hitY + ", lowestY :" + closestY );
						if (hitY > closestY)
						{
							closestHit = mHit2D;
							closestY = hitY;
						}

						state.IsGroundedInfo [i] = true;
					}
				}
				else
				{
					state.IsGroundedInfo [i] = false;
				}

				rayIndex += rayIndexIncrease;
			}

			if (mHitCount == 0) return;
			if (closestY < mColliderBound.yBottom - RaySafetyDis.y + mTranslateVector.y) return;

			//지면에 닿았다
			state.IsCollidingBelow = true;
			mTranslateVector.y = 0;
			state.StandingOn = closestHit.collider.gameObject;
			state.SlopeAngle = Vector2.Angle (closestHit.normal, Vector2.up);

			if (state.SlopeAngle > 0)
			{
				if (fowardY > mTr.position.y && state.SlopeAngle > MaximumSlopeAngle)
				{
					mTranslateVector.x = 0;
				}
				mTr.position = new Vector3 (mTr.position.x, sumY / mHitCount, mTr.position.z);
			}
			else
			{
				mTr.position = new Vector3 (mTr.position.x, closestY + RaySafetyDis.y, mTr.position.z);
			}
		}

		void CastRaysSide ()
		{
			float horizontalRayLength = mColliderBound.wHalf + RaySafetyDis.x + Mathf.Abs (mTranslateVector.x);

			Vector2 horizontalRayCastToTop = new Vector2 (mColliderBound.xCenter, mColliderBound.yTop);
			Vector2 horizontalRayCastFromBottom = new Vector2 (mColliderBound.xCenter, mColliderBound.yBottom + _toleranceHeight);

			mHitCount = 0;

			//위에서 아래로 내려가면서 지정한 분할 수 만큼 검사
			for (int i = 0; i < RayHorizontalCount; i++)
			{
				Vector2 rayOriginPoint = Vector2.Lerp (horizontalRayCastToTop, horizontalRayCastFromBottom, (float)i / (float)(RayHorizontalCount - 1));

//				if ( state.WasColldingBelowLastFrame && i == RayHorizontalCount - 1)
//					mHit2D = PhysicsUtil.DrawRayCast (rayOriginPoint, mMoveDirection, horizontalRayLength, mDefaultPlatformMask, Color.red);
//				else
//					mHit2D = PhysicsUtil.DrawRayCast (rayOriginPoint, mMoveDirection, horizontalRayLength, DruggedEngine.MASK_EXCEPT_ONEWAY_GROUND, Color.red);

				if (false)
					mHit2D = PhysicsUtil.DrawRayCast (rayOriginPoint, mMoveDirection, horizontalRayLength, DruggedEngine.MASK_ALL_GROUND, Color.red);
				else
					mHit2D = PhysicsUtil.DrawRayCast (rayOriginPoint, mMoveDirection, horizontalRayLength, DruggedEngine.MASK_EXCEPT_ONEWAY_GROUND, Color.red);
				
				if (mHit2D)
				{
					if (i == RayHorizontalCount - 1 && Vector2.Angle (mHit2D.normal, Vector2.up) < MaximumSlopeAngle)
					{
						//가장 아래의 레이가 막혔지만 허용 가능한 경사이므로 막혔다고 체크하지 않는다.
					}
					else
					{
						++mHitCount;
						break;
					}
				}
			}

			if (mHitCount == 0) return;


			if (mMoveDirection.x == 1)
			{
				state.IsCollidingRight = true;
				mTranslateVector.x = mHit2D.point.x - mColliderBound.xRight - RaySafetyDis.x;
			}
			else
			{
				state.IsCollidingLeft = true;
				mTranslateVector.x = mHit2D.point.x - mColliderBound.xLeft + RaySafetyDis.x;
			}

			state.CollidingSide = mHit2D.collider;

			if (mHit2D.rigidbody != null) _sideHittedPushableObject.Add (mHit2D.rigidbody);
		}

		void CastRaysAbove ()
		{
			//낙하중일땐 무시
			if (mTranslateVector.y < 0) return;

			float rayLength = mColliderBound.hHalf + RaySafetyDis.y + mTranslateVector.y;

			Vector2 verticalRayCastStart = new Vector2 (mColliderBound.xLeft + mTranslateVector.x, mColliderBound.yCenter);
			Vector2 verticalRayCastEnd = new Vector2 (mColliderBound.xRight + mTranslateVector.x, mColliderBound.yCenter);

			mHitCount = 0;

			for (int i = 0; i < VERTICAL_RAY_NUM; i++)
			{
				Vector2 rayOriginPoint = Vector2.Lerp (verticalRayCastStart, verticalRayCastEnd, (float)i / (float)(VERTICAL_RAY_NUM - 1));
				mHit2D = PhysicsUtil.DrawRayCast (rayOriginPoint, Vector2.up, rayLength, DruggedEngine.MASK_EXCEPT_ONEWAY_GROUND, Color.blue);

				if (mHit2D)
				{
					++mHitCount;
					break;
				}

			}

			if (mHitCount == 0) return;

			state.IsCollidingAbove = true;

			if (state.IsCollidingBelow == false)
			{
				float ty = mHit2D.point.y - mColliderBound.h - RaySafetyDis.y;
				mTr.position = new Vector3 (mTr.position.x, ty, mTr.position.z);
				mTranslateVector.y = 0;
			}
		}

		//----------------------------------------------------------------------------------------------------------
		// 충돌체 크기 변경
		//----------------------------------------------------------------------------------------------------------
		public void UpdateColliderSize (float xScale, float yScale)
		{
			UpdateColliderSize (new Vector2 (_colliderDefaultSize.x * xScale, _colliderDefaultSize.y * yScale));
		}

		public void ResetColliderSize ()
		{
			UpdateColliderSize (_colliderDefaultSize);
		}

		void UpdateColliderSize (Vector2 size)
		{
			mCollider.size = size;
			mCollider.offset = new Vector2 (0f, mCollider.size.y * 0.5f);

			UpdateBound ();
		}

		//----------------------------------------------------------------------------------------------------------
		// 현재 BoxCollider의 위치, 사이즈에 대한 정보를 충돌 검사 계산 시 사용하기 용이한 형태의 자료구조로 생성한다.
		//----------------------------------------------------------------------------------------------------------
		void UpdateBound ()
		{
			Bounds bounds = mCollider.bounds;
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;
			Vector3 center = bounds.center;
			Vector3 size = bounds.size;

			mColliderBound = new ColliderInfo (
				min.x, center.x, max.x, size.x,
				min.y, center.y, max.y, size.y
			);

			//editor 인 경우 캐릭터의 Collider를 명확히 표시하자.
			if (Application.isEditor)
			{
				Color drawColor = Color.green;

				Debug.DrawLine (new Vector2 (mColliderBound.xLeft, mColliderBound.yBottom), new Vector2 (mColliderBound.xRight, mColliderBound.yBottom), drawColor);
				Debug.DrawLine (new Vector2 (mColliderBound.xLeft, mColliderBound.yCenter), new Vector2 (mColliderBound.xRight, mColliderBound.yCenter), drawColor);
				Debug.DrawLine (new Vector2 (mColliderBound.xLeft, mColliderBound.yTop), new Vector2 (mColliderBound.xRight, mColliderBound.yTop), drawColor);

				Debug.DrawLine (new Vector2 (mColliderBound.xLeft, mColliderBound.yBottom), new Vector2 (mColliderBound.xLeft, mColliderBound.yTop), drawColor);
				Debug.DrawLine (new Vector2 (mColliderBound.xCenter, mColliderBound.yBottom), new Vector2 (mColliderBound.xCenter, mColliderBound.yTop), drawColor);
				Debug.DrawLine (new Vector2 (mColliderBound.xRight, mColliderBound.yBottom), new Vector2 (mColliderBound.xRight, mColliderBound.yTop), drawColor);
			}
		}

		//----------------------------------------------------------------------------------------------------------
		// 충돌판정을 끄고 켠다.
		//----------------------------------------------------------------------------------------------------------

		public void PassThroughOneway ()
		{
			StartCoroutine (DisableCollisionsWithOneWayPlatforms( 0.1f ));
		}

		public virtual IEnumerator DisableCollisionsWithOneWayPlatforms (float duration)
		{
			mDefaultPlatformMask = DruggedEngine.MASK_EXCEPT_ONEWAY_GROUND;
			yield return new WaitForSeconds (duration);
			mDefaultPlatformMask = DruggedEngine.MASK_ALL_GROUND;
		}

		public void CollisionsOn ()
		{
			mCheckCollisions = true;
		}

		public void CollisionsOff (float duration = 0f)
		{
			if (duration == 0f) mCheckCollisions = false;
			else StartCoroutine (DisableCollisionRoutine (duration));
		}

		IEnumerator DisableCollisionRoutine (float duration)
		{
			mCheckCollisions = false;
			yield return new WaitForSeconds (duration);
			mCheckCollisions = true;
		}

		void PushHittedObject ()
		{
			/*
            //사이드로 충돌했을 때 저장해둔 목록을 가져와 파라메터에서 설정한 값으로 민다.
			foreach (Rigidbody2D body in _sideHittedPushableObject)
			{
				if (body == null || body.isKinematic)
					continue;

				Vector3 pushDir = new Vector3(_velocity.x, 0, 0);
				//  body.AddForce ( pushDir.normalized * Physics2DPushForce );
				body.velocity = new Vector3(_velocity.x, 0, 0) * 2;
			}
			*/
		}

		public void SetPhysicsSpace (PhysicInfo physicInfo)
		{
			_physicSpaceInfo = physicInfo;
		}

		public void ResetPhysicInfo ()
		{
			_physicSpaceInfo = new PhysicInfo (0, 1);
		}

		public bool IsCollidingHead {
			get {
				if (mHeadCheckerExtents != Vector3.zero)
				{

					Vector2 pointA = mTr.position + mHeadCheckerPos - mHeadCheckerExtents;
					Vector2 pointB = mTr.position + mHeadCheckerPos + mHeadCheckerExtents;
					return Physics2D.OverlapArea (pointA, pointB, DruggedEngine.MASK_EXCEPT_ONEWAY_GROUND);
				}
				else
				{
					return state.IsCollidingAbove;
				}
			}
		}

		struct ColliderInfo
		{
			public float xLeft, xCenter, xRight;
			public float w, wHalf;
			public float yBottom, yCenter, yTop;
			public float h, hHalf;

			public ColliderInfo (
				float xLeft, float xCenter, float xRight, float w,
				float yBottom, float yCenter, float yTop, float h)
			{
				this.xLeft = xLeft;
				this.xCenter = xCenter;
				this.xRight = xRight;
				this.w = w;
				this.wHalf = w * 0.5f;

				this.yBottom = yBottom;
				this.yCenter = yCenter;
				this.yTop = yTop;
				this.h = h;
				this.hHalf = h * 0.5f;
			}

			public Vector2 bottom {
				get { return new Vector2 (xCenter, yBottom); }
			}
		}
	}

	public class DEControllerState
	{
		// 상하좌우 충돌여부
		public bool IsCollidingAbove { get; set; }
		public bool IsCollidingBelow { get; set; }
		public bool IsCollidingLeft { get; set; }
		public bool IsCollidingRight { get; set; }
		public bool HasCollisions { get { return IsCollidingLeft || IsCollidingRight || IsCollidingAbove || IsCollidingBelow; } }

		public bool WasColldingBelowLastFrame { get; set; }
		public bool WasColldingAdoveLastFrame { get; set; }
		public bool JustGotGrounded { get; set; }

		public bool IsGrounded { get { return IsCollidingBelow; } }
		public bool IsGroundedForward { get { return IsGroundedInfo [0]; } }
		public bool IsGroundedCenter { get { return IsGroundedInfo [1]; } }
		public bool IsGroundedBack { get { return IsGroundedInfo [2]; } }
		public List<bool> IsGroundedInfo{ get; private set; }

		public float SlopeAngle { get; set; }

		//IsCollidingBelow 와 같이 변해야 한다.
		public GameObject StandingOn { get; set;}
		public Collider2D CollidingSide { get; set; }

		public bool IsOnOneway {
			get {
				if (IsGrounded == false) return false;
				else if (StandingOn == null) return false;

				Platform platform = StandingOn.GetComponent<Platform>();
				if( platform == null ) return false;
				else return platform.oneway;
			}
		}

		public DEControllerState ()
		{
			IsGroundedInfo = new List<bool> (new bool[DEController.VERTICAL_RAY_NUM]);

			Reset ();
		}

		public void ClearPlatform ()
		{
			IsCollidingBelow = false;
			StandingOn = null;
		}

		public void Reset ()
		{
			IsCollidingAbove = IsCollidingBelow = IsCollidingLeft = IsCollidingRight = false;

			JustGotGrounded = false;
			SlopeAngle = 0;
			StandingOn = null;
			CollidingSide = null;
		}

		public void SaveLastStateAndReset ()
		{
			WasColldingBelowLastFrame = IsCollidingBelow;
			WasColldingAdoveLastFrame = IsCollidingAbove;

			Reset ();
		}
	}
}

