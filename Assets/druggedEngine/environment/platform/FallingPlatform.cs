using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    /// <summary>
    /// 밟으면 추락하는 바닥
    /// 현재 단순 추락, 다시 밟을 수 있지만 추락 시 콜라이더를 disable 하는 것 필요
    /// IPlayerRespawnListener 구현 하여 스폰 시 다시 정상위치로 되돌리는 기능 추가 고려
    /// 현재 밟은 시간이 누적되지만 시간이 되기 전에 다시 점프하면 리셋되는 기능 고려
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class FallingPlatform : Platform, IPlayerRespawnListener
    {
        /// 바닥이 추락할 때 까지 남은 시간.
        public float TimeBeforeFall = 2f;
        public float FallSpeed = 3f;

        bool _characterOn;
        Animator _animator;
        Vector2 _newPosition;
        float _remainTime;

        override protected void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
            _remainTime = TimeBeforeFall;
        }

        override protected void Start()
        {
            base.Start();
        }

        protected virtual void Update()
        {
            if (_characterOn)
            {
                _remainTime -= Time.deltaTime;
            }

            if (_remainTime < 0)
            {
				//mCollider.enabled = false;
                _characterOn = false;
                UpdateAnimator();

                _newPosition = new Vector2(0, -FallSpeed * Time.deltaTime);


                transform.Translate(_newPosition, Space.World);

                //일정시간이 지나면 비활성.
                gameObject.SetActive(false);
            }
        }

        private void UpdateAnimator()
        {
            EngineUtils.UpdateAnimatorBool(_animator, "shake", _characterOn);
        }

        public void onPlayerRespawnInThisCheckpoint(CheckPoint checkpoint )
        {
            gameObject.SetActive(true);
			//mCollider.enabled = true;
        }

        void OnCollisionEnter2D(Collision2D coll)
        {
            DEController controller = coll.gameObject.GetComponent<DEController>();
            if (controller == null)
                return;

            _characterOn = true;

            UpdateAnimator();
        }

        public void OnCollisionExit2D(Collision2D coll)
        {
            DEController controller = coll.gameObject.GetComponent<DEController>();
            if (controller == null)
                return;

            _characterOn = false;
            _remainTime = TimeBeforeFall;
            UpdateAnimator();
        }
    }

}
