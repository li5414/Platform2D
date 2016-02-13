using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace druggedcode
{
	/// <summary>
	// 현재 Fixed와 Update가 같은 로직이다.
	// 콜라이더가 있는 경우( 강체와 같이 ) position 을 이동시키는건 부하가 된다고 한다. 적절한 최적화가 필요함
	/// </summary>
	public class PathFollow : MonoBehaviour
	{
		public UpdateType updateType;

		public SmoothType Type = SmoothType.MoveTowards;
		public float Speed = 1;
		public float MaxDistanceToGoal = .1f;
		public bool initOnFirstPath;

		public Transform[] paths;

		public Vector2 velocity{ get; private set; }

		public Vector2 deltaVector{ get; private set; }

		IEnumerator<Transform> mPathEnumerator;
		Transform mTr;
		Vector3 mLastPos;
		Vector3 mNextPos;

		void Awake ()
		{
			mTr = transform;
		}

		void Start ()
		{
			SetPaths (paths);

			if (initOnFirstPath) mTr.position = mNextPos;

			mLastPos = mTr.position;
		}

		public void SetPaths (Transform[] paths)
		{
			this.paths = paths;

			if (this.paths == null)
			{
				mPathEnumerator = null;
				return;
			}

			mPathEnumerator = GetPathEnumerator ();

			MoveNext ();
		}

		IEnumerator<Transform> GetPathEnumerator ()
		{
			if (paths == null || paths.Length < 1) yield break;

			var direction = 1;
			var index = 0;
			while (true)
			{
				yield return paths [index];

				if (paths.Length == 1) continue;

				if (index <= 0) direction = 1;
				else if (index >= paths.Length - 1) direction = -1;

				index = index + direction;
			}
		}

		void MoveNext ()
		{
			if (mPathEnumerator.MoveNext ()) mNextPos = mPathEnumerator.Current.position;
			else mNextPos = mTr.position;
		}

		public void Update ()
		{
			if (updateType == UpdateType.Update) Move (Time.deltaTime);
		}

		void LateUpdate ()
		{
			if (updateType == UpdateType.LateUpdate) Move (Time.deltaTime);
		}

		void FixedUpdate ()
		{
			if (updateType == UpdateType.FixedUpdate) Move (Time.fixedDeltaTime);
		}

		void Move (float deltaTime)
		{
			if (mPathEnumerator == null || mPathEnumerator.Current == null)
				return;

			if (Type == SmoothType.MoveTowards)
			{
				mTr.position = Vector3.MoveTowards (mTr.position, mNextPos, deltaTime * Speed);
			}
			else if (Type == SmoothType.Lerp)
			{
				mTr.position = Vector3.Lerp (mTr.position, mNextPos, deltaTime * Speed);
			}

			var distanceSquared = (mTr.position - mNextPos).sqrMagnitude;
			if (distanceSquared < MaxDistanceToGoal * MaxDistanceToGoal)
			{
				MoveNext ();
			}

			deltaVector = mTr.position - mLastPos;
			velocity = deltaVector / deltaTime;

			mLastPos = mTr.position;
		}

		#if UNITY_EDITOR
		void OnDrawGizmos ()
		{
			if (Application.isEditor == false || paths == null || paths.Length < 2) return;

			var points = paths.Where (t => t != null).ToList ();

			if (points.Count < 2) return;

			Gizmos.color = Color.yellow;

			for (var i = 1; i < points.Count; i++)
			{
				Gizmos.DrawLine (points [i - 1].position, points [i].position);
			}
		}
		#endif
	}
}
