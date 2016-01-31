using UnityEngine;

namespace druggedcode.engine
{
    /// <summary>
    /// 캐릭터에 붙이면 탄환을 발사 할 수 있게 된다. ShootOnce,ShootStart,ShootStop 는 캐릭터를 통하지 않고 Input 에서 바로 호출된다.
    /// SetHorizontalAxis, SetVerticalAxis 가 InputManager 에서 직접 호출되는데 붙여진 캐릭터의 값을 가져오는 방법을 고려
    /// </summary>
    public class CharacterShoot : MonoBehaviour
    {
        ///Weapon 으로 사용할 무기 프리팹 (건)
        public Weapon InitialWeapon;

        ///무기가 부착될 위치
        public Transform WeaponAttachment;

        /// true 로 지정하면 8방향으로 쏠 수 있게 된다
        public bool EightDirectionShooting = true;
        /// true 라면 엄격한 8방향으로 쏜다
        public bool StrictEightDirectionShooting = true;


        private Weapon _weapon;
        private float _fireTimer;

        private float _horizontalMove;
        private float _verticalMove;

        private DECharacter _character;
        //  private DEController _controller;

        void Start()
        {
            _character = GetComponent<DECharacter>();
            //  _controller = GetComponent<DEController>();

            // WeaponAttachment 가 없다면 본체에 붙인다.
            if (WeaponAttachment == null)
                WeaponAttachment = transform;

            ChangeWeapon(InitialWeapon);
        }

        /// 무기를 변경
        public void ChangeWeapon(Weapon weaponPrefab)
        {
            // 만약 무기를 가지고 있다면 스톱시킨다.
            if (_weapon != null)
            {
                ShootStop();
            }

            // 무기를 생성하고 교체. 초기화 한다.
            _weapon = (Weapon)Instantiate(weaponPrefab, WeaponAttachment.transform.position, WeaponAttachment.transform.rotation);
            _weapon.transform.parent = transform;
            _weapon.SetGunFlamesEmission(false);
            _weapon.SetGunShellsEmission(false);
        }

        /// 총을 한번 쏜다.
        public void ShootOnce()
        {
            if (_character.Permissions.ShootEnabled == false || _character.State.IsDead)
                return;

            //  //발사 방향을 초기화 한다,( 등반 중에 총을 쏘려고 하는 등의 경우 )
            //  if (_character.State.CanShoot == false)
            //  {
            //      _character.State.FiringDirection = 3;
            //      return;
            //  }

            // 탄을 발사하고 발사 시각을 초기화 한다
            FireProjectile();
            _fireTimer = 0;
        }

        /// 지속적으로 총발사를 한다.
        public void ShootStart()
        {
            if (_character.Permissions.ShootEnabled == false || _character.State.IsDead)
                return;

            //  if (_character.State.CanShoot == false)
            //  {
            //      _character.State.FiringDirection = 3;
            //      return;
            //  }

            // 캐릭터 상태 변경
            //  _character.State.FiringStop = false;
            //  _character.State.Firing = true;

            //무기 효과 재생
            _weapon.SetGunFlamesEmission(true);
            _weapon.SetGunShellsEmission(true);

            _fireTimer += Time.deltaTime;

            //이전발사시각으로 부터 흐른 시간이 무기의 재사용시간보다 크면 발사
            if (_fireTimer > _weapon.FireRate)
            {
                // 탄을 발사하고 발사 시각을 초기화 한다
                FireProjectile();
                _fireTimer = 0;
            }
        }

        //총발사 중지
        public void ShootStop()
        {
            if (_character.Permissions.ShootEnabled == false)
                return;

            //  if (_character.State.CanShoot == false)
            //  {
            //      _character.State.FiringDirection = 3;
            //      return;
            //  }

            // 캐릭터 상태 변경
            //  _character.State.FiringStop = true;
            //  _character.State.Firing = false;

            //  // 발사 초기화
            //  _character.State.FiringDirection = 3;
            _weapon.GunFlames.enableEmission = false;
            _weapon.GunShells.enableEmission = false;
        }

