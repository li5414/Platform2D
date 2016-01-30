using UnityEngine;

namespace druggedcode
{
    public class ANativePlugin: MonoBehaviour
    {
        public virtual void Log(string strMsg)
        {
            Debug.Log( strMsg );
        }
        
        public virtual void Alert(string title, string message, string submit = "OK good")
        {
            Debug.Log( "title : " + title + " msg : " + message + ", submit : " + submit );
        }
        
        public virtual void Confirm(string title, string message, string positive = "OK", string negative = "CANCEL")
        {
            Debug.Log( "title : " + title + " msg : " + message + ", positive : " + positive + ", negative : " + negative );
        }
    }
}

