using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using druggedcode;

public class ServerCommunicator : Singleton<ServerCommunicator>
{
    public IEnumerator Login()
    {
    	yield return new WaitForSeconds(0.01f);
    }

    public YieldInstruction Move()
    {
        return null;
    }
}
