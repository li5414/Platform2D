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

        public bool movable { get; set; }
		public bool oneway;

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
        }

        virtual protected void Start()
        {
            if (mPathFollow != null) movable = true;

			if( oneway ) LayerUtil.ChangeLayer(gameObject, DruggedEngine.MASK_ONEWAY);
			else LayerUtil.ChangeLayer(gameObject, DruggedEngine.MASK_PLATFORM );
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
                    return Vector2.zero;
                }
                else
                {
                    return mPathFollow.velocity;
                }
            }
        }
    }

}
