using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DECharacterController))]
public class CharacterControllerEditor : Editor
{
    DECharacterController mController;
    void OnEnable()
    {
        mController = (DECharacterController) target;
    }
    
    #if UNITY_EDITOR
    public override void OnInspectorGUI()
    {
        if( Application.isPlaying)
        {
            EditorGUILayout.LabelField("vel",mController.velocity.ToString());
            EditorGUILayout.LabelField("groundForwad",mController.ForwardOnGround.ToString());
            EditorGUILayout.LabelField("groundCenter",mController.CenterOnGround.ToString());
            EditorGUILayout.LabelField("groundBack",mController.BackOnGround.ToString());
            EditorGUILayout.ObjectField("standingPlatform",mController.standingPlatform,typeof(MovingPlatform),true);
            EditorGUILayout.ObjectField("oneywayPlatform",mController.oneWayPlatform,typeof(OneWayPlatform),true);
            
            DrawDefaultInspector();
        }
        else
        {
            DrawDefaultInspector();
        }
    }
    #endif
}
