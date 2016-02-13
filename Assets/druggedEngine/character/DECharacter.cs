using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace druggedcode.engine
{
	public class DECharacter : MonoBehaviour
	{
		public enum Facing
		{
			RIGHT,
			LEFT
		}

		//----------------------------------------------------------------------------------------------------------
		// Inspector
		//----------------------------------------------------------------------------------------------------------

		public AnimationType animType;
		public float AccelOnGround = 10f;
		public float AccelOnAir = 3f;

		[Header("Speed")]
		public float CrouchSpeed = 1f;
		public float WalkSpeed = 4f;
		public float RunSpeed = 10f;
		public float LadderSpeed = 2f;

		[Header("Jump")]
		public float JumpHeight = 3f;
		public float JumpHeightOnAir = 2f;
		public int jumpMax = 3;

		//----------------------------------------------------------------------------------------------------------
		// event
		//----------------------------------------------------------------------------------------------------------
		public UnityAction<DECharacter> OnUpdateInput;

		//----------------------------------------------------------------------------------------------------------
		// public
		//----------------------------------------------------------------------------------------------------------
		public Ladder currentLadder{ get;set; }
		public DEController controller{get; private set;}
		public int jumpCount{get;protected set;}
		public float horizontalAxis { get; set;}
		public float verticalAxis { get; set; }
		public float CurrentVX { get; set; }

		//----------------------------------------------------------------------------------------------------------
		// private,protected
		//----------------------------------------------------------------------------------------------------------

		Transform mTr;
		protected int mJumpCount;
		protected float jumStartTime;
		protected float jumpElapsedTime{ get{ return Time.time - jumStartTime; }} 

		//캐릭터의 중력을 활성화 하거나 비활성화 할때 이전 중력값을 기억하기 위한 용도
		protected float _originalGravity;
		protected Facing mFacaing;

		SkeletonAnimation mSkeletonAnimation;
		protected SkeletonGhost mGhost;

		bool mMoveLocked;

		public void DeActive()
		{
			//origin was player
//			if( mIsActive == false ) return;
//
//			Stop();
//			Controllable( false );
//
//			_controller.enabled = false;
//
//			mIsActive = false;
		}

		public void Active()
		{
			//origin was player
//			if( mIsActive ) return;
//
//			Controllable( true );
//
//			_controller.enabled = true;
//
//			mIsActive = true;
		}
	}
}


