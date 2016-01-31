using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    /// <summary>
    /// 플레이어가 충돌하면 사망한다.
    /// </summary>
    public class KillPlayerOnTouch : MonoBehaviour
    {
        public void OnTriggerEnter2D(Collider2D collider)
        {
            var player = collider.GetComponent<DECharacter>();
            if (player == null) return;

            if (collider.tag != "Player") return;
			
			player.Kill();
        }
    }
}

