using UnityEngine;
using System.Reflection;
using UnityEditor;
using System;

namespace druggedcode
{
    [CustomEditor(typeof(SortingLayerChanger))]
    public class SortingLayerChangerInspector : Editor
    {
        static MethodInfo EditorGUILayoutSortingLayerField;
        SortingLayerChanger tg;
        
        SerializedProperty sortingLayerID;

        void OnEnable()
        {
            tg = (SortingLayerChanger) target;
             
            if (EditorGUILayoutSortingLayerField == null)
            {
                EditorGUILayoutSortingLayerField = typeof(EditorGUILayout).GetMethod
                (
                    "SortingLayerField",
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(GUIContent), typeof(SerializedProperty), typeof(GUIStyle) },
                    null
                );
            }

            sortingLayerID = serializedObject.FindProperty("sortingLayerID");
        }

        override public void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            serializedObject.Update();
            
            EditorGUI.BeginChangeCheck();

            if (EditorGUILayoutSortingLayerField != null && sortingLayerID != null)
            {
                EditorGUILayoutSortingLayerField.Invoke(null, new object[] { new GUIContent("Sorting Layer"), sortingLayerID, EditorStyles.popup });
            }
            
            if( EditorGUI.EndChangeCheck())
            {
                if( serializedObject.ApplyModifiedProperties() )
                {
                    tg.Excute();
                }
                EditorUtility.SetDirty( tg );
            }
        }
    }
}
