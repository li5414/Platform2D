using UnityEngine;

namespace druggedcode.engine
{
    [RequireComponent(typeof(BoxCollider2D))]
    /// <summary>
    ///  적에게(혹은 원하는 무엇이든) 에게 붙이면 밟을 수 있게 된다.
    /// </summary>
    public class Stompable : MonoBehaviour
    {
        ///밟기를 감지할 래이케스트의 수
        public int NumberOfRays = 5;

        ///밟은 사람에게 전달된 힘
        public float KnockbackForce = 15f;
        /// 플레이어의 레이어
        public LayerMask PlayerMask;

        /// 매번 밟을때마다 적에게 줄 데미
        public int DamagePerStomp;

        private BoxCollider2D _boxCollider;

        void Awake()
        {
            _boxCollider = GetComponent<BoxCollider2D>();
            //  _health = (Health)GetComponent<Health>();
        }

        /// 매프레임 위로 레이를 쏜다
        void Update()
        {
            CastRaysAbove();
        }
        
        virtual protected void Stomped( DEController controller )
        {
            controller.vy = KnockbackForce;
            //  _health.TakeDamage(DamagePerStomp, gameObject);
        }

        private void CastRaysAbove()
        {
            float rayLength = 0.5f;

            bool hitConnected = false;
            int hitConnectedIndex = 0;
            RaycastHit2D hittedRay;

            Vector2 verticalRayCastStart = new Vector2(_boxCollider.bounds.min.x,
                                                        _boxCollider.bounds.max.y);
            Vector2 verticalRayCastEnd = new Vector2(_boxCollider.bounds.max.x,
                                                    _boxCollider.bounds.max.y);

            RaycastHit2D[] hitsStorage = new RaycastHit2D[NumberOfRays];

            for (int i = 0; i < NumberOfRays; i++)
            {
                Vector2 rayOriginPoint = Vector2.Lerp(verticalRayCastStart, verticalRayCastEnd, (float)i / (float)(NumberOfRays - 1));
                
                hittedRay = PhysicsUtil.DrawRayCast(rayOriginPoint, Vector2.up, rayLength, PlayerMask, Color.black);
                hitsStorage[i] = hittedRay;

                if ( hittedRay )
                {
                    hitConnected = true;
                    hitConnectedIndex = i;
                    break;
                }
            }

            if (hitConnected)
            {
                hittedRay = hitsStorage[hitConnectedIndex];
                
                DEController controller = hittedRay.collider.GetComponent<DEController>();
                if (controller != null)
                {
                    Stomped( controller );
                }
            }
        }
    }
}
