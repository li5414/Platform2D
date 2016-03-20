using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	public abstract class Projectile : Damager
    {
		///탄환이 제거될 때 생성할 이펙트 프리팹
		public GameObject disapperPrefab;

		[Header("Options")]
		public float lifeTime = 10f;

		[Header("Explode")]
		public float radius = 1.5f;
		public float impactForce = 20;

		protected float speed = 1f;
		protected Vector2 direction;

		private Vector3 mLastPosition;
		private Vector3 mVelocity;

		//필요한 경우 override 하여 moveType 변경
		protected UpdateType moveType = UpdateType.Update;


		//발사한 쪽이 움직이고 있었다면 발사자의 속도를 고려해 speed 를 결정 해야한다.((Mathf.Abs(InitialVelocity.x) + Speed)
		virtual public void Fire( GameObject owner, float speed, Vector2 dir )
		{
			mOwner = owner;
			transform.right = dir;
			direction = dir;
			this.speed = speed;

			mLastPosition = mTr.position;
			mVelocity = Vector3.zero;
		}

		abstract protected void Move();

		void Update()
		{
			if( moveType == UpdateType.Update || moveType == UpdateType.LateUpdate ) Move();
		}

		void LateUpdate()
		{
			mVelocity = ( mLastPosition - mTr.position ) / Time.deltaTime;
			mLastPosition = mTr.position;

			if ((lifeTime -= Time.deltaTime) <= 0f) Disappear ();
		}

        void FixedUpdate()
        {
			if( moveType == UpdateType.FixedUpdate ) Move();
        }

		protected void Disappear()
		{
			if (disapperPrefab != null)
			{
				FXManager.Instance.SpawnFX( disapperPrefab, mTr.position, mTr.rotation );
			}

			Destroy(gameObject);
		}

		protected void Exploade( Vector3 point )
		{
			Collider2D[] colliders = Physics2D.OverlapCircleAll(point, radius);
			foreach (Collider2D c in colliders)
			{
				if (c.isTrigger)
					continue;

				if (c.attachedRigidbody != null)
				{
					Vector2 origin = (Vector2) point - (Vector2)transform.right;
					Vector2 dir = (Vector2) point - origin;
					HitData data = new HitData(damage,(Vector2)(dir * impactForce) + new Vector2(0, 5));

					c.attachedRigidbody.SendMessage("Hit", data, SendMessageOptions.DontRequireReceiver);
				}
			}

			Impact( point );
		}
    }
}