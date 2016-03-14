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
		public float lifeTime = 10f;

		override protected void Move ()
		{
			if (destination == null)
			{
				mTr.Translate (transform.right * speed * Time.deltaTime, Space.World);

				if ((lifeTime -= Time.deltaTime ) <= 0f)
				{
					Disappear ();
				}
			}
			else
			{
				mTr.position = Vector3.MoveTowards (mTr.position, destination.position, Time.deltaTime * speed);
				var distanceSquared = (destination.position - mTr.position).sqrMagnitude;

				if (distanceSquared > .01f * .01f) return;

				Disappear ();
			}
		}
	}
}