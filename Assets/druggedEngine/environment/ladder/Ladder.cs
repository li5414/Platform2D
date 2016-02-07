using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Ladder : MonoBehaviour
    {
        /// 사다리의 정상에 있는 플랫폼
        [SerializeField]
        TempOneWayPlatform _ladderPlatform;

        BoxCollider2D _col;
        
        public float PlatformY{ get; private set;}
        
        void Awake()
        {
            _col = GetComponent<BoxCollider2D>();
            _col.isTrigger = true;
        }
        
        void Start()
        {
            if( _ladderPlatform == null )
            {
                _col.enabled = false;
                throw new System.NullReferenceException( "사다리는 반드시 이어지는 OnewayPlatform을 가져야만 한다.");
            } 
            
            PlatformY = _ladderPlatform.transform.position.y;
        }

        public void OnTriggerEnter2D(Collider2D collider)
        {
            DEPlayer player = collider.GetComponent<DEPlayer>();

            if (player == null)
                return;

            player.currentLadder = this;
        }

        public void OnTriggerExit2D(Collider2D collider)
        {
            DEPlayer player = collider.GetComponent<DEPlayer>();
            if (player == null)
                return;

            player.currentLadder = null;
        }
    }
}
