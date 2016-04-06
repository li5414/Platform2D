using UnityEngine;

namespace druggedcode.engine
{
	public class ShotgunWeapon : MonoBehaviour
    {
        public Transform muzzle;
        public GameObject impactPrefab;
        public GameObject linePrefab;
        public float damage = 1;
        public int pellets = 5;
        public float spread = 15;
        public LayerMask targetMask;
        public float range = 10;
        public float force = 20;
        public float recoil = 1;

//        public override void Attack()
//        {
//            Vector3 position = muzzle.position;
//            Vector3 dir = muzzle.right;
//            clip--;
//            for (int i = 0; i < pellets; i++)
//            {
//                Vector3 modDir = Quaternion.Euler(0, 0, Random.Range(-spread, spread)) * dir;
//                RaycastHit2D hit = Physics2D.Raycast(position, modDir, range, targetMask);
//                float dist = range;
//                if (hit.collider != null)
//                {
//                    if (hit.collider.attachedRigidbody != null)
//                    {
//                        hit.collider.attachedRigidbody.SendMessage("Hit", new HitData(damage, modDir * force), SendMessageOptions.DontRequireReceiver);
//                    }
//                    dist = Vector3.Distance(hit.point, position);
//                    Instantiate(impactPrefab, hit.point, Quaternion.Euler(0, 0, Random.Range(0, 360)));
//                }
//
//                var go = (GameObject)Instantiate(linePrefab, position, Quaternion.FromToRotation(Vector3.right, modDir));
//                go.transform.localScale = new Vector3(dist, 1, 1);
//            }
//        }
//
//        public override Vector2 GetRecoil()
//        {
//            Vector2 r = muzzle.right * this.recoil;
//            r.y *= 0.75f;
//            return r;
//        }
    }
}

