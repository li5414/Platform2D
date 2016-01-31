using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace druggedcode.engine
{
    /// <summary>
    /// 접촉한 객체를 목적지로 텔레포트 시킨다.
    /// 현재 모든 콜라이더를 체크한다. 특정  물건들을 지정 할 필요가 있다.
    /// </summary>
    [RequireComponent(typeof( Collider2D))]
    public class Teleporter : MonoBehaviour 
    {
        public Teleporter Destination;

        public GameObject teleportInEffect;
        public GameObject teleportOutEffect;

        //플레이어에게만 적용할 것인지 여부
        public bool OnlyAffectsPlayer;

        //텔레포트를 무시할 목록. 목적지로 이동 시킬 경우 목적지가 다시 원위치로 보내는 것을 방지 하기 위해 사용
        List<Transform> _ignoreList;
        Collider2D _col;

        void Awake()
        {
            _col = GetComponent<Collider2D>();
            _col.isTrigger = true;
            _ignoreList = new List<Transform>();
        }

        public void OnTriggerEnter2D(Collider2D collider)
        {
            if (Destination == null)
                return;

            if ( _ignoreList.Contains( collider.transform ))
            {
                return;
            }

            // 플레이어에게만 영향을 끼치는 경우
            if (OnlyAffectsPlayer)
            {
				if (collider.tag != Config.TAG_PLAYER)
                    return;
            }

            In();

            Destination.Out(collider.transform);
        }

        public void In()
        {
            if (teleportInEffect != null)
            {
                Instantiate( teleportInEffect,transform.position,Quaternion.identity);
            }
        }

        public void Out( Transform tr )
        {
            _ignoreList.Add(tr);
            tr.position = transform.position;

            if (teleportOutEffect != null)
            {
                Instantiate( teleportOutEffect,transform.position,Quaternion.identity);
            }
        }

        public void OnTriggerExit2D(Collider2D collider)
        {
            if ( _ignoreList.Contains( collider.transform ))
            {
                _ignoreList.Remove( collider.transform );
            }
        }

    }

}