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

        Collider2D mCollider;
        PathFollow mPathFollow;

        public Collider2D GetCollider()
        {
            return mCollider;
        }

        void Awake()
        {
            mCollider = GetComponent<Collider2D>();
            mPathFollow = GetComponent<PathFollow>();

			if( mCollider is EdgeCollider2D ) oneway = true;
        }

        void Start()
        {
            if (mPathFollow != null) movable = true;

			if( oneway ) LayerUtil.ChangeLayer(gameObject, DruggedEngine.MASK_ONEWAY);
			else LayerUtil.ChangeLayer(gameObject, DruggedEngine.MASK_PLATFORM );
        }

        public void PassThough(FailCharacterController controller)
        {
            if (oneway == false) return;

            controller.currentMask = DruggedEngine.MASK_ENVIRONMENT;
            controller.IgnoreCollision(mCollider, true);
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
