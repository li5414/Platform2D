using UnityEngine;
namespace druggedcode.engine
{
    public class PlayerFSM
    {

        public enum PlayerState
        {
            NULL,
            IDLE,
            WALK,
            RUN,
            LOOKUP,
            CROUCH,
            CROUCH_MOVE,
            JUMP,
            FALL,
            WALL_CLING,
            LADDER_CLIMB,
            LADDER_CLIMB_MOVE,
            DASH,
            DEAD
        }

        DEController _controller;
        DEControllerState _controllerState;

        DECharacter _character;
        DECharacterState _characterState;

        PlayerState _currentState = PlayerState.NULL;

        override public string ToString()
        {
            return _currentState.ToString();
        }

        public PlayerFSM(DECharacter ch, DEController controller)
        {
            _character = ch;
            _characterState = _character.State;

            _controller = controller;
            _controllerState = _controller.State;

            _currentState = PlayerState.NULL;
        }

        public void SetState(PlayerState nextState)
        {
            if (_currentState == nextState) return;

            StateExit();
            
            //Debug.Log( _currentState + " > " + nextState );
            _currentState = nextState;

            StateEnter();
        }

        public PlayerState CurrentState
        {
            get{ return _currentState; }
        }

        public void Update()
        {
            StateUpdate();
        }

        protected virtual void StateEnter()
        {
            switch (_currentState)
            {
                case PlayerState.IDLE:
                    _character.GravityActive(true);
                    _character.SetAnimation("idle");
                    _character.CurrentVX = _character.WalkSpeed;
                    break;

                case PlayerState.WALK:
                    _character.GravityActive(true);
                    _character.SetAnimation("walk");
                    _character.CurrentVX = _character.WalkSpeed;
                    break;

                case PlayerState.RUN:
                    _character.GravityActive(true);
                    _character.SetAnimation("run");
                    _character.CurrentVX = _character.RunSpeed;
                    break;

                case PlayerState.LOOKUP:
                    _character.GravityActive(true);
                    _character.SetAnimation("lookup");
                    //_sceneCamera.LookUp();
                    break;

                case PlayerState.JUMP:
                    _character.GravityActive(true);
                    _character.SetAnimation("jump",false);
                    break;

                case PlayerState.FALL:
                    _character.GravityActive(true);
                    _character.SetAnimation("fall", false);
                    break;

                case PlayerState.WALL_CLING:
                    _character.GravityActive(true);
                    _character.SetAnimation("wall_cling");

                    _controller.GravityScale = 0.1f;
                    _character.AnimFlip = true;
                    _character.Stop();
                    _character.ResetJump();
                    break;

                case PlayerState.CROUCH:
                    _character.GravityActive(true);
                    _character.SetAnimation("crouch");

                    _character.CurrentVX = _character.CrouchSpeed;
                    _controller.UpdateColliderSize(1f, 0.7f);
                    break;

                case PlayerState.CROUCH_MOVE:
                    _character.SetAnimation("crouch_move");

                    _character.CurrentVX = _character.CrouchSpeed;
                    _controller.UpdateColliderSize(1f, 0.7f);
                    break;

                case PlayerState.LADDER_CLIMB:
                    _character.GravityActive(false);

                    _character.SetAnimation("ladderClimb");

                    _character.Stop();
                    _characterState.IsLadderClimb = true;
                    _character.ResetJump();
                    break;

                case PlayerState.LADDER_CLIMB_MOVE:
                    _characterState.IsLadderClimb = true;
                    _character.SetAnimation("ladderClimb_move");
                    break;

                case PlayerState.DASH:
                    //  _character.SetAnimation(0, "run", true);
                    //  _character.GravityActive(false);
                    //  _character.Stop();
                    break;

                case PlayerState.DEAD:
                    _characterState.IsDead = true;
                    _character.SetAnimation("dead", false);
                    break;
            }
        }

