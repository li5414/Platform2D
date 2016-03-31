using UnityEngine;
using UnityEditor;

namespace druggedcode.engine
{
	[CustomEditor(typeof(DECharacterOld))]
    public class DECharacterOldEditor : Editor
    {
		protected DECharacterOld mActor;
        void OnEnable()
        {
			mActor = (DECharacterOld) target;
        }
        
        #if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
			if( Application.isPlaying && mActor.gameObject.activeInHierarchy )
            {
				EditorGUILayout.LabelField("state",mActor.State.ToString());
				EditorGUILayout.LabelField("hp",mActor.Health.ToString());
                EditorGUILayout.ObjectField("ladder",mActor.CurrentLadder,typeof(Ladder),true);
				EditorGUILayout.LabelField ("axisX", mActor.horizontalAxis .ToString());

				DrawDefaultInspector();
            }
            else
            {
                DrawDefaultInspector();
            }
        }
        #endif
    }
}

