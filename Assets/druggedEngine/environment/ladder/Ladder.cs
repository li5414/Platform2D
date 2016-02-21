using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    [RequireComponent(typeof(BoxCollider2D))]
	[ExecuteInEditMode]
    public class Ladder : MonoBehaviour
    {
        /// 사다리의 정상에 있는 플랫폼
		public Platform ladderPlatform;
        BoxCollider2D mCollider;
        
        public float PlatformY{ get; private set;}
        
        void Awake()
        {
            mCollider = GetComponent<BoxCollider2D>();
            mCollider.isTrigger = true;
        }
        
        void Start()
        {
            LayerUtil.ChangeLayer(gameObject, DruggedEngine.MASK_LADDER,false);
            PlatformY = ladderPlatform.transform.position.y;
        }

        public void OnTriggerEnter2D(Collider2D collider)
        {
            DECharacter character = collider.GetComponent<DECharacter>();
            if (character == null) return;

            character.currentLadder = this;
        }

        public void OnTriggerExit2D(Collider2D collider)
        {
            DECharacter character = collider.GetComponent<DECharacter>();
            if (character == null) return;
            
            character.currentLadder = null;
        }
    }
}
