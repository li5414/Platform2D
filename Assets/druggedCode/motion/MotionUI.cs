using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

namespace druggedcode
{
    public class MotionUI
    {
        public static void SetAlphaInChildren(GameObject go, float alpha, string ignoreName = "")
        {
            foreach (Graphic g in go.GetComponentsInChildren<Graphic>())
            {
                if (g.name == ignoreName) continue;
                SetAlpha( g, alpha );
            }
        }

        public static void SetAlpha( Graphic g, float alpha )
        {
            Color color = g.color;
            color.a = alpha;
            g.color = color;
        }

        public static IEnumerator ChangeColor( Graphic g, float duration, Color color, UnityAction cb = null )
        {
            if( g == null ) yield break;

            if( duration == 0 )
            {
                g.color = color;
            }
            else
            {
                Color startColor = g.color;
                float t = 0f;
                while( t < 1.0f )
                {
                    if( g == null ) break;

                    Color newColor = Color.Lerp( startColor, color, t );
                    g.color = newColor;

                    t += Time.deltaTime / duration;

                    yield return null;
                }
            }

            if( cb != null ) cb();

            yield break;
        }

        public static IEnumerator FadeAlpha( Graphic g, float duration, float alpha, UnityAction cb = null )
        {
            if (g == null) yield break;

            g.gameObject.SetActive(true);

            float from = g.color.a;

            if( duration == 0 )
            {
                SetAlpha( g, alpha );
            }
            else
            {
                float a = from;
                float t = 0f;
                while (t < 1)
                {
                    if( g == null ) break;
                    a = Mathf.Lerp( from,alpha,t );

                    SetAlpha( g, a );
                    t += Time.deltaTime / duration;
                    yield return null;
                }
            }

            if( alpha == 0f ) g.gameObject.SetActive(false);

            if( cb != null ) cb();

            yield break;
        }
    }
}

