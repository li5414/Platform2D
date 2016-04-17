using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	public delegate void BoundingBoxDelegate( BoundingBoxFollower follower, Collider2D coll );

	[RequireComponent(typeof(BoundingBoxFollower))]
	public class BoundingBoxObserver : MonoBehaviour
	{
		public event BoundingBoxDelegate OnTrigger;

		BoundingBoxFollower mFollower;

		void Awake()
		{
			mFollower = GetComponent<BoundingBoxFollower>();
		}

		void OnTriggerEnter2D( Collider2D coll )
		{
			if( OnTrigger != null ) OnTrigger( mFollower, coll );
		}
	}
}
