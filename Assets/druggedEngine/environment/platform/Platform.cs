using UnityEngine;

namespace druggedcode.engine
{
    [RequireComponent(typeof(Collider2D))]
    [ExecuteInEditMode]
    public class Platform : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float friction = 1f;

        public bool movable;

		public Collider2D collider{get;private set;}
        PathFollow mPathFollow;

        void Awake()
        {
			collider = GetComponent<Collider2D>();

            mPathFollow = GetComponent<PathFollow>();
        }

        void Start()
        {
            if (Application.isPlaying == false) return;

            LayerUtil.ChangeLayer(gameObject, DruggedEngine.MASK_ENVIRONMENT);
            LayerUtil.ChanageSortingLayer(gameObject, DruggedEngine.SORTING_LAYER_ENVIRONMENT);

            if (mPathFollow != null)
            {
                movable = true;
                mPathFollow.updateType = DruggedEngine.MOVE_PLATFORM;
            }
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
