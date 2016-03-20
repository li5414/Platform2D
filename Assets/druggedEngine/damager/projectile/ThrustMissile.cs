using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	public class ThrustMissile : Projectile
	{
		public Transform graphics;
		public Thruster thruster;
		public float thrusterForce;
		public float activateDelay = 0f;
		public int maxBounce = 3;

		int mBounces = 0;
		Rigidbody2D mRb;
		bool thrustActive = false;

		override protected void Awake()
		{
			base.Awake();
			moveType = UpdateType.FixedUpdate;
			mRb = GetComponent<Rigidbody2D>();
		}

		override protected void Start()
		{
			base.Start();
			mRb.velocity = transform.right * speed;
			StartCoroutine(Activate( activateDelay ));
		}

		IEnumerator Activate(float delay)
		{
			yield return new WaitForSeconds(delay);
			thrustActive = true;
			thruster.goalThrust = 0.5f;
			mRb.velocity = transform.right * mRb.velocity.magnitude;
		}

		override protected void Move()
		{
			if (thrustActive == true)
			{
				mRb.AddForce(transform.right * thrusterForce * Time.deltaTime);
			}

			if (thrustActive && mRb.velocity.magnitude > 0)
			{
				graphics.rotation = Quaternion.Slerp(graphics.rotation, Quaternion.FromToRotation(Vector3.right, mRb.velocity), Time.deltaTime * 10);
			}
		}

		void OnCollisionEnter2D(Collision2D collision)
		{
			mBounces++;

//			foreach (ContactPoint2D cp in collision.contacts)
//			{
//				if ((1 << cp.collider.gameObject.layer & targetMask) > 0)
//				{
//					Explode(cp.point);
//					return;
//				}
//			}
//
//			if( mBounces > maxBounce)
//			{
//				Explode(transform.position);
//			}
		}
	}
}
