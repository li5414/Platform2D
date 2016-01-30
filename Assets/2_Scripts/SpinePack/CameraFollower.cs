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
using System.Collections.Generic;

public class CameraFollower : MonoBehaviour {
	static List<Transform> targets = new List<Transform>();
	public static Coroutine AddTemporaryTarget (Transform t, float duration) {
		return instance.StartCoroutine(instance.TemporaryTarget(t, duration));
	}

	public static void AddTarget (Transform t) {
		targets.Add(t);
	}

	public static void RemoveTarget (Transform t) {
		targets.Remove(t);
	}

	static CameraFollower instance;

	public Transform target;
	public Vector3 min;
	public Vector3 max;
	public Vector3 offset;
	public float speed = 10;
	public Vector3 rigidbodyFactor;
	Rigidbody2D rb;

	void OnEnable () {
		instance = this;
	}

	void Start () {
		if (target != null)
			targets.Add(target);
	}

	IEnumerator TemporaryTarget (Transform t, float duration) {
		targets.Add(t);
		yield return new WaitForSeconds(duration);
		targets.Remove(t);
	}

	Vector3 GetGoalPosition (Transform t) {
		Vector3 goalPos = t.position + offset;

		if (t.GetComponent<Rigidbody2D>()) {
			Vector3 velocity = t.GetComponent<Rigidbody2D>().velocity;
			velocity.x *= rigidbodyFactor.x;
			velocity.y *= rigidbodyFactor.y;

			goalPos += velocity;
		}


		goalPos.x = Mathf.Clamp(goalPos.x, min.x, max.x);
		goalPos.y = Mathf.Clamp(goalPos.y, min.y, max.y);
		goalPos.z = Mathf.Clamp(goalPos.z, min.z, max.z);

		return goalPos;
	}

	void LateUpdate () {
		if (targets.Count == 0)
			return;

		Vector3 goalPos = Vector3.zero;
		foreach (Transform t in targets) {
			goalPos += GetGoalPosition(t);
		}

		goalPos /= targets.Count;

		goalPos.x = Mathf.Clamp(goalPos.x, min.x, max.x);
		goalPos.y = Mathf.Clamp(goalPos.y, min.y, max.y);
		goalPos.z = Mathf.Clamp(goalPos.z, min.z, max.z);

		transform.position = Vector3.Lerp(transform.position, goalPos, speed * Time.deltaTime);
	}
}
