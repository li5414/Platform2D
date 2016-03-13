using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    public class Projectile : MonoBehaviour
    {
        public Transform graphics;
        public float initialSpeed;
        public float thrusterForce;
        public LayerMask targetMask;
        public GameObject impactPrefab;
        public string impactSound;
        public float damage;
        public float radius = 1.5f;
        public float impactForce = 20;
        public float lifeSpan = 3;
        public Thruster thruster;
        Rigidbody2D rb;
        bool thrustActive = false;
        int bounces = 0;

        // Use this for initialization
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.velocity = transform.right * initialSpeed;
            StartCoroutine(Activate(0));
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }


        void OnCollisionEnter2D(Collision2D collision)
        {

            bool hitTarget = false;
            bounces++;
            foreach (var cp in collision.contacts)
            {
                if ((1 << cp.collider.gameObject.layer & targetMask) > 0)
                {
                    Impact(cp.point);
                    hitTarget = true;
                    break;
                }
            }

            if (bounces > 3 && !hitTarget)
                Impact(transform.position);
        }

        IEnumerator Activate(float delay)
        {
            yield return new WaitForSeconds(delay);
            thrustActive = true;
            thruster.goalThrust = 1;
            rb.velocity = transform.right * rb.velocity.magnitude;
        }

        void FixedUpdate()
        {
            if (thrustActive == true)
            {
                rb.AddForce(transform.right * thrusterForce * Time.deltaTime);
            }

            if (thrustActive && rb.velocity.magnitude > 0)
                graphics.rotation = Quaternion.Slerp(graphics.rotation, Quaternion.FromToRotation(Vector3.right, rb.velocity), Time.deltaTime * 10);

            /*
            float dist = speed * Time.deltaTime;
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, transform.right, dist, targetMask);
            bool impact = false;
            Collider2D hitCollider;
            Vector2 point = Vector2.zero;
            foreach (var hit in hits) {
                if (hit.collider.isTrigger)
                    continue;

                hitCollider = hit.collider;
                point = hit.point;
                impact = true;
                break;
            }

            transform.Translate(dist, 0, 0);

            if (impact) {

            }*/
        }

        void Impact(Vector2 point)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(point, radius);
            foreach (var c in colliders)
            {
                if (c.isTrigger)
                    continue;

                if (c.attachedRigidbody != null)
                {
                    Vector2 origin = point - (Vector2)transform.right;
                    Vector2 dir = point - origin;
                    c.attachedRigidbody.SendMessage("Hit", new HitData(damage, origin, point, (Vector2)(dir * impactForce) + new Vector2(0, 5)), SendMessageOptions.DontRequireReceiver);
                }

            }

            Instantiate(impactPrefab, point, transform.rotation);
            // SoundManager.Instance.PlaySFX()
            // SoundPalette.PlaySound(impactSound, 1, 1, transform.position);
            Destroy(gameObject);
        }
    }
}