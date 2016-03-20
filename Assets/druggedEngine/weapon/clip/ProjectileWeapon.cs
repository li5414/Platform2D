using UnityEngine;

namespace druggedcode.engine
{
	public class ProjectileWeapon : ClipWeapon
    {
        public GameObject projectilePrefab;
        public Transform muzzle;
        public LayerMask targetMask;
        public float recoil = 4;

        [SpineSlot]
        public string missleSlot;
        public SkeletonRenderer weaponRenderer;

        public override void Setup()
        {
            if (clip == 0)
            {
                weaponRenderer.skeleton.FindSlot(missleSlot).Attachment = null;
            }
        }

        public override void Attack()
        {
            Vector3 position = muzzle.position;
            Vector3 dir = muzzle.right;
            clip--;
            Instantiate(projectilePrefab, position, Quaternion.FromToRotation(Vector3.right, dir));
        }

        public override Vector2 GetRecoil()
        {
            Vector2 r = muzzle.right * this.recoil;
            r.y *= 0.4f;
            return r;
        }
    }
}

