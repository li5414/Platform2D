using UnityEngine;
using UnityEditor;
using System.Collections;

namespace druggedcode.engine
{
	[CustomEditor (typeof(DEController))]
	[CanEditMultipleObjects]

	public class DEControllerEditor : Editor
	{
		DEController controller;
		DEControllerState state;

		void OnEnable ()
		{
			controller = (DEController)target;
			state = controller.state;
		}

		#if UNITY_EDITOR
		public override void OnInspectorGUI ()
		{
			if (Application.isPlaying)
			{
				if (controller.gameObject.activeSelf == false) return;

				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Colliding Left", state.IsCollidingLeft.ToString ());
				EditorGUILayout.LabelField ("Colliding Right", state.IsCollidingRight.ToString ());
				EditorGUILayout.LabelField ("Colliding Above", state.IsCollidingAbove.ToString ());
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Grounded", state.IsGrounded + "( " + state.IsGroundedForward + ", "+ state.IsGroundedCenter + ", " + state.IsGroundedBack + " )");
				EditorGUILayout.LabelField ("Falling", controller.Velocity.y < 0 ? "true" : "false" );
				EditorGUILayout.Space ();
				EditorGUILayout.ObjectField ("StandingPlatform", state.StandingPlatfom, typeof(Platform), true);
				EditorGUILayout.ObjectField ("CollidingSide", state.CollidingSide, typeof(Collider2D), true);
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Slope Angle", state.SlopeAngle.ToString ());
				EditorGUILayout.LabelField ("GravityScale", controller.GravityScale.ToString ());
				EditorGUILayout.Vector2Field ("velocity", controller.Velocity);

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