using UnityEngine;
using System.Collections;
using Spine;

namespace druggedcode.engine
{
    public class DECharacter : MonoBehaviour, IDamageable
    {
        public enum Facing
        {
            RIGHT,
            LEFT
        }
        //----------------------------------------------------------------------------------------------------------
        // parameters
        //----------------------------------------------------------------------------------------------------------
        [Header("Animation Type")]
        public AnimationType animType;
        public enum AnimationType
        {
            Spine,
            Sprite,
            Mechanim
        }

        [Header("Health")]
        /// 최대hp
        public int MaxHealth = 100;

        public float AccelOnGround = 10f;
        public float AccelOnAir = 10f;

        [Header("Speed")]
        /// 이동속도. 런타임중 상황에 따라 3가지 속도들로 변경된다.
        public float CrouchSpeed = 2f;
        public float WalkSpeed = 4f;
        public float RunSpeed = 8f;
        public float LadderSpeed = 2f;

        [Header("Jump")]
        ///점프 높이
        public float JumpHeight = 15f;
        ///최대 점프 가능 횟수
        public int JumpNum = 3;


        //----------------------------------------------------------------------------------------------------------
        // --
        //----------------------------------------------------------------------------------------------------------
        public HealthState _healthState;

        [SerializeField]
        EffectAndSound _effectAndSound;

		protected bool mIsActive;

        public DECharacterPermissions Permissions;

        //----------------------------------------------------------------------------------------------------------
        // etc
        //----------------------------------------------------------------------------------------------------------
		public LocationLinker currentManualLinker{ get;set; }
		public DialogueZone currentDialogueZone{ get;set; }
		public Ladder currentLadder{ get;set; }

        //----------------------------------------------------------------------------------------------------------
        // cach
        //----------------------------------------------------------------------------------------------------------
        protected Transform _tr;
        protected DEController _controller;
        public DEController controller { get{ return _controller; }}
        protected DECharacterState _state;
        public DECharacterState State { get { return _state; } }

		protected AI mAI;
        protected Color _initialColor;

        //캐릭터의 중력을 활성화 하거나 비활성화 할때 이전 중력값을 기억하기 위한 용도
        protected float _originalGravity;

        protected Facing _facing = Facing.RIGHT;
        public Facing CurrentFacing { get { return _facing; } }
		public float horizontalAxis { get; set;}
		public float verticalAxis { get; set; }

        public float CurrentVX { get; set; }

        protected PlayerFSM mFsm;
        public PlayerFSM fsm { get { return mFsm; } }

        protected SkeletonAnimation _anim;
        protected TrackEntry _currentEntry;

        protected Vector3 _headCheckerPos;
        protected Vector3 _headExtents;
        
        bool mMoveLocked;

        virtual protected void Awake()
        {
            _tr = transform;

            _controller = GetComponent<DEController>();

            _state = new DECharacterState();

            _anim = GetComponentInChildren<SkeletonAnimation>();

			mAI = GetComponent<AI>();
			mFsm = new PlayerFSM(this, _controller);
			_healthState.Init(MaxHealth, MaxHealth);

			Transform head = _tr.Find("headChecker");
			if (head != null)
			{
				BoxCollider2D headCheckerCol = head.GetComponent<BoxCollider2D>();
				_headExtents = headCheckerCol.bounds.extents;
				_headCheckerPos = head.transform.localPosition;
				headCheckerCol.enabled = false;
			}

			if (GetComponent<Renderer>() != null) _initialColor = GetComponent<Renderer>().material.color;
            
            mMoveLocked = false;
        }

        virtual protected void Start()
        {
            /*/
            _anim.state.Start += OnStart;
            _anim.state.End += OnEnd;
            _anim.state.Complete += OnComplete;
            _anim.state.Event += OnEvent;
            //*/
        }


        protected virtual void Update()
        {
			if( mIsActive == false ) return;
            if (_state.IsDead) return;

            mFsm.Update();
            _state.Reset();
        }

