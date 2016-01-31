using UnityEngine;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;

namespace druggedcode.engine
{
    public class DECamera : Singleton<DECamera>
    {
        Transform mTr;

		Transform mSkyBox;
		Vector3 mDiffSkyBox;

        ProCamera2D mPro;
        ProCamera2DNumericBoundaries mBoundaries;

        override protected void Awake()
        {
            base.Awake();

            mTr = transform;

            mPro = GetComponent< ProCamera2D >();
            mBoundaries = GetComponent< ProCamera2DNumericBoundaries >();
            
            Reset();
        }
        
        public void AddTarget(Transform targetTransform, float offsetX = 0F, float offsetY = 0F )
        {
            mPro.AddCameraTarget( targetTransform ).TargetOffset = new Vector2( offsetX, offsetY );
        }

		public void RemoveAllTarget()
		{
			mPro.RemoveAllCameraTargets();
		}
        
        public void SetBound( BoundariesInfo info )
        {
            if( info == null )
            {
                mBoundaries.UseNumericBoundaries = false;
                return;
            }
            
            mBoundaries.UseTopBoundary = info.UseTopBoundary;
            mBoundaries.UseBottomBoundary = info.UseBottomBoundary;
            mBoundaries.UseRightBoundary = info.UseRightBoundary;
            mBoundaries.UseLeftBoundary = info.UseLeftBoundary;
            
            mBoundaries.TopBoundary = mBoundaries.TargetTopBoundary = info.TopLimit;
            mBoundaries.BottomBoundary = mBoundaries.TargetBottomBoundary = info.BottomLimit;
            mBoundaries.LeftBoundary = mBoundaries.TargetLeftBoundary = info.LeftLimit;
            mBoundaries.RightBoundary = mBoundaries.TargetRightBoundary = info.RightLimit;
            
            mBoundaries.UseNumericBoundaries = true;
        }

        public void SetSkybox( Transform skybox )
        {
			mSkyBox = skybox;
			if( mSkyBox == null ) return;

			mDiffSkyBox = new Vector3(0f,0f, mSkyBox.position.z - mTr.position.z);
//            skybox.SetParent(transform);
//            skybox.localPosition = new Vector3(0, 0, skybox.localPosition.z);
        }

		void LateUpdate()
		{
			if( mSkyBox != null )
			{
				mSkyBox.transform.position = mTr.position + mDiffSkyBox;
			}
		}
        
        public void CenterOnTargets()
        {
            mPro.Reset( true );
        }
        
        public void Reset()
        {
            mPro.enabled = false;
            transform.localPosition = new Vector3( 0f, 0f, -10f );
        }
        
        public void Run()
        {
            mPro.enabled = true;   
        }
    }
}
