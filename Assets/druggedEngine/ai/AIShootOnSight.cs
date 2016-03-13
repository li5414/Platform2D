using UnityEngine;

namespace druggedcode.engine
{
	/// <summary>
	/// 플레이어가 사거리 안에 들어오면 미사일을 발사. AISimpleWalk 와 같이 부착되어 이동하면서 공격하는 AI가 된다.
	/// </summary>
	public class AIShootOnSight : MonoBehaviour 
	{
		
		/// 발사속도
		public float FireRate = 1;
		// 발사 할 탄환
		public ProjectileOld Projectile;
	
		/// 공격할 사거리
		public float ShootDistance = 10f;
	
		// private stuff
		private float _canFireIn;
		private Vector2 _direction;
		private Vector2 _directionLeft;
		private Vector2 _directionRight;
		//  private DEController _controller;
		
		void Awake()
		{
			_directionLeft = new Vector2(-1,0);
			_directionRight = new Vector2(1,0);
	
			//  _controller = GetComponent<DEController>();
	
			_canFireIn = FireRate;
			
		}
		
		virtual protected void Fire()
		{
			// if the ray has hit the player, we fire a projectile
			//  Projectile projectile = (Projectile)Instantiate(Projectile, transform.position,transform.rotation);
			//  projectile.Initialize(gameObject,_direction,_controller.Velocity);
			_canFireIn=FireRate;
		}
		
		void Update () 
		{
			if (( _canFireIn -= Time.deltaTime) > 0)
			{
				return;
			}
	
			if ( transform.localScale.x < 0) 
			{
				_direction = -_directionLeft;
			}
			else
			{
				_direction = -_directionRight;
			}
			
			// 전방으로 레이 발사.
			Vector2 raycastOrigin = new Vector2( transform.position.x, transform.position.y-(transform.localScale.y/2) );
			RaycastHit2D raycast = Physics2D.Raycast(raycastOrigin,_direction,ShootDistance,1<<LayerMask.NameToLayer("Player"));
			if (!raycast)
				return;
		}
	}
}
