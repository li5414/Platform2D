using UnityEngine;
using System.Collections;

namespace druggedcode
{
    public class PhysicsUtil
    {
        /// <summary>
        /// 레이를 화면에 표시하면서 Raycast 실행
        /// </summary>
        /// <returns>The cast.</returns>
        public static RaycastHit2D DrawRayCast(
            Vector2 rayOriginPoint,
            Vector2 rayDirection,
            float rayDistance,
            LayerMask mask,
            Color color )
        {           
            if( Application.isEditor ) Debug.DrawRay( rayOriginPoint, rayDirection*rayDistance, color );

            return Physics2D.Raycast(rayOriginPoint,rayDirection,rayDistance,mask);
        }
    }
}