        void LateUpdate()
        {
			if( mIsActive == false ) return;

            //막 지상에 착지했다면 점프 수를 초기화 한다.
            if (_controller.State.JustGotGrounded)
            {
                ResetJump();
            }

            // 막 땅에 닿았다면 먼지 이펙트를 생성
            if (_controller.State.JustGotGrounded)
            {
                if (_effectAndSound.TouchTheGroundEffect != null) Instantiate(_effectAndSound.TouchTheGroundEffect, transform.position, transform.rotation);
            }
        }

        public void Kill()
        {
            GravityActive(true);
            _state.TriggerDead = true;
            //todo. respawn after dead motion.
        }

        public void Spawn( CheckPoint cp )
        {
            _tr.position = cp.transform.position;

            SetFacing(Facing.RIGHT);
            ResetJump();

			mFsm.SetState( PlayerFSM.PlayerState.IDLE );

            /* reset state
            InDialogueZone = false;
            CurrentDialogueZone = null;
            */
        }

        //캐릭터에 의존하는 파티클 등이 있다면 localScale 반전 외에 추가적 조작이 필요하다.
        public void SetFacing(Facing facing)
        {
            _facing = facing;

            switch (_facing)
            {
                case Facing.RIGHT:
                    _tr.localScale = new Vector3(1f, _tr.localScale.y, _tr.localScale.z);
                    break;

                case Facing.LEFT:
                    _tr.localScale = new Vector3(-1f, _tr.localScale.y, _tr.localScale.z);
                    break;
            }
        }

        //----------------------------------------------------------------------------------------------------------
        // anim
        //----------------------------------------------------------------------------------------------------------
        public bool AnimFlip
        {
            set { _anim.skeleton.FlipX = value; }
        }

        public TrackEntry SetAnimation(string animationName, bool loop = true, int trackIndex = 0)
        {
            //        if (_currentEntry != null && _currentEntry.Animation.name == animationName)
            //        {
            //            return _currentEntry;
            //        }

            _currentEntry = _anim.state.SetAnimation(trackIndex, animationName, loop);
            return _currentEntry;
        }

        void ChangeColor()
        {
            //            SkeletonAnimation anim = GetComponent<SkeletonAnimation>();
            //
            //            foreach (string slotname in slots){
            //                foreach (Spine.Slot slot in anim.skeleton.slots){
            //                    if (slotname.Equals(slot.data.name)){
            //                        slot.R = color.r;
            //                        slot.G = color.g;
            //                        slot.B = color.b;
            //                        slot.A = color.a;
            //                    }
            //                }
            //            }
        }

        public bool HasAnim(string animName)
        {
            if (_anim.state.Data.SkeletonData.FindAnimation(animName) == null)
                return false;
            else
                return true;
        }

        //----------------------------------------------------------------------------------------------------------
        // axis
        //----------------------------------------------------------------------------------------------------------

        virtual public void Move()
        {
            if( mMoveLocked ) return;
            
            if (_facing == Facing.LEFT && horizontalAxis > 0.1f)
            {
                SetFacing(Facing.RIGHT);
            }
            else if (_facing == Facing.RIGHT && horizontalAxis < -0.1f)
            {
                SetFacing(Facing.LEFT);
            }
            
            if ( horizontalAxis == 0)
            {
                _controller.vx = 0;
            }
            else
            {
                float vx = horizontalAxis * CurrentVX;
                var movementFactor = _controller.State.IsGrounded ? AccelOnGround : AccelOnAir;
                _controller.vx = Mathf.Lerp(_controller.vx, vx, Time.deltaTime * movementFactor);
                // _controller.vx = vx;
            }
        }
        
        //_state.CanMove 와 관계없이 일정시간 동안 움직임을 제어하지 못하게 한다.
        public void MoveLock( float duration )
        {
            StartCoroutine(MoveLockRoutine(duration));
        }
        
        IEnumerator MoveLockRoutine(float duration)
        {
			mMoveLocked = true;
			yield return new WaitForRealSeconds( duration );
            mMoveLocked = false;
        }

        //----------------------------------------------------------------------------------------------------------
        // behaviour
        //----------------------------------------------------------------------------------------------------------

		//run
        public void Run()
        {
            if (Permissions.RunEnabled == false)
                return;

            _state.IsRun = true;
        }

