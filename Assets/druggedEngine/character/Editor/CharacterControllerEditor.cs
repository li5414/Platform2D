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
			EditorGUILayout.LabelField ("OnGround", mController.state.OnGround.ToString ());
			EditorGUILayout.LabelField ("OnForward", mController.state.OnForwardGround.ToString ());
			EditorGUILayout.LabelField ("OnCenter", mController.state.OnCenterGround.ToString ());
			EditorGUILayout.LabelField ("OnBack", mController.state.OnBackGround.ToString ());
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
