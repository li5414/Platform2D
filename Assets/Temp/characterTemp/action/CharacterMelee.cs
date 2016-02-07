using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    /// <summary>
    /// 캐릭터에 추가하면 근접공격이 가능해진다.
    /// </summary>
    public class CharacterMelee : MonoBehaviour
    {
        ///근접 충돌을 위한 충돌체(box,corcle....) 가급적이면 캐릭터에 부착. 충돌체에는 MeleeWeapon 콤포넌트가 달려있다
        public GameObject MeleeCollider;
        /// 공격 시간(s)
        public float MeleeAttackDuration = 0.3f;

        private DECharacter _character;

        void Start()
        {
            // initialize the private vars
            _character = GetComponent<DECharacter>();

            if (MeleeCollider != null)
            {
                MeleeCollider.SetActive(false);
            }
        }

        /// <summary>
        /// 근접공격을 한다.
        /// </summary>
        public void Melee()
        {
            if (_character.Permissions.MeleeAttackEnabled == false)
                return;

            //캐릭터가 움직일 수 없는 곳에 위치했다면 공격하지 않는다.

            //  if (_character.State.CanMelee == false)
            //      return;

            //  // 밀리 애니메이션이 재생되도록 관련 트랜지션을 true 로 설정. 콜라이더를 활성화한다.
            //  _character.State.MeleeAttacking = true;
            MeleeCollider.SetActive(true);
            //코루틴을 시작햇 0.3 초뒤에 밀리어택 상태를 끝낸다( 애니메이션에 따라 수정 )
            StartCoroutine(MeleeEnd());
        }

        private IEnumerator MeleeEnd()
        {
            yield return new WaitForSeconds(MeleeAttackDuration);
            MeleeCollider.SetActive(false);
            //  _character.State.MeleeAttacking = false;
        }
    }
}