        public void StopRun()
        {
            _state.IsRun = false;
        }

		//jump
        // 아래로 이동시켜 떨어질 수 있도록 한다. 잠깐 동안 캐릭터의 플랫폼 충돌을 끈다
        public void Jump()
        {
            if (Permissions.JumpEnabled == false || _state.JumpLeft < 1)
                return;

            if ( verticalAxis < -0.1f )
            {
                mFsm.JumpBelow();
            }
            else
            {
                mFsm.Jump();
            }
        }

        public void DoJumpBelow()
        {
            _controller.transform.position = new Vector2(_tr.position.x, _tr.position.y - 0.1f);
            _controller.State.IsCollidingBelow = false;
            _controller.State.StandingPlatfom = null;
            _controller.DisableCollisions(0.2f);
        }
        
        public void DoWallJump()
        {
            MoveLock(0.2f);
            if ( horizontalAxis < 0)
            {
                SetFacing(DECharacter.Facing.RIGHT);
                _controller.vx = RunSpeed * 1.3f;
            }
            else if ( horizontalAxis > 0)
            {
                SetFacing(DECharacter.Facing.LEFT);
                _controller.vx = -RunSpeed * 1.3f;
            }

            DoJump();
        }

        public void DoJump()
        {
            // 무빙 플랫폼에( 혹은 움직이는 바닥 ) 잠깐동안 플랫폼 충돌을 끈다.플랫폼이 위로 움직일때 캐릭터가 점프를 하면 끼는 현상 방지
            if (_controller.State.IsGrounded && _controller.State.StandingPlatfom.isMovable)
            {
                _controller.DisableCollisions(0.1f);
            }
            
            _controller.State.IsCollidingBelow = false;
            _controller.State.StandingPlatfom = null;

            //float jumpPower = Mathf.Clamp( Mathf.Sqrt( 2f * JumpHeight * Mathf.Abs( _controller.gravity )),3,100000);
            float jumpPower = Mathf.Sqrt( 2f * JumpHeight * Mathf.Abs( _controller.gravity ));
            _controller.vy = jumpPower;
            
            _state.JumpLatestTime = Time.time;
            if (_effectAndSound.JumpSfx != null) SoundManager.Instance.PlaySound(_effectAndSound.JumpSfx, transform.position);
            
            UseJump();
        }

        public void UseJump()
        {
            _state.JumpLeft -= 1;
            _state.JumpCount += 1;
        }

        public void ResetJump()
        {
            _state.JumpLeft = JumpNum;
            _state.JumpCount = 0;
        }

		//attack
        public void Attack()
        {
            mFsm.Attack();
        }

        /// <summary>
        /// 플레이어가 데미지를 입었을 때 호출된다. ICanTakeDamage 구현
        /// </summary>
        public virtual void TakeDamage(int damage, GameObject instigator)
        {
            //hit sound play
            //hit effect instantiate

            // 일정시간동안 레이어12 와 13 과의 충돌을 제거한다 (탄,적)
            Physics2D.IgnoreLayerCollision(9, 12, true);
            Physics2D.IgnoreLayerCollision(9, 13, true);

            StartCoroutine(ResetLayerCollision(0.5f));

            // 캐릭터의 sprite 를 반짝거리게 만든다.
            if (GetComponent<Renderer>() != null)
            {
                Color flickerColor = new Color32(255, 20, 20, 255);
                StartCoroutine(Flicker(_initialColor, flickerColor, 0.05f));
            }

            //데미지만큼 hp 감소

            //            Health -= damage;
            //            if (Health<=0)
            //            {
            //                LevelManager.Instance.KillPlayer();
            //            }
        }

        /// <summary>
        /// 일정시간 이후 layer 12(미사일) 와 13 (적 )과 다시 충돌 가능하도록 만든다.
        /// </summary>
        IEnumerator ResetLayerCollision(float delay)
        {
            yield return new WaitForSeconds(delay);
            Physics2D.IgnoreLayerCollision(9, 12, false);
            Physics2D.IgnoreLayerCollision(9, 13, false);
        }

