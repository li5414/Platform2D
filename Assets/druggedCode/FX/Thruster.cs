
using UnityEngine;
using System.Collections;

namespace druggedcode
{
    public class Thruster : MonoBehaviour
    {
        public ParticleSystem pSys;
        public new AudioSource audio;
        [Range(0, 1)]
        public float thrust;
        [Range(0, 1)]
        public float goalThrust;
        public float emissionRate = 200;
        public float adjustSpeed = 5;

        ParticleSystem.EmissionModule mEM;

        void Awake()
        {
            mEM = pSys.GetComponent<ParticleSystem>().emission;
        }

        void OnEnable()
        {
            mEM.enabled = false;
        }

        void Update()
        {
            thrust = Mathf.Lerp(thrust, goalThrust, Time.deltaTime * adjustSpeed);
            if (thrust > 0.05f)
            {
                mEM.enabled = true;
                audio.mute = false;
            }
            else
            {
                mEM.enabled = false;
                audio.mute = true;
            }


            ParticleSystem.MinMaxCurve mmc = new ParticleSystem.MinMaxCurve();
            mmc.constantMax = Mathf.Lerp(0, emissionRate, thrust);
            mEM.rate = mmc;
            transform.localScale = Vector3.one * thrust;
            audio.volume = thrust;

            audio.pitch = Mathf.Lerp(0.4f, 1f, Mathf.InverseLerp(0.5f, 2f, Time.timeScale));
        }

    }
}
