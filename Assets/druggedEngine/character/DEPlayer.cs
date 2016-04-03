using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    public class DEPlayer : DEActor
    {
		public bool isJumpPressed { get; set; }
        public LocationLinker currentManualLinker { get; set; }
        public DialogueZone currentDialogueZone { get; set; }

		[Header ("Effect")]
		public GameObject jumpEffectPrefab;
		public GameObject airJumpEffectPrefab;

		override protected void OnDisable()
		{
			base.OnDisable();
			currentManualLinker = null;
			currentDialogueZone = null;
		}

		public void UnUse( Transform poolTransform )
		{
			mTr.SetParent( poolTransform );
			Controller.Stop();
			gameObject.SetActive( false );
		}

		//대화창등이 시작될 때 캐릭터를 멈춘다. 
		public void Pause(bool onoff = true)
		{
			if (onoff)
			{
				Idle();
			}
			else
			{

			}
		}

		override protected void AirJump()
		{
			base.AirJump();
			GameObject jumpEffect = airJumpEffectPrefab == null ? jumpEffectPrefab : airJumpEffectPrefab;
			FXManager.Instance.SpawnFX(jumpEffect, mTr.position, new Vector3(mFacing * 1f, 1f, 1f));
		}

		void OnTriggerLocation( LocationLinker linker, bool isEnter)
		{
			if( linker == null ) return;

			if( isEnter ) linker.Enter( this );
			else linker.Exit( this );
		}

		override protected void OnTriggerEnter2D( Collider2D other )
		{
			base.OnTriggerEnter2D( other );

			OnTriggerLocation( other.GetComponent<LocationLinker>(),true);
		}

		override protected void OnTriggerExit2D( Collider2D other )
		{
			base.OnTriggerExit2D( other );

			OnTriggerLocation( other.GetComponent<LocationLinker>(),false);
		}
    }
}
