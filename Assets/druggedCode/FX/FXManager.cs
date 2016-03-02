using UnityEngine;
using System.Collections;

namespace druggedcode
{
	public class FXManager : Singleton<FXManager>
	{
		public int maxFX = 11;

		Transform mTr;
		override protected void Awake()
		{
			base.Awake();

			mTr = transform;
		}

		public void SpawnFX( GameObject prefab, Vector3 pos )
		{
			SpawnFX( prefab,pos, Quaternion.identity, Vector3.one );
		}

		public void SpawnFX( GameObject prefab, Vector3 pos, Quaternion r )
		{
			SpawnFX( prefab,pos, r, Vector3.one );
		}

		public void SpawnFX( GameObject prefab, Vector3 pos, Vector3 scale )
		{
			SpawnFX( prefab,pos, Quaternion.identity, scale );
		}

		public void SpawnFX( GameObject prefab, Vector3 pos, Quaternion r, Vector3 scale )
		{
			if( prefab == null ) return;
			GameObject go = Instantiate( prefab, pos, r ) as GameObject;
			go.transform.localScale = scale;
		}
	}
}

