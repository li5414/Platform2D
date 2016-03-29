using UnityEngine;
using UnityEditor;
using System.Collections;

namespace druggedcode.engine
{
	[CustomEditor(typeof( DEActor ))]
	public class NewCharacterEditor : Editor
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
				EditorGUILayout.LabelField("State",mCharacter.State.ToString());
				EditorGUILayout.Vector2Field( "Axis", mCharacter.axis );
				EditorGUILayout.LabelField( "CurrentSpeed", mCharacter.CurrentSpeed.ToString());
				EditorGUILayout.LabelField("hp",mCharacter.Health.ToString());

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