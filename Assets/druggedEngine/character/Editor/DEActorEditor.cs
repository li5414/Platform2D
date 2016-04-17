using UnityEngine;
using UnityEditor;
using System.Collections;

namespace druggedcode.engine
{
	[CustomEditor(typeof( DEActor ))]
	public class DEActorEditor : Editor
	{
		protected DEActor mCharacter;
		void OnEnable()
		{
			mCharacter = (DEActor) target;
		}

		#if UNITY_EDITOR
		public override void OnInspectorGUI()
		{
			if( Application.isPlaying && mCharacter.gameObject.activeInHierarchy )
			{
				EditorGUILayout.LabelField("State",mCharacter.State.ToString() + " < " + mCharacter.LastState.ToString());
				EditorGUILayout.Vector2Field( "Axis", mCharacter.Axis );
				EditorGUILayout.LabelField( "CurrentSpeed", mCharacter.CurrentSpeed.ToString());
				EditorGUILayout.LabelField("hp",mCharacter.Health.ToString());
				EditorGUILayout.Space ();
				DrawDefaultInspector();
			}
			else
			{
				DrawDefaultInspector();
			}
		}
		#endif
	}
}