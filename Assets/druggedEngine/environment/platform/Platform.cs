using UnityEngine;
using System;

namespace druggedcode.engine
{
    [RequireComponent(typeof(Collider2D))]
    [ExecuteInEditMode]
    public class Platform : MonoBehaviour
    {
        [Range(0f,1f)]
        public float friction = 1f;
        public WallClinType WallClingType = WallClinType.NOTHING;
        
        public enum WallClinType
        {
            NOTHING,
            LEFT,
            RIGHT,
            BOTH
        }
        
        protected Collider2D _collider;
        protected PathFollow _pathFollow;
        virtual protected void Awake()
        {
            _collider = GetComponent<Collider2D>();
            if (_collider == null)
            {
                throw new Exception("Platform have to attaced 'Collider2D'!  gameObject: [ " + gameObject.name + " ]");
            }

            _pathFollow = GetComponent<PathFollow>();

        }

        virtual protected void Start()
        {
            UpdateLayer();
            UpdateSortingLayer();
        }

        virtual protected void UpdateLayer()
        {
            LayerUtil.ChangeLayer(gameObject, DruggedEngine.NormalPlatform);
        }
        
        virtual protected void UpdateSortingLayer()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var ren in renderers)
            {
                ren.sortingLayerName = DruggedEngine.SORTING_LAYER_PLATFORM;
            }
        }

        public bool isMovable
        {
            get
            {
                if( _pathFollow == null ) return false;
                else return true;
            }
        }

        public Vector2 translateVector
        {
            get
            {
                if( _pathFollow == null )
                {
                    return Vector2.zero;
                }
                else
                {
                    return _pathFollow.translateVector;
                }
            }
        }

        public Vector2 velocity
        {
            get
            {
                if( _pathFollow == null )
                {
                    return Vector2.zero;
                }
                else
                {
                    return _pathFollow.velocity;
                }
            }
        }
    }
    
}