        /// 무기의 탄환을 발사 한다
        void FireProjectile()
        {
            float HorizontalShoot = _horizontalMove;
            float VerticalShoot = _verticalMove;

            if (_weapon.ProjectileFireLocation == null)
                return;

            // 8방향 발사가 false 이면 두개의 방향을 0로 설정한다 
            if (EightDirectionShooting == false)
            {
                HorizontalShoot = 0;
                VerticalShoot = 0;
            }

            // 엄격한 8방향 모드인 경우 방향의 값을 round 처리
            if (StrictEightDirectionShooting)
            {
                HorizontalShoot = Mathf.Round(HorizontalShoot);
                VerticalShoot = Mathf.Round(VerticalShoot);
            }

            //방향
            float angle = Mathf.Atan2(HorizontalShoot, VerticalShoot) * Mathf.Rad2Deg;

            Vector2 direction = Vector2.up;

            // 유저가 다른 방향 버튼을 누르고 있지 않다면 ( 0 인 경우 포함 ) 발사 방향을 캐릭터의 방향을 베이스로 한다
            if (HorizontalShoot > -0.1f && HorizontalShoot < 0.1f && VerticalShoot > -0.1f && VerticalShoot < 0.1f)
            {
                bool _isFacingRight = transform.localScale.x > 0;
                angle = _isFacingRight ? 90f : -90f;

            }

            //캐릭터의 슈팅에 의존해 애니메이션을 설정 

            //  // 위
            //  if (Mathf.Abs(HorizontalShoot) < 0.1f && VerticalShoot > 0.1f)
            //      _character.State.FiringDirection = 1;

            //  // 대각선 위 
            //  if (Mathf.Abs(HorizontalShoot) > 0.1f && VerticalShoot > 0.1f)
            //      _character.State.FiringDirection = 2;

            //  // 대각선 아래
            //  if (Mathf.Abs(HorizontalShoot) > 0.1f && VerticalShoot < -0.1f)
            //      _character.State.FiringDirection = 4;

            //  // 아래
            //  if (Mathf.Abs(HorizontalShoot) < 0.1f && VerticalShoot < -0.1f)
            //      _character.State.FiringDirection = 5;

            //  //기본
            //  if (Mathf.Abs(VerticalShoot) < 0.1f)
            //      _character.State.FiringDirection = 3;

            direction = Quaternion.Euler(0, 0, -angle) * direction;

            //탄환 
            //  var projectile = (Projectile)Instantiate(_weapon.Projectile, _weapon.ProjectileFireLocation.position, _weapon.ProjectileFireLocation.rotation);
            //  projectile.Initialize(gameObject, direction, _controller.Velocity);

            if (_weapon.GunShootFx != null)
                SoundManager.Instance.PlaySound(_weapon.GunShootFx, transform.position);
        }

        private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle)
        {

            angle = angle * (Mathf.PI / 180f);
            var rotatedX = Mathf.Cos(angle) * (point.x - pivot.x) - Mathf.Sin(angle) * (point.y - pivot.y) + pivot.x;
            var rotatedY = Mathf.Sin(angle) * (point.x - pivot.x) + Mathf.Cos(angle) * (point.y - pivot.y) + pivot.y;
            return new Vector3(rotatedX, rotatedY, 0);
        }

        public void SetHorizontalAxis(float value)
        {
            _horizontalMove = value;
        }

        public void SetVerticalAxis(float value)
        {
            _verticalMove = value;
        }

        public void Flip()
        {
            if (_weapon.GunShells != null)
                _weapon.GunShells.transform.eulerAngles = new Vector3(_weapon.GunShells.transform.eulerAngles.x, _weapon.GunShells.transform.eulerAngles.y + 180, _weapon.GunShells.transform.eulerAngles.z);
            if (_weapon.GunFlames != null)
                _weapon.GunFlames.transform.eulerAngles = new Vector3(_weapon.GunFlames.transform.eulerAngles.x, _weapon.GunFlames.transform.eulerAngles.y + 180, _weapon.GunFlames.transform.eulerAngles.z);
        }

        void Update()
        {

        }
    }
}