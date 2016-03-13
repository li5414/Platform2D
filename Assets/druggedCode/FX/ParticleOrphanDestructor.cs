using UnityEngine;
using System.Collections;

namespace druggedcode
{
    public class ParticleOrphanDestructor : MonoBehaviour
    {

        bool orphaned = false;
        ParticleSystem pSys;

        void Awake()
        {
            pSys = GetComponent<ParticleSystem>();
        }
        void Update()
        {
            if (!orphaned)
            {
                if (transform.parent == null)
                {
                    orphaned = true;

                    ParticleSystem.EmissionModule em = pSys.GetComponent<ParticleSystem>().emission;
                    em.enabled = false;

                    pSys.loop = false;
                }
            }
            else
            {
                if (pSys.IsAlive(true) == false)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
