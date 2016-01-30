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

public class AnimatedFXHelper : MonoBehaviour {

	public bool destroyWhenFinished;
	public float autoDestructDelay;
	public bool setAnimationSpeed;
	public float animationSpeed;
	public Vector3 translate;
	public Space translateSpace;
	public Vector3 rotate;
	public Space rotateSpace;
	public Vector3 scale;

	public Vector2 surfaceCast;
	public Vector2 surfaceOffset;
	public LayerMask surfaceMask;
	

	public void PlaySound (string sound) {
		SoundPalette.PlaySound(sound, 1, 1, transform.position);
	}

	public void Remove () {
		Destroy(gameObject);
	}

	public void RemoveObject (Object obj) {
		Destroy(obj);
	}

	void Start () {
		if (setAnimationSpeed) {
			Animation animation = GetComponent<Animation>();
			if (animation) {
				foreach (AnimationState state in animation) {
					state.speed = animationSpeed;
				}
			}
		}
		
		if (destroyWhenFinished)
			StartCoroutine(DestroyWhenFinished());

		if (autoDestructDelay > 0)
			Destroy(gameObject, autoDestructDelay);

		if (surfaceCast.sqrMagnitude > 0) {
			
			Vector3 dir = surfaceCast;
			dir.x *= Mathf.Sign(transform.localScale.x);
			dir.y *= Mathf.Sign(transform.localScale.y);
			dir = transform.TransformDirection(dir);
			RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, surfaceCast.magnitude, surfaceMask);
			if (hit.collider != null) {
				transform.position = hit.point;
				transform.Translate(surfaceOffset);
			}
		}
	}

	void Update () {
		if (translate.sqrMagnitude > 0)
			transform.Translate(translate * Time.deltaTime, translateSpace);

		if (rotate.sqrMagnitude > 0)
			transform.Rotate(rotate * Time.deltaTime, rotateSpace);

		if (scale.sqrMagnitude > 0)
			transform.localScale += scale * Time.deltaTime;	
	}

	IEnumerator DestroyWhenFinished () {
		yield return new WaitForSeconds(0.1f);
		Animation animation = GetComponent<Animation>();
		while (animation.isPlaying)
			yield return null;

		Remove();
	}
}
