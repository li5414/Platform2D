using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
	/// <summary>
	// Trigger 되면 현재 레벨을 다시 시작한다.
	/// </summary>
	
    [RequireComponent( typeof( Collider2D ))]
    public class LevelRestarter : MonoBehaviour 
	{	
		void Awake()
		{
			print ("LevelRestarter : " + gameObject.name );
		}

		void OnTriggerEnter2D (Collider2D collider)
		{
            if (collider.tag == "Player")
            {
				GameManager.Instance.Restart();
            }
		}
	}
}
