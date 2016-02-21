﻿using UnityEngine;
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
            if( Application.isPlaying)
            {
                EditorGUILayout.ObjectField("ladder",mCharacter.currentLadder,typeof(Ladder),true);
            }
            else
            {
                DrawDefaultInspector();
            }
        }
        #endif
    }
}
