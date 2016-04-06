using UnityEngine;
using Spine;

namespace druggedcode.engine
{
    public class Weapon : MonoBehaviour
    {

		#region INSPECTOR
        // setup anim 은 애니메이션이 아니라 1frame 짜리 셋팅. slot 의 attachment 설정
        public new string name;

        [Header("Anim")]
        [SpineAnimation(startsWith: "Setup")]
        public string setupAnim;
        [SpineAnimation(startsWith: "Attack")]
        public string idleAnim;
        
        [SpineAnimation(startsWith: "Attack")]
        public string attackGroundAnim;

		[SpineAnimation(startsWith: "Attack")]
		public string attackAirAnim;

		public LayerMask CollisionMask;
		public int weaponDamage = 1;
        //public float nextFireTime = 0;

		public float waitAttackDuration;

		#endregion

		protected Spine.Animation mSetupAnim;
		protected DEActor mOwner;
		protected SkeletonAnimation mSkeletonAnimation;
		protected Skeleton mSkeleton;
		protected SkeletonData mData;

		protected bool mWaitNextAttack;
		protected float mWaitNextAttackEndTime;

		/// <summary>
		/// Init from DECharacter's Start
		/// </summary>
		virtual public void Init( DEActor owner, SkeletonAnimation sAnimation )
        {
			mOwner = owner;
			mSkeletonAnimation = sAnimation;
			mSkeleton = mSkeletonAnimation.Skeleton;
			mData = mSkeleton.Data;

			mSetupAnim = mData.FindAnimation(setupAnim);

			mSkeletonAnimation.state.Event += HandleEvent;
			mSkeletonAnimation.state.Complete += HandleComplete;
        }

		virtual protected void HandleEvent(Spine.AnimationState animState, int trackIndex, Spine.Event e)
		{
			switch (e.Data.name)
			{
				case DEActor.ANIM_EVENT_WAITATTACK:
					WaitNextAttack ( e.Int, e.Float, e.String );
					break;

				case DEActor.ANIM_EVENT_FIRE:
					//OnFire(e.Int, e.Float, e.String);
					break;

				case DEActor.ANIM_EVENT_EJECT_CASING:
					//OnEjectCasing(e.Int, e.Float, e.String);
					break;
			}
		}

		virtual protected void HandleComplete(Spine.AnimationState animState, int trackIndex, int loopCount)
		{
			//			var entry = animState.GetCurrent(trackIndex);
			//			if (entry.Animation.Name.IndexOf("Attack") == 0)
			//			{
			//				Idle();
			//			}
		}

		virtual protected void WaitNextAttack( int i, float f, string s)
		{
			mWaitNextAttack = true;
			mWaitNextAttackEndTime = Time.time + waitAttackDuration;

			mOwner.currentAnimationTimeScale( 0f );
			/*
            mCanAttack = true;
            mWaitNextAttack = true;
            currentAnimationTimeScale(0f);
            */
		}

		virtual public void UnEquip()
		{

		}

		virtual public void Equip()
        {
			if(mSetupAnim != null ) mSetupAnim.Apply( mSkeleton, 0, 1, false, null );

//			mOwner.PlayAnimation( idleAnim, true, 1);
        }

		virtual public bool IsReady()
		{
			return false;
		}

		virtual public void AttackGround()
        {
			Debug.LogWarning("Not implemented!");
        }

		virtual public void AttackAir()
		{
			Debug.LogWarning("Not implemented!");
		}
        
        //공격 반동
        virtual public Vector2 GetRecoil()
        {
            return Vector2.zero;
        }
    }

	[System.Serializable]
	public class AttakData
	{
		public string name;
		public float damageRatio = 1f;
		public Vector2 force;

		public GameObject hitPrefab;
		public Vector2 hitPrefabDirection;
	}
}

