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
        
        [Header("Sound")]
        public AudioClip footStep;

		[Header("Prefab")]
		public GameObject jumpPrefab;
        
        protected Collider2D mCollider;
		protected PathFollow mPathFollow;

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

		public Collider2D GetCollider()
		{
			return mCollider;
		}

		void OnTriggerEnter2D( Collider2D other )
		{
			print("oneway Enter: " + name + " in " + other.name );

			if( oneway == false ) return;
			print("1" );

			NewController controller = other.GetComponent< NewController>();
			if( controller == null ) return;
			print("2" );

			controller.ExceptOneway( this );

//			if (other.attachedRigidbody.velocity.y > -1)
//			{
//				Physics2D.IgnoreCollision(mCollider, other, true);
//			}
		}

		void OnTriggerExit2D( Collider2D other )
		{
			if( oneway == false ) return;

			print("oneway Exit: " + name + " out " + other.name );
			StartCoroutine(DelayedCollision(other));
		}

		IEnumerator DelayedCollision (Collider2D other) {
			yield return new WaitForSeconds(0.1f);
			Physics2D.IgnoreCollision( mCollider, other, false);
		}


		public bool oneway{ get; private set; }
		public bool movable{ get; set; }

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
