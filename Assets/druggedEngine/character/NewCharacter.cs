using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace druggedcode.engine
{
	public class NewCharacter : MonoBehaviour
	{
        
        //----------------------------------------------------------------------------------------------------------
		// input
		//----------------------------------------------------------------------------------------------------------
		public float horizontalAxis { get; set; }
		public float verticalAxis { get; set; }
		public bool isRun { get; set; }
        
        public UnityAction<NewCharacter> OnUpdateInput;
        
        void Update()
        {
            if (OnUpdateInput != null) OnUpdateInput (this);
        }
	}
}
