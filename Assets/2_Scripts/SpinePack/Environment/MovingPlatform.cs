using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour {

	public Vector2 DeltaPosition {
		get {
			return deltaPosition;
		}
	}

	public Vector2 Velocity {
		get {
			return velocity;
		}
	}

	public Vector2 treadmill;

	Vector2 deltaPosition;
	Vector2 lastPosition;
	Vector2 velocity;

	void Awake () {
		lastPosition = transform.position;
	}

	void FixedUpdate () {
		Vector2 pos = transform.position;
		deltaPosition = (pos - lastPosition);

		velocity = deltaPosition / Time.deltaTime;
		velocity += (Vector2)transform.TransformDirection(treadmill);

		lastPosition = pos;
	}
}

