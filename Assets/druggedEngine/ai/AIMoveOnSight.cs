using UnityEngine;

namespace druggedcode.engine
{
	/// <summary>
	/// 시야안에 플레이어가 들어오면 플레이어 방향으로 이동
	/// </summary>
	public class AIMoveOnSight : MonoBehaviour 
	{
		
		///속력
		public float Speed = 3f;
	
		/// AI 가 플레이어를 볼 수 있는 시야 최대거리
		public float ViewDistance = 10f;
	
		/// 최초 고개방향(이동과 관계 없이 디자인의 고개 방향에 관여 )
		public bool CharacterFacingRight = true;
		
		/// 움직임을 멈출 수평거리. 
		public float StopDistance = 1f;
	
		//---------------------------------------------------------------------------------------------------------
		// PRIVATE
		//---------------------------------------------------------------------------------------------------------
		private float _canFireIn;
		private Vector2 _direction;
		private float _distance;
		private DEController _controller;
		private Animator _animator;
		private int _facingModifier;
		
		void Awake()
		{
			_controller = GetComponent<DEController>();
			_animator = GetComponent<Animator>();
			
			_direction = Vector2.right;
	
			if (CharacterFacingRight)
				_facingModifier = -1;
			else
				_facingModifier = 1;
		}
	
		/// <summary>
		/// 매프레임 좌우를 확인. 이후 최적화를 위해 좌우를 번갈아 가면서 확인 하는 쪽 고려.
		/// </summary>
		void Update () 
		{
			bool hit = false;
	
			_distance = 0;
	
			// 플레이어의 확인하기 위해 왼쪽으로 레이발사
			Vector2 raycastOrigin = new Vector2
			(
				transform.position.x,
				transform.position.y +( transform.localScale.y / 2 )
			);
			RaycastHit2D raycast = PhysicsUtil.DrawRayCast( raycastOrigin, -Vector2.right, ViewDistance, 1<<LayerMask.NameToLayer("Player"), Color.gray);
	
			//플레이어를 발견하면 방향 설정. 거리 저장.
			if ( raycast )
			{
				hit = true;
				_direction = -Vector2.right;
				_distance= raycast.distance;
			}
			
			//오른쪽으로 발사.
			raycastOrigin = new Vector2
			(
				transform.position.x,
				transform.position.y + ( transform.localScale.y / 2 )
			);
			raycast = PhysicsUtil.DrawRayCast( raycastOrigin, Vector2.right, ViewDistance, 1<<LayerMask.NameToLayer("Player"), Color.gray );
	
			//플레이어를 발견하면 방향 설정. 거리 저장.
			if (raycast)
			{
				hit = true;
				_direction = Vector2.right;
				_distance = raycast.distance;
			}
			
	
			// 플레이어를 발견하면 플레이어 방향으로 이동
			if ( hit &&  ( _distance > StopDistance ))
				_controller.vx = _direction.x * Speed;
			else
				_controller.vx = 0;
	
	
			float facing = 0f;
			if ( _direction == Vector2.right)
				facing = -Mathf.Abs(transform.localScale.x) * _facingModifier;
			else
				facing = Mathf.Abs(transform.localScale.x) * _facingModifier;
	
			transform.localScale = new Vector3( facing, transform.localScale.y, transform.localScale.z);
	
			// 애니메이터가 있다면 속력 업데이트
			if (_animator != null)
			{
				AnimatorUtil.SetFloat( _animator,"Speed", Mathf.Abs(_controller.vx));
			}
		}
	}
}
