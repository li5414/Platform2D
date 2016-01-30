using UnityEngine;
namespace druggedcode.extensions.ui
{
    public static class RectTransformExtensions
    {
        public static void AsPanel( this RectTransform rtr, float left = 0f, float top = 0f, float right = 0f, float bottom = 0f )
        {
            rtr.anchorMin = new Vector2(0f, 0f);
            rtr.anchorMax = new Vector2(1f, 1f);
            rtr.offsetMin = new Vector2(left, bottom );
            rtr.offsetMax = new Vector2(-right, -top);
        }
    }
}

