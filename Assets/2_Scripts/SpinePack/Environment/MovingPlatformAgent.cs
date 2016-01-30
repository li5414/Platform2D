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

public class MovingPlatformAgent : MonoBehaviour {

	public LayerMask platformMask;
	public float castDistance = 1;
	public float castRadius = 1;
	public bool useCircleMode = false;

	public MovingPlatform platform;

	Rigidbody2D rb;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D>();
	}

	// Update is called once per frame
	void Update () {

	}


	void FixedUpdate () {
		if (useCircleMode)
			CircleCheck();
		else
			RayCheck();

		if (platform != null) {
			Vector3 velocity = Vector3.zero;
			velocity.x = platform.Velocity.x;
			velocity.y = platform.Velocity.y;

			if (rb != null)
				rb.velocity = velocity;
			else
				transform.position += velocity * Time.deltaTime;
		}
	}

	void RayCheck () {
		RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(0, -1), castDistance, platformMask);
		platform = null;
		if (hit.transform != null) {
			if(!hit.collider.isTrigger)
				platform = hit.collider.GetComponent<MovingPlatform>();
		}
	}

	void CircleCheck () {
		var colliders = Physics2D.OverlapCircleAll(transform.position, castRadius, platformMask);

		platform = null;

		for (int i = 0; i < colliders.Length; i++) {
			var collider = colliders[i];
			if (collider != null) {
				//only care about stuff beneath
				if (!collider.isTrigger && collider.bounds.center.y < transform.position.y) {
					platform = collider.GetComponent<MovingPlatform>();
					break;
				}

			}
		}

	}
}
