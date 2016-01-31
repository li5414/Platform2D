using UnityEngine;


public class BoxmanController : GameCharacter
{
    /*
    ActionState[] ableList =
    {
        ActionState.IDLE,
        ActionState.WALK,
        ActionState.DANCE,
        ActionState.DEAD
    };
    */

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

    [Header("References")]
    public GameObject punchHitPrefab;

    override protected void Start()
    {
		base.Start();
        if (Random.value > 0.5f)
        {
            skeletonAnimation.skeleton.FlipX = !skeletonAnimation.skeleton.FlipX;
            mFlipped = skeletonAnimation.skeleton.FlipX;
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        var boundingBoxFollower = collider.GetComponent<BoundingBoxFollower>();
        if (boundingBoxFollower != null)
        {
            var attachmentName = boundingBoxFollower.CurrentAttachmentName;

            int fromSign = collider.transform.position.x < transform.position.x ? -1 : 1;

            switch (attachmentName)
            {
                case "Punch":
                    Hit(new Vector2(-fromSign * 2, 0), 1, fromSign);
                    PunchImpact(collider, Vector2.zero, new Vector2(-fromSign, 0));
                    break;
                case "UpperCut":
                    Hit(new Vector2(-fromSign * 4, 8), 4, fromSign);
                    PunchImpact(collider, new Vector2(0, 1.25f), new Vector2(0, 1));
                    break;
                case "HeadDive":
                    Hit(new Vector2(0, -8), 8, 0);
                    break;
            }
        }
    }

    void PunchImpact(Collider2D collider, Vector2 offset, Vector2 direction)
    {
        if( punchHitPrefab == null ) return;
        Instantiate(punchHitPrefab, collider.bounds.center + (Vector3)offset, Quaternion.FromToRotation(Vector3.right, direction));
    }

    void Hit(float damage)
    {
        Hit((hp - damage <= 0) ? new Vector2(0, -12) : Vector2.zero, damage, 0);
    }

    void Hit(HitData data)
    {
        Hit(data.velocity, data.damage, data.origin.x > data.point.x ? 1 : -1);
    }

    void Hit(Vector2 velocity, float damage, int fromSign)
    {

        SoundPalette.PlaySound(hitSound, 1, 1, transform.position);

        if (hp <= 0)
            return;

        hp -= damage;

        if (hp <= 0)
        {
            SetState( ActionState.DEAD );
            var ragdoll = GetComponentInChildren<SkeletonRagdoll2D>();
            ragdoll.Apply();
            ragdoll.RootRigidbody.velocity = velocity * 10;
            var agent = ragdoll.RootRigidbody.gameObject.AddComponent<MovingPlatformAgent>();
            var rootCollider = ragdoll.RootRigidbody.GetComponent<Collider2D>();
            agent.platformMask = groundMask;
            agent.castRadius = rootCollider.GetType() == typeof(CircleCollider2D) ? ((CircleCollider2D)rootCollider).radius * 8f : rootCollider.bounds.size.y;
            agent.useCircleMode = true;
            var rbs = ragdoll.RootRigidbody.transform.parent.GetComponentsInChildren<Rigidbody2D>();
            foreach (var r in rbs)
            {
                r.gameObject.AddComponent<RagdollImpactEffector>();
            }

            Destroy(mRb);
            Destroy(primaryCollider);
        }
        else
        {
            mRb.velocity = velocity;

            string anim = "";
            if ((mFlipped ? -1 : 1) != fromSign)
            {
                anim = hitBackwardAnim;
            }
            else
            {
                anim = hitForwardAnim;
            }

            skeletonAnimation.state.SetAnimation(0, anim, false);
            skeletonAnimation.state.AddAnimation(0, idleAnim, true, 0.2f);
        }
    }

    void FixedUpdate()
    {
        if (mRb == null)
            return;

        var movingPlatform = MovingPlatformCast(new Vector3(0, 0.1f, 0));

        if (movingPlatform)
        {
            var track = skeletonAnimation.state.GetCurrent(0);

            //hacky, but functional.  only apply moving velocity if not being hit.
            if (track.Animation.Name == idleAnim)
            {
                Vector3 velocity = mRb.velocity;
                velocity.x = movingPlatform.Velocity.x;
                velocity.y = movingPlatform.Velocity.y;
                mRb.velocity = velocity;
            }

        }
    }
}
