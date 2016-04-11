using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine;

namespace druggedcode.engine
{
	public class MeleeWeapon : Weapon
	{
		override public void AttackGround ()
		{
			if (mWaitNextAttack)
			{
				NextAttack();
			}
			else
			{
				mOwner.Controller.Stop ();
				mOwner.PlayAnimation (attackGroundAnim);
			}

			mIsReady = false;
		}

		override public void AttackAir ()
		{
			/*
            mAttackIndex = 0;
            Stop();

            if (verticalAxis > 0.1f && string.IsNullOrEmpty(attackUpAnim) == false)
            {
                PlayAnimation(attackUpAnim);
            }
            else if (verticalAxis < -0.1f && string.IsNullOrEmpty(attackDownAnim) == false)
            {
                PlayAnimation(attackDownAnim);
            }
            else if (string.IsNullOrEmpty(attackGroundAnim) == false)
            {
                PlayAnimation(attackGroundAnim);
            }
            */
		}

		public void BoundingTrigger (BoundingBoxFollower follower, Collider2D other)
		{
			if (LayerUtil.Contains (mTargetLayer, other.gameObject.layer) == false)
			{
				return;
			}

			string attackName = follower.CurrentAttachmentName;

			AttakData data = mHitDataDic [attackName];
			if (data == null) return;

			print ("Hit:" + attackName + ", tg: " + other.name);

			PolygonCollider2D me = follower.CurrentCollider;
			int xDir = other.transform.position.x < mOwner.transform.position.x ? -1 : 1;

			SendHit (data, other, xDir);
			ShowImpact (data, me, xDir);
		}

		void SendHit (AttakData data, Collider2D collider, int xDir)
		{
			IDamageable damageable = collider.GetComponent<IDamageable> ();

			if (damageable == null) return;

			float damage = weaponDamage * data.damageRatio;
			Vector2 force = new Vector2 (xDir * data.force.x, data.force.y);
			HitData hitData = new HitData (damage, force);

			damageable.Hit (hitData);
		}

		void ShowImpact (AttakData data, PolygonCollider2D current, int xDir)
		{
			if (data.hitPrefab == null) return;

			Vector3 hitPos = current.bounds.center + Vector3.zero;
			Vector2 hitDirection = new Vector2 (xDir * data.hitPrefabDirection.x, data.hitPrefabDirection.y);
			Quaternion hitRotation = Quaternion.FromToRotation (Vector2.right, hitDirection);
			FXManager.Instance.SpawnFX (data.hitPrefab, hitPos, hitRotation);
		}
	}
}
