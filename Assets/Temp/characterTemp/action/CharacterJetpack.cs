using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    /// <summary>
    /// 캐릭터에 부착하면 캐릭터가 제트펙을 쏴 날 수 있음.
    /// </summary>
    public class CharacterJetpack : MonoBehaviour
    {

        ///캐릭터와 관련된 제트팩
        public ParticleSystem Jetpack;

        ///제트팩으로 캐릭터에게 전달될 y 힘
        public float JetpackForce = 2.5f;

        /// true 가 되면 연료를 무제한으로 사용할 수 있다.
        public bool JetpackUnlimited = false;

        ///제트팩 최대 사용시간(s)
        public float JetpackFuelDuration = 5f;
        ///연료보급 재사용 대기
        public float JetpackRefuelCooldown = 1f;

        private DECharacter _characterBehavior;
        private DEController _controller;

		ParticleSystem.EmissionModule mEM;

        void Start()
        {
            //  _characterBehavior = GetComponent<Character>();
            //  _controller = GetComponent<DEController>();

            if (Jetpack != null)
            {
				mEM = Jetpack.emission;
				mEM.enabled = false;
                //  GUIManager.Instance.SetJetpackBar(!JetpackUnlimited); //제트팩이 무한이면 GUI 에서 제트팩 바를 숨긴다.
                //  _characterBehavior.BehaviorState.JetpackFuelDurationLeft = JetpackFuelDuration;
            }
        }
		
		/*
        //버튼을 누르고 있다면 InputManager 에서 update 마다 지속 적으로 호출된다.
        public void JetpackStart()
        {
            if ((_characterBehavior.Permissions.JetpackEnabled) == false || _characterBehavior.BehaviorState.CanJetpack == false || (_characterBehavior.BehaviorState.IsDead))
                return;

            if (_characterBehavior.BehaviorState.CanMoveFreely == false)
                return;

            // 무제한이 아니고 연료가 남아있지 않다면
            if (JetpackUnlimited == false && _characterBehavior.BehaviorState.JetpackFuelDurationLeft <= 0f)
            {
                JetpackStop();
                _characterBehavior.BehaviorState.CanJetpack = false;
                return;
            }

            _controller.SetVerticalForce(JetpackForce);
            _characterBehavior.BehaviorState.Jetpacking = true;
            _characterBehavior.BehaviorState.CanMelee = false;
            _characterBehavior.BehaviorState.CanJump = false;
            Jetpack.enableEmission = true;

            if (JetpackUnlimited == false)
            {
                StartCoroutine(JetpackFuelBurn());

            }
        }

        /// <summary>
        /// 제트팩 연료를 태운다.
        /// </summary>
        private IEnumerator JetpackFuelBurn()
        {
            // while the character is jetpacking and while we have fuel left, we decrease the remaining fuel
            float timer = _characterBehavior.BehaviorState.JetpackFuelDurationLeft;

            //연료가 남아있고 제트팩중이라면 연료를 소모한다.
            while ((timer > 0) && (_characterBehavior.BehaviorState.Jetpacking))
            {
                timer -= Time.deltaTime;
                _characterBehavior.BehaviorState.JetpackFuelDurationLeft = timer;
                yield return 0;
            }
        }

        /// <summary>
        /// 제트팩 분출을 멈춘다.
        /// </summary>
        public void JetpackStop()
        {
            if (Jetpack == null)
                return;
            _characterBehavior.BehaviorState.Jetpacking = false;
            _characterBehavior.BehaviorState.CanMelee = true;
            Jetpack.enableEmission = false;
            _characterBehavior.BehaviorState.CanJump = true;

            //제트팩 무제한이 아니라면 연료 보급을 시작
            if (JetpackUnlimited == false)
                StartCoroutine(JetpackRefuel());
        }




        /// <summary>
        /// 제트팩 연료를 보충
        /// </summary>
        private IEnumerator JetpackRefuel()
        {
            yield return new WaitForSeconds(JetpackRefuelCooldown);
            float timer = _characterBehavior.BehaviorState.JetpackFuelDurationLeft;

            while ((timer < JetpackFuelDuration) && (_characterBehavior.BehaviorState.Jetpacking == false))
            {
                timer += Time.deltaTime / 2;
                _characterBehavior.BehaviorState.JetpackFuelDurationLeft = timer;

                //남은 연료 시간이 1초 이상이 되는 경우에만 다시 제트팩을 사용할 수 있게 한다
                if ((_characterBehavior.BehaviorState.CanJetpack == false) && (timer > 1f))
                    _characterBehavior.BehaviorState.CanJetpack = true;
                yield return 0;
            }
        }
		*/

        void Update()
        {

        }
    }
}
