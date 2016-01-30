/*****************************************************************************
 * Spine Asset Pack License
 * Version 1.0
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to use the Asset Pack and derivative works only as
 * incorporated and embedded components of your software applications and to
 * distribute such software applications. Any source code contained in the Asset
 * Pack may not be distributed in source form. You may otherwise not reproduce,
 * distribute, sublicense, rent, lease or lend the Asset Pack. It is emphasized
 * that you are not entitled to distribute or transfer the Asset Pack in any way
 * other way than as integrated components of your software applications.
 * 
 * THIS ASSET PACK IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS ASSET PACK, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/
using UnityEngine;
using System.Collections;

public class GunmanMissile : MonoBehaviour {

	public Transform graphics;
	public float initialSpeed;
	public float thrusterForce;
	public LayerMask targetMask;
	public GameObject impactPrefab;
	public string impactSound;
	public float damage;
	public float radius = 1.5f;
	public float impactForce = 20;
	public float lifeSpan = 3;
	public Thruster thruster;
	Rigidbody2D rb;
	bool thrustActive = false;
	int bounces = 0;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D>();
		rb.velocity = transform.right * initialSpeed;
		StartCoroutine(Activate(0));
	}

	void OnDrawGizmos () {
		Gizmos.DrawWireSphere(transform.position, radius);
	}


	void OnCollisionEnter2D (Collision2D collision) {

		bool hitTarget = false;
		bounces++;
		foreach (var cp in collision.contacts) {
			if ((1 << cp.collider.gameObject.layer & targetMask) > 0) {
				Impact(cp.point);
				hitTarget = true;
				break;
			}
		}

		if (bounces > 3 && !hitTarget)
			Impact(transform.position);
	}

	IEnumerator Activate (float delay) {
		yield return new WaitForSeconds(delay);
		thrustActive = true;
		thruster.goalThrust = 1;
		rb.velocity = transform.right * rb.velocity.magnitude;
	}

	void FixedUpdate () {
		if (thrustActive == true) {
			rb.AddForce(transform.right * thrusterForce * Time.deltaTime);
		}

		if(thrustActive && rb.velocity.magnitude > 0)
			graphics.rotation = Quaternion.Slerp(graphics.rotation, Quaternion.FromToRotation(Vector3.right, rb.velocity), Time.deltaTime * 10);
		
		/*
		float dist = speed * Time.deltaTime;
		RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, transform.right, dist, targetMask);
		bool impact = false;
		Collider2D hitCollider;
		Vector2 point = Vector2.zero;
		foreach (var hit in hits) {
			if (hit.collider.isTrigger)
				continue;

			hitCollider = hit.collider;
			point = hit.point;
			impact = true;
			break;
		}

		transform.Translate(dist, 0, 0);

		if (impact) {

		}*/
	}

	void Impact (Vector2 point) {
		Collider2D[] colliders = Physics2D.OverlapCircleAll(point, radius);
		foreach (var c in colliders) {
			if (c.isTrigger)
				continue;

			if (c.attachedRigidbody != null) {
				Vector2 origin = point - (Vector2)transform.right;
				Vector2 dir = point - origin;
				c.attachedRigidbody.SendMessage("Hit", new HitData(damage, origin, point, (Vector2)(dir * impactForce) + new Vector2(0, 5)), SendMessageOptions.DontRequireReceiver);
			}

		}

		Instantiate(impactPrefab, point, transform.rotation);
		SoundPalette.PlaySound(impactSound, 1, 1, transform.position);
		Destroy(gameObject);
	}
}
