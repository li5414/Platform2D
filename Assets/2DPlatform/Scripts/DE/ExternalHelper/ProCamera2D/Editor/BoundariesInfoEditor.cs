using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Com.LuisPedroFonseca.ProCamera2D
{
    [CustomEditor(typeof(BoundariesInfo))]
    [CanEditMultipleObjects]
    public class BoundariesInfoEditor : Editor
    {
        MonoScript _script;
        GUIContent _tooltip;

        void OnEnable()
        {
            var boundariesInfo = (BoundariesInfo)target;

            _script = MonoScript.FromMonoBehaviour(boundariesInfo);
        }

        public override void OnInspectorGUI()
        {
            var boundariesInfo = (BoundariesInfo)target;

            serializedObject.Update();

            // Show script link
            _script = EditorGUILayout.ObjectField("Script", _script, typeof(MonoScript), false) as MonoScript;

            // Boundaries relative
            _tooltip = new GUIContent("Are boundaries relative", "Are the boundaries relative to this or are they world positions?");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("AreBoundariesRelative"), _tooltip);

            // Top boundary
            EditorGUILayout.BeginHorizontal();

            _tooltip = new GUIContent("Use Top Boundary", "Should the camera top position be limited?");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UseTopBoundary"), _tooltip);

            if(boundariesInfo.UseTopBoundary)
            {
                _tooltip = new GUIContent(" ", "Camera top boundary");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("TopBoundary"), _tooltip);
            }

            EditorGUILayout.EndHorizontal();

            // Bottom boundary
            EditorGUILayout.BeginHorizontal();

            _tooltip = new GUIContent("Use Bottom Boundary", "Should the camera bottom position be limited?");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UseBottomBoundary"), _tooltip);

            if(boundariesInfo.UseBottomBoundary)
            {
                _tooltip = new GUIContent(" ", "Camera bottom boundary");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BottomBoundary"), _tooltip);
            }

            EditorGUILayout.EndHorizontal();

            // Left boundary
            EditorGUILayout.BeginHorizontal();

            _tooltip = new GUIContent("Use Left Boundary", "Should the camera left position be limited?");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UseLeftBoundary"), _tooltip);

            if(boundariesInfo.UseLeftBoundary)
            {
                _tooltip = new GUIContent(" ", "Camera left boundary");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LeftBoundary"), _tooltip);
            }

            EditorGUILayout.EndHorizontal();

            // Right boundary
            EditorGUILayout.BeginHorizontal();

            _tooltip = new GUIContent("Use Right Boundary", "Should the camera right position be limited?");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("UseRightBoundary"), _tooltip);

            if(boundariesInfo.UseRightBoundary)
            {
                _tooltip = new GUIContent(" ", "Camera right boundary");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RightBoundary"), _tooltip);
            }

            EditorGUILayout.EndHorizontal();

            // Change zoom
            _tooltip = new GUIContent("Change Zoom", "Change the camera zoom in/out?");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ChangeZoom"), _tooltip);

            if (boundariesInfo.ChangeZoom)
            {
                // Target zoom
                _tooltip = new GUIContent("Zoom Amount", "");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("TargetZoom"), _tooltip);
            }

            // Limit boundaries
            boundariesInfo.ClampBound();

            serializedObject.ApplyModifiedProperties();
        }
    }
}

