using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace druggedcode
{
    /// <summary>
    /// 지정한 경로를 따라 움직이도록 한다.
    /// </summary>
    public class PathFollow : MonoBehaviour 
    {
        public enum FollowType
        {
            MoveTowards,
            Lerp
        }
        
        public FollowType Type = FollowType.MoveTowards;
        public float Speed = 1;
        public float MaxDistanceToGoal = .1f;
        public bool initOnFirstPath;

        [SerializeField]
        Transform[] _paths;
        [SerializeField]
        Vector2 _velocity;
        [SerializeField]
        Vector2 _translateVector;

        IEnumerator<Transform> _pathEnumerator;
        Transform _trans;
        Vector3 _latestPos;
        Vector3 _nextPosition;

        
        public void Start ()
        {
            if( _paths == null )
            {
                Debug.LogError("Path Cannot be null", gameObject);
                return;
            }


            _trans = transform;
            _latestPos = _trans.position;

            SetPaths( _paths );

            if( initOnFirstPath )
            {
                _trans.position = _nextPosition;
            }
        }

        public void SetPaths( Transform[] paths )
        {
            _paths = paths;

            if( _paths == null )
            {
                _pathEnumerator = null;
                return;
            }

            _pathEnumerator = GetPathEnumerator();

            MoveNext();
        }

        void MoveNext()
        {
            if( _pathEnumerator.MoveNext())
            {
                _nextPosition = _pathEnumerator.Current.position;
            }
            else
            {
                _nextPosition = _trans.position;
            }
        }

        public void Update ()
        {
            if( _pathEnumerator == null || _pathEnumerator.Current == null )
                return;
            
            if( Type == FollowType.MoveTowards )
            {
                _trans.position = Vector3.MoveTowards(_trans.position, _nextPosition, Time.deltaTime * Speed);
            }
            else if( Type == FollowType.Lerp )
            {
                _trans.position = Vector3.Lerp(_trans.position, _nextPosition, Time.deltaTime * Speed);
            }
            
            var distanceSquared = ( _trans.position - _nextPosition).sqrMagnitude;
            if( distanceSquared < MaxDistanceToGoal * MaxDistanceToGoal )
            {
                MoveNext();
            }

            _translateVector = _trans.position - _latestPos;
            _velocity = _translateVector / Time.deltaTime;

            _latestPos = _trans.position;
            
            //Debug.Log( "PathUpdate : " + _latestPos.y + " : " + _translateVector.y + " : " + _velocity.y );
        }

        public IEnumerator<Transform> GetPathEnumerator()
        {
            if( _paths == null || _paths.Length < 1)
            {
                yield break;
            }
            
            var direction = 1;
            var index = 0;
            while (true)
            {
                yield return _paths[index];
                
                if( _paths.Length == 1 )
                {
                    continue;
                }
                
                if( index <= 0 )
                {
                    direction = 1;
                }
                else if( index >= _paths.Length - 1 )
                {
                    direction = -1;
                }
                
                index = index + direction;
            }
        }

        void OnDrawGizmos()
        {
            if( Application.isEditor == false || _paths == null || _paths.Length < 2)
                return;


            var points = _paths.Where(t=> t != null).ToList();
            
            if (points.Count < 2)
                return;
            
            for (var i = 1; i < points.Count; i++)
            {
                Gizmos.DrawLine(points[i - 1].position, points[i].position);
            }
        }

        public Vector2 velocity
        {
            get
            {
                return _velocity;
            }
        }

        public Vector2 translateVector
        {
            get
            {
                return _translateVector;
            }
        }
    }
}
