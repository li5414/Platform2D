using UnityEngine;

namespace druggedcode.engine
{
    /// <summary>
    /// 밀리무기의 충돌체가 달린 오브젝트에 추가, 충돌체는 trigger 체크
    /// </summary>
    public class MeleeWeapon : MonoBehaviour
    {
        ///  밀리어택이 충돌할 레이어 마스크
        public LayerMask CollisionMask;
        /// 데미지
        public int Damage;

        /// 충돌 시 플레이할 파티클 이펙트 프리팹
        public GameObject HitEffect;

        /// 어택 오너
        public GameObject Owner;

        public virtual void OnTriggerEnter2D(Collider2D collider)
        {
            //충돌한 객체의 레이어 검사

            if (LayerUtil.Contains(CollisionMask, collider.gameObject.layer) == false) return;

            // 충돌한 객체가 오너인 경우 리턴	
            var isOwner = collider.gameObject == Owner;
            if (isOwner) return;

            // ICanTakeDamage 를 구현한 콤포넌트가 있다면 데미지 전달 
            IDamageable takeDamage = collider.GetComponent<IDamageable>();
            if (takeDamage != null)
            {
                OnCollideTakeDamage(collider, takeDamage);
                return;
            }

            OnCollideOther(collider);
        }


        //히트 파티클을 발생시키고 데미지를 전달 
        void OnCollideTakeDamage(Collider2D collider, IDamageable takeDamage)
        {
            Instantiate(HitEffect, collider.transform.position, collider.transform.rotation);
            takeDamage.TakeDamage(Damage, gameObject);
            DisableMeleeWeapon();
        }

        void OnCollideOther(Collider2D collider)
        {
            DisableMeleeWeapon();
        }

        void DisableMeleeWeapon()
        {

        }
    }
}
