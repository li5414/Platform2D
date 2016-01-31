using UnityEngine;
using System.Collections;
/// <summary>
/// 지정된 목표점으로 움직이는 미사일
/// </summary>
public class PathedProjectile : MonoBehaviour
{
	/// 제거될 때 표시되는 이펙트 
	public GameObject DestroyEffect;
	
	/// 미사일의 도착지
	private Transform _destination;
	
	/// 속력
	private float _speed;
	
	//속도와 목적지를 초기화
	public void Initialize( Transform destination, float speed)
	{
		_destination=destination;
		_speed=speed;
	}
	
	/// <summary>
	/// 매프레임마다 목적지로 점차적으로 이동한다. 일정 거리보다 가까워지면 스스로를 제거
	/// </summary>
	void Update () 
	{
		transform.position = Vector3.MoveTowards( transform.position, _destination.position, Time.deltaTime * _speed );
		
		var distanceSquared = ( _destination.transform.position - transform.position ).sqrMagnitude;
		
		if(distanceSquared > .01f * .01f)
			return;
		
		if (DestroyEffect!=null)
		{
			Instantiate(DestroyEffect,transform.position,transform.rotation); 
		}
		
		Destroy(gameObject);
	}	
}
