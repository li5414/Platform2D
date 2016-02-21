using UnityEngine;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;

public class BoundariesInfo : BasePC2D
{
    public bool AreBoundariesRelative;

    public bool UseTopBoundary = true;
    [SerializeField]
    float TopBoundary = 10;

    public bool UseBottomBoundary = true;
    [SerializeField]
    float BottomBoundary = -10;

    public bool UseLeftBoundary = true;
    [SerializeField]
    float LeftBoundary = -10;

    public bool UseRightBoundary = true;
    [SerializeField]
    float RightBoundary = 10;

    public bool ChangeZoom;
    public float TargetZoom = 1.5f;

    public float TopLimit
    {
        get
        {
            if (AreBoundariesRelative) return Vector3V(transform.position) + TopBoundary;
            else return TopBoundary;
        }
    }

    public float BottomLimit
    {
        get
        {
            if (AreBoundariesRelative) return Vector3V(transform.position) + BottomBoundary;
            else return BottomBoundary;
        }
    }

    public float LeftLimit
    {
        get
        {
            if (AreBoundariesRelative) return Vector3H(transform.position) + LeftBoundary;
            else return LeftBoundary;
        }
    }

    public float RightLimit
    {
        get
        {
            if (AreBoundariesRelative) return Vector3H(transform.position) + RightBoundary;
            else return RightBoundary;
        }
    }

    public void ClampBound()
    {
        if (LeftBoundary > RightBoundary) LeftBoundary = RightBoundary;
        if (RightBoundary < LeftBoundary) RightBoundary = LeftBoundary;
        if (BottomBoundary > TopBoundary) BottomBoundary = TopBoundary;
        if (TopBoundary < BottomBoundary) TopBoundary = BottomBoundary;
    }

    #if UNITY_EDITOR
    override protected void DrawGizmos()
    {
        base.DrawGizmos();

        float cameraDepthOffset = Vector3D(ProCamera2D.transform.localPosition) + Mathf.Abs(Vector3D(ProCamera2D.transform.localPosition)) * Vector3D(ProCamera2D.transform.forward);

        Gizmos.DrawIcon(VectorHVD(Vector3H(transform.position), Vector3V(transform.position), cameraDepthOffset - .01f * Mathf.Sign(Vector3D(ProCamera2D.transform.position))), "ProCamera2D/gizmo_icon_bg.png", false);

        if (UseTopBoundary)
        {
            Gizmos.DrawIcon(VectorHVD(Vector3H(transform.position), Vector3V(transform.position) + 1, cameraDepthOffset), "ProCamera2D/gizmo_icon_bound_top.png", false);
        }

        if (UseBottomBoundary)
        {
            Gizmos.DrawIcon(VectorHVD(Vector3H(transform.position), Vector3V(transform.position)+ 1, cameraDepthOffset), "ProCamera2D/gizmo_icon_bound_bottom.png", false);
        }

        if (UseRightBoundary)
        {
            Gizmos.DrawIcon(VectorHVD(Vector3H(transform.position), Vector3V(transform.position)+ 1, cameraDepthOffset), "ProCamera2D/gizmo_icon_bound_right.png", false);
        }

        if (UseLeftBoundary)
        {
            Gizmos.DrawIcon(VectorHVD(Vector3H(transform.position), Vector3V(transform.position)+ 1, cameraDepthOffset), "ProCamera2D/gizmo_icon_bound_left.png", false);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (ProCamera2D == null)
            return;

        float cameraDepthOffset = Vector3D(ProCamera2D.transform.localPosition) + Mathf.Abs(Vector3D(ProCamera2D.transform.localPosition)) * Vector3D(ProCamera2D.transform.forward);
        var cameraCenter = VectorHVD(Vector3H(transform.position), Vector3V(transform.position), cameraDepthOffset);
        var cameraDimensions = Utils.GetScreenSizeInWorldCoords(ProCamera2D.GetComponent<Camera>(), Mathf.Abs(Vector3D(ProCamera2D.transform.localPosition)));

        Gizmos.color = EditorPrefsX.GetColor(PrefsData.BoundariesTriggerColorKey, PrefsData.BoundariesTriggerColorValue);
        if (UseTopBoundary)
        {
            Gizmos.DrawRay(VectorHVD(Vector3H(transform.position) - cameraDimensions.x / 2, TopLimit, cameraDepthOffset), ProCamera2D.transform.right * cameraDimensions.x);
            Utils.DrawArrowForGizmo(cameraCenter, VectorHV(0, TopLimit - Vector3V(transform.position)));
        }

        if (UseBottomBoundary)
        {
            Gizmos.DrawRay(VectorHVD(Vector3H(transform.position) - cameraDimensions.x / 2, BottomLimit, cameraDepthOffset), ProCamera2D.transform.right * cameraDimensions.x);
            Utils.DrawArrowForGizmo(cameraCenter, VectorHV(0, BottomLimit - Vector3V(transform.position)));
        }

        if (UseRightBoundary)
        {
            Gizmos.DrawRay(VectorHVD(RightLimit, Vector3V(transform.position) - cameraDimensions.y / 2, cameraDepthOffset), ProCamera2D.transform.up * cameraDimensions.y);
            Utils.DrawArrowForGizmo(cameraCenter, VectorHV(RightLimit - Vector3H(transform.position), 0));
        }

        if (UseLeftBoundary)
        {
            Gizmos.DrawRay(VectorHVD(LeftLimit, Vector3V(transform.position) - cameraDimensions.y / 2, cameraDepthOffset), ProCamera2D.transform.up * cameraDimensions.y);
            Utils.DrawArrowForGizmo(cameraCenter, VectorHV(LeftLimit - Vector3H(transform.position), 0));
        }
    }
    #endif
}
