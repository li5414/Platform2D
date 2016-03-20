using UnityEngine;
using System.Collections;

namespace druggedcode
{
	public class GizmoUtil
	{
		static public void DrawCollider( BoxCollider2D collider, Color color )
		{
			Vector3 origin = collider.transform.position;
			origin += new Vector3(collider.offset.x, collider.offset.y, 0.0f);

			Vector2 scale = collider.transform.lossyScale;

			float minX = -collider.size.x * 0.5f * scale.x;
			float maxX = collider.size.x * 0.5f * scale.x;
			float minY = -collider.size.y * 0.5f * scale.y;
			float maxY = collider.size.y * 0.5f * scale.y;

			Vector3 p1 = origin + new Vector3( minX, minY, 0f);
			Vector3 p2 = origin + new Vector3( maxX, minY, 0f);
			Vector3 p3 = origin + new Vector3( maxX, maxY, 0f);
			Vector3 p4 = origin + new Vector3( minX, maxY, 0f);

			Gizmos.color = color;
			Gizmos.DrawLine(p1, p2);
			Gizmos.DrawLine(p2, p3);
			Gizmos.DrawLine(p3, p4);
			Gizmos.DrawLine(p4, p1);
		}
	}
}
