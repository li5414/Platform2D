using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace druggedcode.engine
{
    [RequireComponent(typeof(Collider2D))]
    [ExecuteInEditMode]
    public class Platform : MonoBehaviour
    {
		[Header("Option")]
        [Range(0f, 1f)]
		public float friction = 1f;
		public Vector2 treadmill;

        [Header("Sound")]
        public AudioClip footStep;

		[Header("Prefab")]
		public GameObject jumpPrefab;

		public bool oneway{ get; private set; }
		public bool movable{ get; set; }
		public Collider2D platformCollider{ get{ return mCollider; }}

		protected PathFollow mPathFollow;
		protected Collider2D mCollider;
		protected Collider2D mOnewayTriggerCollider;

        virtual protected void Awake()
        {
			mPathFollow = GetComponent<PathFollow>();

			Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
			int colliderCount = colliders.Length;

			if( colliderCount == 1 )
			{
				mCollider = colliders[0];

				oneway = false;
			}
			else if( colliderCount == 2 )
			{
				foreach( Collider2D col in colliders )
				{
					if( col.isTrigger ) mOnewayTriggerCollider = col;
					else mCollider = col;
				}

				oneway = true;
			}
			else
			{
				Debug.LogError("Platform must have 1 or 2 collider.");
			}

			if( name == "onewayChild" )
			{
				print( "name : " + name + ", count :" + colliderCount + ", oneway : " + oneway );
			}
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

		void OnTriggerEnter2D( Collider2D other )
		{
			if( oneway == false ) return;

			DEController controller = other.GetComponentInParent< DEController>();
			if( controller == null ) return;

			if( controller.vy > -1f )
			{
				print(">  oneway Enter: " + name + " in " + other.name );
				controller.ExceptOneway( this );
			}
		}

		void OnTriggerExit2D( Collider2D other )
		{
			if( oneway == false ) return;

			DEController controller = other.GetComponentInParent< DEController>();
			if( controller == null ) return;

			print("    <  oneway Exit: " + name + " out " + other.name );

			controller.IncludeOneway( this );
		}

		#if UNITY_EDITOR
		void OnDrawGizmos()
		{
			if (Application.isPlaying == false) return;

			if( mOnewayTriggerCollider != null )
			{
				GizmoUtil.DrawCollider( mOnewayTriggerCollider,Color.yellow);
			}
		}
		#endif

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
