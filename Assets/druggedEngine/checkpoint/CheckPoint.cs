using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace druggedcode.engine
{
    /// <summary>
    /// CheckPoint 클래스. 레벨을 시작하거나 죽었을 경우 플레이어는 여기서 스폰된다.
    /// </summary>
    public class CheckPoint : MonoBehaviour
    {
        public string id;

        //캐릭터가 리스폰 되었을 때 이를 알아야할 옵저버객체들
        List<IPlayerRespawnListener> mListeners;


        bool mActive;
        public bool active
        {
            get{ return mActive; }
            set
            {
                mActive = value;
                if (mActive) PlayerHitCheckPoint();
                else PlayerLeftCheckPoint();
            }
        }
        
        void Awake()
        {
            mListeners = new List<IPlayerRespawnListener>();
        }
        
        void PlayerHitCheckPoint()
        {
            Debug.Log("[CheckPoint] '" + name + "' setted.");
        }

        // 최근 체크포인트로 저장되었다가 해제되었을 때 호출된다.
        void PlayerLeftCheckPoint()
        {
            Debug.Log("[CheckPoint] '" + name + "' lefted.");
        }

        /// <summary>
        /// 플레이어를가 체크포인트에서 스폰되었다. 옵저버들에게 통지.
        /// </summary>
		public void Spawn()
        {
			Debug.Log("[CheckPoint] " + name + " spawned");
            foreach (var listener in mListeners)
            {
                listener.onPlayerRespawnInThisCheckpoint(this );
            }
        }

        /// <summary>
        /// 옵저버 등록
        /// </summary>
        public void AssignObjectToCheckPoint(IPlayerRespawnListener listener)
        {
            mListeners.Add(listener);
        }

        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
        #endif
    }
}