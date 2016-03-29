using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    [RequireComponent(typeof( Collider2D ))]
    public class Item : MonoBehaviour
    {
        public GameObject GetEffect;
        protected Collider2D _col;

        virtual protected void Awake()
        {
            _col = GetComponent<Collider2D>();
            _col.isTrigger = true;
        }

        void Start()
        {
            Create();
        }

        virtual protected void Create()
        {
            gameObject.SetActive(true);
        }

        virtual protected void Getted()
        {
            gameObject.SetActive(false);
        }

        public void OnTriggerEnter2D (Collider2D collider) 
        {
            // 충돌한 객체가 캐릭터가 아니라면 리턴
            var player = collider.GetComponent<DEPlayer>();

            if (player == null)
                return;

            if (GetEffect != null)
            {
                Instantiate(GetEffect,transform.position, Quaternion.identity );
            }

            Getted();
        }

        public void onPlayerRespawnInThisCheckpoint(CheckPoint checkpoint, DEPlayer player)
        {
            Create();
        }
    }
}