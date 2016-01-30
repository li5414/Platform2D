using UnityEngine;
using System.Collections;

namespace druggedcode
{
    public class FollowTarget : MonoBehaviour
    {
        
        public float xMargin = 0f;
        public float yMargin = 0f;
        public float xSmooth = 4f;
        public float ySmooth = 4f;
        public bool useLimit;
        public Vector2 maxXY;
        public Vector2 minXY;
        public Vector3 offset;
        public Transform target;
        public Transform chaser;
        bool useXMargin;
        bool useYMargin;
        Vector3 _targetPosition;
        
        void Awake()
        {
            
            Renew();
        }
        
        public void Renew()
        {
            if (xMargin != 0.0f)
                useXMargin = true;
            
            if (yMargin != 0.0f)
                useYMargin = true;
        }
        
        bool CheckXMargin()
        {
            return Mathf.Abs(target.position.x - chaser.position.x) > xMargin;
        }
        
        bool CheckYMargin()
        {
            return Mathf.Abs(target.position.y - chaser.position.y) > yMargin;
        }
        
        void Update()
        {
            if (target)
                TrackTarget();
        }
        
        public void TrackTarget(bool direct = false)
        {
            _targetPosition = target.position + offset;
            
            Vector3 newPosition = chaser.position;
            
            //x calculate
            if (direct == false && useXMargin && CheckXMargin())
                newPosition.x = Mathf.Lerp(chaser.position.x, _targetPosition.x, xSmooth * Time.deltaTime);
            else
                newPosition.x = _targetPosition.x;
            
            if (direct == false && useYMargin && CheckYMargin())
                newPosition.y = Mathf.Lerp(chaser.position.y, _targetPosition.y, ySmooth * Time.deltaTime);
            else
                newPosition.y = _targetPosition.y;
            
            if (useLimit)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, minXY.x, maxXY.x);
                newPosition.y = Mathf.Clamp(newPosition.y, minXY.y, maxXY.y);
            }
            
            chaser.position = newPosition;
            
        }
    }
}
