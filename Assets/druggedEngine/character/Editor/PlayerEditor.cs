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
			EditorGUILayout.LabelField ("some", mCharacter.IsFlipped.ToString());
			DrawDefaultInspector ();
		}
		else
		{
			DrawDefaultInspector ();
		}
	}
	#endif
}
