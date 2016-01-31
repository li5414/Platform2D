using UnityEngine;


namespace druggedcode.engine
{
    /// <summary>
    /// 무엇인가에 가로막히기 전까지 전진한다. 캐릭터가 리스폰 되면 초기 위치로 이동
    /// </summary>
    public class AISimpleWalk : MonoBehaviour, IPlayerRespawnListener
    {
        /// 속력
        public float Speed;
        /// 처음 움직일 방향
        public bool GoesRightInitially = true;


        private DEController _controller;
        private Vector2 _direction;
        private Vector2 _startPosition;
        private Vector2 _initialDirection;

        void Awake()
        {
            _controller = GetComponent<DEController>();
            _startPosition = transform.position;
            _direction = GoesRightInitially ? Vector2.right : -Vector2.right;
            _initialDirection = _direction;
        }

        public void Update()
        {

            // 현재 방향에 따라 움직인다.
            _controller.vx = _direction.x * Speed;

            // 무엇인가에 충돌한다면 방향을 전환.
            if (_direction.x < 0 && _controller.State.IsCollidingLeft ||
                _direction.x > 0 && _controller.State.IsCollidingRight)
            {
                _direction = -_direction;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }

        public void onPlayerRespawnInThisCheckpoint(CheckPoint checkpoint, DECharacter player)
        {
            _direction = _initialDirection;
            transform.localScale = new Vector3(1, 1, 1);
            transform.position = _startPosition;
            gameObject.SetActive(true);
        }

    }

}
