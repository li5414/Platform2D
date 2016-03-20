using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	[RequireComponent(typeof(BoundingBoxFollower))]
	public class BoundingBoxLinker : MonoBehaviour
	{
		public MeleeWeapon meleeWeapon;

		BoundingBoxFollower mFollower;

		void Awake()
		{
			mFollower = GetComponent<BoundingBoxFollower>();
		}

		void OnTriggerEnter2D( Collider2D collider )
		{
			meleeWeapon.BoundingTrigger( mFollower, collider );
		}
	}
}
