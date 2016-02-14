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

		/*
		void OnOneWayEnter(Collider2D other)
		{
			//passThrough한 캐릭터는 여기로 들어오지 말아야 한다.이미 처리 되었기 때문
			//걷거나, 점프하여 위로 올라가려고 하는 경우가 이곳으로 통해야 한다.

			Physics2D.IgnoreCollision(other, mCollider, true);
		}

		void OnOneWayExit(Collider2D other)
		{
			StartCoroutine(DelayedCollision(other));
		}

		IEnumerator DelayedCollision(Collider2D other)
		{
			yield return new WaitForSeconds(0.1f);

			Physics2D.IgnoreCollision(other, mCollider, false);
			FailCharacterController controller = other.GetComponentInParent<FailCharacterController>();
			if (controller != null)
			{
				controller.currentMask = DruggedEngine.MASK_ALL_GROUND;
			}
		}
		*/
    }
}
