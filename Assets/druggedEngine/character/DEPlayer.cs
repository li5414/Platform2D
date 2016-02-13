using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	public class DEPlayer : DECharacter
	{
		public LocationLinker currentManualLinker{ get;set; }
		public DialogueZone currentDialogueZone{ get;set; }
	}
}
