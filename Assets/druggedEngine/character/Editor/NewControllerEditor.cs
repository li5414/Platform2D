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
				EditorGUILayout.Vector2Field ("velocity", mController.Velocity);

				EditorGUILayout.LabelField ("Grounded", mState.IsOnGround + "( slope: " + mState.SlopeAngle + " )");
				EditorGUILayout.ObjectField ("StandingPlatform", mState.StandingOn, typeof(GameObject), true);

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


