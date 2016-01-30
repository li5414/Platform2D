using UnityEngine;
using System.Collections;

namespace druggedcode
{
    public class SmoothFollow : MonoBehaviour
    {
        
        public Transform target;
        public float distance = 10.0f;
        public float height = 5.0f;
        public float heightDamping = 2.0f;
        public float rotationDamping = 3.0f;
        
        void LateUpdate()
        {
            
            Follow();
        }
        
        void Follow()
        {
            
            if (!target)
                return;
            
            float wantedRotationAngle = target.eulerAngles.y;
            float wantedHeight = target.position.y + height;
            
            float currentRotationAngle = transform.eulerAngles.y;
            float currentHeight = transform.position.y;
            
            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
            
            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);
            
            Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);
            
            Vector3 pos = target.position - currentRotation * Vector3.forward * distance;
            pos.y = currentHeight;
            
            transform.position = pos;
            
            transform.LookAt(target);
        }
    }
}
