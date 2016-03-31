using UnityEngine;
using UnityEditor;

namespace druggedcode.engine
{
    [CustomEditor(typeof(Hitman))]
	public class HitmanEditor : DEActorEditor
    {
        #if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
        #endif
    }
}

