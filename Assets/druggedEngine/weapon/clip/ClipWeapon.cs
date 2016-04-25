using UnityEngine;
using System.Collections;
using Spine;
using Spine.Unity;

namespace druggedcode.engine
{
	public class ClipWeapon : MonoBehaviour
	{
		[SpineAnimation(startsWith: "Aim")]
		public string aimAnim;
		[SpineAnimation(startsWith: "Reload")]
		public string reloadAnim;

		public float minAngle = -40;
		public float maxAngle = 40;

		public bool reloadLock;

		public int clipSize = 10;
		public int clip = 10;
		public int ammo = 50;

		[Header("Casing")]
		public GameObject casingPrefab;
		public Transform casingEjectPoint;

		public Spine.Animation AimAnim;
		public Spine.Animation ReloadAnim;

//		override public void Init( DEActor owner, SkeletonAnimation sAnimation )
//		{
//			base.Init( owner, sAnimation );
//			AimAnim = mData.FindAnimation(aimAnim);
//			ReloadAnim = mData.FindAnimation(reloadAnim);
//		}

//		override public void Attack()
//		{
			/*
			//조준하고,
			if (mCurrentWeapon.reloadLock == false &&
				mCurrentWeapon.clip > 0 &&
				Time.time >= mCurrentWeapon.nextFireTime)
			{
				PlayAnimation(mCurrentWeapon.fireAnim, false, 1);
				mCurrentWeapon.nextFireTime = Time.time + mCurrentWeapon.refireRate;
			}
			else if (mCurrentWeapon.reloadLock == false &&
				Time.time >= mCurrentWeapon.nextFireTime)
			{
				if (mCurrentWeapon.ammo > 0 && mCurrentWeapon.clip < mCurrentWeapon.clipSize)
				{
					PlayAnimation(mCurrentWeapon.reloadAnim, false, 1);
					mCurrentWeapon.reloadLock = true;
				}
			}

			TrackEntry entry = GetCurrent(1);
			//리로드 가 아닌 경우 aiming 
			if( mCurrentWeapon.reloadLock == false )
			{
				if( entry == null ||
					entry.Animation != mCurrentWeapon.FireAnim && entry.Animation != mCurrentWeapon.AimAnim )
				{
					PlayAnimation( mCurrentWeapon.aimAnim,true,1);
				}

				float angle = 45f;
			}
			//리로드 중인 경우
			else
			{
				if( mCurrentWeapon.reloadLock == false &&
					( entry == null || entry.Animation != mCurrentWeapon.FireAnim && entry.Animation != mCurrentWeapon.IdleAnim ))
				{
					PlayAnimation( mCurrentWeapon.idleAnim, true, 1 );
				}
			}
			*/
//		}

//		bool Reload()
//		{
//			if (ammo == 0)
//				return false;
//
//			int refill = clipSize;
//			if (refill > ammo)
//				refill = clipSize - ammo;
//			ammo -= refill;
//			clip = refill;
//
//			return true;
//		}
	}
}
