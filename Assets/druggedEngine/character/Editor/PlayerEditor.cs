using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(Fighter))]
public class PlayerEditor : Editor
{
	//GameController mController;

	Fighter mCharacter;

	void OnEnable ()
	{
		mCharacter = (Fighter)target;
	}
    
	#if UNITY_EDITOR
	public override void OnInspectorGUI ()
	{
		if (Application.isPlaying)
		{
			EditorGUILayout.LabelField ("state", mCharacter.state.ToString());
            EditorGUILayout.LabelField ("jump", mCharacter.JumpCount + " / " +mCharacter.maxJumps );
			// DrawDefaultInspector ();
		}
		else
		{
			DrawDefaultInspector ();
		}
	}
	#endif
}
