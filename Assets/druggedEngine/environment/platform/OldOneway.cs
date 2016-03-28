using UnityEngine;
using System.Collections;

//edge 가 바닥 collider로 되어있고 아래를 판단하는 트리거 영역이 box로 되어있다
public class OldOneway : MonoBehaviour {

	public new Collider2D collider;
	public Collider2D trigger;
	public float fastFallingThreshhold = -3;

	void OnTriggerEnter2D (Collider2D collider)
	{
		if (collider.attachedRigidbody.velocity.y > fastFallingThreshhold) {
			Physics2D.IgnoreCollision(this.collider, collider, true);
		}
	}

	void OnTriggerExit2D (Collider2D collider) {
		StartCoroutine(DelayedCollision(collider));
	}

	IEnumerator DelayedCollision (Collider2D collider) {
		yield return new WaitForSeconds(0.1f);
		Physics2D.IgnoreCollision(this.collider, collider, false);
	}
}
