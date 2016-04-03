using UnityEngine;
using System.Collections;
using druggedcode.engine;

namespace druggedcode.engine
{
	[RequireComponent (typeof(BoxCollider2D))]
	public class LocationLinker : MonoBehaviour
	{
		public enum LinkType
		{
			AUTO,
			MANUAL
		}

		public string locationID;
		public string cpID;
		public LinkType type;

		public GameObject promptPrefab;
		public float DistanceFromTop = 0;

		BoxCollider2D mCollider;
		GameObject mPrompt;
		Coroutine mPromptRoutine;
		DEPlayer mEnteredPlayer;

		void Awake ()
		{
			mCollider = GetComponent<BoxCollider2D> ();
			mCollider.isTrigger = true;
		}

		void Start ()
		{
			LayerUtil.ChangeLayer (gameObject, DruggedEngine.MASK_TRIGGER_AT_PLAYER);
		}

		public void Move()
		{
			GameManager.Instance.MoveLocation (locationID, cpID);
		}

		public void Enter (DEPlayer player)
		{
			if( mEnteredPlayer != null ) return;

			mEnteredPlayer = player;

			switch (type)
			{
				case LinkType.AUTO:
					Move();
					break;

				case LinkType.MANUAL:
					ShowPrompt ();
					player.currentManualLinker = this;
					break;
			}
		}

		public void Exit (DEPlayer player)
		{
			if( mEnteredPlayer == null || mEnteredPlayer != player ) return;

			mEnteredPlayer = null;
			switch (type)
			{
				case LinkType.AUTO:
					break;

				case LinkType.MANUAL:
					HidePrompt ();
					player.currentManualLinker = null;
					break;
			}
		}

		void ShowPrompt ()
		{
			if( mPrompt != null ) return;

			mPrompt = (GameObject)Instantiate (promptPrefab);          
			mPrompt.transform.position = new Vector2 (mCollider.bounds.center.x, mCollider.bounds.max.y + DistanceFromTop); 
			mPrompt.transform.parent = transform;
			mPrompt.GetComponent<SpriteRenderer> ().material.color = new Color (1f, 1f, 1f, 0f);

			if (mPromptRoutine != null) StopCoroutine (mPromptRoutine);
			mPromptRoutine = StartCoroutine (Motion2D.FadeAlpha (mPrompt.GetComponent<SpriteRenderer> (), 0.2f, 1f)); 
		}

		void HidePrompt ()
		{
			if (mPromptRoutine != null) StopCoroutine (mPromptRoutine);

			StartCoroutine (Motion2D.FadeAlpha (mPrompt.GetComponent<SpriteRenderer> (), 0.2f, 0f,
				( Renderer r) =>
				{
					if (mPrompt != null)
					{
						Destroy (mPrompt);
						mPrompt = null;
					}
				})
			);
		}
	}
}