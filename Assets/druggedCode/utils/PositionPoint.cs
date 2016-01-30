using UnityEngine;
using System.Collections;

namespace druggedcode
{
    public class PositionPoint : MonoBehaviour
    {
        public float range = 0f;

        Transform _tr;

        void Awake()
        {
            _tr = GetComponent<Transform>();
        }

        public Vector3 GetPosition()
        {
            return _tr.position;
        }

        public Vector3 GetRandomCirclePosition()
        {
            Vector2 ran = Random.insideUnitCircle * range;
            return new Vector3(_tr.position.x + ran.x, _tr.position.y + ran.y, _tr.position.z);
        }

        public Vector3 GetRandomSpherePosition()
        {
            return _tr.position + Random.insideUnitSphere * range;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.1f);

            if (range > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, range);
            }
        }
    }
}

