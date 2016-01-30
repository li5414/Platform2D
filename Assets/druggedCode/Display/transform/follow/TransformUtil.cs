using UnityEngine;
using System.Collections;
using System;
using druggedcode;

namespace druggedcode
{
    public class TransformUtil : Singleton<TransformUtil>
    {
        public static void ResetTransformation( Transform trans)
        {
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
        }



        public void MoveFromToBySpeed(Transform target,
                                      Vector2 to, float speed = 1.0f,
                                      Action<Transform> callback = null)
        {
            MoveFromToBySpeed(target, target.position, to, speed, callback);
        }
        
        public void MoveFromToBySpeed(Transform target,
                                      Vector2 from, Vector2 to, float speed = 1.0f,
                                      Action<Transform> callback = null)
        {
            float dist = Vector2.Distance(from, to);
            float duration = dist / speed;
            
            MoveFromToByTime(target, from, to, duration, callback);
        }
        
        public void MoveFromToByTime(Transform target, Vector2 to, float duration = 1.0f,
                                     Action<Transform> callback = null)
        {
            MoveFromToByTime(target, target.position, to, duration, callback);
        }
        
        public void MoveFromToByTime(Transform target,
                                     Vector2 from, Vector2 to, float duration = 1.0f,
                                     Action<Transform> callback = null)
        {
            if (target == null)
                return;
            
            if (duration <= 0)
            {
                target.position = to;
                
                if (callback != null)
                    callback(target);
                
                return;
            }
            
            StartCoroutine(MoveFromToRoutine(target, from, to, duration, callback));
            
        }
        
        IEnumerator MoveFromToRoutine(Transform target,
                                      Vector2 from, Vector2 to, float duration,
                                      Action<Transform> callback)
        {
            float t = 0f;
            
            while (t < 1)
            {
                target.position = Vector2.Lerp(from, to, t);
                t += Time.deltaTime / duration;
                yield return null;
            }
            
            if (callback != null)
                callback(target);
        }
    }
}

