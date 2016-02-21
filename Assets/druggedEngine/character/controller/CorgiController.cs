using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace druggedcode.engine
{
	[RequireComponent (typeof(BoxCollider2D))]
	public class CorgiController : MonoBehaviour
	{

		const float LARGE_VALUE = 500000f;
		const float SMALL_VALUE = 0.0001f;
		const float MOVINGPLATFORM_GRAVITY = -150f;

		//----------------------------------------------------------------------------------------------------------
		// Inspector
		//----------------------------------------------------------------------------------------------------------
		[Header ("Collision")]
		public BoxCollider2D headChecker;
		public Vector2 MaxVelocity = new Vector2 (200f, 200f);
		[Range (0, 90)]
		public float MaximumSlopeAngle = 45;
		[Range (0, 1)]
		public float ToleranceHeightRatio = 0.2f;
		public float Physics2DPushForce = 2.0f;
		public float SpeedAccelerationOnGround = 20f;
		public float SpeedAccelerationInAir = 5f;

		[Header ("Raycasting")]
		[Range (3, 8)]
		public int NumberOfHorizontalRays = 4;
		[Range (3, 8)]
		public int NumberOfVerticalRays = 4;
		public float RaySafetyDis = 0.1f;
		public float RayGroundOffset = 0.5f;

		//----------------------------------------------------------------------------------------------------------
		//get;set;
		//----------------------------------------------------------------------------------------------------------
		public CorgiControllerState State { get; set; }
		public Vector2 Speed { get { return mVelocity; } }
		public float Gravity { get { return DruggedEngine.Gravity + mPhysicSpaceInfo.Gravity; } }

		//----------------------------------------------------------------------------------------------------------
		// property
		//----------------------------------------------------------------------------------------------------------
		Transform mTr;
		Vector3 mHeadCheckerPos;
		Vector3 mHeadCheckerExtents = Vector3.zero;
		Vector2 mColliderDefaultSize;

		Vector2 mMoveDirection = Vector2.right;
		Vector2 mVelocity;
		Vector2 mTranslateVector;
		PhysicInfo mPhysicSpaceInfo;

		float _fallSlowFactor;
		Vector2 _externalForce;

		BoxCollider2D mCollider;
		GameObject _lastStandingOn;
		float _movingPlatformsCurrentGravity;


		float mToleranceHeight = 0.05f;

		ColliderInfo mColliderBound;
		LayerMask mDefaultPlatformMask;

		List<Rigidbody2D> mSideHittedRigidbodies;

		bool mCheckCollisions;

		void Awake ()
		{
			GetComponent<Rigidbody2D> ().isKinematic = true;

			mTr = transform;

			mCollider = GetComponent<BoxCollider2D> ();
			mToleranceHeight = mCollider.bounds.size.y * ToleranceHeightRatio;
			mColliderDefaultSize = mCollider.size;

			mSideHittedRigidbodies = new List<Rigidbody2D> ();

			if (headChecker != null)
			{
				mHeadCheckerPos = headChecker.transform.localPosition;
				mHeadCheckerExtents = headChecker.bounds.extents;
				headChecker.enabled = false;
			}

			State = new CorgiControllerState ();
			UpdateColliderBound ();
		}

		void Start ()
		{
			mDefaultPlatformMask = DruggedEngine.MASK_ALL_GROUND;
			CollisionsOn();
		}

		public void AddForce (Vector2 force)
		{
			mVelocity += force;	
			_externalForce += force;
		}

		public void AddHorizontalForce (float x)
		{
			mVelocity.x += x;
			_externalForce.x += x;
		}

		public void AddVerticalForce (float y)
		{
			mVelocity.y += y;
			_externalForce.y += y;
		}

		public void SetForce (Vector2 force)
		{
			mVelocity = force;
			_externalForce = force;	
		}

		public void SetHorizontalForce (float x)
		{
			mVelocity.x = x;
			_externalForce.x = x;
		}

		public void SetVerticalForce (float y)
		{
			mVelocity.y = y;
			_externalForce.y = y;

		}

		void FixedUpdate ()
		{	
			mSideHittedRigidbodies.Clear ();

			CalculateTranslateVector ();

			UpdateColliderBound ();

			State.SaveLastStateAndReset ();

			CheckMoveDirection ();

			CheckCollisions();

			mTr.Translate (mTranslateVector, Space.World);

			mVelocity.x = Mathf.Clamp (mVelocity.x, -MaxVelocity.x, MaxVelocity.x);
			mVelocity.y = Mathf.Clamp (mVelocity.y, -MaxVelocity.y, MaxVelocity.y);

			_externalForce.x = 0;
			_externalForce.y = 0;
		}

		void CalculateTranslateVector ()
		{
			mVelocity.y += (Gravity + _movingPlatformsCurrentGravity) * Time.deltaTime;

			if ( mVelocity.y < 0f )
			{
				if (_fallSlowFactor != 0) mVelocity.y *= _fallSlowFactor;
			}

			mTranslateVector = mVelocity * Time.deltaTime;
		}

		void CheckMoveDirection ()
		{
			if (mMoveDirection.x == 1 && mVelocity.x < 0)
			{
				mMoveDirection = new Vector2 (-1, 0);
			}
			else if (mMoveDirection.x == -1 && mVelocity.x > 0)
			{
				mMoveDirection = new Vector2 (1, 0);
			}
		}

		void CheckCollisions ()
		{
			if (mCheckCollisions == false) return;

			CastRaysToTheSides ();
			CastRaysBelow ();	
			CastRaysAbove ();

			if (Time.deltaTime > 0) mVelocity = mTranslateVector / Time.deltaTime;
			if (State.HasCollisions) PushHittedObject (); //밀수 있는 것들은 민다.

			if (State.WasCollidingBelowLastFrame == false && State.IsCollidingBelow) State.JustGotGrounded = true;

		}

		void CastRaysToTheSides ()
		{			
			float rayLength = mColliderBound.wHalf + RaySafetyDis + Mathf.Abs (mTranslateVector.x);

			Vector2 horizontalRayCastToTop = new Vector2 (mColliderBound.xCenter, mColliderBound.yTop);
			Vector2 horizontalRayCastFromBottom = new Vector2 (mColliderBound.xCenter, mColliderBound.yBottom + mToleranceHeight);

			RaycastHit2D[] hitsStorage = new RaycastHit2D[NumberOfHorizontalRays];	

			for (int i = 0; i < NumberOfHorizontalRays; i++)
			{	
				Vector2 rayOriginPoint = Vector2.Lerp (horizontalRayCastFromBottom, horizontalRayCastToTop, (float)i / (float)(NumberOfHorizontalRays - 1));

				if (State.WasCollidingBelowLastFrame && i == 0)
					hitsStorage [i] = PhysicsUtil.DrawRayCast (rayOriginPoint, mMoveDirection, rayLength, mDefaultPlatformMask, Color.red);
				else
					hitsStorage [i] = PhysicsUtil.DrawRayCast (rayOriginPoint, mMoveDirection, rayLength, DruggedEngine.MASK_EXCEPT_ONEWAY_GROUND, Color.red);			

				if (hitsStorage [i].distance > 0)
				{						
					float hitAngle = Mathf.Abs (Vector2.Angle (hitsStorage [i].normal, Vector2.up));		

					State.SlopeAngle = hitAngle;					

					if (hitAngle > MaximumSlopeAngle)
					{														
						if (mMoveDirection.x < 0f)
							State.IsCollidingLeft = true;
						else
							State.IsCollidingRight = true;						


						if (mMoveDirection.x <= 0f)
						{
							mTranslateVector.x = -Mathf.Abs (hitsStorage [i].point.x - horizontalRayCastFromBottom.x)
							+ mColliderBound.wHalf
							+ RaySafetyDis;
						}
						else
						{						
							mTranslateVector.x = Mathf.Abs (hitsStorage [i].point.x - horizontalRayCastFromBottom.x)
							- mColliderBound.wHalf
							- RaySafetyDis;						
						}					

						mSideHittedRigidbodies.Add (hitsStorage [i].rigidbody);
						mVelocity = new Vector2 (0, mVelocity.y);
						break;
					}
				}						
			}


		}

		void CastRaysBelow ()
		{
			if (Gravity > 0f && mTranslateVector.y > 0f ) return;

			float rayLength = mColliderBound.hHalf + RayGroundOffset;	
			if (mTranslateVector.y < 0f) rayLength += Mathf.Abs (mTranslateVector.y);

			Vector2 verticalRayCastFromLeft = new Vector2 (mColliderBound.xLeft + mTranslateVector.x, mColliderBound.yCenter);
			Vector2 verticalRayCastToRight = new Vector2 (mColliderBound.xRight + mTranslateVector.x, mColliderBound.yCenter);

			RaycastHit2D[] hitsStorage = new RaycastHit2D[NumberOfVerticalRays];
			float smallestDistance = LARGE_VALUE; 
			int smallestDistanceIndex = 0; 						
			bool hitConnected = false; 		

			for (int i = 0; i < NumberOfVerticalRays; i++)
			{			
				Vector2 rayOriginPoint = Vector2.Lerp (verticalRayCastFromLeft, verticalRayCastToRight, (float)i / (float)(NumberOfVerticalRays - 1));

				if ((mTranslateVector.y > 0) && (!State.WasCollidingBelowLastFrame))
					hitsStorage [i] = PhysicsUtil.DrawRayCast (rayOriginPoint, -Vector2.up, rayLength, DruggedEngine.MASK_EXCEPT_ONEWAY_GROUND, Color.blue);
				else
					hitsStorage [i] = PhysicsUtil.DrawRayCast (rayOriginPoint, -Vector2.up, rayLength, mDefaultPlatformMask, Color.blue);					

				if ((Mathf.Abs (hitsStorage [smallestDistanceIndex].point.y - verticalRayCastFromLeft.y)) < SMALL_VALUE)
				{
					break;
				}		

				if (hitsStorage [i])
				{
					hitConnected = true;
					if (hitsStorage [i].distance < smallestDistance)
					{
						smallestDistanceIndex = i;
						smallestDistance = hitsStorage [i].distance;
					}
				}								
			}
			if (hitConnected)
			{

				State.StandingOn = hitsStorage [smallestDistanceIndex].collider.gameObject;

				// if the character is jumping onto a (1-way) platform but not high enough, we do nothing
				if (
					State.WasCollidingBelowLastFrame == false
					&& (smallestDistance < mColliderBound.hHalf )
					&& false
					&& (
						State.StandingOn.layer == LayerMask.NameToLayer ("OneWayPlatforms") ||
						State.StandingOn.layer == LayerMask.NameToLayer ("MovingOneWayPlatforms") 
					))
				{
					State.IsCollidingBelow = false;
					return;
				}

				State.IsCollidingBelow = true;


				// if we're applying an external force (jumping, jetpack...) we only apply that
				if (_externalForce.y > 0)
				{
					mTranslateVector.y = mVelocity.y * Time.deltaTime;
					State.IsCollidingBelow = false;
				}
				// if not, we just adjust the position based on the raycast hit
				else
				{
					mTranslateVector.y = -Mathf.Abs (hitsStorage [smallestDistanceIndex].point.y - verticalRayCastFromLeft.y)
					+ mColliderBound.hHalf / 2
						+ RaySafetyDis;
				}

				if (!State.WasCollidingBelowLastFrame && mVelocity.y > 0)
				{
					mTranslateVector.y += mVelocity.y * Time.deltaTime;
				}

				if (Mathf.Abs (mTranslateVector.y) < SMALL_VALUE)
					mTranslateVector.y = 0;

				// we check if the character is standing on a moving platform
				Platform movingPlatform = hitsStorage [smallestDistanceIndex].collider.GetComponent<Platform> ();
				if (movingPlatform != null)
				{
					_movingPlatformsCurrentGravity = MOVINGPLATFORM_GRAVITY;
					mTr.Translate (movingPlatform.velocity * Time.deltaTime);
					mTranslateVector.y = 0;					
				}
				else
				{
					_movingPlatformsCurrentGravity = 0;
				}
			}
			else
			{
				_movingPlatformsCurrentGravity = 0;
				State.IsCollidingBelow = false;
			}	


		}

		void CastRaysAbove ()
		{
			if (mTranslateVector.y < 0f) return;

			float rayLength = State.IsGrounded ? RaySafetyDis : mTranslateVector.y * Time.deltaTime;
			rayLength += mColliderBound.hHalf;

			bool hitConnected = false; 

			Vector2 verticalRayCastStart = new Vector2 (mColliderBound.xLeft + mTranslateVector.x, mColliderBound.yCenter);
			Vector2 verticalRayCastEnd = new Vector2 (mColliderBound.xRight + mTranslateVector.x, mColliderBound.yCenter);

			RaycastHit2D[] hitsStorage = new RaycastHit2D[NumberOfVerticalRays];
			float smallestDistance = LARGE_VALUE; 

			for (int i = 0; i < NumberOfVerticalRays; i++)
			{							
				Vector2 rayOriginPoint = Vector2.Lerp (verticalRayCastStart, verticalRayCastEnd, (float)i / (float)(NumberOfVerticalRays - 1));
				hitsStorage [i] = PhysicsUtil.DrawRayCast (rayOriginPoint, Vector2.up, rayLength, DruggedEngine.MASK_EXCEPT_ONEWAY_GROUND, Color.green);	

				if (hitsStorage [i])
				{
					hitConnected = true;
					if (hitsStorage [i].distance < smallestDistance)
					{
						smallestDistance = hitsStorage [i].distance;
					}
				}	

			}	

			if (hitConnected)
			{
				mVelocity.y = 0;
				mTranslateVector.y = smallestDistance - mColliderBound.hHalf;

				if ((State.IsGrounded) && (mTranslateVector.y < 0))
				{
					mTranslateVector.y = 0;
				}

				State.IsCollidingAbove = true;

				if (!State.WasCollidingAdoveLastFrame)
				{
					mTranslateVector.x = 0;
					mVelocity = new Vector2 (0, mVelocity.y);
				}
			}	
		}

		public virtual void UpdateColliderBound ()
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

			#if UNITY_EDITOR
			Color drawColor = Color.green;

			Debug.DrawLine (new Vector2 (mColliderBound.xLeft, mColliderBound.yBottom), new Vector2 (mColliderBound.xRight, mColliderBound.yBottom), drawColor);
			Debug.DrawLine (new Vector2 (mColliderBound.xLeft, mColliderBound.yCenter), new Vector2 (mColliderBound.xRight, mColliderBound.yCenter), drawColor);
			Debug.DrawLine (new Vector2 (mColliderBound.xLeft, mColliderBound.yTop), new Vector2 (mColliderBound.xRight, mColliderBound.yTop), drawColor);

			Debug.DrawLine (new Vector2 (mColliderBound.xLeft, mColliderBound.yBottom), new Vector2 (mColliderBound.xLeft, mColliderBound.yTop), drawColor);
			Debug.DrawLine (new Vector2 (mColliderBound.xCenter, mColliderBound.yBottom), new Vector2 (mColliderBound.xCenter, mColliderBound.yTop), drawColor);
			Debug.DrawLine (new Vector2 (mColliderBound.xRight, mColliderBound.yBottom), new Vector2 (mColliderBound.xRight, mColliderBound.yTop), drawColor);
			#endif
		}

		public virtual IEnumerator DisableCollisionsWithOneWayPlatforms (float duration)
		{
			mDefaultPlatformMask = DruggedEngine.MASK_EXCEPT_ONEWAY_GROUND;
			yield return new WaitForSeconds (duration);
			mDefaultPlatformMask = DruggedEngine.MASK_ALL_GROUND;
		}

		public void ResetMovingPlatformsGravity ()
		{
			_movingPlatformsCurrentGravity = 0f;
		}

		public void CollisionsOn ()
		{
			mCheckCollisions = true;
		}

		public void CollisionsOff ()
		{
			mCheckCollisions = false;
		}


		public void SetPhysicsSpace (PhysicInfo physicInfo)
		{
			mPhysicSpaceInfo = physicInfo;
		}

		public void ResetPhysicInfo ()
		{
			mPhysicSpaceInfo = new PhysicInfo (0, 1);
		}

		public void SlowFall (float factor)
		{
			_fallSlowFactor = factor;
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
					return State.IsCollidingAbove;
				}
			}
		}

		// Events


		void PushHittedObject ()
		{
			foreach (Rigidbody2D body in mSideHittedRigidbodies)
			{			
				if (body.isKinematic) continue;

				Vector3 pushDir = new Vector3 (_externalForce.x, 0, 0);
				body.velocity = pushDir.normalized * Physics2DPushForce;		
			}		
		}

		void OnTriggerEnter2D (Collider2D collider)
		{

//			CorgiControllerPhysicsVolume2D parameters = collider.gameObject.GetComponent<CorgiControllerPhysicsVolume2D>();
//			if (parameters == null)
//				return;
//			// if the object we're colliding with has parameters, we apply them to our character.
//			_overrideParameters = parameters.ControllerParameters;
		}

		void OnTriggerStay2D (Collider2D collider)
		{
		}

		void OnTriggerExit2D (Collider2D collider)
		{		
//			CorgiControllerPhysicsVolume2D parameters = collider.gameObject.GetComponent<CorgiControllerPhysicsVolume2D>();
//			if (parameters == null)
//				return;
//
//			// if the object we were colliding with had parameters, we reset our character's parameters
//			_overrideParameters = null;
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

	public class CorgiControllerState
	{
		public bool IsCollidingRight { get; set; }

		public bool IsCollidingLeft { get; set; }

		public bool IsCollidingAbove { get; set; }

		public bool IsCollidingBelow { get; set; }

		public bool HasCollisions { get { return IsCollidingRight || IsCollidingLeft || IsCollidingAbove || IsCollidingBelow; } }

		public bool WasCollidingBelowLastFrame { get ; set; }

		public bool WasCollidingAdoveLastFrame { get ; set; }

		public bool JustGotGrounded { get ; set; }

		public bool IsGrounded { get { return IsCollidingBelow; } }

		public float SlopeAngle { get; set; }

		public GameObject StandingOn { get; set; }

		public Collider2D CollidingSide { get; set; }

		public CorgiControllerState ()
		{
			Reset ();
		}

		public virtual void Reset ()
		{
			IsCollidingLeft = 
				IsCollidingRight = 
					IsCollidingAbove =
						JustGotGrounded = false;
			SlopeAngle = 0;
		}

		public void ClearPlatform ()
		{
			IsCollidingBelow = false;
			StandingOn = null;
		}

		public void SaveLastStateAndReset ()
		{
			WasCollidingBelowLastFrame = IsCollidingBelow;
			WasCollidingAdoveLastFrame = IsCollidingAbove;

			Reset ();
		}

		public override string ToString ()
		{
			return string.Format ("(controller: r:{0} l:{1} a:{2} b:{3} down-slope:{4} up-slope:{5} angle: {6}",
				IsCollidingRight,
				IsCollidingLeft,
				IsCollidingAbove,
				IsCollidingBelow,
				SlopeAngle);
		}
	}
}