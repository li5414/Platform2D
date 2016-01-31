using System;
using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    /// <summary>
    /// 캐릭터의 권한 목록. fsm 과 관계없이 우선적으로 적용된다.
    /// 특정 씬이나 상황, 캐릭터 별 제한 등으로 이용
    /// </summary>
    [Serializable]
    public class DECharacterPermissions
    {
        public bool RunEnabled=true;
        public bool DashEnabled=true;
        public bool JetpackEnabled=true;
        public bool JumpEnabled=true;
        public bool ShootEnabled=true;
        public bool WallJumpEnabled=true;
        public bool WallClingingEnabled=true;
        public bool MeleeAttackEnabled=true;    
    }
}
