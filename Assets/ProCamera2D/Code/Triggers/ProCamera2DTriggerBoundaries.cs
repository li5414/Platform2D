using UnityEngine;
using System.Collections;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    public class ProCamera2DTriggerBoundaries : BaseTrigger
    {
        public static string TriggerName = "Boundaries Trigger";

        public ProCamera2DNumericBoundaries NumericBoundaries;
        public ProCamera2DPositionAndSizeOverrider PositionAndSizeOverrider;

        public bool AreBoundariesRelative = true;
        
        public bool UseTopBoundary = true;
        public float TopBoundary = 10;
        public bool UseBottomBoundary = true;
        public float BottomBoundary = -10;
        public bool UseLeftBoundary = true;
        public float LeftBoundary = -10;
        public bool UseRightBoundary = true;
        public float RightBoundary = 10;

        public float TransitionDuration = 1f;
        public EaseType TransitionEaseType;

        public bool ChangeZoom;
        public float TargetZoom = 1.5f;
        public float ZoomSmoothness = 1f;

        public bool IsCurrentTrigger { get { return NumericBoundaries.CurrentBoundariesTriggerID == _instanceID; } }

        public bool SetAsStartingBoundaries
        {
            set
            {
                if (value && !_setAsStartingBoundaries)
                {
                    var allBoundariesTriggers = FindObjectsOfType(typeof(ProCamera2DTriggerBoundaries));
                    foreach (ProCamera2DTriggerBoundaries trigger in allBoundariesTriggers)
                    {
                        trigger.SetAsStartingBoundaries = false;
                    }
                }

                _setAsStartingBoundaries = value;
            }

            get
            {
                return _setAsStartingBoundaries;
            }
        }

        /// <summary>Internal use only. Use the property SetAsStartingBoundaries</summary>
        public bool _setAsStartingBoundaries;

        float _initialCamSize;

        BoundariesAnimator _boundsAnim;

        float _targetTopBoundary;
        float _targetBottomBoundary;
        float _targetLeftBoundary;
        float _targetRightBoundary;

        void Start()
        {
            if (ProCamera2D == null)
                return;

            if (PositionAndSizeOverrider == null)
            {
                var positionAndSizeOverrider = ProCamera2D.GetComponent<ProCamera2DPositionAndSizeOverrider>();
                PositionAndSizeOverrider = positionAndSizeOverrider == null ? ProCamera2D.gameObject.AddComponent<ProCamera2DPositionAndSizeOverrider>() : positionAndSizeOverrider;
                PositionAndSizeOverrider.hideFlags = HideFlags.HideInInspector;
            }

            if (NumericBoundaries == null)
            {
                var numericBoundaries = FindObjectOfType<ProCamera2DNumericBoundaries>();
                NumericBoundaries = numericBoundaries == null ? ProCamera2D.gameObject.AddComponent<ProCamera2DNumericBoundaries>() : numericBoundaries;
            }

            _boundsAnim = new BoundariesAnimator(ProCamera2D, NumericBoundaries);
            _boundsAnim.OnTransitionStarted += () =>
            {
                if (NumericBoundaries.OnBoundariesTransitionStarted != null)
                    NumericBoundaries.OnBoundariesTransitionStarted();
            };

            _boundsAnim.OnTransitionFinished += () =>
            {
                if (NumericBoundaries.OnBoundariesTransitionFinished != null)
                    NumericBoundaries.OnBoundariesTransitionFinished();
            };

            GetTargetBoundaries();

            if (SetAsStartingBoundaries)
                SetBoundaries();

            _initialCamSize = ProCamera2D.GameCameraSize;
        }

        protected override void EnteredTrigger()
        {
            base.EnteredTrigger();

            if (NumericBoundaries.CurrentBoundariesTriggerID != _instanceID)
            {
                NumericBoundaries.CurrentBoundariesTriggerID = _instanceID;
                Transition();
            }
        }

        /// <summary>
        /// Sets the Numeric Boundaries extension to the values of this trigger
        /// </summary>
        public void SetBoundaries()
        {
            if (NumericBoundaries != null)
            {
                NumericBoundaries.CurrentBoundariesTriggerID = _instanceID;

                NumericBoundaries.UseLeftBoundary = UseLeftBoundary;
                if (UseLeftBoundary)
                    NumericBoundaries.LeftBoundary = NumericBoundaries.TargetLeftBoundary = _targetLeftBoundary;

                NumericBoundaries.UseRightBoundary = UseRightBoundary;
                if (UseRightBoundary)
                    NumericBoundaries.RightBoundary = NumericBoundaries.TargetRightBoundary = _targetRightBoundary;

                NumericBoundaries.UseTopBoundary = UseTopBoundary;
                if (UseTopBoundary)
                    NumericBoundaries.TopBoundary = NumericBoundaries.TargetTopBoundary = _targetTopBoundary;

                NumericBoundaries.UseBottomBoundary = UseBottomBoundary;
                if (UseBottomBoundary)
                    NumericBoundaries.BottomBoundary = NumericBoundaries.TargetBottomBoundary = _targetBottomBoundary;

                if (!UseTopBoundary && !UseBottomBoundary && !UseLeftBoundary && !UseRightBoundary)
                    NumericBoundaries.UseNumericBoundaries = false;
                else
                    NumericBoundaries.UseNumericBoundaries = true;
            }
        }

        void GetTargetBoundaries()
        {
            if (AreBoundariesRelative)
            {
                _targetTopBoundary = Vector3V(transform.position) + TopBoundary;
                _targetBottomBoundary = Vector3V(transform.position) + BottomBoundary;
                _targetLeftBoundary = Vector3H(transform.position) + LeftBoundary;
                _targetRightBoundary = Vector3H(transform.position) + RightBoundary;
            }
            else
            {
                _targetTopBoundary = TopBoundary;
                _targetBottomBoundary = BottomBoundary;
                _targetLeftBoundary = LeftBoundary;
                _targetRightBoundary = RightBoundary;
            }
        }

        void Transition()
        {
            if (!UseTopBoundary && !UseBottomBoundary && !UseLeftBoundary && !UseRightBoundary)
            {
                NumericBoundaries.UseNumericBoundaries = false;
                return;
            }

            // Avoid unnecessary transitions
            var skip = true;
            if ((UseTopBoundary && NumericBoundaries.TopBoundary != TopBoundary))
                skip = false;
            if ((UseBottomBoundary && NumericBoundaries.BottomBoundary != BottomBoundary))
                skip = false;
            if ((UseLeftBoundary && NumericBoundaries.LeftBoundary != LeftBoundary))
                skip = false;
            if ((UseRightBoundary && NumericBoundaries.RightBoundary != RightBoundary))
                skip = false;
            if (skip)
                return;

            NumericBoundaries.UseNumericBoundaries = true;
            
            GetTargetBoundaries();

            _boundsAnim.UseTopBoundary = UseTopBoundary;
            _boundsAnim.TopBoundary = _targetTopBoundary;
            _boundsAnim.UseBottomBoundary = UseBottomBoundary;
            _boundsAnim.BottomBoundary = _targetBottomBoundary;
            _boundsAnim.UseLeftBoundary = UseLeftBoundary;
            _boundsAnim.LeftBoundary = _targetLeftBoundary;
            _boundsAnim.UseRightBoundary = UseRightBoundary;
            _boundsAnim.RightBoundary = _targetRightBoundary;

            _boundsAnim.TransitionDuration = TransitionDuration;
            _boundsAnim.TransitionEaseType = TransitionEaseType;

            // Zoom
            if (ChangeZoom)
                ProCamera2D.UpdateScreenSize(_initialCamSize / TargetZoom, ZoomSmoothness, TransitionEaseType);
            
            // Start bounds animation
            _boundsAnim.Transition();
            
            // Move camera with the position overrider
            if (_boundsAnim.AnimsCount > 1)
            {
                if (NumericBoundaries.MoveCameraToTargetRoutine != null)
                    NumericBoundaries.StopCoroutine(NumericBoundaries.MoveCameraToTargetRoutine);

                NumericBoundaries.MoveCameraToTargetRoutine = NumericBoundaries.StartCoroutine(MoveCameraToTarget());
            }
        }

        IEnumerator MoveCameraToTarget()
        {
            var initialCamPosH = Vector3H(ProCamera2D.CameraPosition);
            var initialCamPosV = Vector3V(ProCamera2D.CameraPosition);

            PositionAndSizeOverrider.OverridePosition = VectorHVD(initialCamPosH, initialCamPosV, Vector3D(ProCamera2D.CameraPosition));
            PositionAndSizeOverrider.OverrideSize = 0;
            PositionAndSizeOverrider.UseNumericBoundaries = true;
            PositionAndSizeOverrider.Enabled = true;

            var t = 0f;
            while (t <= 1.0f)
            {
                t += ProCamera2D.DeltaTime / TransitionDuration;

                var currentCamPosH = Utils.EaseFromTo(initialCamPosH, ProCamera2D.CameraTargetPositionSmoothed.x, t, TransitionEaseType);
                var currentCamPosV = Utils.EaseFromTo(initialCamPosV, ProCamera2D.CameraTargetPositionSmoothed.y, t, TransitionEaseType);

                PositionAndSizeOverrider.UseTopBoundary = NumericBoundaries.UseTopBoundary;
                PositionAndSizeOverrider.TopBoundary = NumericBoundaries.TopBoundary;
                PositionAndSizeOverrider.UseBottomBoundary = NumericBoundaries.UseBottomBoundary;
                PositionAndSizeOverrider.BottomBoundary = NumericBoundaries.BottomBoundary;
                PositionAndSizeOverrider.UseLeftBoundary = NumericBoundaries.UseLeftBoundary;
                PositionAndSizeOverrider.LeftBoundary = NumericBoundaries.LeftBoundary;
                PositionAndSizeOverrider.UseRightBoundary = NumericBoundaries.UseRightBoundary;
                PositionAndSizeOverrider.RightBoundary = NumericBoundaries.RightBoundary;
                PositionAndSizeOverrider.OverridePosition = VectorHVD(currentCamPosH, currentCamPosV, Vector3D(ProCamera2D.CameraPosition));

                yield return ProCamera2D.GetYield();
            }

            PositionAndSizeOverrider.Enabled = false;
            NumericBoundaries.MoveCameraToTargetRoutine = null;
        }

        #if UNITY_EDITOR
        override protected void DrawGizmos()
        {
            base.DrawGizmos();

            float cameraDepthOffset = Vector3D(ProCamera2D.transform.localPosition) + Mathf.Abs(Vector3D(ProCamera2D.transform.localPosition)) * Vector3D(ProCamera2D.transform.forward);

            Gizmos.DrawIcon(VectorHVD(Vector3H(transform.position), Vector3V(transform.position), cameraDepthOffset - .01f * Mathf.Sign(Vector3D(ProCamera2D.transform.position))), "ProCamera2D/gizmo_icon_bg.png", false);

            if (UseTopBoundary)
            {
                Gizmos.DrawIcon(VectorHVD(Vector3H(transform.position), Vector3V(transform.position), cameraDepthOffset), "ProCamera2D/gizmo_icon_bound_top.png", false);
            }

            if (UseBottomBoundary)
            {
                Gizmos.DrawIcon(VectorHVD(Vector3H(transform.position), Vector3V(transform.position), cameraDepthOffset), "ProCamera2D/gizmo_icon_bound_bottom.png", false);
            }

            if (UseRightBoundary)
            {
                Gizmos.DrawIcon(VectorHVD(Vector3H(transform.position), Vector3V(transform.position), cameraDepthOffset), "ProCamera2D/gizmo_icon_bound_right.png", false);
            }

            if (UseLeftBoundary)
            {
                Gizmos.DrawIcon(VectorHVD(Vector3H(transform.position), Vector3V(transform.position), cameraDepthOffset), "ProCamera2D/gizmo_icon_bound_left.png", false);
            }

            if (SetAsStartingBoundaries)
                Gizmos.DrawIcon(VectorHVD(Vector3H(transform.position), Vector3V(transform.position), cameraDepthOffset), "ProCamera2D/gizmo_icon_start.png", false);
        }

        void OnDrawGizmosSelected()
        {
            if (ProCamera2D == null)
                return;
            
            float cameraDepthOffset = Vector3D(ProCamera2D.transform.localPosition) + Mathf.Abs(Vector3D(ProCamera2D.transform.localPosition)) * Vector3D(ProCamera2D.transform.forward);
            var cameraCenter = VectorHVD(Vector3H(transform.position), Vector3V(transform.position), cameraDepthOffset);
            var cameraDimensions = Utils.GetScreenSizeInWorldCoords(ProCamera2D.GetComponent<Camera>(), Mathf.Abs(Vector3D(ProCamera2D.transform.localPosition)));

            GetTargetBoundaries();

            Gizmos.color = EditorPrefsX.GetColor(PrefsData.BoundariesTriggerColorKey, PrefsData.BoundariesTriggerColorValue);
            if (UseTopBoundary)
            {
                Gizmos.DrawRay(VectorHVD(Vector3H(transform.position) - cameraDimensions.x / 2, _targetTopBoundary, cameraDepthOffset), ProCamera2D.transform.right * cameraDimensions.x);
                Utils.DrawArrowForGizmo(cameraCenter, VectorHV(0, _targetTopBoundary - Vector3V(transform.position)));
            }

            if (UseBottomBoundary)
            {
                Gizmos.DrawRay(VectorHVD(Vector3H(transform.position) - cameraDimensions.x / 2, _targetBottomBoundary, cameraDepthOffset), ProCamera2D.transform.right * cameraDimensions.x);
                Utils.DrawArrowForGizmo(cameraCenter, VectorHV(0, _targetBottomBoundary - Vector3V(transform.position)));
            }

            if (UseRightBoundary)
            {
                Gizmos.DrawRay(VectorHVD(_targetRightBoundary, Vector3V(transform.position) - cameraDimensions.y / 2, cameraDepthOffset), ProCamera2D.transform.up * cameraDimensions.y);
                Utils.DrawArrowForGizmo(cameraCenter, VectorHV(_targetRightBoundary - Vector3H(transform.position), 0));
            }

            if (UseLeftBoundary)
            {
                Gizmos.DrawRay(VectorHVD(_targetLeftBoundary, Vector3V(transform.position) - cameraDimensions.y / 2, cameraDepthOffset), ProCamera2D.transform.up * cameraDimensions.y);
                Utils.DrawArrowForGizmo(cameraCenter, VectorHV(_targetLeftBoundary - Vector3H(transform.position), 0));
            }
        }
        #endif
    }
}