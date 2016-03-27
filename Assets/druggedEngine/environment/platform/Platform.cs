using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    [RequireComponent(typeof(Collider2D))]
    [ExecuteInEditMode]
    public class Platform : MonoBehaviour
    {
        [Range(0f, 1f)]
		public float friction = 1f;
		public Vector2 treadmill;

		public bool oneway{get;private set;}
        
        [Header("Sound")]
        public AudioClip footStep;

		[Header("Prefab")]
		public GameObject jumpPrefab;
        
        
        public bool movable { get; set; }
        
        protected Collider2D mCollider;
		protected PathFollow mPathFollow;

        public Collider2D GetCollider()
        {
            return mCollider;
        }

        virtual protected void Awake()
        {
            mCollider = GetComponent<Collider2D>();
            mPathFollow = GetComponent<PathFollow>();

			if( mCollider is EdgeCollider2D ) oneway = true;
            else oneway = false;
        }

        virtual protected void Start()
        {
            if (mPathFollow != null) movable = true;

			if( oneway ) LayerUtil.ChangeLayer(gameObject, DruggedEngine.MASK_ONEWAY);
			else LayerUtil.ChangeLayer(gameObject, DruggedEngine.MASK_PLATFORM );
        }

		public void PlaySound( Vector3 pos, float v = 1f , float p = 1f)
		{
			if( footStep == null ) return;

			SoundManager.Instance.PlaySFX( footStep,v,p, pos );
		}

		public void ShowEffect( Vector3 pos, Vector3 scale )
		{
			if( jumpPrefab == null ) return;
			((GameObject)Instantiate ( jumpPrefab, pos, Quaternion.identity)).transform.localScale = scale;
		}

        public Vector2 translateVector
        {
            get
            {
                if (mPathFollow == null)
                {
                    return Vector2.zero;
                }
                else
                {
                    return mPathFollow.deltaVector;
                }
            }
        }

        public Vector2 velocity
        {
            get
            {
                if (mPathFollow == null)
                {
					return Vector2.zero + treadmill;
                }
                else
                {
					return mPathFollow.velocity + treadmill;
                }
            }
        }
    }

}
