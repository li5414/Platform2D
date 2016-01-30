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
using Spine;

public class GunmanWeapon : MonoBehaviour {

	//data
	public new string name;
	[SpineAnimation(startsWith: "Setup")]
	public string setupAnim;
	[SpineAnimation(startsWith: "Idle")]
	public string idleAnim;
	[SpineAnimation(startsWith: "Aim")]
	public string aimAnim;
	[SpineAnimation(startsWith: "Fire")]
	public string fireAnim;
	[SpineAnimation(startsWith: "Reload")]
	public string reloadAnim;
	public GameObject casingPrefab;
	public Transform casingEjectPoint;
	public float minAngle = -40;
	public float maxAngle = 40;
	public float refireRate = 0.5f;
	public int clipSize = 10;
	public int clip = 10;
	public int ammo = 50;

	public Spine.Animation SetupAnim;
	public Spine.Animation IdleAnim;
	public Spine.Animation AimAnim;
	public Spine.Animation FireAnim;
	public Spine.Animation ReloadAnim;

	//states & locks
	public bool reloadLock;
	public float nextFireTime = 0;

	public void CacheSpineAnimations (SkeletonData data) {
		SetupAnim = data.FindAnimation(setupAnim);
		IdleAnim = data.FindAnimation(idleAnim);
		AimAnim = data.FindAnimation(aimAnim);
		FireAnim = data.FindAnimation(fireAnim);
		ReloadAnim = data.FindAnimation(reloadAnim);
	}

	public virtual void Setup () {

	}

	public virtual void Fire () {
		Debug.LogWarning("Not implemented!");
	}

	public virtual bool Reload () {
		if (ammo == 0)
			return false;

		int refill = clipSize;
		if (refill > ammo)
			refill = clipSize - ammo;
		ammo -= refill;
		clip = refill;

		return true;
	}

	public virtual Vector2 GetRecoil () {
		return Vector2.zero;
	}
}