using UnityEngine;

namespace druggedcode.engine
{
    public class GunWeapon : Weapon
    {
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

        public override void Fire()
        {
            Vector3 position = muzzle.position;
            Vector3 dir = muzzle.right;
            clip--;

            RaycastHit2D[] hits = Physics2D.RaycastAll(position, dir, range, targetMask);


            Vector2 farthestPoint = position;
            //int end = piercing ? hits.Length : 1;
            bool validHit = false;
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit.collider.isTrigger)
                    continue;

                Instantiate(impactPrefab, hit.point, Quaternion.Euler(0, 0, Random.Range(0, 360)));

                if (hit.collider.attachedRigidbody != null)
                {
                    hit.collider.attachedRigidbody.SendMessage("Hit", new HitData(damage, position, hit.point, dir * force), SendMessageOptions.DontRequireReceiver);
                }
                farthestPoint = hit.point;
                validHit = true;
                if (((1 << hit.collider.gameObject.layer) & blockMask) > 0)
                {
                    break;
                }

                if (!piercing)
                    break;
            }

            float dist = Vector2.Distance(position, farthestPoint);
            if (dist > 0 || (dist == 0 && !validHit))
            {
                var go = (GameObject)Instantiate(linePrefab, position, Quaternion.FromToRotation(Vector3.right, dir));
                if (dist == 0)
                    dist = range;

                go.transform.localScale = new Vector3(dist, 1, 1);
            }
            else if (dist == 0 && validHit)
            {
                //do nothing, inside object
            }
        }

        public override Vector2 GetRecoil()
        {
            Vector2 r = muzzle.right * this.recoil;
            r.y *= 0.4f;
            return r;
        }
    }
}

