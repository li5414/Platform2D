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
		public SkeletonRenderer skeletonRenderer;

		// setup anim 은 애니메이션이 아니라 1frame 짜리 셋팅. slot 의 attachment 설정
        [Header("Anim")]
		[SpineAnimation(startsWith: "Setup",dataField: "skeletonRenderer")]
        public string setupAnim;
		[SpineAnimation(startsWith: "Attack",dataField: "skeletonRenderer")]
        public string idleAnim;
		[SpineAnimation(startsWith: "Attack",dataField: "skeletonRenderer")]
        public string attackGroundAnim;
		[SpineAnimation(startsWith: "Attack",dataField: "skeletonRenderer")]
		public string attackAirAnim;

		//dts 외부 데이터로 설정
		[Header("Property")]
		public new string name;
		public int weaponDamage = 1;
		public float waitAttackDuration = 0.5f;
		public List<AttackData> AttackDataList;

		#endregion

		protected DEActor mOwner;
		protected SkeletonAnimation mSkeletonAnimation;
		protected Spine.Animation mAnimSetup;
		protected Spine.Animation mAnimIdle;
		protected Spine.Animation mAnimGround;
		protected Spine.Animation mAnimAir;
		protected Skeleton mSkeleton;

		protected bool mWaitNextAttack;
		protected float mWaitNextAttackEndTime;
		protected Dictionary<string, AttackData> mHitDataDic;
		protected bool mIsReady;
		protected LayerMask mTargetLayer;

		protected int mAttackIndex;

		public event UnityAction OnAttackEnd;

		#region Initialize
		void Awake()
		{
			mHitDataDic = new Dictionary<string, AttackData>();

			foreach( AttackData data in AttackDataList )
			{
				mHitDataDic.Add( data.name, data );
			}
		}

		/// <summary>
		/// Init from DECharacter's Start
		/// </summary>
		virtual public void Init( DEActor owner, SkeletonAnimation sAnimation )
        {
			mOwner = owner;
			mSkeletonAnimation = sAnimation;
			mSkeleton = mSkeletonAnimation.Skeleton;

			mAnimSetup = mSkeleton.Data.FindAnimation(setupAnim);
			mAnimIdle = mSkeleton.Data.FindAnimation(idleAnim);
			mAnimGround = mSkeleton.Data.FindAnimation(attackGroundAnim);
			mAnimAir = mSkeleton.Data.FindAnimation(attackAirAnim);

			InitTargetLayer();
		}

		virtual protected void InitTargetLayer()
		{
			if( mOwner is DEPlayer ) mTargetLayer = DruggedEngine.MASK_ENEMY;
			else if( mOwner is DEEnemy ) mTargetLayer = DruggedEngine.MASK_PLAYER;
		}

		virtual public void Equip()
		{
			mIsReady = true;
			mAttackIndex = 0;

			if(mAnimSetup != null ) mAnimSetup.Apply( mSkeleton, 0, 1, false, null );
			if(mAnimIdle != null ) mOwner.PlayAnimation( mAnimIdle );

			mSkeletonAnimation.state.Event += HandleEvent;
			mSkeletonAnimation.state.Complete += HandleComplete;
		}

		virtual public void UnEquip()
		{
			OnAttackEnd = null;

			StopCoroutine("WaitNextAttackRoutine");

			mSkeletonAnimation.state.Event -= HandleEvent;
			mSkeletonAnimation.state.Complete -= HandleComplete;
		}
		#endregion

		#region AnimationEvent
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
			var entry = animState.GetCurrent(trackIndex);
			if (entry.Animation.Name.IndexOf("Attack") == 0) AttackEnd();
		}
		#endregion

		#region Attack
		public void Attack()
		{
			if( mOwner.Controller.State.IsGrounded )
			{
				AttackGround();
			}
			else
			{
				AttackAir();
			}
		}

		virtual protected void AttackGround()
		{
			Debug.LogWarning("Not implemented!");
		}

		virtual protected void AttackAir()
		{
			Debug.LogWarning("Not implemented!");
		}

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

			mAttackIndex = 0;
			mIsReady = true;
		}
		#endregion
        
		#region get set
		virtual public bool IsReady
		{
			get
			{
				if( mIsReady == false ) return false;

				if( mOwner.Controller.State.IsGrounded && mAnimGround == null )
				{
					return false;
				}
				else if( mOwner.Controller.State.IsGrounded == false && mAnimAir == null )
				{
					return false;
				}

				return true;
			}
			protected set { mIsReady = value; }
		}

        //공격 반동
        virtual public Vector2 Rebound
        {
			get{ return Vector2.zero; }
        }
		#endregion
    }

	[System.Serializable]
	public class AttackData
	{
		public string name;
		public float damageRatio = 1f;
		public Vector2 force;

		public GameObject hitPrefab;
		public Vector2 hitPrefabDirection = Vector2.right;
	}
}

