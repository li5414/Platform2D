using UnityEngine;
using System;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    public class ProCamera2DNumericBoundaries : BasePC2D
    {
        public static string ExtensionName = "Numeric Boundaries";

        public Action OnBoundariesTransitionStarted;
        public Action OnBoundariesTransitionFinished;

        public bool UseNumericBoundaries;
        public bool UseTopBoundary;
        public float TopBoundary = 10f;
        public float TargetTopBoundary;

        public bool UseBottomBoundary;
        public float BottomBoundary = -10f;
        public float TargetBottomBoundary;

        public bool UseLeftBoundary;
        public float LeftBoundary = -10f;
        public float TargetLeftBoundary;

        public bool UseRightBoundary;
        public float RightBoundary = 10f;
        public float TargetRightBoundary;

        public bool IsCameraSizeBounded;
        public bool IsCameraPositionHorizontallyBounded;
        public bool IsCameraPositionVerticallyBounded;

        public Coroutine BoundariesAnimRoutine;
        public Coroutine TopBoundaryAnimRoutine;
        public Coroutine BottomBoundaryAnimRoutine;
        public Coroutine LeftBoundaryAnimRoutine;
        public Coroutine RightBoundaryAnimRoutine;
        public int CurrentBoundariesTriggerID;

        public Coroutine MoveCameraToTargetRoutine;

        public bool UseElasticBoundaries;

        [Range(0, 10f)]
        public float HorizontalElasticityDuration = .5f;
        public float HorizontalElasticitySize = 2f;

        [Range(0, 10f)]
        public float VerticalElasticityDuration = .5f;
        public float VerticalElasticitySize = 2f;

        public EaseType ElasticityEaseType = EaseType.EaseInOut;

        float _verticallyBoundedDuration;
        float _horizontallyBoundedDuration;

        
        override protected void OnPostMoveUpdate(float deltaTime)
        {
            LimitSizeAndPositionToNumericBoundaries();
        }

        void LimitSizeAndPositionToNumericBoundaries()
        {
            if (!UseNumericBoundaries)
                return;

            // Set new size if outside boundaries
            IsCameraSizeBounded = false;
            var cameraBounds = new Vector2(RightBoundary - LeftBoundary, TopBoundary - BottomBoundary);
            if (UseRightBoundary && UseLeftBoundary && ProCamera2D.ScreenSizeInWorldCoordinates.x > cameraBounds.x)
            {
                ProCamera2D.UpdateScreenSize(cameraBounds.x / ProCamera2D.GameCamera.aspect / 2);
                IsCameraSizeBounded = true;
            }

            if (UseTopBoundary && UseBottomBoundary && ProCamera2D.ScreenSizeInWorldCoordinates.y > cameraBounds.y)
            {
                ProCamera2D.UpdateScreenSize(cameraBounds.y / 2);
                IsCameraSizeBounded = true;
            }

            // Check movement in the horizontal dir
            IsCameraPositionHorizontallyBounded = false;
            ProCamera2D.IsCameraPositionLeftBounded = false;
            ProCamera2D.IsCameraPositionRightBounded = false;
            IsCameraPositionVerticallyBounded = false;
            ProCamera2D.IsCameraPositionTopBounded = false;
            ProCamera2D.IsCameraPositionBottomBounded = false;
            var newPosH = Vector3H(_transform.localPosition);
            if (UseLeftBoundary && newPosH - ProCamera2D.ScreenSizeInWorldCoordinates.x / 2 < LeftBoundary)
            {
                newPosH = LeftBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.x / 2;
                IsCameraPositionHorizontallyBounded = true;
                ProCamera2D.IsCameraPositionLeftBounded = true;
            }
            else if (UseRightBoundary && newPosH + ProCamera2D.ScreenSizeInWorldCoordinates.x / 2 > RightBoundary)
            {
                newPosH = RightBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.x / 2;
                IsCameraPositionHorizontallyBounded = true;
                ProCamera2D.IsCameraPositionRightBounded = true;
            }

            // Check movement in the vertical dir
            var newPosV = Vector3V(_transform.localPosition);
            if (UseBottomBoundary && newPosV - ProCamera2D.ScreenSizeInWorldCoordinates.y / 2 < BottomBoundary)
            {
                newPosV = BottomBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.y / 2;
                IsCameraPositionVerticallyBounded = true;
                ProCamera2D.IsCameraPositionBottomBounded = true;
            }
            else if (UseTopBoundary && newPosV + ProCamera2D.ScreenSizeInWorldCoordinates.y / 2 > TopBoundary)
            {
                newPosV = TopBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.y / 2;
                IsCameraPositionVerticallyBounded = true;
                ProCamera2D.IsCameraPositionTopBounded = true;
            }

            // Elastic Boundaries
            if (UseElasticBoundaries)
            {
                // Horizontal
                if (IsCameraPositionHorizontallyBounded)
                {
                    _horizontallyBoundedDuration = Mathf.Min(HorizontalElasticityDuration, _horizontallyBoundedDuration + Time.deltaTime);

                    var perc = 1f;
                    if (HorizontalElasticityDuration > 0)
                        perc = _horizontallyBoundedDuration / HorizontalElasticityDuration;

                    if (ProCamera2D.IsCameraPositionLeftBounded)
                    {
                        newPosH = Utils.EaseFromTo(
                            Mathf.Max(
                                LeftBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.x / 2 - HorizontalElasticitySize, 
                                Vector3H(_transform.localPosition)), 
                            LeftBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.x / 2, 
                            perc,
                            ElasticityEaseType);
                    }
                    else
                    {
                        newPosH = Utils.EaseFromTo(
                            Mathf.Min(
                                RightBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.x / 2 + HorizontalElasticitySize, 
                                Vector3H(_transform.localPosition)), 
                            RightBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.x / 2, 
                            perc,
                            ElasticityEaseType);
                    }
                }
                else
                {
                    _horizontallyBoundedDuration = Mathf.Max(0, _horizontallyBoundedDuration - Time.deltaTime);
                }

                // Vertical
                if (IsCameraPositionVerticallyBounded)
                {
                    _verticallyBoundedDuration = Mathf.Min(VerticalElasticityDuration, _verticallyBoundedDuration + Time.deltaTime);

                    var perc = 1f;
                    if (VerticalElasticityDuration > 0)
                        perc = _verticallyBoundedDuration / VerticalElasticityDuration;

                    if (ProCamera2D.IsCameraPositionBottomBounded)
                    {
                        newPosV = Utils.EaseFromTo(
                            Mathf.Max(
                                BottomBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.y / 2 - VerticalElasticitySize, 
                                Vector3V(_transform.localPosition)), 
                            BottomBoundary + ProCamera2D.ScreenSizeInWorldCoordinates.y / 2, 
                            perc,
                            ElasticityEaseType);
                    }
                    else
                    {
                        newPosV = Utils.EaseFromTo(
                            Mathf.Min(
                                TopBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.y / 2 + VerticalElasticitySize, 
                                Vector3V(_transform.localPosition)), 
                            TopBoundary - ProCamera2D.ScreenSizeInWorldCoordinates.y / 2, 
                            perc,
                            ElasticityEaseType);
                    }
                }
                else
                {
                    _verticallyBoundedDuration = Mathf.Max(0, _verticallyBoundedDuration - Time.deltaTime);
                }
            }

            // Set to the new position
            if (IsCameraPositionHorizontallyBounded || IsCameraPositionVerticallyBounded)
                ProCamera2D.CameraPosition = VectorHVD(newPosH, newPosV, Vector3D(_transform.localPosition));
        }

        #if UNITY_EDITOR
        override protected void DrawGizmos()
        {
            base.DrawGizmos();

            var gameCamera = ProCamera2D.GetComponent<Camera>();
            var cameraDimensions = gameCamera.orthographic ? Utils.GetScreenSizeInWorldCoords(gameCamera) : Utils.GetScreenSizeInWorldCoords(gameCamera, Mathf.Abs(Vector3D(transform.localPosition)));
            float cameraDepthOffset = Vector3D(ProCamera2D.transform.localPosition) + Mathf.Abs(Vector3D(transform.localPosition)) * Vector3D(ProCamera2D.transform.forward);

            Gizmos.color = EditorPrefsX.GetColor(PrefsData.NumericBoundariesColorKey, PrefsData.NumericBoundariesColorValue);

            if (UseNumericBoundaries)
            {
                if (UseTopBoundary)
                    Gizmos.DrawRay(VectorHVD(Vector3H(transform.localPosition) - cameraDimensions.x / 2, TopBoundary, cameraDepthOffset), transform.right * cameraDimensions.x);

                if (UseBottomBoundary)
                    Gizmos.DrawRay(VectorHVD(Vector3H(transform.localPosition) - cameraDimensions.x / 2, BottomBoundary, cameraDepthOffset), transform.right * cameraDimensions.x);

                if (UseRightBoundary)
                    Gizmos.DrawRay(VectorHVD(RightBoundary, Vector3V(transform.localPosition) - cameraDimensions.y / 2, cameraDepthOffset), transform.up * cameraDimensions.y);

                if (UseLeftBoundary)
                    Gizmos.DrawRay(VectorHVD(LeftBoundary, Vector3V(transform.localPosition) - cameraDimensions.y / 2, cameraDepthOffset), transform.up * cameraDimensions.y);
            }
        }
        #endif
    }
}