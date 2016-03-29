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

        PhysicsData physicInfo;

        void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;

            physicInfo = new PhysicsData( -DruggedEngine.Gravity + RelativeGravity, MoveFactor );
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            DEController controller = collider.GetComponent<DEController>();

            if ( controller == null )
                return;

            In( controller );

            ShowInOutEffect( controller.transform.position );
        }

        void OnTriggerExit2D(Collider2D collider)
        {
            DEController controller = collider.GetComponent<DEController>();
            
            if ( controller == null )
                return;

            Out( controller );

            ShowInOutEffect( controller.transform.position );
        }

        virtual protected void In( DEController controller )
        {
            controller.SetExternalPhysics( physicInfo );
        }
        
        virtual protected void Out( DEController controller )
        {
            controller.ResetExternalPhysics();
        }
        
        virtual protected void ShowInOutEffect( Vector3 pos )
        {
            if( InOutEffectPrefab != null )
            {
                Instantiate(InOutEffectPrefab,pos,Quaternion.identity);     
            }
        }
    }
    
    public struct PhysicsData
    {
        public float Gravity;
        public float MoveFactor;
        
        public PhysicsData( float gravity = 0, float moveFactor = 1 )
        {
            this.Gravity = gravity;
            this.MoveFactor = moveFactor;
        }
    }
}
