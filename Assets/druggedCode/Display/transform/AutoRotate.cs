using UnityEngine;

namespace druggedcode
{
    /// <summary>
    /// 자동으로 회전한다.
    /// </summary>
    public class AutoRotate : MonoBehaviour
    {
        public float rotationSpeed = 100f;
        
        Transform _tr;
        
        void Awake()
        {
            _tr = GetComponent<Transform>();
        }
        
        void Update()
        {
            _tr.Rotate(rotationSpeed * Vector3.forward * Time.deltaTime);
        }
    }
}