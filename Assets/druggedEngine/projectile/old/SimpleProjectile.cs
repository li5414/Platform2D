using UnityEngine;

namespace druggedcode.engine
{
    /// <summary>
    /// 기본적은 간단한 탄환체
    /// </summary>
    public class SimpleProjectile : ProjectileOld
    {
        ///데미지
        public int Damage;

        ///탄환이 제거될 때 생성할 이펙트 프리팹 ( 파티클 이펙트 일 수도, 복합적 오브젝트일 수도 있다. 부모 클래스에 정의 되는것은 어떨지 )
        public GameObject DestroyedEffect;

        /// 탄환이 살아있을 라이프타임
        public float TimeToLive;

        /// <summary>
        /// 매프레임 라이프타임을 체크하고 이동시킨다
        /// </summary>
        public void Update()
        {
            if ((TimeToLive -= Time.deltaTime) <= 0)
            {
                DestroyProjectile();
                return;
            }

            //탄환 이동. 설정된 방향 * ( 발사한 주인의 속도를 통해 구한 xAxis 값(절대값) + 지정한 탄환 속도 )
            transform.Translate(Direction * ((Mathf.Abs(InitialVelocity.x) + Speed) * Time.deltaTime), Space.World);
        }

        /// <summary>
        /// 허락된 충돌체이지만 데미지를 입힐 수 없는 물체와 충돌한 경우
        /// </summary>
        protected override void OnCollideOther(Collider2D collider)
        {
            DestroyProjectile();
        }

        /// <summary>
        /// 대상에게 데미지를 준다
        /// </summary>
        protected override void OnCollideTakeDamage(Collider2D collider, IDamageable takeDamage)
        {
            takeDamage.TakeDamage(Damage, gameObject);
            DestroyProjectile();
        }

        /// <summary>
        /// 탄환 제거 
        /// </summary>
        private void DestroyProjectile()
        {
            if (DestroyedEffect != null)
            {
                Instantiate(DestroyedEffect, transform.position, transform.rotation);
            }

            Destroy(gameObject);
        }
    }
}