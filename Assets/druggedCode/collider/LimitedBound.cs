using UnityEngine;
using System.Collections;

namespace druggedcode
{
    [RequireComponent( typeof( BoxCollider2D ))]
    public class LimitedBound : MonoBehaviour
    {
        public float LeftLimit {get; private set;}
        public float RightLimit {get; private set;}
        public float BottomLimit {get; private set;}
        public float TopLimit {get; private set;}

        Bounds mBounds;

        void Awake () 
        {
            BoxCollider2D col = GetComponent<BoxCollider2D>();

            mBounds = col.bounds;

            LeftLimit = mBounds.min.x;
            RightLimit = mBounds.max.x;
            BottomLimit = mBounds.min.y;
            TopLimit = mBounds.max.y;

            col.enabled = false;
        }

        void OnDrawGizmos()
        {
            if (Application.isEditor)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(mBounds.center, mBounds.size);
            }
        }
    }
}

