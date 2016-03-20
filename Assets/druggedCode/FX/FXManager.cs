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

		public GameObject SpawnFX( GameObject prefab, Vector3 pos )
		{
			return SpawnFX( prefab,pos, Quaternion.identity, Vector3.one );
		}

		public GameObject SpawnFX( GameObject prefab, Vector3 pos, Quaternion r )
		{
			return SpawnFX( prefab,pos, r, Vector3.one );
		}

		public GameObject SpawnFX( GameObject prefab, Vector3 pos, Vector3 scale )
		{
			return SpawnFX( prefab,pos, Quaternion.identity, scale );
		}

		public GameObject SpawnFX( GameObject prefab, Vector3 pos, Quaternion r, Vector3 scale )
		{
			if( prefab == null ) return null;
			GameObject go = Instantiate( prefab, pos, r ) as GameObject;
			go.transform.SetParent( mTr );
			go.transform.localScale = scale;
			return go;
		}
//			where T: DTS
		public T SpawnFX<T>( T prefab, Vector3 pos ) where T: MonoBehaviour
		{
			return SpawnFX<T>( prefab,pos, Quaternion.identity, Vector3.one );
		}

		public T SpawnFX<T>( T prefab, Vector3 pos, Quaternion r ) where T: MonoBehaviour
		{
			return SpawnFX<T>( prefab,pos, r, Vector3.one );
		}

		public T SpawnFX<T>( T prefab, Vector3 pos, Vector3 scale ) where T: MonoBehaviour
		{
			return SpawnFX<T>( prefab,pos, Quaternion.identity, scale );
		}

		public T SpawnFX<T>( T prefab, Vector3 pos, Quaternion r, Vector3 scale ) where T: MonoBehaviour
		{
			GameObject go = SpawnFX( prefab.gameObject, pos, r, scale );
			if( go == null ) return null;
			else return go.GetComponent<T>();
		}
	}
}

