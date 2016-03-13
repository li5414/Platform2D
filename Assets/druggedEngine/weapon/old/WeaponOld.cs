using UnityEngine;

namespace druggedcode.engine
{
    /// <summary>
    /// Weapon parameters
    /// </summary>
    public class WeaponOld : MonoBehaviour
    {
        /// 발사될 탄환 프리팹
        public ProjectileOld Projectile;

        /// 발사 빈도
        public float FireRate;

        /// 발사 시 재생할 파티클 이펙트
        public ParticleSystem GunFlames;
        public ParticleSystem GunShells;

        /// 탄환이 생성될 위치 
        public Transform ProjectileFireLocation;
        public AudioClip GunShootFx;

		ParticleSystem.EmissionModule mFrameEM;
		ParticleSystem.EmissionModule mGunEM;
        void Start()
        {
			mFrameEM = GunFlames.emission;
			mGunEM = GunShells.emission;

            SetGunFlamesEmission(false);
            SetGunShellsEmission(false);
        }

        public void SetGunFlamesEmission(bool state)
        {
			mFrameEM.enabled = state;
        }

        public void SetGunShellsEmission(bool state)
        {
			mGunEM.enabled = state;
        }
    }
}
