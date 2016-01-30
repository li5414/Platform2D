using UnityEngine;
using System.Collections;

namespace druggedcode
{
    public class DebugUtil
    {
        static public bool NullChecker( System.Object obj, string existMsg = "exsit", string nullMsg = "null")
        {
            if( obj != null )
            {
                Debug.Log( existMsg );
                return true;
            }
            else
            {
                Debug.Log( nullMsg );
                return false;
            }
        }
    }
}

