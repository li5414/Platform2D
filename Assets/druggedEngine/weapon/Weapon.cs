using UnityEngine;
using Spine;

namespace druggedcode.engine
{
    public class Weapon : MonoBehaviour
    {
        // setup anim 은 애니메이션이 아니라 1frame 짜리 셋팅. slot 의 attachment 설정
        public new string name;

        [Header("Anim")]
        [SpineAnimation(startsWith: "Setup")]
        public string setupAnim;
        [SpineAnimation(startsWith: "Idle")]
        public string idleAnim;
        
        [SpineAnimation(startsWith: "Attack")]
        public string attackAnim;

		public LayerMask CollisionMask;
		public int weaponDamage = 1;

        public float nextFireTime = 0;

		protected Spine.Animation mSetupAnim;
		protected DEActor mOwner;
		protected Skeleton mSkeleton;
		protected SkeletonData mData;

		/// <summary>
		/// Init from DECharacter's Start
		/// </summary>
		virtual public void Init( DEActor owner, Skeleton skeleton )
        {
			mOwner = owner;
			mSkeleton = skeleton;
			mData = skeleton.Data;

			mSetupAnim = mData.FindAnimation(setupAnim);
        }

		virtual public void Setup()
        {
			if(mSetupAnim != null ) mSetupAnim.Apply( mSkeleton, 0, 1, false, null );

			mOwner.PlayAnimation( idleAnim, true, 1);
        }

		virtual public bool IsReady()
		{
			return false;
		}

		virtual public void Attack()
        {
            Debug.LogWarning("Not implemented!");
        }
        
        //발사 반동
        public virtual Vector2 GetRecoil()
        {
            return Vector2.zero;
        }
    }
}

