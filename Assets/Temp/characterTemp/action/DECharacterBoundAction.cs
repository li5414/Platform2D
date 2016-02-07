using UnityEngine;

namespace druggedcode.engine
{
    /// <summary>
    /// 플레이어가 스테이지의 경계에 도달할 경우 일어나는 것을 처리.
    /// </summary>
    [RequireComponent(typeof(DECharacter))]
    public class DECharacterBoundAction : MonoBehaviour
    {
        //무,위치제약,사망
        public enum BoundAction
        {
            Nothing,
            Constrain,
            Kill
        }

        // 스테이지의 각경계의 도달했을 때 해야할 행동
        public BoundAction Above = BoundAction.Nothing;
        public BoundAction Below = BoundAction.Kill;
        public BoundAction Left = BoundAction.Constrain;
        public BoundAction Right = BoundAction.Constrain;

        BoundariesInfo mLimitedBound;

        DECharacter _character;
        Transform _tr;
        BoxCollider2D _boxCollider;

        void Awake()
        {
            
        }

        void Start()
        {
            World world = GameManager.Instance.world;

            if (world == null) return;
            if (world.currentLocation == null) return;

            mLimitedBound = world.currentLocation.defaultBoundary;;

            if (mLimitedBound == null) return;

            _character = GetComponent<DECharacter>();
            _tr = transform;

            _boxCollider = GetComponent<BoxCollider2D>();
        }

        public void Update()
        {
            if (mLimitedBound == null || _character.State.IsDead) return;

            //플레이어의 충돌체 크기를 계산
            var colliderSize = new Vector2(
                                   _boxCollider.size.x * Mathf.Abs(_tr.localScale.x),
                                   _boxCollider.size.y * Mathf.Abs(_tr.localScale.y)
                               ) / 2;


            //위 검사
            if (Above != BoundAction.Nothing)
            {
                if (_tr.position.y + colliderSize.y > mLimitedBound.TopLimit)
                {
                    switch (Above)
                    {
                        case BoundAction.Kill:
                            killCharacter();
                            break;

                        case BoundAction.Constrain:
                            ConstrainPosition(new Vector2(_tr.position.x, mLimitedBound.TopLimit - colliderSize.y));
                            break;
                    }
                }
            }

            //아래 검사
            if (Below != BoundAction.Nothing)
            {
                if (_tr.position.y - colliderSize.y < mLimitedBound.BottomLimit)
                {
                    switch (Below)
                    {
                        case BoundAction.Kill:
                            killCharacter();
                            break;

                        case BoundAction.Constrain:
                            ConstrainPosition(new Vector2(_tr.position.x, mLimitedBound.BottomLimit + colliderSize.y));
                            break;
                    }
                }
            }

            //오른쪽 검사
            if (Right != BoundAction.Nothing)
            {
                if (_tr.position.x + colliderSize.x > mLimitedBound.RightLimit)
                {
                    switch (Right)
                    {
                        case BoundAction.Kill:
                            killCharacter();
                            break;

                        case BoundAction.Constrain:
                            ConstrainPosition(new Vector2(mLimitedBound.RightLimit - colliderSize.x, _tr.position.y));
                            break;
                    }
                }
            }

            //왼쪽 검사
            if (Left != BoundAction.Nothing)
            {
                if (_tr.position.x - colliderSize.x < mLimitedBound.LeftLimit)
                {
                    switch (Left)
                    {
                        case BoundAction.Kill:
                            killCharacter();
                            break;

                        case BoundAction.Constrain:

                            print("left. " + _tr.position.x + " limit: " + mLimitedBound.LeftLimit + " . colliderSize:" + colliderSize);

                            ConstrainPosition(new Vector2(mLimitedBound.LeftLimit + colliderSize.x, _tr.position.y));
                            break;
                    }
                }
            }
        }

        void killCharacter()
        {
            _character.Kill();
        }

        void ConstrainPosition(Vector2 constrainedPosition)
        {
            _tr.position = constrainedPosition;
            print("const : " + constrainedPosition);
        }
    }
}