using UnityEngine;
using System.Collections;
using druggedcode.engine;

namespace druggedcode.engine
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class LocationLinker : MonoBehaviour
    {
        public enum LinkType
        {
            AUTO,
            MANUAL
        }

        public string locationID;
        public string cpID;
        public LinkType type;

        public float DistanceFromTop = 0;

        BoxCollider2D mCollider;
        private GameObject mPrompt;

        void Awake()
        {
            mCollider = GetComponent<BoxCollider2D>();
            mCollider.isTrigger = true;
        }

		void Start()
		{
			LayerUtil.ChangeLayer( gameObject, DruggedEngine.MASK_TRIGGER_AT_PLAYER );
		}

        public void In(DEPlayerOld player)
        {
            GameManager.Instance.MoveLocation(locationID, cpID);
        }

        public void Out(DEPlayerOld player)
        {
            player.transform.position = transform.position;
        }

        void ShowPrompt()
        {
            mPrompt = (GameObject)Instantiate(ResourceManager.Instance.Load("GUI/LinkerIndicator"));          
            mPrompt.transform.position = new Vector2(mCollider.bounds.center.x, mCollider.bounds.max.y + DistanceFromTop); 
            mPrompt.transform.parent = transform;
            mPrompt.GetComponent<SpriteRenderer>().material.color = new Color(1f, 1f, 1f, 0f);

            StartCoroutine(Motion2D.FadeAlpha(mPrompt.GetComponent<SpriteRenderer>(), 0.2f, 1f)); 
        }

        void HidePrompt()
        {
            StartCoroutine(Motion2D.FadeAlpha(mPrompt.GetComponent<SpriteRenderer>(),0.2f,0f,
                ( Renderer r) => {
                    Destroy(r.gameObject);
                })
            );
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            DEPlayerOld player = other.GetComponent<DEPlayerOld>();
            if (player == null) return;

            switch (type)
            {
                case LinkType.AUTO:
                    In(player);
                    break;

                case LinkType.MANUAL:
                    ShowPrompt();
                    player.currentManualLinker = this;
                    break;
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            DEPlayerOld player = other.GetComponent<DEPlayerOld>();
            if (player == null) return;
            switch (type)
            {
                case LinkType.AUTO:
                    break;

                case LinkType.MANUAL:
                    HidePrompt();
                    player.currentManualLinker = null;
                    break;
            }
        }
    }
}