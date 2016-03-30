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

		override protected void AirJump()
		{
			base.AirJump();
			GameObject jumpEffect = airJumpEffectPrefab == null ? jumpEffectPrefab : airJumpEffectPrefab;
			FXManager.Instance.SpawnFX(jumpEffect, mTr.position, new Vector3(mFacing * 1f, 1f, 1f));
		}
    }
}
