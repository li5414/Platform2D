using UnityEngine;
using System.Collections;

namespace druggedcode
{
    public class Casing : MonoBehaviour
    {

        public Vector2 minVelocity;
        public Vector2 maxVelocity;
        public float minRotation;
        public float maxRotation;
        public float lifeSpan = 1;
        public bool localAxis;

        void Start()
        {
            var rb = GetComponent<Rigidbody2D>();

            Vector2 velocity = new Vector2(Random.Range(minVelocity.x, maxVelocity.x), Random.Range(minVelocity.y, maxVelocity.y));

            if (localAxis)
                velocity = transform.TransformDirection(velocity);

            rb.velocity = velocity;
            rb.angularVelocity = Random.Range(minRotation, maxRotation);

            Destroy(gameObject, lifeSpan);
        }
    }
}

