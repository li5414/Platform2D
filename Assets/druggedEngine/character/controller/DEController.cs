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
		public float RaySafetyDis = 0.1f;
		public float RayGroundOffset = 0.5f;

		//----------------------------------------------------------------------------------------------------------
		//get;set;
		//----------------------------------------------------------------------------------------------------------
		public DEControllerState state { get; private set; }

		public float CurrentSpeed { get; set; }

		public float axisX { get; set; }

		public Vector2 Velocity { get { return mVelocity; } }

		public float vx { get { return mVelocity.x; } set { mVelocity.x = value; } }

		public float vy { get { return mVelocity.y; } set { mVelocity.y = value; } }

		public float gravity { get { return DruggedEngine.Gravity + _physicSpaceInfo.Gravity; } }

		public float PlatformFriction{ get { return state.StandingPlatfom == null ? 1f : state.StandingPlatfom.friction; } }

		public Vector2 PlatformVelocity{ get { return state.StandingPlatfom == null ? Vector2.zero : state.StandingPlatfom.velocity; } }

		//----------------------------------------------------------------------------------------------------------
		// 속도, 움직임. y가 마이너스인 경우가 낙하상태이다.
		//----------------------------------------------------------------------------------------------------------

		Vector2 _addForce;
		Vector2 mVelocity;
		Vector2 mMoveDirection = Vector2.right;



		//현재 velocity 에 의해 현재 프레임에서 움직일 벡터
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

		//넘어갈수있는 장애물 높이

		// 충돌 체크 여부
		int mHitCount;
		//충돌 횟수
		ColliderInfo _bound;
		RaycastHit2D mHit2D;
		Vector2 _rayOriginPoint;
		List<Rigidbody2D> _sideHittedPushableObject;
		//좌우로 충돌된 플랫폼


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
			UpdateBound ();
		}

		//----------------------------------------------------------------------------------------------------------
		// 속도관련
		//----------------------------------------------------------------------------------------------------------

		public void AddForce (Vector2 force)
		{
			_addForce += force;
		}

		public void AddHorizontalForce (float x)
		{
			_addForce.x += x;
		}

		public void AddVerticalForce (float y)
		{
			_addForce.y += y;
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
			axisX = 0f;
			mVelocity = Vector2.zero;
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

		void LateUpdate ()
		{
			CalculateTranslateVector ();
			CheckMoveDirection ();

			state.SaveLastStateAndReset ();
			CheckCollisions ();

			mTr.Translate (mTranslateVector, Space.World);

			UpdateBound ();
		}

		void CalculateTranslateVector ()
		{
			float delta = Time.deltaTime;
			Vector2 current = mVelocity;

			//x speed
			if (mMoveLocked == false)
			{
				float moveFactor = state.IsGrounded ? AccelOnGround : AccelOnAir;
				float tx = axisX * CurrentSpeed;
				if (tx != 0f) tx = Mathf.Lerp (mVelocity.x, tx, delta * moveFactor);

				if (state.IsGrounded)
				{
					mVelocity.x += (tx - mVelocity.x) * PlatformFriction;
				}
				else
				{
					mVelocity.x = tx;
				}

				mVelocity += _addForce;
				mVelocity.x *= _physicSpaceInfo.MoveFactor;
			}

			//y speed
			if (mLockedVY != 0f)
			{
				mVelocity.y = mLockedVY;
			}
			else
			{
				mVelocity.y += gravity * GravityScale * delta;
			}

			_addForce = Vector2.zero;

			mTranslateVector = mVelocity * Time.deltaTime;
		}

		void CheckMoveDirection ()
		{
			if ( mMoveDirection.x == 1 && mVelocity.x < 0)
			{
				mMoveDirection = new Vector2 (-1, 0);
			}
			else if (mMoveDirection.x == -1 && mVelocity.x > 0)
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

			mVelocity = mTranslateVector / Time.deltaTime; //충돌로 인해 변경된 벡터를 바탕으로 속도 재설정.

			if (state.HasCollisions) PushHittedObject (); //밀수 있는 것들은 민다.

			//지상에 막 닿은건지 아닌지를 판단한다.
			if (state.WasColldingBelowLastFrame == false && state.IsGrounded) state.JustGotGrounded = true;
		}

		void CastRaysBelow ()
		{
			//상승중일땐 무시
			if (mTranslateVector.y > 0) return;
			//  float rayLength = _bound.hHalf + Mathf.Abs(_translateVector.y);
			//  if( _state.IsGrounded ) rayLength += RayGroundOffset;
			float rayLength = _bound.hHalf + RayGroundOffset + Mathf.Abs (mTranslateVector.y);

			int rayIndex, increase;
			if (mMoveDirection.x == 1)
			{
				rayIndex = VERTICAL_RAY_NUM - 1;
				increase = -1;
			}
			else
			{
				rayIndex = 0;
				increase = 1;
			}

			Vector2 verticalRayCastFromLeft = new Vector2 (_bound.xLeft + mTranslateVector.x, _bound.yCenter);
			Vector2 verticalRayCastToRight = new Vector2 (_bound.xRight + mTranslateVector.x, _bound.yCenter);

			mHitCount = 0;

			float lowestY = -LARGE_VALUE;
			float sumY = 0f;
			float fowardY = 0;
			RaycastHit2D closestHit = new RaycastHit2D();
			float closestAngle = 0f;
			float tolerance = _bound.yBottom + _toleranceHeight;

			float hitY, angle;

			for (int i = 0; i < VERTICAL_RAY_NUM; i++)
			{
				_rayOriginPoint = Vector2.Lerp (verticalRayCastFromLeft, verticalRayCastToRight, (float)rayIndex / (float)(VERTICAL_RAY_NUM - 1));

				if (i == 0) mHit2D = PhysicsUtil.DrawRayCast (_rayOriginPoint, -Vector2.up, rayLength, DruggedEngine.MASK_ALL_GROUND, Color.red);
				else if (i == 1) mHit2D = PhysicsUtil.DrawRayCast (_rayOriginPoint, -Vector2.up, rayLength, DruggedEngine.MASK_ALL_GROUND, Color.green);
				else if (i == 2) mHit2D = PhysicsUtil.DrawRayCast (_rayOriginPoint, -Vector2.up, rayLength, DruggedEngine.MASK_ALL_GROUND, Color.blue);

				if (mHit2D)
				{
					hitY = mHit2D.point.y;
					if (i == 0) fowardY = hitY;
					angle = Vector2.Angle (mHit2D.normal, Vector2.up);

					//바닥이 아니라고 무시해야하는 경우. oneway를 스으윽 발 아래로 내려볼때 발생하는 버그 해결해야함
					if (false)
					{
						state.IsGroundedInfo [i] = false;
					}
					else
					{
						++mHitCount;
						sumY += hitY;
						if (hitY > lowestY)
						{
							closestHit = mHit2D;
							closestAngle = angle;
							lowestY = hitY;
						}

						state.IsGroundedInfo [i] = true;
					}
				}
				else
				{
					state.IsGroundedInfo [i] = false;
				}

				rayIndex += increase;
			}

			//충돌된 것이 없다.
			if (mHitCount == 0) return;
			if (lowestY < _bound.yBottom + mTranslateVector.y) return;

			Platform hittedPlatform = closestHit.collider.gameObject.GetComponent<Platform> ();

			//아래 조건이 어떤상황인지 모르겠다.
//			if (state.WasColldingBelowLastFrame == false && lowestY > _bound.yBottom && hittedPlatform.oneway )
//			{
//				return;
//			}

			state.IsCollidingBelow = true;
			state.SlopeAngle = closestAngle;
			mTranslateVector.y = 0;

			if (hittedPlatform == null)
			{
				state.StandingPlatfom = null;
			}
			else
			{
				state.StandingPlatfom = hittedPlatform;
			}

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
				mTr.position = new Vector3 (mTr.position.x, lowestY, mTr.position.z);
			}
		}

		void CastRaysSide ()
		{
			float horizontalRayLength = _bound.wHalf + RaySafetyDis + Mathf.Abs (mTranslateVector.x);

			Vector2 horizontalRayCastToTop = new Vector2 (_bound.xCenter, _bound.yTop);
			Vector2 horizontalRayCastFromBottom = new Vector2 (_bound.xCenter, _bound.yBottom + _toleranceHeight);

			mHitCount = 0;

			//위에서 아래로 내려가면서 지정한 분할 수 만큼 검사
			for (int i = 0; i < RayHorizontalCount; i++)
			{
				_rayOriginPoint = Vector2.Lerp (horizontalRayCastToTop, horizontalRayCastFromBottom, (float)i / (float)(RayHorizontalCount - 1));

				//if (_state.WasColldingBelowLastFrame && i == RayHorizontalCount - 1)
				if (false)
					mHit2D = PhysicsUtil.DrawRayCast (_rayOriginPoint, mMoveDirection, horizontalRayLength, DruggedEngine.MASK_ALL_GROUND, Color.red);
				else
					mHit2D = PhysicsUtil.DrawRayCast (_rayOriginPoint, mMoveDirection, horizontalRayLength, DruggedEngine.MASK_EXCEPT_ONEWAY_GROUND, Color.red);

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
				mTranslateVector.x = mHit2D.point.x - _bound.xRight - RaySafetyDis;
			}
			else
			{
				state.IsCollidingLeft = true;
				mTranslateVector.x = mHit2D.point.x - _bound.xLeft + RaySafetyDis;
			}

			state.CollidingSide = mHit2D.collider;

			if (mHit2D.rigidbody != null) _sideHittedPushableObject.Add (mHit2D.rigidbody);
		}

		void CastRaysAbove ()
		{
			//낙하중일땐 무시
			if (mTranslateVector.y < 0) return;

			float rayLength = _bound.hHalf + RaySafetyDis + mTranslateVector.y;

			Vector2 verticalRayCastStart = new Vector2 (_bound.xLeft + mTranslateVector.x, _bound.yCenter);
			Vector2 verticalRayCastEnd = new Vector2 (_bound.xRight + mTranslateVector.x, _bound.yCenter);

			mHitCount = 0;

			for (int i = 0; i < VERTICAL_RAY_NUM; i++)
			{
				_rayOriginPoint = Vector2.Lerp (verticalRayCastStart, verticalRayCastEnd, (float)i / (float)(VERTICAL_RAY_NUM - 1));
				mHit2D = PhysicsUtil.DrawRayCast (_rayOriginPoint, Vector2.up, rayLength, DruggedEngine.MASK_EXCEPT_ONEWAY_GROUND, Color.blue);

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
				float ty = mHit2D.point.y - _bound.h - RaySafetyDis;
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
		public void UpdateBound ()
		{
			Bounds bounds = mCollider.bounds;
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;
			Vector3 center = bounds.center;
			Vector3 size = bounds.size;

			_bound = new ColliderInfo (
				min.x, center.x, max.x, size.x,
				min.y, center.y, max.y, size.y
			);

			//editor 인 경우 캐릭터의 Collider를 명확히 표시하자.
			if (Application.isEditor)
			{
				Color drawColor = Color.green;

				Debug.DrawLine (new Vector2 (_bound.xLeft, _bound.yBottom), new Vector2 (_bound.xRight, _bound.yBottom), drawColor);
				Debug.DrawLine (new Vector2 (_bound.xLeft, _bound.yCenter), new Vector2 (_bound.xRight, _bound.yCenter), drawColor);
				Debug.DrawLine (new Vector2 (_bound.xLeft, _bound.yTop), new Vector2 (_bound.xRight, _bound.yTop), drawColor);

				Debug.DrawLine (new Vector2 (_bound.xLeft, _bound.yBottom), new Vector2 (_bound.xLeft, _bound.yTop), drawColor);
				Debug.DrawLine (new Vector2 (_bound.xCenter, _bound.yBottom), new Vector2 (_bound.xCenter, _bound.yTop), drawColor);
				Debug.DrawLine (new Vector2 (_bound.xRight, _bound.yBottom), new Vector2 (_bound.xRight, _bound.yTop), drawColor);
			}
		}

		//----------------------------------------------------------------------------------------------------------
		// 충돌판정을 끄고 켠다.
		//----------------------------------------------------------------------------------------------------------

		public void PassThroughOneway ()
		{
			mTr.position = new Vector2 (mTr.position.x, mTr.position.y - 0.1f);
			state.ClearPlatform ();
			CollisionsOff (0.1f);
//			StartCoroutine (PassThroughOnewayRoutine());
		}

		IEnumerator PassThroughOnewayRoutine ()
		{
			Platform throughOneway = state.StandingPlatfom;
			Transform throughTr = throughOneway.transform;

			mTr.position = new Vector2 (mTr.position.x, mTr.position.y - 0.1f);
			state.ClearPlatform ();

			CollisionsOff ();

			while (throughTr.position.y < _bound.yCenter)
			{
				yield return null;
			}

			CollisionsOn ();
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
			_sideHittedPushableObject.Clear ();
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
		public Platform StandingPlatfom { get; set; }
		public Collider2D CollidingSide { get; set; }

		public bool IsOnOneway {
			get {
				if (IsGrounded == false) return false;
				else if (StandingPlatfom == null) return false;
				else return StandingPlatfom.oneway;
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
			StandingPlatfom = null;
		}

		public void Reset ()
		{
			IsCollidingAbove = IsCollidingBelow = IsCollidingLeft = IsCollidingRight = false;

			JustGotGrounded = false;
			SlopeAngle = 0;
			StandingPlatfom = null;
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

