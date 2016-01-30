using UnityEngine;
using System.Collections;

namespace druggedcode
{
    [ExecuteInEditMode]
    /// <summary>
    /// 패럴렉스 이동이 필요한 객체에 부착
    /// 이 스크립트는 David Dion-Paquet 의
    /// http://www.gamasutra.com/blogs/DavidDionPaquet/20140601/218766/Creating_a_parallax_system_in_Unity3D_is_harder_than_it_seems.php
    /// 글을 기반으로 한다.
    /// </summary>

    public class ParallaxElement : MonoBehaviour 
    {
        public bool EditMode;

        public float HorizontalSpeed;
        public float VerticalSpeed;

        /// 카메라의 방향과 같은지 아닌지 정의
        public bool MoveCamsSameDirection;

        // private
        Vector3 _previousCameraPosition;
        bool _previousMoveParallax;
        Transform _cameraTransform;
        Transform _tr;

        void Awake()
        {
            _tr = transform;
        }
        
        void Start()
        {
            Camera mainCamera = Camera.main;
            if( mainCamera == null ) return;
            
            _cameraTransform = mainCamera.transform;
            _previousCameraPosition = _cameraTransform.position;
        }

        /// <summary>
        /// 매 프레임마다 카메라의 위치에 따라 시차 레이어를 이동
        /// </summary>
        void Update () 
        {
            if (EditMode == false && Application.isPlaying == false )
                return;

            Vector3 distance = _cameraTransform.position - _previousCameraPosition;

            float direction = (MoveCamsSameDirection) ? -1f : 1f;

            _tr.position += Vector3.Scale(distance, new Vector3(HorizontalSpeed, VerticalSpeed)) * direction;

            _previousCameraPosition = _cameraTransform.position;
        }
    }
}