        protected virtual void StateUpdate()
        {
            if (_characterState.TriggerDead)
            {
                SetState(PlayerState.DEAD);
                return;
            }

            switch (_currentState)
            {
                case PlayerState.IDLE:
                    if (CheckFall()) return;
                    if (CheckLadderClimb()) return;
                    if (CheckLookUp()) return;
                    if (CheckCrouch()) return;

                    if (_character.horizontalAxis != 0f) SetState(PlayerState.WALK);
                    else _character.Move();
                    break;

                case PlayerState.WALK:
                    if (CheckFall()) return;
                    if (CheckLadderClimb()) return;
                    if (CheckLookUp()) return;
                    if (CheckCrouch()) return;

                    if (_character.horizontalAxis == 0f)
                    {
                        SetState(PlayerState.IDLE);
                    }
                    else if (_characterState.IsRun)
                    {
                        SetState(PlayerState.RUN);
                    }
                    else
                    {
                        _character.Move();
                    }

                    break;

                case PlayerState.RUN:
                    if (CheckFall()) return;
                    if (CheckLadderClimb()) return;
                    if (CheckCrouch()) return;

                    if (_characterState.IsRun == false || _character.horizontalAxis == 0f)
                    {
                        SetState(PlayerState.IDLE);
                    }
                    else
                    {
                        _character.Move();
                    }

                    break;

                case PlayerState.LOOKUP:
                    if (CheckFall()) return;
                    if (CheckCrouch()) return;

                    if (_character.verticalAxis < 0.1f || _character.horizontalAxis != 0)
                    {
                        SetState(PlayerState.IDLE);
                    }

                    break;

                case PlayerState.JUMP:
                    if (CheckAirToGround()) return;
                    if (CheckLadderClimb()) return;
                    if (CheckWallClinging()) return;
                    if (_controllerState.IsFalling)
                    {
                        SetState(PlayerState.FALL);
                    }
                    else
                    {
                        _character.Move();
                    }

                    break;

                case PlayerState.FALL:
                    if (CheckAirToGround()) return;
                    if (CheckLadderClimb()) return;
                    if (CheckWallClinging()) return;

                    _character.Move();

                    break;


                case PlayerState.WALL_CLING:
                    if (CheckAirToGround()) return;

                    if (_controllerState.HittedClingWall == null || _character.horizontalAxis == 0f)
                    {
                        SetState(PlayerState.FALL);
                        return;
                    }

                    break;

                case PlayerState.CROUCH:
                    if (CheckFall()) return;
                    if (CheckCrouchToIdle()) return;

                    if (_character.horizontalAxis != 0f)
                    {
                        SetState(PlayerState.CROUCH_MOVE);
                    }

                    break;

                case PlayerState.CROUCH_MOVE:
                    if (CheckFall()) return;
                    if (CheckCrouchToIdle()) return;

                    if (_character.horizontalAxis == 0f)
                    {
                        SetState(PlayerState.CROUCH);
                    }

                    _character.Move();

                    break;

                case PlayerState.LADDER_CLIMB:
                    if (_character.verticalAxis != 0f)
                    {
                        SetState(PlayerState.LADDER_CLIMB_MOVE);
                    }

                    break;

                case PlayerState.LADDER_CLIMB_MOVE:
                    // 땅에 닿았다면 등반을 멈춘다. 
                    if (_controllerState.IsGrounded && true /*State.LadderTopColliding == false*/ )
                    {
                        SetState(PlayerState.IDLE);
                        return;
                    }

                    // 캐릭터가 사다리의 정상 바닥보다 y 위치가 올라간 경우 등반을 멈춘다.
                    if (_character.currentLadder.PlatformY < _character.transform.position.y)
                    {
                        SetState(PlayerState.IDLE);
                        return;
                    }

                    if (_character.verticalAxis == 0f)
                    {
                        SetState(PlayerState.LADDER_CLIMB);
                    }
                    else
                    {
                        _controller.vy = _character.verticalAxis * _character.WalkSpeed * 0.5f;
                    }

                    break;
            }
        }
        protected virtual void StateExit()
        {
            switch (_currentState)
            {
                case PlayerState.LOOKUP:
                    //_sceneCamera.ResetLookUpDown();
                    break;

                case PlayerState.WALL_CLING:
                    _controller.GravityScale = 1f;
                    _character.AnimFlip = false;
                    break;

                case PlayerState.CROUCH:
                case PlayerState.CROUCH_MOVE:
                    _controller.ResetColliderSize();
                    break;
                case PlayerState.LADDER_CLIMB:
                case PlayerState.LADDER_CLIMB_MOVE:
                    _characterState.IsLadderClimb = false;
                    break;

            }
        }

