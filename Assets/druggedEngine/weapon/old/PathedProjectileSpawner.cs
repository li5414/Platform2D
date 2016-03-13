using UnityEngine;
using System.Collections;
/// <summary>
/// 목적지 탄환 (PathedProjectile 을 생성(발사)
/// 목적지가 변하면 실시간으로 따라가기 때문에 유도탄일수도 있다.
/// </summary>
public class PathedProjectileSpawner : MonoBehaviour 
{
	/// 발사할 탄환의 목적지
	public Transform Destination;
	
	/// 생성될 탄환 프리팹
	public PathedProjectile Projectile;
	
	/// 발사 이펙트
	public GameObject SpawnEffect;
	
	/// 탄환 속력 
	public float Speed;
	
	/// 빈도수
	public float FireRate;
	
	private float _nextShotInSeconds;
	
	void Start () 
	{
		_nextShotInSeconds = FireRate;
	}

	/// <summary>
	/// 매프레임 시간을 체크하여 일정 시간마다 탄환을 발사한다.
	/// </summary>
	void Update () 
	{
		if(( _nextShotInSeconds -= Time.deltaTime ) > 0)
			return;
			
		_nextShotInSeconds = FireRate;
		
		var projectile = (PathedProjectile) Instantiate(Projectile, transform.position,transform.rotation);
		projectile.Initialize(Destination,Speed);
		
		if ( SpawnEffect != null )
		{
			Instantiate( SpawnEffect, transform.position, transform.rotation );
		}
	}

	/// <summary>
	/// 포대에서 목적지까지 선을 긋는다.
	/// </summary>
	public void OnDrawGizmos()
	{
		if ( Destination == null )
			return;
		
		Gizmos.color = Color.red;
		Gizmos.DrawLine( transform.position, Destination.position );
	}
}
