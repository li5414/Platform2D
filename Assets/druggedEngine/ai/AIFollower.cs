using UnityEngine;
namespace druggedcode.engine
{
	/// <summary>
	/// CharacterBehavior+CorgiController2D 객체에 부착하면 Player 게임오브젝트를 따라다니게 된다.
	/// </summary>
	public class AIFollower : MonoBehaviour 
	{
		/// true 인경우 에이전트가 플레이어를 따라다닌다.
		public bool AgentFollowsPlayer{get;set;}
		
		/// 에이전트가 달리기를 시작해야할 최소 거리( Horizontal )
		public float RunDistance = 10f;
		/// 보통의 걸음걸이로 따라가기 시작할 최소 거리( Horizontal )
		public float WalkDistance = 5f;
		///따라다니기를 멈출 거리.
		public float StopDistance = 1f;
		/// 제트팩을 사용할 최소 수직거리
		public float JetpackDistance = 0.2f;
	
	
		//-------------------------------------------------------------------------------------------------------------------------------------
		//-- private
		//-------------------------------------------------------------------------------------------------------------------------------------
	
		private Transform _target;
		private DECharacter _character;
		private DEController _controller;
		//  private CharacterJetpack _jetpack;
		private float _speed;
		private float _direction;
		
		void Awake()
		{
			//행동에 필요한 컴포넌트 저장
			_character = GetComponentInParent<DECharacter>();
			_controller = GetComponentInParent<DEController>();
			//  _jetpack = (CharacterJetpack)GetComponentInParent<CharacterJetpack>();
	
			AgentFollowsPlayer = true;
		}
		
		void Start () 
		{
			//플레이어 탐색
			_target = GameManager.Instance.player.transform;
		}
		
		/// <summary>
		/// 매프레임 에이전트가 플레이어를 향해 움직이도록 한다.
		/// </summary>
		void Update () 
		{
			if ( AgentFollowsPlayer == false )
				return;
				
			if ( _target == null || _character == null || _controller == null )
				return;
			
			//가로 거리
			float distance = Mathf.Abs(_target.position.x - transform.position.x);
					
			//가야할 방향을 알아낸다.
			_direction = _target.position.x > transform.position.x ? 1f : -1f;
			
			//에이전트와 플레이어의 가로 거리에 따라 움직이여할 속력을 구한다.
			//달리기
			if ( distance > RunDistance )
			{
				_speed = 1;
				_character.Run();
			}
			else
			{
				_character.StopRun();
			}
	
			//걷기
			if (distance < RunDistance && distance > WalkDistance )
			{
				_speed=1;
			}
	
			//점차적으로 천천히 걷는다.
			if (distance < WalkDistance && distance > StopDistance )
			{
				_speed = distance / WalkDistance;
			}
	
			//멈춘다.
			if (distance<StopDistance)
			{
				_speed = 0f;
			}
			
			//구해진 속력으로 에이전트의 가로축 이동
			_character.horizontalAxis = _speed * _direction;
			
			//앞뒤로 무엇인가 충돌 되는 것이 있다면 점프를 하게 한다.
			if ( _controller.State.IsCollidingRight || _controller.State.IsCollidingLeft )
			{
				_character.Jump();
			}
			
			/*
			// 만약 제트팩을 장착했다면 수직 거리가 있는 경우 제트팩을 사용한다.
			if (_jetpack!=null)
			{
				if ( _target.position.y > transform.position.y + JetpackDistance )
				{
					_jetpack.JetpackStart();
				}
				else
				{
					_jetpack.JetpackStop();
				}
			}
			*/
		}
	}
}
