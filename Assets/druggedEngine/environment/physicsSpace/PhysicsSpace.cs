using System;
using UnityEngine;

namespace druggedcode.engine
{
    
    [RequireComponent(typeof( Collider2D ))]
    public class PhysicsSpace : MonoBehaviour
    {
        public GameObject InOutEffectPrefab;
        public float RelativeGravity = 0f;
        public float MoveFactor = 1f;

        PhysicInfo physicInfo;

        void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;

            physicInfo = new PhysicInfo( -DruggedEngine.Gravity + RelativeGravity, MoveFactor );
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            DECharacter character = collider.GetComponent<DECharacter>();

            if ( character == null )
                return;

            In( character );

            ShowInOutEffect( character.transform.position );
        }

        void OnTriggerExit2D(Collider2D collider)
        {
            DECharacter character = collider.GetComponent<DECharacter>();
            
            if ( character == null )
                return;

            Out( character );

            ShowInOutEffect( character.transform.position );
        }

        virtual protected void In( DECharacter ch )
        {
            ch.UpdatePhysicInfo( physicInfo );
        }
        
        virtual protected void Out( DECharacter ch )
        {
            ch.ResetPhysicInfo();
        }
        
        virtual protected void ShowInOutEffect( Vector3 pos )
        {
            if( InOutEffectPrefab != null )
            {
                Instantiate(InOutEffectPrefab,pos,Quaternion.identity);     
            }
        }
    }
    
    public struct PhysicInfo
    {
        public float Gravity;
        public float MoveFactor;
        
        public PhysicInfo( float gravity = 0, float moveFactor = 1 )
        {
            this.Gravity = gravity;
            this.MoveFactor = moveFactor;
        }
    }
}
