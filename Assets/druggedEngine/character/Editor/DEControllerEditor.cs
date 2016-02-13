using UnityEngine;
using UnityEditor;
using System.Collections;

namespace druggedcode.engine
{
	[CustomEditor (typeof(DEController))]
	[CanEditMultipleObjects]

	public class DEControllerEditor : Editor
	{
		DEController _controller;
		DEControllerState _state;

		void OnEnable ()
		{
			_controller = (DEController)target;
			_state = _controller.state;
		}

		#if UNITY_EDITOR
		public override void OnInspectorGUI ()
		{
			if (Application.isPlaying)
			{
				if (_controller.gameObject.activeSelf == false) return;

				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Colliding Left", _state.IsCollidingLeft.ToString ());
				EditorGUILayout.LabelField ("Colliding Right", _state.IsCollidingRight.ToString ());
				EditorGUILayout.LabelField ("Colliding Above", _state.IsCollidingAbove.ToString ());
				EditorGUILayout.LabelField ("Colliding Below", _state.IsGrounded.ToString ());
				EditorGUILayout.LabelField ("Falling", _state.IsFalling.ToString ());
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Grounded", _state.IsGrounded.ToString ());
				EditorGUILayout.ObjectField ("StandingPlatform", _state.StandingPlatfom, typeof(Platform), true);
				EditorGUILayout.ObjectField ("HittedClingWall", _state.HittedClingWall, typeof(Platform), true);
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Slope Angle", _state.SlopeAngle.ToString ());
				EditorGUILayout.LabelField ("GravityScale", _controller.GravityScale.ToString ());
				EditorGUILayout.Vector2Field ("velocity", _controller.Velocity);

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