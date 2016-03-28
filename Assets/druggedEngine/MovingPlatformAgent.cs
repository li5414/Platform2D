using UnityEngine;
using System.Collections;
using druggedcode.engine;

namespace druggedcode.engine
{
	public class MovingPlatformAgent : MonoBehaviour
	{
		public LayerMask platformMask;
		public float castDistance = 1;
		public float castRadius = 1;
		public bool useCircleMode = false;

		public Platform platform;

		Rigidbody2D mRB;
		Transform mTr;

		// Use this for initialization
		void Start ()
		{
			mRB = GetComponent<Rigidbody2D> ();
			mTr = transform;
		}

		void Update ()
		{

		}


		void FixedUpdate ()
		{
			if (useCircleMode) CircleCheck ();
			else RayCheck ();

			if (platform != null)
			{
				Vector3 velocity = Vector3.zero;
				velocity.x = platform.velocity.x;
				velocity.y = platform.velocity.y;

				if (mRB != null) mRB.velocity = velocity;
				else mTr.position += velocity * Time.deltaTime;
			}
		}

		void RayCheck ()
		{
			RaycastHit2D hit = Physics2D.Raycast (mTr.position, new Vector2 (0, -1), castDistance, platformMask);

			platform = null;
			if (hit.transform != null)
			{
				if ( hit.collider.isTrigger == false ) platform = hit.collider.GetComponent<Platform> ();
			}
		}

		void CircleCheck ()
		{
			var colliders = Physics2D.OverlapCircleAll (mTr.position, castRadius, platformMask);
			platform = null;

			for (int i = 0; i < colliders.Length; i++)
			{
				var collider = colliders [i];
				if (collider != null)
				{
					if (collider.isTrigger == false && collider.bounds.center.y < mTr.position.y)
					{
						platform = collider.GetComponent<Platform> ();
						break;
					}

				}
			}
		}

		#if UNITY_EDITOR
		void OnDrawGizmos()
		{
			if( useCircleMode )
			{
				Gizmos.DrawWireSphere( transform.position, castRadius );
			}
			else
			{
				Gizmos.DrawLine( transform.position, transform.TransformPoint( Vector2.down * castDistance ));
			}
		}
		#endif
	}

}
