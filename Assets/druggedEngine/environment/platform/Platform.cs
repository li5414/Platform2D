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
        public bool oneway { get; set; }

        Collider2D mCollider;
        PathFollow mPathFollow;

        OneWayTrigger mOneWayTrigger;

        public Collider2D GetCollider()
        {
            return mCollider;
        }

        void Awake()
        {
            mCollider = GetComponent<Collider2D>();
            mPathFollow = GetComponent<PathFollow>();
            mOneWayTrigger = GetComponentInChildren<OneWayTrigger>();
        }

        void Start()
        {
            if (mPathFollow != null)
            {
                movable = true;
            }

            if (mOneWayTrigger != null)
            {
                oneway = true;
                mOneWayTrigger.OnOneWayEnter += OnOneWayEnter;
                mOneWayTrigger.OnOneWayExit += OnOneWayExit;
            }

            LayerUtil.ChangeLayer(gameObject, DruggedEngine.MASK_PLATFORM);
            LayerUtil.ChanageSortingLayer(gameObject, DruggedEngine.SORTING_LAYER_ENVIRONMENT);
        }

        public void PassThough(FailCharacterController controller)
        {
            if (oneway == false) return;

            controller.currentMask = DruggedEngine.MASK_ENVIRONMENT;
            controller.IgnoreCollision(mCollider, true);
        }

        void OnOneWayEnter(Collider2D other)
        {
            //passThrough한 캐릭터는 여기로 들어오지 말아야 한다.이미 처리 되었기 때문
            //걷거나, 점프하여 위로 올라가려고 하는 경우가 이곳으로 통해야 한다.

            Physics2D.IgnoreCollision(other, mCollider, true);
        }

        void OnOneWayExit(Collider2D other)
        {
            StartCoroutine(DelayedCollision(other));
        }

        IEnumerator DelayedCollision(Collider2D other)
        {
            yield return new WaitForSeconds(0.1f);

            Physics2D.IgnoreCollision(other, mCollider, false);
            FailCharacterController controller = other.GetComponentInParent<FailCharacterController>();
            if (controller != null)
            {
                controller.currentMask = DruggedEngine.MASK_ALL_GROUND;
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