		//----------------------------------------------------------------------------------------------------------
		// contorl
		//----------------------------------------------------------------------------------------------------------

		public void Stop()
		{
			horizontalAxis = 0;
			verticalAxis = 0;

			_controller.Stop();
		}

        // 캐릭터의 중력을 활성화 하거나 비활성화한다.
        public void GravityActive(bool state)
        {
            if (state)
            {
                if (_controller.GravityScale == 0)
                {
                    _controller.GravityScale = _originalGravity;
                }
            }
            else
            {
                if (_controller.GravityScale != 0)
                {
                    _originalGravity = _controller.GravityScale;
                }
                _controller.GravityScale = 0;
            }
        }

        // 캐릭터의sprite 를 깜짝이는 코루틴 ( 예를들어 다쳤을 때 )
        IEnumerator Flicker(Color initialColor, Color flickerColor, float flickerSpeed)
        {
            if (GetComponent<Renderer>() != null)
            {
                for (var n = 0; n < 10; n++)
                {
                    GetComponent<Renderer>().material.color = initialColor;
                    yield return new WaitForSeconds(flickerSpeed);
                    GetComponent<Renderer>().material.color = flickerColor;
                    yield return new WaitForSeconds(flickerSpeed);
                }
                GetComponent<Renderer>().material.color = initialColor;
            }
        }

        //-----------------------------------------------------------------------------------------------
        // -- 매니저가 관리하는게 나을수도?
        //-----------------------------------------------------------------------------------------------

        public void UpdatePhysicInfo(PhysicInfo physicInfo)
        {
            _controller.SetPhysicsSpace(physicInfo);
        }

        public void ResetPhysicInfo()
        {
            _controller.ResetPhysicInfo();
        }

        public void OnTriggerEnter2D(Collider2D collider)
        {
            //  Debug.Log("character trigger. " + collider.name + " me: " + gameObject.name);
        }

        //----------------------------------------------------------------------------------------------------------
        // get;set;
        //----------------------------------------------------------------------------------------------------------
        public TrackEntry currentTrackEntry
        {
            get { return _currentEntry; }
        }

        public float currentAnimationDuration
        {
            get { return _currentEntry.animation.Duration; }
        }

        public bool IsCollidingHead
        {
            get
            {
                if (_headExtents != Vector3.zero)
                {
                    bool check = Physics2D.OverlapArea(_tr.position + _headCheckerPos - _headExtents, _tr.position + _headCheckerPos + _headExtents, DruggedEngine.MASK_PLATFORM);
                    return check;
                }
                else
                {
                    return _controller.State.IsCollidingAbove;
                }
            }
        }

        //----------------------------------------------------------------------------------------------------------
        // event
        //----------------------------------------------------------------------------------------------------------

        public void OnStart(Spine.AnimationState state, int trackIndex)
        {
            Debug.Log(" - onStart > " + trackIndex + " " + state.GetCurrent(trackIndex));
        }

        public void OnEnd(Spine.AnimationState state, int trackIndex)
        {
            Debug.Log(" - onEnd > " + trackIndex + " " + state.GetCurrent(trackIndex));
        }

        public void OnComplete(Spine.AnimationState state, int trackIndex, int loopCount)
        {
            Debug.Log(" - onComplete > " + trackIndex + " " + state.GetCurrent(trackIndex) + ": loopCount " + loopCount);
        }

        public void OnEvent(Spine.AnimationState state, int trackIndex, Spine.Event e)
        {
            Debug.Log(" - onEvent > " + trackIndex + " " + state.GetCurrent(trackIndex) + ": event " + e + ", " + e.Int);
        }
    }

    [System.Serializable]
    public class EffectAndSound
    {
        [Space(10)]
        [Header("Particle Effects")]
        public GameObject DamagedEffect;
        public GameObject HealEffect;
        public GameObject DeadEffect;
        public GameObject TouchTheGroundEffect;

        [Space(10)]
        [Header("Sounds")]
        public AudioClip DamagedSfx;
        public AudioClip HealSfx;
        public AudioClip DeadSfx;
        public AudioClip JumpSfx;
    }
}
