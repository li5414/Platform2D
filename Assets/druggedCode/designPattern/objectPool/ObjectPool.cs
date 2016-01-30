using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace druggedcode
{
    /// <summary>
    /// 폴링 되는 오브젝트는 필요 시 OnEnable 혹은 OnDisable 를 구현한다
    /// </summary>
    public class ObjectPool : Singleton<ObjectPool>
    {
        static int _helperIndex;
        static int _helperLength;
        static GameObject _helperObj;
        static Pool _helperPool;
        
        public GameObject[] allowGameObjects;
        public int[] amountToBuffer;
        public int defaultBufferAmount = 1;
        public bool canGrow = true;
        
        Dictionary< string, Pool> containerDic;
        
        override protected void Awake()
        {
            base.Awake();
            
            containerDic = new Dictionary<string, Pool>();
            
            //        Debug.Log("prefabs len:" + allowGameObjects.Length);
            
            GameObject prefab;
            for (int index = 0; index < allowGameObjects.Length; ++index)
            {
                prefab = allowGameObjects [index];
                
                if (prefab == null)
                    continue;
                
                int bufferAmount;
                if (index < amountToBuffer.Length) 
                    bufferAmount = amountToBuffer [index];
                else
                    bufferAmount = defaultBufferAmount;
                
                //            Debug.Log("\t buffer is :" + bufferAmount + ", name is :" + prefab.name);
                
                for (int bufferCount = 0; bufferCount < bufferAmount; bufferCount++)
                {
                    ToPool( createGameObject( prefab ));
                }
            }
        }
        
        GameObject createGameObject( GameObject prefab )
        {
            GameObject obj = (GameObject)Instantiate(prefab);
            obj.name = prefab.name;
            return obj;
        }
        
        public bool IsAllow(GameObject obj)
        {
            if( obj == null ) return false;
            
            _helperIndex = 0;
            _helperLength = allowGameObjects.Length;
            
            for (_helperIndex = 0; _helperIndex < _helperLength; ++_helperIndex)
            {
                _helperObj = allowGameObjects [_helperIndex];
                if (_helperObj.name == obj.name)
                    return true;
            }
            
            return false;
        }
        
        public void ToPool(GameObject obj)
        {
            if (IsAllow(obj) == false)
                return;
            
            _helperPool = getPool( obj );
            _helperPool.Add( obj );
        }
        
        public GameObject FromPool(GameObject obj )
        {
            if (IsAllow(obj) == false)
                return null;
            
            _helperPool = getPool( obj );
            
            if( _helperPool.Count > 0 )
            {
                return _helperPool.Get();
            }
            else if( canGrow )
            {
                return createGameObject( obj );
            }
            
            return null;
        }
        
        
        Pool getPool( GameObject obj )
        {
            Pool pool;
            if (containerDic.TryGetValue( obj.name, out pool))
                return pool;
            else
            {
                pool = new Pool( obj.name );
                pool.Container.transform.parent = transform;
                containerDic.Add( obj.name, pool );
                return pool;
            }
        }
        
        
        class Pool
        {
            string _name;
            GameObject _container;
            List<GameObject> _poolList;
            
            public Pool( string poolName )
            {
                _name = poolName;
                _container = new GameObject( _name + "_PoolContainer" );
                _poolList = new List<GameObject>();
            }
            
            public void Add( GameObject obj )
            {
                obj.SetActive(false);
                obj.transform.parent = _container.transform;
                
                if( _poolList.Contains( obj ) == false )
                    _poolList.Add( obj );
                
            }
            
            public GameObject Get()
            {
                GameObject obj = _poolList[0];
                _poolList.RemoveAt(0);
                obj.transform.parent = null;
                obj.SetActive( true );
                return obj;
                
            }
            
            public int Count
            {
                get{ return _poolList.Count; }
                
            }
            
            public GameObject Container
            {
                get{ return _container; }
            }
            
            
            public string Name
            {
                get{ return _name; }
                set{ _name = value; }
            }
        }
    }
}

