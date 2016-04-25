using UnityEngine;
using System.Collections;
using druggedcode;
using Spine.Unity;

namespace druggedcode.engine
{
    public class RagdollImpactEffector : MonoBehaviour
    {

        static float nextImpactTime = 0;

        public string impactSound = "Impact/Random";
        float spawnTime;

        void Awake()
        {
            spawnTime = Time.time;
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (Time.time < spawnTime + 0.25f)
                return;

            var boundingBoxFollower = collider.GetComponent<BoundingBoxFollower>();
            if (boundingBoxFollower != null)
            {
                var attachmentName = boundingBoxFollower.CurrentAttachmentName;

                int fromSign = collider.transform.position.x < transform.position.x ? -1 : 1;

                switch (attachmentName)
                {
                    case "Punch":
                        Hit(new Vector2(-fromSign * 20, 8));
                        break;
                    case "UpperCut":
                        Hit(new Vector2(-fromSign * 20, 75));
                        break;
                    case "HeadDive":
                        Hit(new Vector2(0, 30));
                        break;
                }
            }
        }

        void Hit(Vector2 v)
        {
            GetComponent<Rigidbody2D>().velocity = v;
            if (Time.time > nextImpactTime)
            {
                SoundManager.Instance.PlaySFX(impactSound, 0.5f, 1, transform.position);
            }
            nextImpactTime = Time.time + 0.2f;
        }

        void Hit(HitData data)
        {
            Hit(data.force * 3);
        }
    }
}