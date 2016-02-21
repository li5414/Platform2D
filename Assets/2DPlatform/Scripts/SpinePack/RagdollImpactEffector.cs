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
			SoundPalette.PlaySound(impactSound, 0.5f, 1, transform.position);
		nextImpactTime = Time.time + 0.2f;
	}

	void Hit (HitData data) {
		Hit(data.velocity * 3);
	}
}