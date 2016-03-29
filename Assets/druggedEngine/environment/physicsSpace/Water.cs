using UnityEngine;

namespace druggedcode.engine
{
    public class Water : PhysicsSpace
    {
        /// 물밖으로 캐릭터가 나올 때 캐릭터에게 전달할 힘
        public float WaterExitForce = 8f;

        override protected void In(DEController controller)
        {
            base.In(controller);
        }

        override protected void Out(DEController controller)
        {
            base.Out(controller);
            
            controller.vy = Mathf.Abs(WaterExitForce);
            DEPlayer player = controller.GetComponent<DEPlayer>();
            if( player != null )
            {
                player.JumpCount = 1;
            }
        }
    }
}
