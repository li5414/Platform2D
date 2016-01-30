using UnityEngine;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    public class ProCamera2DGeometryBoundaries : BasePC2D
    {
        public static string ExtensionName = "Geometry Boundaries";

        [Tooltip("The layer that contains the (3d) colliders that define the boundaries for the camera")]
        public LayerMask BoundariesLayerMask;

        MoveInColliderBoundaries _cameraMoveInColliderBoundaries;

        override protected void Awake()
        {
            base.Awake();

            _cameraMoveInColliderBoundaries = new MoveInColliderBoundaries(ProCamera2D);
            _cameraMoveInColliderBoundaries.CameraTransform = _transform;
            _cameraMoveInColliderBoundaries.CameraSize = ProCamera2D.ScreenSizeInWorldCoordinates;
            _cameraMoveInColliderBoundaries.CameraCollisionMask = BoundariesLayerMask;
        }
        
        override protected void OnPostMoveUpdate(float deltaTime)
        {
            MoveInBoundaries();
        }

        void MoveInBoundaries()
        {
            _cameraMoveInColliderBoundaries.CameraSize = ProCamera2D.ScreenSizeInWorldCoordinates;

            // Remove the delta movement
            _transform.Translate(-ProCamera2D.DeltaMovement, Space.World);

            // Apply movement considering the collider boundaries
            _transform.Translate(_cameraMoveInColliderBoundaries.Move(ProCamera2D.DeltaMovement), Space.World);
        }
    }
}