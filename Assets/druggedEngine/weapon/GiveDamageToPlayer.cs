using UnityEngine;

namespace druggedcode.engine
{
    /// <summary>
    /// 오브젝트에 붙이면 플레이어가 충돌했을때 대미지를 준다.(몬스터, 선인장, 톱, 슬라임등에 부착되어있다)
    /// </summary>
    public class GiveDamageToPlayer : MonoBehaviour
    {
        public int DamageToGive = 10;

        private Vector2 _lastPosition;
        private Vector2 _velocity;

        /// <summary>
        /// 오브젝트의 위치와 속도를 저장
        /// </summary>
        public void LateUpdate()
        {
            _velocity = (_lastPosition - (Vector2)transform.position) / Time.deltaTime;
            _lastPosition = transform.position;
        }

        /// <summary>
        /// 캐릭터가 충돌하면 데미지를 주고 플레이어를 넉백 시킨다.
        /// </summary>
        public void OnTriggerEnter2D(Collider2D collider)
        {
            var player = collider.GetComponent<DECharacter>();

            if (player == null)
                return;

            if (collider.tag != "Player")
                return;

            player.TakeDamage(DamageToGive, gameObject);

            var controller = player.GetComponent<DEController>();
            var totalVelocity = controller.Velocity + _velocity;
            float xforce = -1 * Mathf.Sign(totalVelocity.x) * Mathf.Clamp(Mathf.Abs(totalVelocity.x) * 5, 10, 20);
            float yforce = -1 * Mathf.Sign(totalVelocity.y) * Mathf.Clamp(Mathf.Abs(totalVelocity.y) * 2, 0, 15);
            Vector2 nockbackVector = new Vector2(xforce, yforce);

            controller.AddForce( nockbackVector );
        }
    }
}