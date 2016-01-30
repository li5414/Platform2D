using UnityEngine;
using System.Collections;

namespace druggedcode
{
    public class GameObjectUtil {

        static public GameObject Create( string name, Transform parent = null )
        {
            GameObject go = new GameObject( name );

            if( parent != null )
            {
                go.transform.SetParent( parent );
            }

            return go;
        }

        static public T Create<T>( string name, Transform parent = null ) where T : Component
        {
            return Create( name, parent ).AddComponent<T>();
        }

        static public Transform CreateEmptyContainer(string name, Transform parent = null)
        {
            return Create( name, parent ).transform;
        }
    }
}

