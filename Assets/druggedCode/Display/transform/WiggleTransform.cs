using UnityEngine;
using System.Collections;

namespace druggedcode
{
    public class WiggleTransform : MonoBehaviour
    {
        //크기가 변하기 까지 걸리는 속도.(초) 높은 값이 느린 움직임을 의미한다.
        public float scaleSpeed = 0.5f;

        /// 크기 변화 정도 
        public float scaleDistance = 0.01f;

        ///회전 속도.(초) 높은 값이 빠른 움직임을 의미.(스케일과 통일 필요)
        public float rotationSpeed = 1f;
        /// 회전 진폭 (degree)
        public float rotationAmplitude = 3f;


        //-----------------------------------------------------------------------------------------------------------
        // -- private
        //-----------------------------------------------------------------------------------------------------------

        private Vector3 _scaleTarget;
        private Quaternion _rotationTarget;
        private float _accumulator = 0.0f;

        void Start()
        {
            _scaleTarget = WiggleScale();
            _rotationTarget = WiggleRotate();
        }

        /// <summary>
        /// 매 프레임마다 객체의 크기와 회전을 수정하여 춤추듯 보이게 한다.
        /// </summary>
        void Update()
        {
            // 일정시간마다 목표 크기 지정.
            _accumulator += Time.deltaTime;

            if (_accumulator >= scaleSpeed)
            {
                _scaleTarget = WiggleScale();
                _accumulator -= scaleSpeed;
            }

            // 크기를 선형보간 
            var norm = Time.deltaTime / scaleSpeed;
            Vector3 newLocalScale = Vector3.Lerp(transform.localScale, _scaleTarget, norm);
            transform.localScale = newLocalScale;

            // RotateTowards 로 회전
            var normRotation = Time.deltaTime * rotationSpeed;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _rotationTarget, normRotation);
            if (transform.rotation == _rotationTarget)
            {
                _rotationTarget = WiggleRotate();
            }

        }

        private Vector3 WiggleScale()
        {
            return new Vector3(
                (1 + Random.Range(-scaleDistance, scaleDistance)),
                (1 + Random.Range(-scaleDistance, scaleDistance)),
                1
            );
        }

        private Quaternion WiggleRotate()
        {
            return Quaternion.Euler
                (
                    0f,
                    0f,
                    Random.Range(-rotationAmplitude, rotationAmplitude)
                );
        }
    }
}
