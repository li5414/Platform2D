using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine;

namespace druggedcode.engine
{
	public class MeleeWeapon : Weapon
	{
		public List<MeleeData> hitDataList;

		Dictionary<string, MeleeData> mHitDataDic;

		override public void Init( DEActor owner, Skeleton skeleton )
		{
			base.Init( owner, skeleton );

			mHitDataDic = new Dictionary<string, MeleeData>();

			foreach( MeleeData data in hitDataList )
			{
				mHitDataDic.Add( data.name, data );
			}
		}

		public void BoundingTrigger( BoundingBoxFollower follower, Collider2D other )
		{
			if( LayerUtil.Contains( CollisionMask, other.gameObject.layer ) == false )
			{
				return;
			}

			string attackName = follower.CurrentAttachmentName;

			MeleeData data = mHitDataDic[ attackName ];
			if( data == null ) return;

			print("Hit:" + follower.name + ", tg: " + other.name );

			PolygonCollider2D me =  follower.CurrentCollider;
			int xDir = other.transform.position.x < mOwner.transform.position.x ? -1 : 1;

			SendHit( data, other, xDir );
			ShowImpact( data, me, xDir );
		}

		void SendHit( MeleeData data, Collider2D collider, int xDir )
		{
			IDamageable damageable = collider.GetComponent<IDamageable>();

			if (damageable == null) return;

			float damage = weaponDamage * data.damageRatio;
			Vector2 force = new Vector2( xDir * data.force.x, data.force.y );
			HitData hitData = new HitData( damage, force  );

			damageable.Hit( hitData );
		}

		void ShowImpact( MeleeData data, PolygonCollider2D current, int xDir )
		{
			if( data.hitPrefab == null ) return;

			Vector3 hitPos = current.bounds.center + Vector3.zero;
			Vector2 hitDirection = new Vector2( xDir * data.hitPrefabDirection.x, data.hitPrefabDirection.y );
			Quaternion hitRotation = Quaternion.FromToRotation( Vector2.right, hitDirection);
			FXManager.Instance.SpawnFX( data.hitPrefab, hitPos, hitRotation );
		}
	}

	[System.Serializable]
	public class MeleeData
	{
		public string name;
		public float damageRatio = 1f;
		public Vector2 force;

		public GameObject hitPrefab;
		public Vector2 hitPrefabDirection;
	}
}
