using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace druggedcode.engine
{
	[RequireComponent(typeof(Collider2D),typeof(Rigidbody2D))]
	public class DEController : MonoBehaviour
	{
		//----------------------------------------------------------------------------------------------------------
		// helper 상수들
		//----------------------------------------------------------------------------------------------------------
		const float LARGE_VALUE = 500000f;
		const float SMALL_VALUE = 0.0001f;
		const int VERTICAL_RAY_NUM = 3;

		//----------------------------------------------------------------------------------------------------------
		// Inspector
		//----------------------------------------------------------------------------------------------------------


		[Header ("Collision")]
		public BoxCollider2D headChecker;

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
		public LayerMask currentMask;

		//----------------------------------------------------------------------------------------------------------
		// 상태
		//----------------------------------------------------------------------------------------------------------
		//state
		public DEControllerState state { get;private set; }

		//----------------------------------------------------------------------------------------------------------
		// 속도, 움직임. y가 마이너스인 경우가 낙하상태이다.
		//----------------------------------------------------------------------------------------------------------

		Vector2 _addForce;
		float _passedVX = 0f;
		Vector2 _velocity;
		//최종 적용된 속도
		Vector2 _moveDirection = Vector2.right;

		public Vector2 moveDirection { get { return _moveDirection; } }

		Vector2 _translateVector;
		//현재 velocity 에 의해 현재 프레임에서 움직일 벡터

		PhysicInfo _physicSpaceInfo;

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
		List<RaycastHit2D> mGroundHitted;
		List<float> mGroundAngles;

		// 충돌 체크 여부
		int _hitCount;
		//충돌 횟수
		ColliderInfo _bound;
		RaycastHit2D _hit2D;
		Vector2 _rayOriginPoint;
		List<Rigidbody2D> _sideHittedPushableObject;
		//좌우로 충돌된 플랫폼


		void Awake ()
		{
			GetComponent<Rigidbody2D>().isKinematic = true;

			if( headChecker != null )
			{
				mHeadCheckerPos = headChecker.transform.localPosition;
				mHeadCheckerExtents = headChecker.bounds.extents;
				headChecker.enabled = false;
			}


			mCollider = GetComponent<BoxCollider2D>();

			mTr = transform;

			mGroundHitted = new List<RaycastHit2D> (new RaycastHit2D[VERTICAL_RAY_NUM]);
			mGroundAngles = new List<float> (new float[VERTICAL_RAY_NUM]);

			_colliderDefaultSize = mCollider.size;
			_toleranceHeight = mCollider.bounds.size.y * ToleranceHeightRatio;

			_sideHittedPushableObject = new List<Rigidbody2D> ();
			state = new DEControllerState ();

			mCheckCollisions = true;

			ResetPhysicInfo ();
		}

		void Start ()
		{
			currentMask = DruggedEngine.MASK_ALL_GROUND;
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

		public Vector2 Velocity { get { return _velocity; } }

		public float vx { get { return _velocity.x; } set { _passedVX = value; } }

		public float vy { get { return _velocity.y; } set { _velocity.y = value; } }

		bool mMoveLocked;
		Coroutine mLockMoveRoutine;
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

		public void Stop ()
		{
			_passedVX = 0f;
			_velocity = Vector2.zero;
		}

		//----------------------------------------------------------------------------------------------------------
		// core logic. 충돌처리. Update 에서 설정한 값, 상황을  LateUpdate 에서 실제로 처리한다.
		//----------------------------------------------------------------------------------------------------------

		public float gravity {
			get{ return DruggedEngine.Gravity + _physicSpaceInfo.Gravity; }
		}

		void LateUpdate ()
		{
			if (mMoveLocked == false)
			{
				//x lock
			}


			//속도계산
			if (state.IsGrounded)
			{
				_velocity.x += (_passedVX - _velocity.x) * state.StandingPlatfom.friction;
			}
			else
			{
				_velocity.x = _passedVX;
			}

			_velocity += _addForce;
			//            _velocity.y += _physicInfo.Gravity * GravityScale;
			_velocity.y += gravity * GravityScale * Time.deltaTime;
			_velocity.x *= _physicSpaceInfo.MoveFactor;

			//            _speed.y += (Parameters.Gravity + _movingPlatformsCurrentGravity) * Time.deltaTime;

			_addForce = Vector2.zero;

			//속도기반 실제 프레임당 이동 벡터 계산
			_translateVector = _velocity * Time.deltaTime;


			//진행 방향 체크, 지난 프레임 상태 저장 & 리셋
			state.SaveLastStateAndReset ();
			CheckMoveDirection ();
			UpdateBound ();

			//충돌검사. 충돌 상황에 따라 _translateVector 가 변경 될 수 있다.
			if (mCheckCollisions)
			{
				CastRaysBelow ();
				CastRaysToTheSides ();
				CastRaysAbove ();

				_velocity = _translateVector / Time.deltaTime; //충돌로 인해 변경된 벡터를 바탕으로 속도 재설정.

				if (state.HasCollisions) PushHittedObject (); //밀수 있는 것들은 민다.

				//지상에 막 닿은건지 아닌지를 판단한다.
				if (state.WasColldingBelowLastFrame == false && state.IsGrounded) state.JustGotGrounded = true;
			}

			if (_translateVector.y < -SMALL_VALUE)
				state.IsFalling = true;
			else
				state.IsFalling = false;

			//캐릭터 이동
			mTr.Translate (_translateVector, Space.World);
		}

		void CheckMoveDirection ()
		{
			if (_moveDirection.x == 1 && _velocity.x < 0)
			{
				_moveDirection = new Vector2 (-1, 0);
			}
			else if (_moveDirection.x == -1 && _velocity.x > 0)
			{
				_moveDirection = new Vector2 (1, 0);
			}
		}

		void CastRaysBelow ()
		{
			//상승중일땐 무시
			if (_translateVector.y > 0) return;
			//  float rayLength = _bound.hHalf + Mathf.Abs(_translateVector.y);
			//  if( _state.IsGrounded ) rayLength += RayGroundOffset;
			float rayLength = _bound.hHalf + RayGroundOffset + Mathf.Abs (_translateVector.y);

			int rayIndex, increase;
			if (_moveDirection.x == 1)
			{
				rayIndex = VERTICAL_RAY_NUM - 1;
				increase = -1;
			}
			else
			{
				rayIndex = 0;
				increase = 1;
			}

			Vector2 verticalRayCastFromLeft = new Vector2 (_bound.xLeft + _translateVector.x, _bound.yCenter);
			Vector2 verticalRayCastToRight = new Vector2 (_bound.xRight + _translateVector.x, _bound.yCenter);

			_hitCount = 0;

			float lowestY = -LARGE_VALUE;
			float sumY = 0f;
			float fowardY = 0;
			int closestIndex = 0;
			float hitY, angle;

			for (int i = 0; i < VERTICAL_RAY_NUM; i++)
			{
				_rayOriginPoint = Vector2.Lerp (verticalRayCastFromLeft, verticalRayCastToRight, (float)rayIndex / (float)(VERTICAL_RAY_NUM - 1));
				_hit2D = PhysicsUtil.DrawRayCast (_rayOriginPoint, -Vector2.up, rayLength, currentMask, Color.red);

				if (_hit2D)
				{
					hitY = _hit2D.point.y;
					if (i == 0) fowardY = hitY;
					angle = Vector2.Angle (_hit2D.normal, Vector2.up);

					mGroundHitted [i] = _hit2D;
					mGroundAngles [i] = angle;

					//HIT 된 Y 가 넘어갈 수 있는 높이 높고 허용 각도보다 가파르면  막혔다가 판단한다.
					//원웨이가 아니면 이라는 조건이 삭제되었다. 문제되면 복구할것
					//_hit2D.collider.gameObject.GetComponent<Platform> () is Platform == false
					if (hitY > _bound.yBottom + _toleranceHeight &&
					    Vector2.Angle (_hit2D.point - _bound.bottom, _moveDirection) > MaximumSlopeAngle )
					{
						_translateVector.x = 0;
					}
					//아닌 경우 자동으로 올라 설 수 있는 바닥이라고 판단
					else
					{
						++_hitCount;
						sumY += hitY;
						if (hitY > lowestY)
						{
							closestIndex = i;
							lowestY = hitY;
						}
					}
				}

				rayIndex += increase;
			}

			//충돌된 것이 없다.
			if (_hitCount == 0) return;
			if (lowestY < _bound.yBottom + _translateVector.y) return;

			_hit2D = mGroundHitted [closestIndex];

			Platform hittedPlatform = _hit2D.collider.gameObject.GetComponent<Platform> ();

			if (state.WasColldingBelowLastFrame == false && lowestY > _bound.yBottom && hittedPlatform.oneway )
			{
				return;
			}

			state.IsCollidingBelow = true;
			state.StandingPlatfom = hittedPlatform;
			state.SlopeAngle = mGroundAngles [closestIndex];
			_translateVector.y = 0;

			if (state.SlopeAngle > 0)
			{
				if (fowardY > mTr.position.y && state.SlopeAngle > MaximumSlopeAngle)
				{
					_translateVector.x = 0;
				}
				mTr.position = new Vector3 (mTr.position.x, sumY / _hitCount, mTr.position.z);
			}
			else
			{
				mTr.position = new Vector3 (mTr.position.x, lowestY, mTr.position.z);
			}
		}

		void CastRaysToTheSides ()
		{
			float horizontalRayLength = _bound.wHalf + RaySafetyDis + Mathf.Abs (_translateVector.x);

			Vector2 horizontalRayCastFromBottom = new Vector2 (_bound.xCenter, _bound.yBottom + _toleranceHeight);
			Vector2 horizontalRayCastToTop = new Vector2 (_bound.xCenter, _bound.yTop);

			_hitCount = 0;

			//위에서 아래로 내려가면서 지정한 분할 수 만큼 검사
			for (int i = 0; i < RayHorizontalCount; i++)
			{
				_rayOriginPoint = Vector2.Lerp (horizontalRayCastToTop, horizontalRayCastFromBottom, (float)i / (float)(RayHorizontalCount - 1));

				_hit2D = PhysicsUtil.DrawRayCast (_rayOriginPoint, _moveDirection, horizontalRayLength, currentMask, Color.red);

				if (_hit2D)
				{
					if (i == RayHorizontalCount - 1 && Vector2.Angle (_hit2D.normal, Vector2.up) < MaximumSlopeAngle)
					{
						//가장 아래의 레이가 막혔지만 허용 가능한 경사이므로 막혔다고 체크하지 않는다.
					}
					else
					{
						++_hitCount;
						break;
					}
				}
			}

			if (_hitCount == 0) return;

			if (_moveDirection.x == 1)
			{
				state.IsCollidingRight = true;
				_translateVector.x = _hit2D.point.x - _bound.xRight - RaySafetyDis;
			}
			else
			{
				state.IsCollidingLeft = true;
				_translateVector.x = _hit2D.point.x - _bound.xLeft + RaySafetyDis;
			}

			if (state.IsGrounded == false)
			{
				Wall wall = _hit2D.collider.gameObject.GetComponent<TempPlatform> ();
				if (CheckWallCling (wall)) state.HittedClingWall = Wall;
			}

			if (_hit2D.rigidbody != null) _sideHittedPushableObject.Add (_hit2D.rigidbody);
		}

		bool CheckWallCling (TempPlatform wall)
		{
			if (wall == null) return false;
			if (wall.WallClingType == TempPlatform.WallClinType.NOTHING) return false;
			if (wall.WallClingType == TempPlatform.WallClinType.BOTH) return true;
			if (wall.WallClingType == TempPlatform.WallClinType.LEFT && state.IsCollidingRight) return true;
			if (wall.WallClingType == TempPlatform.WallClinType.RIGHT && state.IsCollidingLeft) return true;
			return false;
		}

		void CastRaysAbove ()
		{
			//낙하중일땐 무시
			if (_translateVector.y < 0) return;

			float rayLength = _bound.hHalf + RaySafetyDis + _translateVector.y;

			Vector2 verticalRayCastStart = new Vector2 (_bound.xLeft + _translateVector.x, _bound.yCenter);
			Vector2 verticalRayCastEnd = new Vector2 (_bound.xRight + _translateVector.x, _bound.yCenter);

			_hitCount = 0;

			for (int i = 0; i < VERTICAL_RAY_NUM; i++)
			{
				_rayOriginPoint = Vector2.Lerp (verticalRayCastStart, verticalRayCastEnd, (float)i / (float)(VERTICAL_RAY_NUM - 1));
				_hit2D = PhysicsUtil.DrawRayCast (_rayOriginPoint, Vector2.up, rayLength, DruggedEngine.MASK_ENVIRONMENT, Color.blue);

				if (_hit2D)
				{
					++_hitCount;
					break;
				}

			}

			if (_hitCount == 0) return;

			state.IsCollidingAbove = true;

			if (state.IsCollidingBelow == false)
			{
				float ty = _hit2D.point.y - _bound.h - RaySafetyDis;
				mTr.position = new Vector3 (mTr.position.x, ty, mTr.position.z);
				_translateVector.y = 0;
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

		public void CollisionsOn()
		{
			mCheckCollisions = true;
		}

		public void CollisionsOff( float duration = 0f )
		{
			mCheckCollisions = false;
		}

		public void DisableCollisions (float duration)
		{
			StartCoroutine (DisableCollisionRoutine (duration));
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

		public bool IsCollidingHead
		{
			get
			{
				if ( mHeadCheckerExtents != Vector3.zero)
				{

					Vector2 pointA = mTr.position + mHeadCheckerPos - mHeadCheckerExtents;
					Vector2 pointB = mTr.position + mHeadCheckerPos + mHeadCheckerExtents;
					return Physics2D.OverlapArea( pointA, pointB, DruggedEngine.MASK_EXCEPT_ONEWAY_GROUND);
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

		public bool HasCollisions { get { return IsCollidingRight || IsCollidingLeft || IsCollidingAbove || IsCollidingBelow; } }

		//wall 충돌여부
		public bool IsCollidingWallRight { get { return IsCollidingRight; } }

		public bool IsCollidingWallLeft { get { return IsCollidingLeft; } }

		public bool IsGrounded { get { return IsCollidingBelow; } }

		//IsCollidingBelow 와 같이 변해야 한다.
		public Platform StandingPlatfom { get; set; }
		public Wall HittedClingWall { get; set; }

		//지난프레임상태
		public bool WasColldingBelowLastFrame { get; set; }

		public bool WasColldingAdoveLastFrame { get; set; }

		//막 지상에 닿은 프레임에서 true
		public bool JustGotGrounded { get; set; }

		public bool IsFalling { get; set; }
		// 움직이고 있는 경사면의 각도
		public float SlopeAngle { get; set; }

		public DEControllerState ()
		{
			Reset ();
		}

		public void Reset ()
		{
			IsCollidingAbove = IsCollidingBelow = IsCollidingLeft = IsCollidingRight = false;

			IsFalling = false;           
			JustGotGrounded = false;
			SlopeAngle = 0;
			StandingPlatfom = null;
			HittedClingWall = null;
		}

		public void SaveLastStateAndReset ()
		{
			WasColldingBelowLastFrame = IsCollidingBelow;
			WasColldingAdoveLastFrame = IsCollidingAbove;

			Reset ();
		}
	}
}

