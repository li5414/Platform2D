using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	public class Tower : MonoBehaviour
	{
		[Header("Prefabs")]
		public Projectile projectile;
		public GameObject fireEffect;


		[Header("Options")]
		public Transform destination;
		public bool activeAutomatically = true;
		public Transform muzzle;
		public float speed = 1f;
		[Range(0.1f,10f)]
		public float fireRate = 1f;

		float mNextShotInSeconds;

		Coroutine mReloadRoutine;


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
			if( projectile == null ) return;
			Projectile pro = Instantiate(projectile, muzzle.position,muzzle.rotation) as Projectile;
			pro.speed = speed;
			pro.destination = destination;

			FXManager.Instance.SpawnFX( fireEffect, muzzle.position, muzzle.rotation );
		}

		#if UNITY_EDITOR
		public virtual void OnDrawGizmos()
		{
			if( destination != null )
			{
				Gizmos.color=Color.red;
				Gizmos.DrawLine(muzzle.position, destination.position );
			}
		}
		#endif
	}
}
