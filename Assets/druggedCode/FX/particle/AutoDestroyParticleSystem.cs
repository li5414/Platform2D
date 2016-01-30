using UnityEngine;
using System.Collections;
namespace druggedcode
{
    /// <summary>
    /// Add this class to a ParticleSystem so it auto destroys once it has stopped emitting.
    /// 파티클 시스템에 추가하면 emitting 이 중지하면 자동으로 제거된다.
    /// 파티클이 루핑되면 이 스크립트는 쓸모가 없다.
    /// </summary>
    public class AutoDestroyParticleSystem : MonoBehaviour
    {
        ///true 로 지정하면 파티클이 제거 될 때 부모도 제거된다.
        public bool DestroyParent = false;

        private ParticleSystem _particleSystem;

        public void Start()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        public void Update()
        {
            if (_particleSystem.isPlaying)
                return;

            if (transform.parent != null)
            {
                if (DestroyParent)
                {
                    Destroy(transform.parent.gameObject);
                }
            }

            Destroy(gameObject);
        }
    }
}
