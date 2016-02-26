using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	/// <summary>
	/// 마리오의 그것처럼 아래서 점프로 부딪힐 수 있는 블럭
	/// IPlayerRespawnListener 구현 고려
	/// 히트 할 때마다 아이템을 스폰 할 건지 마지막 히트시에만 스폰할 것인지 결정하는 플래그 필요
	/// </summary>
    [RequireComponent(typeof( Animator))]
	public class BonusBlock : Platform, IPlayerRespawnListener
	{
		public GameObject SpawnedObject;
		public int NumberOfAllowedHits = 3;

		//----------------------------------------------------------------------------------------
		// private
		//----------------------------------------------------------------------------------------
		Animator _animator;
        Vector2 _newPosition;
        bool _hit;
		int _numberOfHitsLeft;

        override protected void Awake()
        {
            base.Awake();

            _animator = GetComponent<Animator>();
            _numberOfHitsLeft = NumberOfAllowedHits;

            _hit = false;
        }

		override protected void Start()
		{
			base.Start();
            UpdateAnimator();
		}

        void UpdateAnimator()
        {
            //히트 수에 따라 애니메이션 상태 수정
            if (_numberOfHitsLeft > 0 ) 
                EngineUtils.UpdateAnimatorBool(_animator,"onoff",true);
            else            
                EngineUtils.UpdateAnimatorBool(_animator,"onoff",false);

            EngineUtils.UpdateAnimatorBool( _animator, "hit" ,_hit );
        }

		// 매프레임 블럭이 히트 되었는지 판단하여 애니메이션을 업데이트 한다.
		void Update()
		{		
            UpdateAnimator();

			_hit = false;
		}

        void SpawnItem()
        {
            Debug.Log("spawn!!");

            if (SpawnedObject == null)
                return;
            
            _numberOfHitsLeft--;

            GameObject spawned = (GameObject)Instantiate(SpawnedObject);
            spawned.transform.position= transform.position;
            spawned.transform.rotation= Quaternion.identity;

            StartCoroutine(Motion2D.MoveFromTo(spawned,transform.position, new Vector2(transform.position.x,transform.position.y+GetComponent<BoxCollider2D>().size.y), 0.3f));

        }

        public void onPlayerRespawnInThisCheckpoint(CheckPoint checkpoint )
        {

        }

		/// <summary>
		/// 콘트롤러가 터치 한 경우( 플레이어들만 가능하도록 하는 것 고려 )
		/// 충돌체의 y 위치가 블럭의 y위치보다 적은 경우를 아래에서 충돌했다고 판단
		/// 이 경우 아래에서부터 살짝만 닿아도 부딪혔다고 판단하게 된다.
		/// 충돌체의 y속도등을 추가 검사해야 할 필요 있다.
		/// </summary>

		public void OnTriggerEnter2D(Collider2D collider)
		{
            DECharacter character = collider.GetComponent<DECharacter>();

			if ( character == null )
				return;

			if ( _numberOfHitsLeft == 0 )
				return;

			if ( collider.transform.position.y < transform.position.y )
			{
				_hit = true;

                SpawnItem();
			}

            UpdateAnimator();
		}
	}
}
