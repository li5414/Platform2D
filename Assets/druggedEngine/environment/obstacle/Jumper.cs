using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    /// <summary>
    /// 플랫폼에 추가하면 CorgiController 를 가진 캐릭터가 접촉 할 경우 자동으로 공중으로 날려보낸다.
    /// </summary>
    public class Jumper : MonoBehaviour 
    {
        public Vector2 jumpPower;
        public GameObject flyPrefab;

        DEController _tg;
        Collider2D _col;

        void Awake()
        {
            _col = GetComponent<Collider2D>();
            _col.isTrigger = true;
        }

        public void OnTriggerEnter2D(Collider2D collider)
        {
            DEController controller = collider.GetComponent<DEController>();
            if ( controller == null )
                return;

            _tg = controller;

            LetFly();
        }

        void LetFly()
        {
            _tg.AddForce(jumpPower);
//            _tg.SetVY( Mathf.Sqrt( 2f * jumpPower.y * -_tg.CurrentParameters.Gravity ));

            if ( flyPrefab != null )
            {
                Instantiate( flyPrefab, transform.position, Quaternion.identity );    
            }
        }
    }

}