
using UnityEngine;
using UnityEditor;

namespace druggedcode.engine
{
    public class DruggedEngineEditor : Editor
    {

        public static bool UseDrawCollider = false;
        [MenuItem("DEngine/DrawCollider")]
        static void ClickDrawCollider()
        {
            UseDrawCollider = !UseDrawCollider;
            Debug.Log("ColliderGizomoOn:" + UseDrawCollider);
            Menu.SetChecked("DEngine/DrawCollider", UseDrawCollider);
        }

        public static bool UseDrawOriginRect = false;
        [MenuItem("DEngine/DrawOriginPos")]
        static void ClickDrawOriginRect()
        {
            UseDrawOriginRect = !UseDrawOriginRect;
            Menu.SetChecked("DEngine/DrawOriginPos", UseDrawOriginRect);
        }
        
        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Active)]
        static void ColliderGizmoDraw(GameObject go, GizmoType gt)
        {
            return;
            /*
            if (UseDrawCollider == false && UseDrawOriginRect == false)
            {
                return;
            }

            if (UseDrawOriginRect)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(go.transform.position, new Vector3(0.1f, 0.1f, 0.1f));
            }

            if (UseDrawCollider)
            {
                if (go.GetComponent<OneWayTrigger>() != null)
                {
                    DrawCollider(go, Color.yellow);
                }
                
                // if (go.GetComponent<TempPlatform>() != null)
                // {
                //     DrawCollider(go, Color.green);
                // }
                // else if (go.GetComponent<Ladder>() != null)
                // {
                //     DrawCollider(go, Color.red);
                // }
                // else if (go.GetComponent<PhysicsSpace>() != null)
                // {
                //     DrawCollider(go, Color.yellow);
                // }
            }
            */
        }

        static void DrawCollider(GameObject go, Color color)
        {
            Gizmos.color = color;

            BoxCollider2D[] bcs = go.GetComponents<BoxCollider2D>();
            Vector2 scale = go.transform.lossyScale;
            
            foreach (BoxCollider2D bc in bcs)
            {
                Vector3 pos = go.transform.position + new Vector3(bc.offset.x, bc.offset.y, 0.0f);
                float minX = -bc.size.x * 0.5f * scale.x;
                float maxX = bc.size.x * 0.5f * scale.x;
                float minY = -bc.size.y * 0.5f * scale.y;
                float maxY = bc.size.y * 0.5f * scale.y;
                
                Vector3 p1 = pos + new Vector3( minX, minY, 0f);
                Vector3 p2 = pos + new Vector3( maxX, minY, 0f);
                Vector3 p3 = pos + new Vector3( maxX, maxY, 0f);
                Vector3 p4 = pos + new Vector3( minX, maxY, 0f);

                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p2, p3);
                Gizmos.DrawLine(p3, p4);
                Gizmos.DrawLine(p4, p1);
            }

            CircleCollider2D[] bcs2 = go.GetComponents<CircleCollider2D>();
            foreach (CircleCollider2D bc in bcs2)
            {
                Vector3 pos = new Vector3(bc.offset.x, bc.offset.y, 0.0f);

                int cmax = 16;
                for (int i = 0; i < cmax; i++)
                {
                    Vector3 p1 = go.transform.position + pos + Quaternion.Euler(0.0f, 0.0f, 360.0f / cmax * (i + 0)) * new Vector3(bc.radius, 0.0f, 0.0f);
                    Vector3 p2 = go.transform.position + pos + Quaternion.Euler(0.0f, 0.0f, 360.0f / cmax * (i + 1)) * new Vector3(bc.radius, 0.0f, 0.0f);
                    Gizmos.DrawLine(p1, p2);
                }
            }

            EdgeCollider2D[] bcs3 = go.GetComponents<EdgeCollider2D>();
            foreach (EdgeCollider2D bc in bcs3)
            {
                for (int i = 0; i < bc.pointCount - 1; i++)
                {
                    Vector3 p1 = go.transform.position + new Vector3(bc.points[i + 0].x, bc.points[i + 0].y);
                    Vector3 p2 = go.transform.position + new Vector3(bc.points[i + 1].x, bc.points[i + 1].y);
                    Gizmos.DrawLine(p1, p2);
                }
            }

            PolygonCollider2D[] bcs4 = go.GetComponents<PolygonCollider2D>();
            foreach (PolygonCollider2D bc in bcs4)
            {
                for (int i = 0; i < bc.pathCount - 1; i++)
                {
                    Vector3 p1 = go.transform.position + new Vector3(bc.points[i + 0].x, bc.points[i + 0].y);
                    Vector3 p2 = go.transform.position + new Vector3(bc.points[i + 1].x, bc.points[i + 1].y);
                    Gizmos.DrawLine(p1, p2);
                }
            }
        }
    }

}
