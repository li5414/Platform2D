using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    public class ProCamera2DSpeedBasedZoom : BasePC2D
    {
        public static string ExtensionName = "Speed Based Zoom";

        [Tooltip("The speed at which the camera will reach it's max zoom out.")]
        public float SpeedForZoomOut = 5f;
        [Tooltip("Below this speed the camera zooms in. Above this speed the camera will start zooming out.")]
        public float SpeedForZoomIn = 2f;

        [Tooltip("Represents how smooth the zoom in of the camera should be. The lower the number the quickest the zoom is. A number too low might cause some stuttering.")]
        public float ZoomInSmoothness = 5f;
        [Tooltip("Represents how smooth the zoom out of the camera should be. The lower the number the quickest the zoom is. A number too low might cause some stuttering.")]
        public float ZoomOutSmoothness = 3f;

        [Tooltip("Represents the maximum amount the camera should zoom in when the camera speed is below SpeedForZoomIn")]
        public float MaxZoomInAmount = 2f;
        [Tooltip("Represents the maximum amount the camera should zoom out when the camera speed is equal to SpeedForZoomOut")]
        public float MaxZoomOutAmount = 2f;

        float _zoomVelocity;

        float _initialCamSize;
        float _previousCamSize;
        float _targetCamSize;
        float _targetCamSizeSmoothed;

        Vector3 _previousCameraPosition;

        [HideInInspector]
        public float CurrentVelocity;

        override protected void Awake()
        {
            base.Awake();

            if (ProCamera2D == null)
                return;

            _previousCameraPosition = ProCamera2D.CameraPosition;

            _initialCamSize = ProCamera2D.GameCameraSize;
            _targetCamSize = _initialCamSize;
            _targetCamSizeSmoothed = _targetCamSize;
        }

        override public void OnReset()
        {
            _zoomVelocity = 0;
            _previousCamSize = _initialCamSize;
            _targetCamSize = _initialCamSize;
            _targetCamSizeSmoothed = _initialCamSize;
            _previousCameraPosition = ProCamera2D.CameraPosition;
        }

        override protected void OnPreMoveUpdate(float deltaTime)
        {
            _targetCamSizeSmoothed = ProCamera2D.GameCameraSize;

            // If the camera is bounded, reset the easing
            if (_previousCamSize == ProCamera2D.ScreenSizeInWorldCoordinates.y)
            {
                _targetCamSize = ProCamera2D.GameCameraSize;
                _targetCamSizeSmoothed = _targetCamSize;
                _zoomVelocity = 0f;
            }

            // Calculate new target cam size
            UpdateTargetCamSize(deltaTime);

            // Detect if the camera size is bounded
            _previousCamSize = ProCamera2D.ScreenSizeInWorldCoordinates.y;

            // Update camera size if needed
            if (Mathf.Abs(ProCamera2D.GameCameraSize - _targetCamSize) > .0001f)
                UpdateScreenSize(_targetCamSize < _targetCamSizeSmoothed ? ZoomInSmoothness : ZoomOutSmoothness);
        }

        void UpdateTargetCamSize(float deltaTime)
        {
            // Get camera velocity
            CurrentVelocity = (_previousCameraPosition - ProCamera2D.CameraPosition).magnitude / deltaTime;
            _previousCameraPosition = ProCamera2D.CameraPosition;

            // Zoom out
            if (CurrentVelocity > SpeedForZoomIn)
            {
                var speedPercentage = (CurrentVelocity - SpeedForZoomIn) / (SpeedForZoomOut - SpeedForZoomIn);
                var newSize = _initialCamSize * (1 + (MaxZoomOutAmount - 1) * Mathf.Clamp01(speedPercentage));

                if (newSize > _targetCamSizeSmoothed)
                    _targetCamSize = newSize;
            }
            // Zoom in
            else
            {
                var speedPercentage = (1 - (CurrentVelocity / SpeedForZoomIn)).Remap(0f, 1f, .5f, 1f);
                var newSize = _initialCamSize / (MaxZoomInAmount * speedPercentage);

                if (newSize < _targetCamSizeSmoothed)
                    _targetCamSize = newSize;
            }
        }

        protected void UpdateScreenSize(float smoothness)
        {
            _targetCamSizeSmoothed = Mathf.SmoothDamp(_targetCamSizeSmoothed, _targetCamSize, ref _zoomVelocity, smoothness);

            ProCamera2D.UpdateScreenSize(_targetCamSizeSmoothed);
        }
    }
}