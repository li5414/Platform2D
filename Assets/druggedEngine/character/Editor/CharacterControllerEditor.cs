using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using druggedcode;
using druggedcode.engine;

[CustomEditor (typeof(DECharacterController))]
public class CharacterControllerEditor : Editor
{
	DECharacterController mController;

	void OnEnable ()
	{
		mController = (DECharacterController)target;
	}

	#if UNITY_EDITOR
	public override void OnInspectorGUI ()
	{
		if (Application.isPlaying)
		{
			EditorGUILayout.LabelField ("vel", mController.velocity.ToString ());
            EditorGUILayout.LabelField ("targetSpeed", mController.targetSpeed.ToString());
			EditorGUILayout.LabelField ("Slope", mController.state.IsOnSlope.ToString() + " (" + mController.state.SlopeAngle +")");
			EditorGUILayout.LabelField ("OnGround", mController.state.IsGround.ToString ());
			EditorGUILayout.LabelField ("OnForward", mController.state.IsGroundForward.ToString ());
			EditorGUILayout.LabelField ("OnCenter", mController.state.IsGroundCenter.ToString ());
			EditorGUILayout.LabelField ("OnBack", mController.state.IsGroundBack.ToString ());
			EditorGUILayout.ObjectField ("forwardPlatform", mController.state.ForwardPlatform, typeof(Platform), true);
			EditorGUILayout.ObjectField ("centerPlatform", mController.state.CenterPlatofrm, typeof(Platform), true);
			EditorGUILayout.ObjectField ("backPlatform", mController.state.BackPlatform, typeof(Platform), true);
			DrawDefaultInspector ();
		}
		else
		{
			DrawDefaultInspector ();
		}
	}
	#endif
}
