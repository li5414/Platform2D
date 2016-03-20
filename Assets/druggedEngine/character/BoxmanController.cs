using UnityEngine;
using System.Collections;
using druggedcode.engine;

public class BoxmanController : MonoBehaviour {
	public enum ActionState { IDLE, WALK, DANCE, DEAD }

	[Header("Animations")]
	[SpineAnimation]
	public string idleAnim;
	[SpineAnimation]
	public string walkAnim;
	[SpineAnimation]
	public string danceAnim;
	[SpineAnimation]
	public string hitForwardAnim;
	[SpineAnimation]
	public string hitBackwardAnim;

	[Header("Stats")]
	public float hp = 4;

	[Header("Audio")]
	public string hitSound;

	[Header("Physics")]
	public Collider2D primaryCollider;
	public LayerMask platformMask;

	[Header("References")]
	public SkeletonAnimation skeletonAnimation;
	public GameObject punchHitPrefab;

	public ActionState state;

	SkeletonUtility skeletonUtility;
	Rigidbody2D rb;
	bool flipped = false;

	void Start () {
		rb = GetComponent<Rigidbody2D>();

		if (Random.value > 0.5f) {
			skeletonAnimation.skeleton.FlipX = !skeletonAnimation.skeleton.FlipX;
			flipped = skeletonAnimation.skeleton.FlipX;
		}
	}

	void OnTriggerEnter2D (Collider2D collider) {
		var boundingBoxFollower = collider.GetComponent<BoundingBoxFollower>();
		if (boundingBoxFollower != null) {
			var attachmentName = boundingBoxFollower.CurrentAttachmentName;

			int fromSign = collider.transform.position.x < transform.position.x ? -1 : 1;

			switch (attachmentName) {
				case "Punch":
					Hit(new Vector2(-fromSign * 2, 0), 1, fromSign);
					PunchImpact(collider, Vector2.zero, new Vector2(-fromSign, 0));
					break;
				case "UpperCut":
					Hit(new Vector2(-fromSign * 4, 8), 4, fromSign);
					PunchImpact(collider, new Vector2(0,1.25f), new Vector2(0, 1));
					break;
				case "HeadDive":
					Hit(new Vector2(0, -8), 8, 0);
					break;
			}
		}
	}

	void PunchImpact (Collider2D collider, Vector2 offset, Vector2 direction) {
		Instantiate(punchHitPrefab, collider.bounds.center + (Vector3)offset, Quaternion.FromToRotation(Vector3.right, direction));
	}

	void Hit (Vector2 velocity, float damage, int fromSign) {

		//SoundPalette.PlaySound(hitSound, 1, 1, transform.position);

		if (hp <= 0)
			return;

		hp -= damage;

		if (hp <= 0) {
			state = ActionState.DEAD;
			var ragdoll = GetComponentInChildren<SkeletonRagdoll2D>();
			ragdoll.Apply();
			ragdoll.RootRigidbody.velocity = velocity * 10;
			var agent = ragdoll.RootRigidbody.gameObject.AddComponent<MovingPlatformAgent>();
			var rootCollider = ragdoll.RootRigidbody.GetComponent<Collider2D>();
			agent.platformMask = platformMask;
			agent.castRadius = rootCollider.GetType() == typeof(CircleCollider2D) ? ((CircleCollider2D)rootCollider).radius * 8f : rootCollider.bounds.size.y;
			agent.useCircleMode = true;
			var rbs = ragdoll.RootRigidbody.transform.parent.GetComponentsInChildren<Rigidbody2D>();
			foreach (var r in rbs) {
				r.gameObject.AddComponent<RagdollImpactEffector>();
			}

			Destroy(rb);
			Destroy(primaryCollider);
		} else {
			rb.velocity = velocity;

			string anim = "";
			if ((flipped ? -1 : 1) != fromSign) {
				anim = hitBackwardAnim;
			} else {
				anim = hitForwardAnim;
			}

			skeletonAnimation.state.SetAnimation(0, anim, false);
			skeletonAnimation.state.AddAnimation(0, idleAnim, true, 0.2f);
		}
	}

	void FixedUpdate () {
		if (rb == null)
			return;

		//var movingPlatform = MovingPlatformCast(new Vector3(0, 0.1f, 0));

//		if (movingPlatform) {
//			var track = skeletonAnimation.state.GetCurrent(0);
//
//			//hacky, but functional.  only apply moving velocity if not being hit.
//			if (track.Animation.Name == idleAnim) {
//				Vector3 velocity = rb.velocity;
//				velocity.x = movingPlatform.Velocity.x;
//				velocity.y = movingPlatform.Velocity.y;
//				rb.velocity = velocity;
//			}
//
//		}
	}

//	MovingPlatform MovingPlatformCast (Vector3 origin) {
//		MovingPlatform platform = null;
//		RaycastHit2D hit = Physics2D.Raycast(transform.TransformPoint(origin), -Vector2.up, 0.2f, platformMask);
//		if (hit.collider != null) {
//			platform = hit.collider.GetComponent<MovingPlatform>();
//		}
//
//
//		return platform;
//	}
}