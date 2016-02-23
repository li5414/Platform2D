using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    /// <summary>
    /// 획득하면 시간이 변화한다
    /// </summary>
    public class ItemTimeModifier : Item
    {
        /// 이펙트가 지속되는 동안 적용될 시간속도
        public float TimeSpeed = 0.5f;
        ///지속시간( 초 )
        public float Duration = 1.0f;

        SpriteRenderer _img;

        override protected void Awake()
        {
            base.Awake();

            _img = GetComponentInChildren<SpriteRenderer>();
        }

        override protected void Getted()
        {
            StartCoroutine(ChangeTime());
        }

        /// <summary>
        /// 지정된 시간동안 시간이 변경되도록 게임 매니저에 요청
        /// </summary>
        private IEnumerator ChangeTime()
        {
            _img.enabled = false;
            _col.enabled = false;

            GameManager.Instance.SetTimeScale(TimeSpeed);
            //UISystem.Instance.SetTimeSplash(true);

            yield return new WaitForSeconds(Duration * TimeSpeed);

            GameManager.Instance.ResetTimeScale();
            //UISystem.Instance.SetTimeSplash(false);

            _img.enabled = true;
            _col.enabled = true;
            gameObject.SetActive(false);
        }
    }
}