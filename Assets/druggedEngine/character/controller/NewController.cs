using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	public class NewController : MonoBehaviour
	{
		const float CAST_GROUND_LENGTH = 0.15f;

		#region inspector
		[Header ("References")]
		public PolygonCollider2D primaryCollider;

		[Header ("Physics")]
		public float fallGravity = -4;
		public float idleFriction = 20;
		public float movingFriction = 0;

		[Header ("Raycasting")]
		public LayerMask characterMask;
		//characters
		public LayerMask groundMask;
		//environmnet,platform
		public LayerMask passThroughMask;
		//environmnet

		//[HideInInspector]
		public LayerMask currentMask;
		#endregion

		#region getter setter
		public NewControllerState state { get; private set; }
		#endregion

		Rigidbody2D mRb;
		Transform mTr;
		PhysicsMaterial2D characterColliderMaterial;
		protected Vector2 mPassedVelocity;

		Vector3 mCastOriginCenter;
		Vector3 mCastOriginBack;
		Vector3 mCastOriginForward;
		Vector3 wallCastOrigin;
		float wallCastDistance;

		bool doPassthrough;
		Platform passThroughPlatform;
		//oneway
		Platform movingPlatform;
		//moving

		Vector2 mAxis;
		int mFacing;

		bool airControllLock;
		float mLockedVY;
		float mTargetSpeed;

		virtual protected void Awake ()
		{
			mRb = GetComponent<Rigidbody2D> ();
			mRb.angularDrag = 1f;
			mRb.drag = 1f;
			mRb.constraints = RigidbodyConstraints2D.FreezeRotation;

			mTr = transform;

			state = new NewControllerState ();

			CalculateRayBounds (primaryCollider);

			if (primaryCollider.sharedMaterial == null)
				characterColliderMaterial = new PhysicsMaterial2D ("CharacterColliderMaterial");
			else
				characterColliderMaterial = Instantiate (primaryCollider.sharedMaterial);

			primaryCollider.sharedMaterial = characterColliderMaterial;
		}

		virtual protected void Start ()
		{
			currentMask = DruggedEngine.MASK_ALL_GROUND;
		}

		void CalculateRayBounds (PolygonCollider2D coll)
		{
			Bounds b = coll.bounds;
			Vector3 min = mTr.InverseTransformPoint (b.min);
			Vector3 center = mTr.InverseTransformPoint (b.center);
			Vector3 max = mTr.InverseTransformPoint (b.max);

			mCastOriginBack.x = min.x;
			mCastOriginBack.y = min.y + 0.1f;

			mCastOriginCenter.x = center.x;
			mCastOriginCenter.y = min.y + 0.1f;

			mCastOriginForward.x = max.x;
			mCastOriginForward.y = min.y + 0.1f;

			wallCastOrigin = center;
			wallCastDistance = b.extents.x + 0.1f;
		}

		public void SetAxis (float axisX, float axisY)
		{
			mAxis.x = axisX;
			mAxis.y = axisY;
		}

		public void SetFacing( int facing )
		{
			mFacing = facing;
		}

		public void SetSpeed( float speed )
		{
			mTargetSpeed = speed;
		}

		#region controll Velocity
		public void SetVelocity( Vector2 v )
		{
			mRb.velocity = v;
		}

		public void SetVX( float vx )
		{
			mRb.velocity.x = vx;
		}

		public void SetVY( float vy )
		{
			mRb.velocity.y = vy;
		}

		public void AddForce (Vector2 force)
		{
			mRb.velocity = mRb.velocity + force;
		}

		public void AddForceX( float x )
		{
			mRb.velocity.x += x;
		}

		public void AddForceY( float y )
		{
			mRb.velocity.y += y;
		}
		#endregion

		void FixedUpdate ()
		{
			MoveX();
			MoveY();
			CheckCollisions ();
		}

		void MoveX()
		{
			mTargetSpeed = runSpeed * Mathf.Sign( mAxis.x );
			mTargetSpeed = walkSpeed * Mathf.Sign( mAxis.x );
			float absAxisX = Mathf.Abs(mAxis.x);

			float currentX = mRb.velocity.x;

			if( state.IsOnGround )
			{
				if( absAxisX > 0.1f )
				{
					currentX = Mathf.MoveTowards(currentX, mTargetSpeed + state.PlatformVelocity.x, Time.deltaTime * 15);
					//if slide > nextVelocity.x = savedXVelocity + platformXVelocity;
					//if attack >  nextVelocity.x = Mathf.MoveTowards(nextVelocity.x, platformXVelocity, Time.deltaTime * 8);
				}
				else
				{
					if( state.PlatformVelocity.x > 0f ) currentX = state.PlatformVelocity.x;
					else currentX = Mathf.MoveTowards(currentX, 0, Time.deltaTime * 10);
				}
			}
			else
			{
				if( airControllLock )
				{
					currentX = Mathf.MoveTowards(currentX, mTargetSpeed, Time.deltaTime * 8);
				}
				else
				{
					currentX = Mathf.MoveTowards(currentX, mTargetSpeed, Time.deltaTime * 8);
				}
			}

			mRb.velocity.x = currentX;
		}

		void MoveY()
		{
			float currentY =  mRb.velocity.y;

			if( state.IsOnGround )
			{
				currentY = state.PlatformVelocity.y;
			}
			else if( currentY > 0 )
			{
				
			}
			else
			{
				if (mLockedVY != 0f) currentY = mLockedVY;
				else currentY += fallGravity * Time.deltaTime;
			}

			mRb.velocity = currentY;
		}

		#region Collision
		void CheckCollisions ()
		{
			state.SaveLastStateAndReset ();

			CastRaysBelow ();

			//밀수 있는 것들은 민다.
		}

		void CastRaysBelow ()
		{
			GameObject nowStanding = GroundCast (mCastOriginCenter); //center
			if (nowStanding == null) nowStanding = GroundCast (mFacing ? mCastOriginForward : mCastOriginBack); //back
			if (nowStanding == null) nowStanding = GroundCast (mFacing ? mCastOriginBack : mCastOriginForward); //forward

			state.StandingOn = nowStanding;
		}

		GameObject GroundCast (Vector3 origin)
		{
			RaycastHit2D hit = Physics2D.Raycast (mTr.TransformPoint (origin), Vector2.down, CAST_GROUND_LENGTH, currentMask);

			if (hit.collider == null) return null;
			if (hit.collider.isTrigger) return null;

			state.SlopeAngle = Vector2.Angle (hit.normal, Vector2.up);
			return hit.collider.gameObject;
		}
		#endregion

		//아래를 누르고 점프를 눌른 경우 PlatformCast(centerGroundCastOrigin); 를 통해 밟고있는 platform 을 찾아 내 판단했다
		public void DoPassThrough (Platform platform)
		{
			StartCoroutine (PassthroughRoutine (platform));
		}

		IEnumerator PassthroughRoutine (Platform platform)
		{
			yield break;
			// currentMask = passThroughMask;
			// Physics2D.IgnoreCollision(primaryCollider, platform.collider, true);
			// passThroughPlatform = platform;
			// yield return new WaitForSeconds(0.5f);
			// Physics2D.IgnoreCollision(primaryCollider, platform.collider, false);
			// currentMask = groundMask;
			// passThroughPlatform = null;
		}


		//Detect being on top of a characters's head
		Rigidbody2D OnTopOfCharacter ()
		{
			Rigidbody2D character = GetRelevantCharacterCast (mCastOriginCenter, 0.15f);
			if (character == null)
				character = GetRelevantCharacterCast (mCastOriginBack, 0.15f);
			if (character == null)
				character = GetRelevantCharacterCast (mCastOriginForward, 0.15f);

			return character;
		}

		//Raycasting stuff
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

		//check to see if pressing against a wall
		public bool PressingAgainstWall {
			get {
				float x = mRb.velocity.x;
				bool usingVelocity = true;
				if (Mathf.Abs (x) < 0.1f)
				{
					x = mAxis.x;
					if (Mathf.Abs (x) == 0f)
					{
						return false;
					} else
					{
						usingVelocity = false;
					}
				}

				RaycastHit2D hit = Physics2D.Raycast (transform.TransformPoint (wallCastOrigin), new Vector2 (x, 0).normalized, wallCastDistance + (usingVelocity ? x * Time.deltaTime : 0), currentMask);
				if (hit.collider != null && !hit.collider.isTrigger)
				{
					if (hit.collider.GetComponent<Platform> ().oneway)
						return false;

					return true;
				}

				return false;
			}
		}

		//work-around for Box2D not updating friction values at runtime...
		void SetFriction (float friction)
		{
			if (friction != characterColliderMaterial.friction)
			{
				characterColliderMaterial.friction = friction;
				primaryCollider.gameObject.SetActive (false);
				primaryCollider.gameObject.SetActive (true);
			}
		}

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


		#if UNITY_EDITOR
		void OnDrawGizmos ()
		{
			if (Application.isPlaying == false)
				return;

			if (state.IsOnGround)
				Gizmos.color = Color.green;
			else
				Gizmos.color = Color.grey;

			Gizmos.DrawWireCube (transform.position, new Vector3 (0.25f, 0.25f, 0.25f));

			Gizmos.DrawWireSphere (transform.TransformPoint (mCastOriginCenter), 0.05f);
			Gizmos.DrawWireSphere (transform.TransformPoint (mCastOriginBack), 0.05f);
			Gizmos.DrawWireSphere (transform.TransformPoint (mCastOriginForward), 0.05f);

			Gizmos.DrawLine (transform.TransformPoint (mCastOriginCenter), transform.TransformPoint (mCastOriginCenter + new Vector3 (0, -0.15f, 0)));
			Gizmos.DrawLine (transform.TransformPoint (mCastOriginBack), transform.TransformPoint (mCastOriginBack + new Vector3 (0, -0.15f, 0)));
			Gizmos.DrawLine (transform.TransformPoint (mCastOriginForward), transform.TransformPoint (mCastOriginForward + new Vector3 (0, -0.15f, 0)));
		}
		#endif
	}

	public class NewControllerState
	{
		public float SlopeAngle { get; set; }

		public bool JustGotGrounded { get; private set; }

		public bool WasColldingBelowLastFrame { get; private set; }

		public bool WasColldingAdoveLastFrame { get; private set; }

		public bool IsCollidingAbove { get; set; }

		public bool IsOnGround{ get; private set; }

		public Platform StandingPlatform { get; private set; }

		public Vector2 PlatformVelocity { get; private set; }

		GameObject mStandingOn;

		public GameObject StandingOn {
			get{ return mStandingOn; }
			set {
				if (mStandingOn == value) return;

				mStandingOn = value;

				if (mStandingOn == null)
				{
					IsOnGround = false;
					PlatformVelocity = Vector2.zero;
				}
				else
				{
					IsOnGround = true;

					if (WasColldingBelowLastFrame == false) JustGotGrounded = true;

					StandingPlatform = mStandingOn.GetComponent<Platform> ();
					if (StandingPlatform == null) PlatformVelocity = Vector2.zero;
					else PlatformVelocity = StandingPlatform.velocity;
				}
			}
		}

		public void SaveLastStateAndReset ()
		{
			WasColldingBelowLastFrame = IsOnGround;
			WasColldingAdoveLastFrame = IsCollidingAbove;

			SlopeAngle = 0f;
			JustGotGrounded = false;
		}
	}
}

