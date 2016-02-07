using UnityEngine;
using UnityEditor;

namespace druggedcode.engine
{
    [CustomEditor(typeof(DEController))]
    [CanEditMultipleObjects]

    public class DEControllerEditor : Editor
    {
        DEController _controller;
        DEControllerState _state;
        void onEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            if (Application.isEditor && Application.isPlaying)
            {
                _controller = (DEController)target;
                _state = _controller.State;
                
                if( _controller.gameObject.activeSelf == false ) return;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Colliding Left", _state.IsCollidingLeft.ToString());
                EditorGUILayout.LabelField("Colliding Right", _state.IsCollidingRight.ToString());
                EditorGUILayout.LabelField("Colliding Above", _state.IsCollidingAbove.ToString());
                EditorGUILayout.LabelField("Colliding Below", _state.IsGrounded.ToString());
                EditorGUILayout.LabelField("Falling", _state.IsFalling.ToString());
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Grounded", _state.IsGrounded.ToString());
                EditorGUILayout.ObjectField("StandingPlatform",_state.StandingPlatfom,typeof(TempPlatform),true);
                EditorGUILayout.ObjectField("HittedClingWall",_state.HittedClingWall,typeof(TempPlatform),true);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Slope Angle", _state.SlopeAngle.ToString());
                EditorGUILayout.LabelField("GravityScale", _controller.GravityScale.ToString());
                EditorGUILayout.Vector2Field("velocity",_controller.Velocity );
                
                DrawDefaultInspector();
                
            }
            else
            {
                DrawDefaultInspector();
            }
        }
    }
}

