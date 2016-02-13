using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    public class Water : PhysicsSpace
    {
        /// 물밖으로 캐릭터가 나올 때 캐릭터에게 전달할 힘
        public float WaterExitForce = 8f;

        override protected void In(DECharacter ch)
        {
            base.In(ch);
        }

        override protected void Out(DECharacter ch)
        {
            base.Out(ch);

			ch.jumpCount = 1;

            DEController controller = ch.GetComponent< DEController >();
            if (controller != null)
            {
                controller.vy = Mathf.Abs(WaterExitForce);
            }
        }
    }
}
