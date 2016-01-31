using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    public class Water : PhysicsSpace
    {
        /// 물밖으로 캐릭터가 나올 때 캐릭터에게 전달할 힘
        public float WaterExitForce = 8f;
        private int _numberOfJumpsSaved;

        override protected void In(DECharacter ch)
        {
            base.In(ch);

            _numberOfJumpsSaved = ch.State.JumpLeft + 1;
        }

        override protected void Out(DECharacter ch)
        {
            base.Out(ch);

            ch.State.JumpLeft = _numberOfJumpsSaved;

            DEController controller = ch.GetComponent< DEController >();
            if (controller != null)
            {
                controller.vy = Mathf.Abs(WaterExitForce);
            }
        }
    }
}
