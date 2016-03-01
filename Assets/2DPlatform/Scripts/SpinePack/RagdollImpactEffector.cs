
using UnityEngine;
using System.Collections;
using druggedcode;

public class RagdollImpactEffector : MonoBehaviour {

	static float nextImpactTime = 0;

	public string impactSound = "Impact/Random";
	float spawnTime;

	void Awake () {
		spawnTime = Time.time;
	}

	void OnTriggerEnter2D (Collider2D collider) {
		if (Time.time < spawnTime + 0.25f)
			return;

		var boundingBoxFollower = collider.GetComponent<BoundingBoxFollower>();
		if (boundingBoxFollower != null) {
			var attachmentName = boundingBoxFollower.CurrentAttachmentName;

			int fromSign = collider.transform.position.x < transform.position.x ? -1 : 1;

			switch (attachmentName) {
				case "Punch":
					Hit(new Vector2(-fromSign * 20, 8));
					break;
				case "UpperCut":
					Hit(new Vector2(-fromSign * 20, 75));
					break;
				case "HeadDive":
					Hit(new Vector2(0, 30));
					break;
			}
		}
	}

	void Hit (Vector2 v) {
		GetComponent<Rigidbody2D>().velocity = v;
		if (Time.time > nextImpactTime)
        {
			SoundManager.Instance.PlaySound(impactSound, 0.5f, 1, transform.position);
        }
		nextImpactTime = Time.time + 0.2f;
	}

	void Hit (HitData data) {
		Hit(data.velocity * 3);
	}
}