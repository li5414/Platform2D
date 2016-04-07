using UnityEngine;
using UnityEditor;
using System.Collections;

namespace druggedcode.engine
{
	[CustomEditor( typeof( DEController ))]
	[CanEditMultipleObjects]

	public class DEControllerEditor : Editor
	{
		DEController mController;
		NewControllerState mState;

		void OnEnable()
		{
			mController = (DEController) target;
			mState = mController.State;
		}

		#if UNITY_EDITOR
		public override void OnInspectorGUI ()
		{
			if (Application.isPlaying && mController.gameObject.activeInHierarchy )
			{
				EditorGUILayout.LabelField ("Grounded", mState.IsGrounded + "( slope: " + mState.SlopeAngle + " )");
				EditorGUILayout.LabelField("Facing",mController.Facing == 1 ? "RIGHT" : "LEFT");
				EditorGUILayout.LabelField ("targetVX", mController.TargetVX.ToString());
				EditorGUILayout.Vector2Field ("velocity", mController.Velocity);
				EditorGUILayout.Vector2Field ("PlatformVelocity", mState.PlatformVelocity);
				EditorGUILayout.LabelField ("Friction", mController.Friction.ToString());
				EditorGUILayout.ObjectField ("StandingPlatform", mState.StandingGameObject, typeof(GameObject), true);
				EditorGUILayout.ObjectField("FrontGameObject", mState.FrontGameObject,typeof(GameObject),true);

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


