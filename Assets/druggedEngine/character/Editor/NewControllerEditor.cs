using UnityEngine;
using UnityEditor;
using System.Collections;

namespace druggedcode.engine
{
	[CustomEditor( typeof( NewController ))]
	[CanEditMultipleObjects]

	public class NewControllerEditor : Editor
	{
		NewController mController;
		NewControllerState mState;

		void OnEnable()
		{
			mController = (NewController) target;
			mState = mController.State;
		}

		#if UNITY_EDITOR
		public override void OnInspectorGUI ()
		{
			if (Application.isPlaying && mController.gameObject.activeInHierarchy )
			{
				EditorGUILayout.LabelField ("Grounded", mState.IsGrounded + "( slope: " + mState.SlopeAngle + " )");
				EditorGUILayout.Vector2Field ("PlatformVelocity", mState.PlatformVelocity);
				EditorGUILayout.Vector2Field ("velocity", mController.Velocity);
				EditorGUILayout.LabelField ("targetVX", mController.TargetVX.ToString());

				EditorGUILayout.LabelField ("Friction", mController.Friction.ToString());
				EditorGUILayout.ObjectField ("StandingPlatform", mState.StandingGameObject, typeof(GameObject), true);

				EditorGUILayout.Space ();
				DrawDefaultInspector ();

			}
			else
			{
				DrawDefaultInspector ();
			}
		}
		#endif
	}
}


