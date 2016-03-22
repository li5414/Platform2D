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
		public Vector2 Velocity { get { return mRb.velocity; } set { mPassedVelocity = value; } }
		public float vx { get { return mRb.velocity.x; } set { mPassedVelocity.x = value; } }
		public float vy { get { return mRb.velocity.y; } set { mPassedVelocity.y = value; } }
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
		//input's xy axis
		float savedXVelocity;
		bool velocityLock;

		bool flipped;
		//handlePhysics 에서 flipped = skeletonAnimation.Skeleton.FlipX;

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

		void FixedUpdate ()
		{
			Move ();
			CheckCollisions ();
		}

		void CheckCollisions ()
		{
			state.SaveLastStateAndReset ();

			CastRaysBelow ();

			//밀수 있는 것들은 민다.
		}

		void CastRaysBelow ()
		{
			GameObject nowStanding = GroundCast (mCastOriginCenter); //center
			if (nowStanding == null) nowStanding = GroundCast (flipped ? mCastOriginForward : mCastOriginBack); //back
			if (nowStanding == null) nowStanding = GroundCast (flipped ? mCastOriginBack : mCastOriginForward); //forward

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

		void Move ()
		{
			Vector2 nextVelocity = mRb.velocity;

			mPassedVelocity;

			nextVelocity.x += state.PlatformVelocity.x;
			nextVelocity.y += state.PlatformVelocity.y;

			mRb.velocity = nextVelocity;
		}

		//큰 충격(물리 페널티의 결과) 후 회복할 수 있도록 vx 를 캐싱한다.
		void HandlePhysics ()
		{
			float absX = Mathf.Abs (mAxis.x);
			float platformXVelocity = 0;
			float platformYVelocity = 0;
			Vector2 velocity = mRb.velocity;

			//aggressively find moving platform
//			movingPlatform = MovingPlatformCast(centerGroundCastOrigin);
//			if(movingPlatform == null)
//				movingPlatform = MovingPlatformCast(backGroundCastOrigin);
//			if(movingPlatform == null)
//				movingPlatform = MovingPlatformCast(forwardGroundCastOrigin);

			if (movingPlatform)
			{
				platformXVelocity = movingPlatform.velocity.x;
				platformYVelocity = movingPlatform.velocity.y;
			}

			float xVelocity = 0;//character 에서 전달된 move 속도.
			//character 에 전달받은 힘에서 platformXVelocity, platformYVelocity 를 추가한걸 velocity 에 설정한다

			//jump up
			if (velocity.y > 0)
				velocity.y = Mathf.MoveTowards (velocity.y, 0, Time.deltaTime * 30);
			//jump down
			velocity.y += fallGravity * Time.deltaTime;
			//wallslide 면
			//velocity.y = Mathf.Clamp(velocity.y, wallSlideSpeed, 0);
			//아래밟은거 체크
			savedXVelocity = velocity.x;

			if (velocityLock) velocity = Vector2.zero;

			mRb.velocity = velocity;
		}

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

