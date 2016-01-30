using UnityEngine;
using UnityEngine.Events;

using System.Collections;
namespace druggedcode
{
    public class Motion2D
    {
        public static void SetAlphaInChildren( GameObject go, float alpha, string ignoreName = "" )
        {
            foreach( Renderer r in go.GetComponentsInChildren<Renderer>())
            {
                if( r.name == ignoreName )continue;
                SetAlpha( r, alpha );
            }
        }

        public static void SetAlpha( Renderer r, float alpha )
        {
            Color color = r.material.color;
            color.a = alpha;
            r.material.color = color;
        }

        public static IEnumerator ChangeColor( Renderer r, float duration, Color color, UnityAction<Renderer> cb = null )
        {
            if( r == null ) yield break;

            if( duration == 0 )
            {
                r.material.color = color;
            }
            else
            {
                Color startColor = r.material.color;
                float t = 0f;
                while( t < 1f )
                {
                    if( r == null ) break;

                    Color newColor = Color.Lerp( startColor, color, t );
                    r.material.color = newColor;

                    t += Time.deltaTime / duration;

                    yield return null;
                }
            }

            if( cb != null ) cb( r );

            yield break;
        }

        public static IEnumerator FadeAlpha( Renderer r, float duration, float toAlpha, UnityAction<Renderer> cb = null )
        {
            if (r == null) yield break;

            r.gameObject.SetActive(true);

            float fromAlpha = r.material.color.a;
            float currentAlpha = fromAlpha;
            float t = 0f;

            if( duration == 0 )
            {
                SetAlpha( r, toAlpha );
            }
            else
            {
                while (t < 1)
                {
                    if( r == null ) break;
                    currentAlpha = Mathf.Lerp( fromAlpha,toAlpha,t );
                    SetAlpha( r, currentAlpha );
                    t += Time.deltaTime / duration;
                    yield return null;
                }
            }

            currentAlpha = toAlpha;
            SetAlpha( r, currentAlpha );

            if( toAlpha == 0f ) r.gameObject.SetActive(false);

            if( cb != null ) cb( r );

            yield break;
        }

        // 객체를 A -> B 로 이동
        public static IEnumerator MoveFromTo(GameObject movingObject, Vector3 pointA, Vector3 pointB, float time)
        {
            float t = 0f;

            while (movingObject.transform.position != pointB)
            {
                t += Time.deltaTime / time;
                movingObject.transform.position = Vector3.Lerp(pointA, pointB, t);
                yield return 0;
            }
        }
    }
}
