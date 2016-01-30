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

public class GunmanGun : GunmanWeapon {

	public Transform muzzle;
	public GameObject impactPrefab;
	public GameObject linePrefab;
	public float damage = 1;
	public LayerMask targetMask;
	public LayerMask blockMask;
	public float range = 10;
	public float force = 50;
	public float recoil = 4;
	public bool piercing;

	public override void Fire () {
		Vector3 position = muzzle.position;
		Vector3 dir = muzzle.right;
		clip--;

		RaycastHit2D[] hits = Physics2D.RaycastAll(position, dir, range, targetMask);


		Vector2 farthestPoint = position;
		//int end = piercing ? hits.Length : 1;
		bool validHit = false;
		for (int i = 0; i < hits.Length; i++) {
			var hit = hits[i];
			if (hit.collider.isTrigger)
				continue;

			Instantiate(impactPrefab, hit.point, Quaternion.Euler(0, 0, Random.Range(0, 360)));

			if (hit.collider.attachedRigidbody != null) {
				hit.collider.attachedRigidbody.SendMessage("Hit", new HitData(damage, position, hit.point, dir * force), SendMessageOptions.DontRequireReceiver);
			}
			farthestPoint = hit.point;
			validHit = true;
			if (((1 << hit.collider.gameObject.layer) & blockMask) > 0) {
				break;
			}

			if (!piercing)
				break;
		}

		float dist = Vector2.Distance(position, farthestPoint);
		if (dist > 0 || (dist == 0 && !validHit)) {
			var go = (GameObject)Instantiate(linePrefab, position, Quaternion.FromToRotation(Vector3.right, dir));
			if (dist == 0)
				dist = range;

			go.transform.localScale = new Vector3(dist, 1, 1);
		} else if (dist == 0 && validHit) {
			//do nothing, inside object
		}
		

	}

	public override Vector2 GetRecoil () {
		Vector2 r = muzzle.right * this.recoil;
		r.y *= 0.4f;
		return r;
	}
}
