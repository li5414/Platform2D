using UnityEngine;
using UnityEditor;

namespace druggedcode.engine
{
    [CustomEditor(typeof(DEPlayer))]
    [CanEditMultipleObjects]

    /// <summary>
    /// CharacterBehavior 커스텀 인스펙터
    /// </summary>

    public class DEPlayerEditor : Editor
    {
        //  DEController _controller;
        //  DEControllerState _state;
        DEPlayer _character;
        SerializedProperty _jumpHeight;
        
        void OnEnable ()
        {
            _jumpHeight = serializedObject.FindProperty("JumpHeight");
            
        }

        public override void OnInspectorGUI()
        {
            if (Application.isEditor && Application.isPlaying)
            {
                _character = (DEPlayer)target;
                
                if( _character.gameObject.activeSelf == false ) return;
                
                if( _character.fsm != null )
                {
                    EditorGUILayout.LabelField("FSM", _character.fsm.ToString());
                }

                EditorGUILayout.LabelField("Facing", _character.CurrentFacing.ToString());
                EditorGUILayout.LabelField("Axis", _character.horizontalAxis + " : " +_character.verticalAxis );
                EditorGUILayout.LabelField("vx", _character.CurrentVX.ToString());
                EditorGUILayout.LabelField("Run", _character.State.IsRun.ToString());
                EditorGUILayout.LabelField("Jump", _character.State.JumpCount + "/" + _character.JumpNum);
                EditorGUILayout.PropertyField( _jumpHeight );
                serializedObject.ApplyModifiedProperties();
                
                DrawDefaultInspector();
            }
            else
            {
                DrawDefaultInspector();
            }
            
            /*
            if ( state!=null)
            {
                EditorGUILayout.LabelField("LadderClimbing",state.LadderClimbing.ToString());
                EditorGUILayout.LabelField("LadderColliding",state.LadderColliding.ToString());
                EditorGUILayout.LabelField("LadderTopColliding",state.LadderTopColliding.ToString());
                EditorGUILayout.LabelField("LadderClimbingSpeed",state.LadderClimbingSpeed.ToString());
            }
            DrawDefaultInspector();
            */
        }
    }
}