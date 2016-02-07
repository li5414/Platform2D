using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    public class DEPlayer : DECharacter
    {
        [Header("Dash")]
        /// 대쉬 지속,파워,쿨다운
        public float DashDuration = 0.15f;
        public float DashForce = 5f;
        public float DashCooldown = 2f;

		override protected void Awake()
		{
			base.Awake();
			tag = Config.TAG_PLAYER;
		}

        override protected void Start()
        {
            base.Start();
            
            _state.CanWallClinging = true;
		}

        public void DeActive()
        {
            if( mIsActive == false ) return;

            Stop();
            Controllable( false );

            _controller.enabled = false;

            mIsActive = false;
        }

        public void Active()
        {
            if( mIsActive ) return;

            Controllable( true );

            _controller.enabled = true;

            mIsActive = true;
        }

        public void Controllable( bool value )
        {
            if( mAI != null ) mAI.enabled = value;
        }

        //-----------------------------------------------------------------------------------------------
        // -- special behavior
        //-----------------------------------------------------------------------------------------------

        /// <summary>
        /// 캐릭터가 대쉬 하거나 다이브 ( 대쉬 시작 시 수직 axis 에 의존 )
        /// </summary>
        public void Dash()
        {
            //  float _dashDirection;
            //  float _boostForce;

            //  if (Permissions.DashEnabled == false || State.IsDead)
            //      return;

            //  if (State.CanMoveFreely == false)
            //      return;

            //  //dash
            //  if (_verticalAxis > -0.8)
            //  {
            //      if (State.CanDash)
            //      {
            //          State.Dashing = true;

            //          if (_facing)
            //          {
            //              _dashDirection = 1f;
            //          }
            //          else
            //          {
            //              _dashDirection = -1f;
            //          }

            //          _boostForce = _dashDirection * DashForce;

            //          State.CanDash = false;

            //          StartCoroutine(Boost(DashDuration, _boostForce, 0, "dash"));
            //      }
            //  }

            //  //dive
            //  if (_verticalAxis < -0.8)
            //  {
            //      _controller.CollisionsOn();
            //      StartCoroutine(Dive());
            //  }
        }

        /// <summary>
        /// </summary>
        IEnumerator Boost(float boostDuration, float boostForceX, float boostForceY, string name)
        {
            yield return null;
            
            /*
            float time = 0f;

            // 전달된 boostDuratino 동안 부스트힘을 콘트롤러에 전달
            while (boostDuration > time)
            {
                if (boostForceX != 0)
                {
                    _controller.AddHorizontalForce(boostForceX);
                }
                if (boostForceY != 0)
                {
                    _controller.AddVerticalForce(boostForceY);
                }

                time += Time.deltaTime;
                yield return 0;
            }
            //boost 가 완료된 뒤 대쉬였다면 대쉬를 멈추고 대쉬 쿨다운을 돌린다.
            if (name == "dash")
            {
                State.Dashing = false;
                yield return new WaitForSeconds(DashCooldown);
                State.CanDash = true;
            }
            if (name == "wallJump")
            {
                //필요한가?
            }
            */
        }

        /// <summary>
        /// 수직 플레이어 다이빙
        /// </summary>
        IEnumerator Dive()
        {
            yield return null;
            /*
            State.Diving = true;

            // 플레이어가 땅에 닿을때까지 아래로 빠르게 힘을 준다.
            while (_controller.State.IsGrounded == false)
            {
                _controller.vy = DruggedEngine.Gravity * 2;//추후 질량을 늘리는 방법으로 변경
                yield return 0;
            }

            // shake 파라메터( 격렬함, 지속시간, 쇠퇴( decay ).
            //  Vector3 ShakeParameters = new Vector3(1.5f, 0.3f, 0.1f);
            //_sceneCamera.Shake(ShakeParameters);
            State.Diving = false;
            */
        }
    }
}

