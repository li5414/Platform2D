using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    public class ProCamera2DPositionAndSizeOverrider : BasePC2D
    {
        public Vector3 OverridePosition;
        public Vector3 OverridenPosition;

        public float OverrideSize;
        public float OverridenSize;

        public bool UseNumericBoundaries;
        public bool UseTopBoundary;
        public float TopBoundary = 10f;
        public bool UseBottomBoundary;
        public float BottomBoundary = -10f;
        public bool UseLeftBoundary;
        public float LeftBoundary = -10f;
        public bool UseRightBoundary;
        public float RightBoundary = 10f;
        
        public bool Enabled
        {
            get
            {
                return __enabled;
            }
            set
            {
                __enabled = value;
                if(!value)
                    OverrideSize = 0;
            }
        }
        bool __enabled;

        override protected void OnPostMoveUpdate(float deltaTime)
        {
            if(Enabled)
                Override();
        }

        void Override()
        {
            OverridenPosition = ProCamera2D.CameraPosition;
            ProCamera2D.CameraPosition = OverridePosition;

            if (UseNumericBoundaries)
                LimitPositionToNumericBoundaries();

            if (OverrideSize > 0)
            {
                OverridenSize = ProCamera2D.GameCameraSize;
                ProCamera2D.UpdateScreenSize(OverrideSize);

                if (UseNumericBoundaries)
                    LimitSizeToNumericBoundaries();
            }
        }

        void LimitPositionToNumericBoundaries()
        {
            // Check movement in the horizontal dir
            var isCameraPositionHorizontallyBounded = false;
            var isCameraPositionVerticallyBounded = false;
            var newPosH = Vector3H(_transform.localPosition);
            if (UseLeftBoundary && newPosH - ProCamera2D.ScreenSizeInWorldCoordinates.x / 2 < LeftBoundary)
            {
                newPosH = LeftBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.x / 2;
                isCameraPositionHorizontallyBounded = true;
            }
            else if (UseRightBoundary && newPosH + ProCamera2D.ScreenSizeInWorldCoordinates.x / 2 > RightBoundary)
            {
                newPosH = RightBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.x / 2;
                isCameraPositionHorizontallyBounded = true;
            }

            // Check movement in the vertical dir
            var newPosV = Vector3V(_transform.localPosition);
            if (UseBottomBoundary && newPosV - ProCamera2D.ScreenSizeInWorldCoordinates.y / 2 < BottomBoundary)
            {
                newPosV = BottomBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.y / 2;
                isCameraPositionVerticallyBounded = true;
            }
            else if (UseTopBoundary && newPosV + ProCamera2D.ScreenSizeInWorldCoordinates.y / 2 > TopBoundary)
            {
                newPosV = TopBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.y / 2;
                isCameraPositionVerticallyBounded = true;
            }

            // Set to the new position
            if (isCameraPositionHorizontallyBounded || isCameraPositionVerticallyBounded)
                ProCamera2D.CameraPosition = VectorHVD(newPosH, newPosV, ProCamera2D.CameraDepthPos);
        }

        void LimitSizeToNumericBoundaries()
        {
            // Set new size if outside boundaries
            var cameraBounds = new Vector2(RightBoundary - LeftBoundary, TopBoundary - BottomBoundary);
            if (UseRightBoundary && UseLeftBoundary && ProCamera2D.ScreenSizeInWorldCoordinates.x > cameraBounds.x)
            {
                ProCamera2D.UpdateScreenSize(cameraBounds.x / ProCamera2D.GameCamera.aspect / 2);
            }

            if (UseTopBoundary && UseBottomBoundary && ProCamera2D.ScreenSizeInWorldCoordinates.y > cameraBounds.y)
            {
                ProCamera2D.UpdateScreenSize(cameraBounds.y / 2);
            }
        }
    }
}