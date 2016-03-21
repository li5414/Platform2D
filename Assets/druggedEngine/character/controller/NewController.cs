using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	public class NewController : MonoBehaviour
	{
		[Header("Input")]
		public float deadZone = 0.05f;
		public float runThreshhold = 0.5f;

		public PolygonCollider2D primaryCollider;

		[Header("Raycasting")]
		public LayerMask characterMask;//characters
		public LayerMask groundMask;//environmnet,platform
		public LayerMask passThroughMask;//environmnet
		[HideInInspector]
		public LayerMask currentMask;

		Rigidbody2D rb;
		Transform mTr;
		PhysicsMaterial2D characterColliderMaterial;

		Vector3 backGroundCastOrigin;
		Vector3 centerGroundCastOrigin;
		Vector3 forwardGroundCastOrigin;
		Vector3 wallCastOrigin;
		float wallCastDistance;

		bool onIncline = false;//경사면에 있나?
		bool doPassthrough;
		Platform passThroughPlatform;//oneway
		Platform movingPlatform;//moving

		public float fallGravity = -4;
		Vector2 moveStick;//input's xy axis
		float savedXVelocity;
		bool velocityLock;

		bool flipped;//handlePhysics 에서 flipped = skeletonAnimation.Skeleton.FlipX;

		virtual protected void Awake()
		{
			rb = GetComponent<Rigidbody2D>();
			mTr = transform;

			CalculateRayBounds( primaryCollider );

			if (primaryCollider.sharedMaterial == null)
				characterColliderMaterial = new PhysicsMaterial2D("CharacterColliderMaterial");
			else
				characterColliderMaterial = Instantiate(primaryCollider.sharedMaterial);

			primaryCollider.sharedMaterial = characterColliderMaterial;

			currentMask = groundMask;
			
		}

		void CalculateRayBounds (PolygonCollider2D coll)
		{
			Bounds b = coll.bounds;
			Vector3 min = mTr.InverseTransformPoint(b.min);
			Vector3 center = mTr.InverseTransformPoint(b.center);
			Vector3 max = mTr.InverseTransformPoint(b.max);

			backGroundCastOrigin.x = min.x;
			backGroundCastOrigin.y = min.y + 0.1f;

			centerGroundCastOrigin.x = center.x;
			centerGroundCastOrigin.y = min.y + 0.1f;

			forwardGroundCastOrigin.x = max.x;
			forwardGroundCastOrigin.y = min.y + 0.1f;

			wallCastOrigin = center;
			wallCastDistance = b.extents.x + 0.1f;
		}

		void Update()
		{
			//UpdateAnim
			//input을 전달받자
		}

		void FixedUpdate()
		{
			Move();
			CheckCollisions();
		}

		void Move()
		{

		}

		void CheckCollisions ()
		{
			//state.SaveLastStateAndReset ();

			//검사

			//지상에 막 닿은건지 아닌지를 판단한다.
		}

		//큰 충격(물리 페널티의 결과) 후 회복할 수 있도록 vx 를 캐싱한다.
		void HandlePhysics ()
		{
			onIncline = false;

			float x = moveStick.x;
			float y = moveStick.y;
			float absX = Mathf.Abs(x);
			float platformXVelocity = 0;
			float platformYVelocity = 0;
			Vector2 velocity = rb.velocity;

			//aggressively find moving platform
			movingPlatform = MovingPlatformCast(centerGroundCastOrigin);
			if(movingPlatform == null)
				movingPlatform = MovingPlatformCast(backGroundCastOrigin);
			if(movingPlatform == null)
				movingPlatform = MovingPlatformCast(forwardGroundCastOrigin);

			if (movingPlatform) {
				platformXVelocity = movingPlatform.velocity.x;
				platformYVelocity = movingPlatform.velocity.y;
			}

			float xVelocity = 0;//character 에서 전달된 move 속도.
			//character 에 전달받은 힘에서 platformXVelocity, platformYVelocity 를 추가한걸 velocity 에 설정한다

			//jump up
			if (velocity.y > 0)
				velocity.y = Mathf.MoveTowards(velocity.y, 0, Time.deltaTime * 30);
			//jump down
			velocity.y += fallGravity * Time.deltaTime;
			//wallslide 면
			//velocity.y = Mathf.Clamp(velocity.y, wallSlideSpeed, 0);
			//아래밟은거 체크
			savedXVelocity = velocity.x;

			if (velocityLock) velocity = Vector2.zero;

			rb.velocity = velocity;
		}

		//아래를 누르고 점프를 눌른 경우 PlatformCast(centerGroundCastOrigin); 를 통해 밟고있는 platform 을 찾아 내 판단했다
		public void DoPassThrough ( Platform platform )
		{
			StartCoroutine(PassthroughRoutine(platform));
		}

		IEnumerator PassthroughRoutine (Platform platform) {
			currentMask = passThroughMask;
			Physics2D.IgnoreCollision(primaryCollider, platform.collider, true);
			passThroughPlatform = platform;
			yield return new WaitForSeconds(0.5f);
			Physics2D.IgnoreCollision(primaryCollider, platform.collider, false);
			currentMask = groundMask;
			passThroughPlatform = null;
		}


		//Detect being on top of a characters's head
		Rigidbody2D OnTopOfCharacter () {
			Rigidbody2D character = GetRelevantCharacterCast(centerGroundCastOrigin, 0.15f);
			if (character == null)
				character = GetRelevantCharacterCast(backGroundCastOrigin, 0.15f);
			if (character == null)
				character = GetRelevantCharacterCast(forwardGroundCastOrigin, 0.15f);

			return character;
		}

		//Raycasting stuff
		Rigidbody2D GetRelevantCharacterCast (Vector3 origin, float dist) {
			RaycastHit2D[] hits = Physics2D.RaycastAll(transform.TransformPoint(origin), Vector2.down, dist, characterMask);
			if (hits.Length > 0) {
				int index = 0;

				if (hits[0].rigidbody == rb) {
					if (hits.Length == 1)
						return null;

					index = 1;
				}
				if (hits[index].rigidbody == rb)
					return null;

				var hit = hits[index];
				if (hit.collider != null && hit.collider.attachedRigidbody != null) {
					return hit.collider.attachedRigidbody;
				}
			}

			return null;
		}

		public bool OnGround {
			get {
				return CenterOnGround || BackOnGround || ForwardOnGround;
			}
		}

		bool BackOnGround {
			get {
				return GroundCast(flipped ? forwardGroundCastOrigin : backGroundCastOrigin);
			}
		}

		bool ForwardOnGround {
			get {
				return GroundCast(flipped ? backGroundCastOrigin : forwardGroundCastOrigin);
			}
		}

		bool CenterOnGround {
			get {
				return GroundCast(centerGroundCastOrigin);
			}
		}

		//see if character is on the ground
		//throw onIncline flag
		bool GroundCast (Vector3 origin) {
			RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(origin), Vector2.down, 0.15f, currentMask);
			if (hit.collider != null && !hit.collider.isTrigger) {
				if (hit.normal.y < 0.4f)
					return false;
				else if (hit.normal.y < 0.95f)
					onIncline = true;

				return true;
			}

			return false;
		}

		//see if character is on a one-way platform
		Platform PlatformCast (Vector3 origin) {
			Platform platform = null;
			RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(origin), Vector2.down, 0.5f, currentMask);
			if (hit.collider != null && !hit.collider.isTrigger) {
				platform = hit.collider.GetComponent<Platform>();
			}

			if( platform == null || platform.oneway == false ) return null;
			else return platform;
		}

		//see if character is on a moving platform
		Platform MovingPlatformCast (Vector3 origin) {
			Platform platform = null;
			RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(origin), Vector2.down, 0.5f, currentMask);
			if (hit.collider != null && !hit.collider.isTrigger) {
				platform = hit.collider.GetComponent<Platform>();
			}

			if( platform == null || platform.movable == false ) return null;
			else return platform;
		}

		//check to see if pressing against a wall
		public bool PressingAgainstWall {
			get {
				float x = rb.velocity.x;
				bool usingVelocity = true;
				if (Mathf.Abs(x) < 0.1f) {
					x = moveStick.x;
					if (Mathf.Abs(x) <= deadZone) {
						return false;
					} else {
						usingVelocity = false;
					}
				}

				RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(wallCastOrigin), new Vector2(x, 0).normalized, wallCastDistance + (usingVelocity ? x * Time.deltaTime : 0), currentMask);
				if (hit.collider != null && !hit.collider.isTrigger) {
					if (hit.collider.GetComponent<Platform>().oneway )
						return false;

					return true;
				}

				return false;
			}
		}

		//work-around for Box2D not updating friction values at runtime...
		void SetFriction (float friction) {
			if (friction != characterColliderMaterial.friction) {
				characterColliderMaterial.friction = friction;
				primaryCollider.gameObject.SetActive(false);
				primaryCollider.gameObject.SetActive(true);
			}
		}

		//TODO:  deal with SetFriction workaround breaking ignore pairs.........
		void IgnoreCharacterCollisions (bool ignore) {
			foreach (GameCharacter gc in All)
				if (gc == this)
					continue;
				else {
					gc.IgnoreCollision(primaryCollider, ignore);
				}
		}


		#if UNITY_EDITOR
		void OnDrawGizmos () {
			Handles.Label(transform.position, state.ToString());
			if (!Application.isPlaying)
				return;

			if (OnGround)
				Gizmos.color = Color.green;
			else
				Gizmos.color = Color.grey;

			Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));

			Gizmos.DrawWireSphere(transform.TransformPoint(centerGroundCastOrigin), 0.07f);
			Gizmos.DrawWireSphere(transform.TransformPoint(backGroundCastOrigin), 0.07f);
			Gizmos.DrawWireSphere(transform.TransformPoint(forwardGroundCastOrigin), 0.07f);

			Gizmos.DrawLine(transform.TransformPoint(centerGroundCastOrigin), transform.TransformPoint(centerGroundCastOrigin + new Vector3(0, -0.15f, 0)));
			Gizmos.DrawLine(transform.TransformPoint(backGroundCastOrigin), transform.TransformPoint(backGroundCastOrigin + new Vector3(0, -0.15f, 0)));
			Gizmos.DrawLine(transform.TransformPoint(forwardGroundCastOrigin), transform.TransformPoint(forwardGroundCastOrigin + new Vector3(0, -0.15f, 0)));
		}
		#endif

		#region DEController API
		public void AddForce( Vector2 force )
		{

		}

		public void AddForceX( float x )
		{
		}

		public void AddForceY( float y )
		{
			
		}

		//월 점프 등 x이동을 잠시 제한
		public void LockMove (float duration)
		{
		}

		public void Stop ()
		{

		}
		//벽 탈 때 y 낙하 속도 제한
		public void LockVY (float lockvy)
		{
		}

		public void UnLockVY ()
		{
		}

		public void UpdateColliderSize (float xScale, float yScale)
		{

		}

		public void ResetColliderSize ()
		{

		}

		public void SetPhysicsSpace (PhysicInfo physicInfo)
		{

		}

		public void ResetPhysicInfo ()
		{
			
		}
		public bool IsCollidingHead { get;}
		#endregion
	}
}

