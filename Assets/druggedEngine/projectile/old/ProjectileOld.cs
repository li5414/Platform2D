using UnityEngine;

namespace druggedcode.engine
{
    /// <summary>
    /// 발사되는 탄환
    /// </summary>
    public abstract class ProjectileOld : MonoBehaviour
    {
        //탄환 속력
        public float Speed;

        /// 탄환 충돌 레이어마스크
        public LayerMask CollisionMask;

        /// 탄환 주인
        public GameObject Owner { get; private set; }

        /// 탄환 방향
        public Vector2 Direction { get; private set; }

        /// 초기 속도. 발사한 주인의 controller 의 Speed 를 받아온다
        public Vector2 InitialVelocity { get; private set; }

        /// <summary>
        ///  주인과 방향 속도를 설정
        /// </summary>
        public void Initialize(GameObject owner, Vector2 direction, Vector2 initialVelocity)
        {
            transform.right = direction;
            Owner = owner;
            Direction = direction;
            InitialVelocity = initialVelocity;

            OnInitialized();
        }

        protected virtual void OnInitialized()
        {
            // override
        }

        /// 탄환이 무엇인가에 충돌 되었을 때 
        public virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (LayerUtil.Contains(CollisionMask, collider.gameObject.layer) == false)
            {
                OnNotCollideWith(collider);
                return;
            }


            var isOwner = collider.gameObject == Owner;
            if (isOwner)
            {
                OnCollideOwner();
                return;
            }

            IDamageable damageable = collider.GetComponent<IDamageable>();

            if (damageable != null)
            {
                OnCollideTakeDamage(collider, damageable);
                return;
            }

            OnCollideOther(collider);
        }

        /// <summary>
        /// 지정한 레이어가 아닌 다른 충돌체와 충돌.
        /// </summary>
        protected virtual void OnNotCollideWith(Collider2D collider)
        {
            // override
        }

        /// <summary>
        /// 발사한 주인과 충돌( 여기를 이용해 부메랑등의 탄환을 구현 )
        /// </summary>
        protected virtual void OnCollideOwner()
        {
            // override
        }

        /// <summary>
        /// 데미지를 입을 수 있는 객체와 충돌한 경우 처리
        /// </summary>
        protected virtual void OnCollideTakeDamage(Collider2D collider, IDamageable damageable)
        {
            // override
        }

        /// <summary>
        /// 충돌은 허용했지만 데미지를 입을 수 없는 충돌체와 충돌
        /// </summary>
        protected virtual void OnCollideOther(Collider2D collider)
        {
            // override
        }

    }
}
