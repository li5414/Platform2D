using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	public class DEPlayer : DECharacter
	{
		public LocationLinker currentManualLinker{ get;set; }
		public DialogueZone currentDialogueZone{ get;set; }

		protected virtual IEnumerator Dive()
		{	
			yield break;
//			// Shake parameters : intensity, duration (in seconds) and decay
//			Vector3 ShakeParameters = new Vector3(1.5f,0.5f,1f);
//			BehaviorState.Diving=true;
//			// while the player is not grounded, we force it to go down fast
//			while (!_controller.State.IsGrounded)
//			{
//				_controller.SetVerticalForce(-Mathf.Abs(_controller.Parameters.Gravity)*2);
//				yield return 0; //go to next frame
//			}
//
//			// once the player is grounded, we shake the camera, and restore the diving state to false
//			_sceneCamera.Shake(ShakeParameters);		
//			BehaviorState.Diving=false;
		}

		protected void LookUp()
		{
//			_character.GravityActive(true);
//			_character.SetAnimation("lookup");
			//_sceneCamera.LookUp();
		}


	}
}
