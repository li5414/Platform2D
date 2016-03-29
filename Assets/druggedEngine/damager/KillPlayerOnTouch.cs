using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    /// <summary>
	/// 플레이어가 충돌하면 사망한다. MASK_TRIGGER_AT_PLAYER 마스크 적용
    /// </summary>
    
	[RequireComponent(typeof( BoxCollider2D ))]
	public class KillPlayerOnTouch : MonoBehaviour
    {
		BoxCollider2D mCollider;
		void Awake()
		{
			mCollider = GetComponent<BoxCollider2D>();
			mCollider.isTrigger = true;
		}

        public void OnTriggerEnter2D(Collider2D collider)
        {
            var player = collider.GetComponent<DEPlayer>();
            if (player == null) return;
			
			player.Dead();
        }

		#if UNITY_EDITOR
		void OnDrawGizmos()
		{
			if( mCollider == null ) mCollider = GetComponent<BoxCollider2D>();
			GizmoUtil.DrawCollider( mCollider, Color.red );
		}
		#endif
	}
}

