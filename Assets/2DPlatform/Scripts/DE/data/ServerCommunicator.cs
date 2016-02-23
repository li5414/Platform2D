using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using druggedcode;

public class ServerCommunicator : Singleton<ServerCommunicator>
{
	public IEnumerator LoadDTS()
	{
		//ResourceFolder에서 로드. 추후 Remote 로 변경
		yield return StartCoroutine( ResourceManager.Instance.Load());
	}

    public IEnumerator Login()
    {
    	yield return new WaitForSeconds(0.01f);
    }

    public YieldInstruction Move()
    {
        return null;
    }
}
