using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
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

		[Header("Property")]
		public int weaponDamage = 1;
		public float waitAttackDuration = 0.5f;
		public List<AttakData> AttackDataList;

		#endregion

		protected Spine.Animation mSetupAnim;
		protected DEActor mOwner;
		protected SkeletonAnimation mSkeletonAnimation;
		protected Skeleton mSkeleton;
		protected SkeletonData mData;

		protected bool mWaitNextAttack;
		protected float mWaitNextAttackEndTime;
		protected Dictionary<string, AttakData> mHitDataDic;
		protected bool mIsReady;
		protected LayerMask mTargetLayer;

		public event UnityAction OnAttackEnd;

		/// <summary>
		/// Init from DECharacter's Start
		/// </summary>
		virtual public void Init( DEActor owner, SkeletonAnimation sAnimation )
        {
			mOwner = owner;

			if( mOwner is DEPlayer ) mTargetLayer = DruggedEngine.MASK_ENEMY;
			else if( mOwner is DEEnemy ) mTargetLayer = DruggedEngine.MASK_PLAYER;

			mSkeletonAnimation = sAnimation;
			mSkeleton = mSkeletonAnimation.Skeleton;
			mData = mSkeleton.Data;

			mSetupAnim = mData.FindAnimation(setupAnim);

			mSkeletonAnimation.state.Event += HandleEvent;
			mSkeletonAnimation.state.Complete += HandleComplete;

			mHitDataDic = new Dictionary<string, AttakData>();

			foreach( AttakData data in AttackDataList )
			{
				mHitDataDic.Add( data.name, data );
			}
		}

		#region ANIMEVENT
		virtual protected void HandleEvent(Spine.AnimationState animState, int trackIndex, Spine.Event e)
		{
			switch (e.Data.name)
			{
				case DEActor.ANIM_EVENT_WAITATTACK:
					//WaitNextAttack ( e.Int, e.Float, e.String );
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
			var entry = animState.GetCurrent(trackIndex);
			print("HandleComplete: " + entry.Animation.name );

			if (entry.Animation.Name.IndexOf("Attack") == 0) AttackEnd();
		}
		#endregion

		virtual protected void WaitNextAttack( int i, float f, string s)
		{
			mIsReady = true;
			mWaitNextAttack = true;
			mWaitNextAttackEndTime = Time.time + waitAttackDuration;

			mOwner.currentAnimationTimeScale( 0f );

			StartCoroutine( "WaitNextAttackRoutine" );
		}

		IEnumerator WaitNextAttackRoutine()
		{
			while( Time.time < mWaitNextAttackEndTime ) yield return null;

			mWaitNextAttack = false;

			AttackEnd();
		}

		virtual protected void NextAttack()
		{
			StopCoroutine("WaitNextAttackRoutine");

			mWaitNextAttack = false;
			mOwner.currentAnimationTimeScale( 1f );

			//				if (mAttackIndex == 3)
			//				{
			//					RemoveTransition (TransitionGround_Fall);
			//				}
		}

		virtual protected void AttackEnd()
		{
			if( OnAttackEnd != null ) OnAttackEnd();

			mIsReady = true;
		}

		virtual public void UnEquip()
		{
			OnAttackEnd = null;

			StopCoroutine("WaitNextAttackRoutine");
		}

		virtual public void Equip()
        {
			mIsReady = true;

			if(mSetupAnim != null ) mSetupAnim.Apply( mSkeleton, 0, 1, false, null );
//			mOwner.PlayAnimation( idleAnim, true, 1);
        }

		virtual public bool IsReady()
		{
			return mIsReady;
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
		public Vector2 hitPrefabDirection = Vector2.right;
	}
}

