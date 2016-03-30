using UnityEngine;
using UnityEditor;

namespace druggedcode.engine
{
	[CustomEditor(typeof(DE))]
    public class DECharacterEditor : Editor
    {
        protected DECharacterOld mCharacter;
        void OnEnable()
        {
            mCharacter = (DECharacterOld) target;
        }
        
        #if UNITY_EDITOR
        public override void OnInspectorGUI()
        {
			if( Application.isPlaying && mCharacter.gameObject.activeInHierarchy )
            {
				EditorGUILayout.LabelField("state",mCharacter.State.ToString());
				EditorGUILayout.LabelField("hp",mCharacter.Health.ToString());
                EditorGUILayout.ObjectField("ladder",mCharacter.CurrentLadder,typeof(Ladder),true);
				EditorGUILayout.LabelField ("axisX", mCharacter.horizontalAxis .ToString());

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

