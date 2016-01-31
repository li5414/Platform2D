using System;
using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    /// <summary>
    /// 현재 프레임에서 캐릭터에서 무엇인가를 할 경우 검사할 수 있는 다양한 상태들.
    /// </summary>
    public class DEControllerState
    {
        // 상하좌우 충돌여부
        public bool IsCollidingAbove { get; set; }
        public bool IsCollidingBelow { get; set; }
        public bool IsCollidingLeft { get; set; }
        public bool IsCollidingRight { get; set; }
        public bool HasCollisions { get { return IsCollidingRight || IsCollidingLeft || IsCollidingAbove || IsCollidingBelow; } }

        //wall 충돌여부
        public bool IsCollidingWallRight { get { return IsCollidingRight; } }
        public bool IsCollidingWallLeft { get { return IsCollidingLeft; } }
        
        // 캐릭터가 지상에 있는가. 혹시 아래에 충돌되었어도 지상에 있다고 판단 하지 않을 수도 있다.그때를 대비해 Below와 IsGrounded 구별
        public bool IsGrounded { get { return (IsCollidingBelow && StandingPlatfom != null );}}
        
        //IsCollidingBelow 와 같이 변해야 한다.
        public Platform StandingPlatfom { get; set; }
        public Platform HittedClingWall { get; set; }
        
        //지난프레임상태
        public bool WasColldingBelowLastFrame { get; set; }
        public bool WasColldingAdoveLastFrame { get; set; }
        
        //막 지상에 닿은 프레임에서 true
        public bool JustGotGrounded { get; set; }
        
        public bool IsFalling { get; set; }
        // 움직이고 있는 경사면의 각도
        public float SlopeAngle { get; set; }
        
        public DEControllerState()
        {
            Reset();
        }
        public void Reset()
        {
            IsCollidingAbove = IsCollidingBelow = IsCollidingLeft = IsCollidingRight = false;
            
            IsFalling = false;           
            JustGotGrounded = false;
            SlopeAngle = 0;
            StandingPlatfom = null;
            HittedClingWall = null;
        }

        public void SaveLastStateAndReset()
        {
            WasColldingBelowLastFrame = IsCollidingBelow;
            WasColldingAdoveLastFrame = IsCollidingAbove;
            
            Reset();
        }
    }
}
