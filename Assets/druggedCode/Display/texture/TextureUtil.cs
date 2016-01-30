
using System;
using UnityEngine;

namespace druggedcode
{
    public class TextureUtil
    {
        static Rect rect = new Rect();
        static Vector2 pivot;
        static public Sprite Texture2DToSprite( Texture2D tex )
        {
            return Texture2DToSprite( tex, 0.0f, 0.0f );
        }
        
        static public Sprite Texture2DToSprite( Texture2D tex, float pivotX, float pivotY )
        {
            rect.Set(0,0,tex.width,tex.height );
            pivot = new Vector2(pivotX,pivotY);
            return Sprite.Create( tex, rect, pivot );
        }
    }
}
