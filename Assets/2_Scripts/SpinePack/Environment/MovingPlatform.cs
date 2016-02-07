using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour
{
    public Vector2 treadmill;

    public Vector2 Velocity;
    public Vector2 deltaPosition;
    Vector2 mLastPosition;

    void Awake()
    {
        mLastPosition = transform.position;
    }

    void FixedUpdate()
    {
        Vector2 pos = transform.position;
        deltaPosition = (pos - mLastPosition);

        Velocity = deltaPosition / Time.deltaTime;
        Velocity += (Vector2)transform.TransformDirection(treadmill);

        mLastPosition = pos;
    }
}

