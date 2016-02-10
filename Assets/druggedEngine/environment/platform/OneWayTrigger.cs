using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace druggedcode.engine
{
    [RequireComponent (typeof(Collider2D))]
    public class OneWayTrigger : MonoBehaviour
    {
        public float checkVy = -0.5f;
        
        public UnityAction<Collider2D> OnOneWayEnter;
        public UnityAction<Collider2D> OnOneWayExit;
        
        public List<Collider2D> mEnteredColliders;
        
        Collider2D mCollider;
        void Awake()
        {
            mCollider = GetComponent<Collider2D>();
            mCollider.isTrigger = true;
            mEnteredColliders = new List<Collider2D>();
        }
        
        void OnTriggerEnter2D (Collider2D other)
		{
            //낙하 중이라면 무시한다.
			if (other.attachedRigidbody.velocity.y <= checkVy ) return;
            
            mEnteredColliders.Add( other );
            if( OnOneWayEnter != null ) OnOneWayEnter( other );
		}

		void OnTriggerExit2D (Collider2D other)
		{
            if( mEnteredColliders.Contains( other ) == false ) return;
            mEnteredColliders.Remove(other);
            if( OnOneWayExit != null ) OnOneWayExit( other );
		}
    }
}
