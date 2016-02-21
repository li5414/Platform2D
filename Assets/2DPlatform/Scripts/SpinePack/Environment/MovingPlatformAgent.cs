using UnityEngine;
using System.Collections;
using druggedcode.engine;

public class MovingPlatformAgent : MonoBehaviour
{

	public LayerMask platformMask;
	public float castDistance = 1;
	public float castRadius = 1;
	public bool useCircleMode = false;

	public Platform platform;

	Rigidbody2D rb;

	// Use this for initialization
	void Start ()
	{
		rb = GetComponent<Rigidbody2D> ();
	}

	void Update ()
	{

	}


	void FixedUpdate ()
	{
		if (useCircleMode)
			CircleCheck ();
		else
			RayCheck ();

		if (platform != null)
		{
			Vector3 velocity = Vector3.zero;
			velocity.x = platform.velocity.x;
			velocity.y = platform.velocity.y;

			if (rb != null)
				rb.velocity = velocity;
			else
				transform.position += velocity * Time.deltaTime;
		}
	}

	void RayCheck ()
	{
		RaycastHit2D hit = Physics2D.Raycast (transform.position, new Vector2 (0, -1), castDistance, platformMask);
		platform = null;
		if (hit.transform != null)
		{
			if (!hit.collider.isTrigger)
				platform = hit.collider.GetComponent<Platform> ();
		}
	}

	void CircleCheck ()
	{
		var colliders = Physics2D.OverlapCircleAll (transform.position, castRadius, platformMask);

		platform = null;

		for (int i = 0; i < colliders.Length; i++)
		{
			var collider = colliders [i];
			if (collider != null)
			{
				//only care about stuff beneath
				if (!collider.isTrigger && collider.bounds.center.y < transform.position.y)
				{
					platform = collider.GetComponent<Platform> ();
					break;
				}

			}
		}

	}
}
