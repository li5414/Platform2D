using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    public class DEPlayer : DEActor
    {
        public LocationLinker currentManualLinker { get; set; }
        public DialogueZone currentDialogueZone { get; set; }
    }
}
