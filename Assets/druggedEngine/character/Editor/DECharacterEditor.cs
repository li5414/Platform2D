using UnityEngine;
using UnityEditor;

namespace druggedcode.engine
{
    [CustomEditor(typeof(DECharacter))]
    public class DECharacterEditor : Editor
    {
        protected DECharacter mCharacter;
        void OnEnable()
        {
            mCharacter = (DECharacter) target;
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

