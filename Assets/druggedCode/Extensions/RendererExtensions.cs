using UnityEngine;
using System.Collections;

namespace druggedcode.extensions
{
    public static class RendererExtensions
    {
        public static bool isVisibleFrom( this Renderer renderer, Camera camera )
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes( camera );
            return GeometryUtility.TestPlanesAABB( planes, renderer.bounds );
        }


        static public Vector2 GetDimensionInPX(GameObject obj) {
            SpriteRenderer spr = obj.GetComponent<SpriteRenderer>();

            if( spr == null ) return Vector2.zero;

            Sprite sp = spr.sprite;

            if( sp == null ) return Vector2.zero;

            Vector2 tmpDimension;


            tmpDimension.x = sp.bounds.size.x * obj.transform.localScale.x;
            tmpDimension.y = sp.bounds.size.y * obj.transform.localScale.y;
            
            return tmpDimension;
        }
    }
}

