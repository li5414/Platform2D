using UnityEngine;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;

namespace druggedcode.engine
{
	public class DECamera : MonoBehaviour
    {
        Transform mTr;

		Transform mSkyBox;
		Vector3 mDiffSkyBox;

        ProCamera2D mPro;
        ProCamera2DNumericBoundaries mBoundaries;
		ProCamera2DForwardFocus mFocus;

        void Awake()
        {
            mTr = transform;

            mPro = GetComponent< ProCamera2D >();
            mBoundaries = GetComponent< ProCamera2DNumericBoundaries >();
			mFocus = GetComponent< ProCamera2DForwardFocus>();
            
            Reset();
        }
        
        public void AddPlayer( DEPlayer player )
        {
			mPro.AddCameraTarget( player.transform ).TargetOffset = new Vector2( 0f, 2f );
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
			mPro.RemoveAllCameraTargets();
			mPro.Reset( false );
            mPro.enabled = false;
			mFocus.enabled = false;
        }
        
        public void Run()
        {
            mPro.enabled = true;
			mFocus.enabled = false;
			CenterOnTargets();
        }
    }
}