        bool CheckLookUp()
        {
            if (_character.verticalAxis > 0.1 && _character.horizontalAxis == 0)
            {
                SetState(PlayerState.LOOKUP);
                return true;
            }

            return false;
        }

        bool CheckWallClinging()
        {
            if (_characterState.CanWallClinging == false) return false;
            if (_controllerState.HittedClingWall == null) return false;
            if (_characterState.JumpElapsedTime < 0.2f) return false;

            if ((_controllerState.IsCollidingLeft && _character.horizontalAxis < -0.1f) ||
                (_controllerState.IsCollidingRight && _character.horizontalAxis > 0.1f))
            {
                SetState(PlayerState.WALL_CLING);
                return true;
            }

            return false;
        }

        bool CheckLadderClimb()
        {
            if (_character.currentLadder == null) return false;

            if (_character.verticalAxis > 0.1f && _character.currentLadder.PlatformY > _character.transform.position.y)
            {
                //사다리를 등반하며 점프하자마자 다시 붙는현상을 피하기위해 약간의 버퍼타임을 둔다. 
                if (_controllerState.IsGrounded == false && _characterState.JumpElapsedTime < 0.3f) return false;

                _character.transform.position = new Vector2(_character.currentLadder.transform.position.x, _character.transform.position.y + 0.1f);
                SetState(PlayerState.LADDER_CLIMB);
                return true;
            }
            else if (_character.verticalAxis < -0.1f && _character.currentLadder.PlatformY <= _character.transform.position.y)
            {
                _character.transform.position = new Vector2(_character.currentLadder.transform.position.x, _character.currentLadder.PlatformY - 0.1f);
                //이후 Controller의 LateUpdate 에서 WasColldingBelowLastFrame를 false로 만들기위해.
                //그래야 사다리 정상의 OneWayplatform의 방해를 받지 않는다.
                _controllerState.IsCollidingBelow = false;
                SetState(PlayerState.LADDER_CLIMB);
                return true;
            }

            return false;
        }

        bool CheckFall()
        {
            if (_controllerState.IsGrounded == false)
            {
                SetState(PlayerState.FALL);
                return true;
            }

            return false;
        }

        bool CheckCrouch()
        {
            if (_character.verticalAxis < -0.1f)
            {
                SetState(PlayerState.CROUCH);
                return true;
            }

            return false;
        }

        bool CheckCrouchToIdle()
        {
            if (_character.verticalAxis >= -0.1f && _character.IsCollidingHead == false)
            {
                SetState(PlayerState.IDLE);
                return true;
            }
            return false;
        }

        bool CheckAirToGround()
        {
            if (_controllerState.IsGrounded)
            {
                SetState(PlayerState.IDLE);
                return true;
            }
            return false;
        }

        //------------------------------------------------------------------------------------------------------------
        //behaviour
        //------------------------------------------------------------------------------------------------------------

        public void Attack()
        {
            Debug.Log("attack");
        }

        public void Jump()
        {
            if(_controllerState.IsCollidingAbove || _character.IsCollidingHead )
                return;

            switch( _currentState )
            {
                case PlayerState.IDLE:
                case PlayerState.WALK:
                case PlayerState.RUN:
                case PlayerState.JUMP:
                case PlayerState.FALL:
                case PlayerState.CROUCH:
                case PlayerState.CROUCH_MOVE:
                case PlayerState.LADDER_CLIMB:
                case PlayerState.LADDER_CLIMB_MOVE:
                case PlayerState.LOOKUP:
                    _character.DoJump();
                    SetState(PlayerState.JUMP);
                    break;

                case PlayerState.WALL_CLING:
                    _character.DoWallJump();
                    SetState(PlayerState.JUMP);
                    break;
            }
        }

        public void JumpBelow()
        {
			if( _controllerState.IsGrounded && _controllerState.StandingPlatfom is Platform )
            {
                _character.DoJumpBelow();
                SetState(PlayerState.FALL);
            }
            else if( _characterState.IsLadderClimb == true )
            {
                _character.DoJumpBelow();
                SetState(PlayerState.FALL);
            }
            else
            {
                Jump();
            }
        }
    }
}
