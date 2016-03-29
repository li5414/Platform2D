using UnityEngine;
using UnityEditor;
using System.Collections;

namespace druggedcode.engine
{
	[CustomEditor (typeof(DEControllerOld))]
	[CanEditMultipleObjects]

	public class DEControllerEditor : Editor
	{
		DEControllerOld controller;
		DEControllerState state;

		void OnEnable ()
		{
			controller = (DEControllerOld)target;
			state = controller.state;
		}

		#if UNITY_EDITOR
		public override void OnInspectorGUI ()
		{
			if (Application.isPlaying)
			{
				if (controller.gameObject.activeInHierarchy == false) return;

				EditorGUILayout.Vector2Field ("velocity", controller.Velocity);

				EditorGUILayout.LabelField ("Grounded", state.IsGrounded + "( " + state.IsGroundedForward + ", "+ state.IsGroundedCenter + ", " + state.IsGroundedBack + " )");
				EditorGUILayout.LabelField ("Slope Angle", state.SlopeAngle.ToString ());
				EditorGUILayout.LabelField ("PlatformFriction", controller.PlatformFriction.ToString());

				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Colliding Left", state.IsCollidingLeft.ToString ());
				EditorGUILayout.LabelField ("Colliding Right", state.IsCollidingRight.ToString ());
				EditorGUILayout.LabelField ("Colliding Above", state.IsCollidingAbove.ToString ());
				EditorGUILayout.LabelField ("Falling", controller.Velocity.y < 0 ? "true" : "false" );

				EditorGUILayout.Space ();
				EditorGUILayout.ObjectField ("StandingPlatform", state.StandingOn, typeof(GameObject), true);
				EditorGUILayout.ObjectField ("CollidingSide", state.CollidingSide, typeof(Collider2D), true);
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("GravityScale", controller.gravityScale.ToString ());

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