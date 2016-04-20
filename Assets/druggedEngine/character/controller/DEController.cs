using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace druggedcode.engine
{
	public class DEController : MonoBehaviour
	{
		const float CAST_FRONT_LENGTH = 0.1f;
		const float CAST_GROUND_LENGTH = 0.1f;
		const float CAST_GROUND_START_Y_OFFSET = 0.05f;
		//CAST_GROUND_LENGTH - CAST_GROUND_START_Y_OFFSET 가 실제 position 에서 아래로 쏜 길이가 된다.
		#region inspector
		[Header ("References")]
		public Collider2D platformCollider;

		[Header("Move")]
		public float accelOnGround = 30f;
		public float accelOnAir = 10f;

		[Header("Jump")]
		public float jumpHeight = 3f;
		public float jumpHeightOnAir = 2f;

		[Header ("Physics")]
		public float ownGravity = -8;
		public float defaultFriction = 0.4f;
		public float defaultBounce = 0;

		[Header ("Raycasting")]
		public LayerMask characterMask;

		//[HideInInspector]
		public LayerMask currentMask;
		#endregion

		#region getter setter
		public NewControllerState State { get; private set; }
		public Vector2 Axis {get;set;}

		public int Facing{get;set;}
		public float Friction{ get{ return mPhysicMaterial.friction; }}
		#endregion

		public UnityAction OnJustGotGrounded;

		Rigidbody2D mRb;
		Transform mTr;
		PhysicsMaterial2D mPhysicMaterial;
		protected Vector2 mPassedVelocity;

		Vector3 mCastOriginCenter;
		Vector3 mCastOriginBack;
		Vector3 mCastOriginForward;
		Vector3 mCastOriginFront;
		float mWallCastDistance;

		bool doPassthrough;
		Platform passThroughPlatform;
		//oneway
		Platform movingPlatform;
		//moving


		bool airControllLock;
		float mLockedVY;
		public float TargetVX{get;set;}

		#region Initialize
		virtual protected void Awake ()
		{
			mRb = GetComponent<Rigidbody2D> ();
			mRb.angularDrag = 0f;
			mRb.drag = 0f;
			mRb.constraints = RigidbodyConstraints2D.FreezeRotation;

			mTr = transform;

			State = new NewControllerState ();

			CalculateRayBounds (platformCollider);

			if (platformCollider.sharedMaterial == null)
			{
				mPhysicMaterial = new PhysicsMaterial2D ("ControllerColliderMaterial");
				SetFriction( defaultFriction );
				SetBounce( defaultBounce );
			}
			else
			{
				mPhysicMaterial = Instantiate (platformCollider.sharedMaterial);
			}

			platformCollider.sharedMaterial = mPhysicMaterial;
		}

		virtual protected void Start ()
		{
			currentMask = DruggedEngine.MASK_MIXED_ALLGROUND;
		}

		void CalculateRayBounds (Collider2D coll)
		{
//			PolygonCollider2D
			if( coll == null )
			{
				print( "name: " + name );
			}

			Bounds b = coll.bounds;
			Vector3 min = mTr.InverseTransformPoint (b.min);
			Vector3 center = mTr.InverseTransformPoint (b.center);
			Vector3 max = mTr.InverseTransformPoint (b.max);

			mCastOriginBack.x = min.x;
			mCastOriginBack.y = min.y + CAST_GROUND_START_Y_OFFSET;

			mCastOriginCenter.x = center.x;
			mCastOriginCenter.y = min.y + CAST_GROUND_START_Y_OFFSET;

			mCastOriginForward.x = max.x;
			mCastOriginForward.y = min.y + CAST_GROUND_START_Y_OFFSET;

			mCastOriginFront = center;
			mWallCastDistance = b.extents.x + CAST_FRONT_LENGTH;
		}

		#endregion

		#region controll Velocity
		public Vector2 Velocity
		{
			get{ return mRb.velocity; }
			set{ mRb.velocity = value; }
		}

		public float vx
		{
			get{ return mRb.velocity.x; }
			set
			{
				TargetVX = value;
				mRb.velocity = new Vector2( value, mRb.velocity.y);
			}
		}

		public float vy
		{
			get{ return mRb.velocity.y; }
			set
			{
				mRb.velocity = new Vector2( mRb.velocity.x, value);
			}
		}

		public void Stop()
		{
			mRb.velocity = Vector2.zero;
			TargetVX = 0f;
		}

		public void AddForce (Vector2 force)
		{
			mRb.velocity = mRb.velocity + force;
		}

		public void AddForceX( float x )
		{
			mRb.velocity = new Vector2( mRb.velocity.x + x, mRb.velocity.y );
		}

		public void AddForceY( float y )
		{
			mRb.velocity = new Vector2( mRb.velocity.x, mRb.velocity.y + y );
		}

		public void LockVY (float lockvy)
		{
			mLockedVY = lockvy;
		}

		public void UnLockVY ()
		{
			mLockedVY = 0f;
		}

		public void Jump()
		{
			float jumpPower = State.IsGrounded ? jumpHeight : jumpHeightOnAir;
			jumpPower = Mathf.Sqrt (2.05f * jumpPower * Mathf.Abs (DruggedEngine.Gravity + ownGravity ));
			jumpPower += State.PlatformVelocity.y;
			vy = jumpPower;
			if( State.IsGrounded ) State.ClearPlatform();
		}
		#endregion

		void FixedUpdate ()
		{
			CheckCollisions ();
			Move();
		}

		#region Move
		void Move()
		{
			Vector2 currentVelocity = mRb.velocity;

			MoveX( ref currentVelocity );
			MoveY( ref currentVelocity );

			mRb.velocity = currentVelocity;
		}

		void MoveX (ref Vector2 velocity)
		{
			float absAxisX = Mathf.Abs(Axis.x);
			float currentX = velocity.x;
			if( State.IsGrounded )
			{
				TargetVX += State.PlatformVelocity.x;
				if( absAxisX > 0.1f )
				{
					currentX = Mathf.MoveTowards( currentX, TargetVX, Time.deltaTime * accelOnGround);
					//if slide > nextVelocity.x = savedXVelocity + platformXVelocity;
					//if attack >  nextVelocity.x = Mathf.MoveTowards(nextVelocity.x, platformXVelocity, Time.deltaTime * 8);
				}
				else
				{
					currentX = Mathf.MoveTowards(currentX, TargetVX, Time.deltaTime * accelOnGround);
				}

//				currentX += State.PlatformVelocity.x;
			}
			else
			{
				if( airControllLock )
				{
					//currentX = Mathf.MoveTowards(currentX, mTargetSpeed, Time.deltaTime * 8);
				}
				else
				{
					currentX = Mathf.MoveTowards( currentX, TargetVX, Time.deltaTime * accelOnAir );
				}

			}

			velocity.x = currentX;
		}

		void MoveY(ref Vector2 velocity)
		{
			float currentY = velocity.y;

			if( State.IsGrounded )
			{
				currentY += State.PlatformVelocity.y;
			}
			else
			{
				if( mRb.gravityScale == 1f )
				{
					currentY += ownGravity * Time.deltaTime;
				}

				if (mLockedVY != 0f) currentY = Mathf.Clamp( currentY, mLockedVY, 0f );
			}

			velocity.y = currentY;
		}

		public void SetPos( float x, float y )
		{
			mTr.position = new Vector3( x, y, mTr.position.z );
		}

		public void SetPosX( float x )
		{
			SetPos(x, mTr.position.y );
		}

		public void SetPosY( float y )
		{
			SetPos( mTr.position.x, y );
		}
		#endregion

		#region Physics
		public void SetFriction (float friction )
		{
			if( friction != mPhysicMaterial.friction )
			{
				mPhysicMaterial.friction = friction;
				platformCollider.gameObject.SetActive (false);
				platformCollider.gameObject.SetActive (true);
			}
		}

		public void SetBounce( float bounce )
		{
			if( bounce != mPhysicMaterial.bounciness )
			{
				mPhysicMaterial.bounciness = bounce;
				platformCollider.gameObject.SetActive (false);
				platformCollider.gameObject.SetActive (true);
			}
		}
        
        public void SetExternalPhysics( PhysicsData info )
        {
            
        }
        
        public void ResetExternalPhysics()
        {
            
        }

		public void GravityActive(bool useGravity )
		{
			if (useGravity)
			{
				mRb.gravityScale = 1f;
			}
			else
			{
				mRb.gravityScale = 0f;
			}
		}
		#endregion

		#region Collision
		void CheckCollisions ()
		{
			State.SaveLastStateAndReset ();

			CastRayFront();
			CastRaysBelow();

			//밀수 있는 것들은 민다.

			if( State.JustGotGrounded )
			{
				if( OnJustGotGrounded != null ) OnJustGotGrounded();
			}
		}

		void CastRayFront()
		{
			Vector2 origin = mTr.TransformPoint( mCastOriginFront );
			float currentVX = mRb.velocity.x;
			float distance = mWallCastDistance;
			if( currentVX != 0 ) distance += Mathf.Abs( currentVX ) * Time.deltaTime;

			RaycastHit2D hit = Physics2D.Raycast( origin, new Vector2( Facing,0), distance, DruggedEngine.MASK_MIXED_EXCEPT_ONEWAY_GROUND );

			if( hit && hit.collider.isTrigger == false )
			{
				State.FrontGameObject = hit.collider.gameObject;
				State.FrontHit = hit;
			}
			else
			{
				State.FrontGameObject = null;
			}
		}

		void CastRaysBelow ()
		{
			if( State.IsGrounded == false && vy > 0f ) return;

			GameObject nowStanding = GroundCast( mCastOriginCenter ); //center

			if (nowStanding == null) nowStanding = GroundCast( Facing == 1 ? mCastOriginBack : mCastOriginForward ); //back
			if (nowStanding == null) nowStanding = GroundCast( Facing == 1 ? mCastOriginForward : mCastOriginBack ); //forward

			State.StandingGameObject = nowStanding;

			if( State.IsGrounded ) State.UpdatePlatformVelocity();
		}

		GameObject GroundCast (Vector3 origin )
		{
			RaycastHit2D hit = Physics2D.Raycast (mTr.TransformPoint (origin), Vector2.down, CAST_GROUND_LENGTH, currentMask);

			if (hit.collider == null) return null;
			if (hit.collider.isTrigger) return null;

			State.SlopeAngle = Vector2.Angle (hit.normal, Vector2.up);
			return hit.collider.gameObject;
		}

		public void UpdateColliderSize (float xScale, float yScale)
		{
			platformCollider.transform.localScale = new Vector3(xScale,yScale,1f);
		}

		public void ResetColliderSize()
		{
			UpdateColliderSize (1f,1f);
		}

		public bool IsCollidingHead
		{
			get
			{
				return false;
			}
		}

		public void PassOneway()
		{
			if( State.IsOnOneway == false ) return;

			ExceptOneway( State.StandingPlatform );
		}

		public void ExceptOneway( Platform oneway )
		{
			currentMask = DruggedEngine.MASK_MIXED_EXCEPT_ONEWAY_GROUND;
			IgnoreCollision( oneway.platformCollider, true );
		}

		public void IncludeOneway( Platform oneway )
		{
			currentMask = DruggedEngine.MASK_MIXED_ALLGROUND;
			IgnoreCollision( oneway.platformCollider, false );
		}

		public void IgnoreCollision( Collider2D col, bool ignore )
		{
			Physics2D.IgnoreCollision( platformCollider, col, ignore );
		}

		#endregion

		#region UNUSED
		//TODO:  deal with SetFriction workaround breaking ignore pairs.........
		void IgnoreCharacterCollisions (bool ignore)
		{
			// foreach (GameCharacter gc in All)
			// 	if (gc == this)
			// 		continue;
			// 	else {
			// 		gc.IgnoreCollision(primaryCollider, ignore);
			// 	}
		}


		//Detect being on top of a characters's head
		Rigidbody2D OnTopOfCharacter ()
		{
			Rigidbody2D character = GetRelevantCharacterCast (mCastOriginCenter, 0.15f);
			if (character == null) character = GetRelevantCharacterCast (mCastOriginBack, 0.15f);
			if (character == null) character = GetRelevantCharacterCast (mCastOriginForward, 0.15f);

			return character;
		}

		Rigidbody2D GetRelevantCharacterCast (Vector3 origin, float dist)
		{
			RaycastHit2D[] hits = Physics2D.RaycastAll (transform.TransformPoint (origin), Vector2.down, dist, characterMask);
			if (hits.Length > 0)
			{
				int index = 0;

				if (hits [0].rigidbody == mRb)
				{
					if (hits.Length == 1)
						return null;

					index = 1;
				}
				if (hits [index].rigidbody == mRb)
					return null;

				var hit = hits [index];
				if (hit.collider != null && hit.collider.attachedRigidbody != null)
				{
					return hit.collider.attachedRigidbody;
				}
			}

			return null;
		}

		#endregion

		#if UNITY_EDITOR
		void OnDrawGizmos ()
		{
			if (Application.isPlaying == false) return;

			//DRAW GROUND
			if (State.IsGrounded) Gizmos.color = Color.green;
			else Gizmos.color = Color.magenta;

			Gizmos.DrawWireSphere (mTr.TransformPoint (mCastOriginCenter), 0.05f);
			Gizmos.DrawWireSphere (mTr.TransformPoint (mCastOriginBack), 0.05f);
			Gizmos.DrawWireSphere (mTr.TransformPoint (mCastOriginForward), 0.05f);

			Gizmos.DrawLine (mTr.TransformPoint (mCastOriginCenter), mTr.TransformPoint (mCastOriginCenter + new Vector3 (0, -CAST_GROUND_LENGTH, 0)));
			Gizmos.DrawLine (mTr.TransformPoint (mCastOriginBack), mTr.TransformPoint (mCastOriginBack + new Vector3 (0, -CAST_GROUND_LENGTH, 0)));
			Gizmos.DrawLine (mTr.TransformPoint (mCastOriginForward), mTr.TransformPoint (mCastOriginForward + new Vector3 (0, -CAST_GROUND_LENGTH, 0)));

			//DRAW FRONT
			if (State.IsCollidingFront ) Gizmos.color = Color.green;
			else Gizmos.color = Color.magenta;

			Gizmos.DrawWireSphere (mTr.TransformPoint (mCastOriginFront), 0.05f);

			float currentVX = mRb.velocity.x;
			float distance = mWallCastDistance;
			if( currentVX != 0 ) distance += Mathf.Abs( currentVX ) * Time.deltaTime;

			Gizmos.DrawLine (mTr.TransformPoint (mCastOriginFront), mTr.TransformPoint (mCastOriginFront + new Vector3 ( Facing * distance, 0, 0)));
		}
		#endif
	}

	#region ControllerState
	public class NewControllerState
	{
		GameObject mStandingOn;
		GameObject mFrontObject;

		public float SlopeAngle { get; set; }

		public bool JustGotGrounded { get; private set; }

		public bool WasColldingBelowLastFrame { get; private set; }

		public bool WasColldingAdoveLastFrame { get; private set; }

		public bool IsCollidingAbove { get; set; }
		public bool IsCollidingFront { get; private set; }

		public bool IsGrounded{ get; private set; }

		public Platform StandingPlatform { get; private set; }

		public Vector2 PlatformVelocity { get; private set; }

		public bool IsOnOneway {
			get {
				if (IsGrounded == false || StandingPlatform == null ) return false;
				return StandingPlatform.oneway;
			}
		}

		public RaycastHit2D FrontHit{ get;set;}
		public GameObject FrontGameObject
		{
			get{ return mFrontObject; }
			set
			{
				mFrontObject = value;

				if( mFrontObject == null )
				{
					IsCollidingFront = false;
				}
				else
				{
					IsCollidingFront = true;
				}
			}
		}

		public GameObject StandingGameObject
		{
			get { return mStandingOn; }
			set {
				if (mStandingOn == value) return;

				mStandingOn = value;

				if (mStandingOn == null)
				{
					IsGrounded = false;
					PlatformVelocity = Vector2.zero;
					StandingPlatform = null;
				}
				else
				{
					IsGrounded = true;

					StandingPlatform = mStandingOn.GetComponent<Platform>();

					if (WasColldingBelowLastFrame == false)
					{
						JustGotGrounded = true;
						//StandingPlatform.StepOn( this );
					}
				}
			}
		}

		public void UpdatePlatformVelocity()
		{
			if (StandingPlatform == null) PlatformVelocity = Vector2.zero;
			else PlatformVelocity = StandingPlatform.velocity;
		}

		public void ClearPlatform()
		{
			StandingGameObject = null;
			SlopeAngle = 0f;
			JustGotGrounded = false;
		}

		public void SaveLastStateAndReset ()
		{
			WasColldingBelowLastFrame = IsGrounded;
			WasColldingAdoveLastFrame = IsCollidingAbove;

			SlopeAngle = 0f;
			JustGotGrounded = false;
		}
	}
	#endregion
}

