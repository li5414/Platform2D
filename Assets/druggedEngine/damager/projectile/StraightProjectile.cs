using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	/// <summary>
	/// destination 가 있다면 매프레임마다 목적지로 점차적으로 이동한다. 일정 거리보다 가까워지면 스스로를 제거
	/// 주어진 속도로 직진한다. 방향 조절은 각을 조절한다. lifreTime 이 지나면 사라진다.
	/// </summary>
	public class StraightProjectile : Projectile
	{
		override protected void Move ()
		{
			//탄환 이동. 설정된 방향 * ( 발사한 주인의 속도를 통해 구한 xAxis 값(절대값) + 지정한 탄환 속도 )
			transform.Translate(direction * speed * Time.deltaTime, Space.World);
//			mTr.Translate (transform.right * speed * Time.deltaTime, Space.World);
		}
	}
}