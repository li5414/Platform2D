using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    public class Projectile : MonoBehaviour
    {
		[Header("Prefabs")]
		public GameObject impactPrefab;
		public GameObject destroyPrefab;

        public LayerMask targetMask;
		public AudioClip impactSound;

		[Header("Options")]
		public Transform destination;
		public float speed = 1;

		[Header("Explode")]
		public float damage;
		public float radius = 1.5f;
        public float impactForce = 20;
        

		protected UpdateType moveType;
		protected Transform mTr;

		virtual protected void Awake()
		{

			mTr = transform;
			moveType = UpdateType.Update;
			//필요한 경우 override 하여 moveType 변경
		}

		virtual protected void Start()
        {
			
        }

		virtual protected void Move()
		{
			
		}

		void Update()
		{
			if( moveType == UpdateType.Update ) Move();
		}

		void LateUpdate()
		{
			if( moveType == UpdateType.LateUpdate ) Move();
		}

        void FixedUpdate()
        {
			if( moveType == UpdateType.FixedUpdate ) Move();
        }

		virtual protected void Disappear()
		{
			if (destroyPrefab != null)
			{
				FXManager.Instance.SpawnFX( destroyPrefab, mTr.position, mTr.rotation );
			}

			Destroy(gameObject);
		}

		protected void Impact( Vector3 point )
		{
			FXManager.Instance.SpawnFX( impactPrefab,point, mTr.rotation );
			SoundManager.Instance.PlaySFX( impactSound,1,1, point );

			Destroy(gameObject);
		}

        protected void Explode(Vector2 point)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(point, radius);
			foreach (Collider2D c in colliders)
            {
                if (c.isTrigger)
                    continue;

                if (c.attachedRigidbody != null)
                {
                    Vector2 origin = point - (Vector2)transform.right;
                    Vector2 dir = point - origin;
					HitData data = new HitData(damage, origin, point, (Vector2)(dir * impactForce) + new Vector2(0, 5));

					c.attachedRigidbody.SendMessage("Hit", data, SendMessageOptions.DontRequireReceiver);
                }
            }

			Impact( point );
        }

		void OnDrawGizmos()
		{
			//Gizmos.DrawWireSphere(transform.position, radius);
		}
    }
}