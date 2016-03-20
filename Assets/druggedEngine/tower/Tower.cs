using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	public class Tower : MonoBehaviour
	{
		[Header("Prefabs")]
		public Projectile projectile;
		public GameObject fireEffect;

		public LayerMask collisionMask;

		[Header("Options")]
		public bool activeAutomatically = true;
		public Transform muzzle;
		public float speed = 1f;
		[Range(0.1f,10f)]
		public float fireRate = 1f;

		float mNextShotInSeconds;

		Coroutine mReloadRoutine;

		Transform mTr;
		void Awake()
		{
			mTr = transform;
		}

		void Start ()
		{
			if( activeAutomatically ) Active();
		}

		public void Active()
		{
			mNextShotInSeconds = fireRate;

			mReloadRoutine = StartCoroutine( FireRoutine() );
		}

		public void Deactive()
		{
			if( mReloadRoutine != null ) StopCoroutine( mReloadRoutine );
		}

		IEnumerator FireRoutine()
		{
			while( true )
			{
				mNextShotInSeconds -= Time.deltaTime;

				if( mNextShotInSeconds < 0 )
				{
					mNextShotInSeconds = fireRate;
					Fire();
				}

				yield return null;
			}
		}

		void Fire()
		{
			Projectile pro = FXManager.Instance.SpawnFX<Projectile>( projectile, muzzle.position, muzzle.rotation );
			if( pro == null ) return;

			Vector2 direction = mTr.rotation * Vector2.right;

			pro.collisionMask = collisionMask;
			pro.Fire( gameObject, speed, direction ); 

			FXManager.Instance.SpawnFX( fireEffect, muzzle.position, muzzle.rotation );
		}
	}
}
