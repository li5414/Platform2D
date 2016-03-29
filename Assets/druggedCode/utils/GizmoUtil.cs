using UnityEngine;
using System.Collections;

namespace druggedcode
{
	public class GizmoUtil
	{
		static public void DrawCollider( Collider2D coll, Color color )
		{
			if( coll is BoxCollider2D )
			{
				DrawBox( coll as BoxCollider2D, color );
			}
		}

		static private void DrawBox( BoxCollider2D coll, Color color )
		{
			Bounds b = coll.bounds;
			Transform c = coll.transform;
			float z = c.position.z;

			Vector3 p1 = new Vector3(b.min.x, b.max.y, z );
			Vector3 p2 = new Vector3(b.max.x, b.max.y, z );
			Vector3 p3 = new Vector3(b.max.x, b.min.y, z );
			Vector3 p4 = new Vector3(b.min.x, b.min.y, z );

			/*
			//48,2
			Vector3 origin = coll.transform.position;
			Debug.Log("Origin: " + origin );
			origin += new Vector3(coll.offset.x, coll.offset.y, 0.0f);
			Debug.Log("after Origin: " + origin );
			Debug.Log("size : " + coll.size );

			//offset 0.5, -1.05

			Vector2 scale = coll.transform.lossyScale;

			float minX = -coll.size.x * 0.5f * scale.x;
			float maxX = coll.size.x * 0.5f * scale.x;
			float minY = -coll.size.y * 0.5f * scale.y;
			float maxY = coll.size.y * 0.5f * scale.y;

			Vector3 p1 = origin + new Vector3( minX, minY, 0f);
			Vector3 p2 = origin + new Vector3( maxX, minY, 0f);
			Vector3 p3 = origin + new Vector3( maxX, maxY, 0f);
			Vector3 p4 = origin + new Vector3( minX, maxY, 0f);

			Debug.Log( "minX:" + minX + ", p1: " + p1 ); 
			Debug.Log("--------------------------------");
			*/

			Gizmos.color = color;
			Gizmos.DrawLine(p1, p2);
			Gizmos.DrawLine(p2, p3);
			Gizmos.DrawLine(p3, p4);
			Gizmos.DrawLine(p4, p1);
		}
	}
}
