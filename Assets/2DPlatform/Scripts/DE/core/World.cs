using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using druggedcode.engine;
using Com.LuisPedroFonseca.ProCamera2D;

public class World : MonoBehaviour
{
    ALocation mCurrentLocation;

	public event UnityAction<ALocation> OnUpdateLocation;

    void Awake()
    {
        ALocation[] temp = GetComponentsInChildren<ALocation>();
        foreach (ALocation loc in temp)
        {
            loc.gameObject.SetActive(false);
        }

        DECharacter[] chs = GetComponentsInChildren<DECharacter>();
        foreach (DECharacter ch in chs)
        {
            ch.gameObject.SetActive(false);
        }
    }

    public bool checkLocation( string locationID )
    {
        if (mCurrentLocation == null) return false;
        return mCurrentLocation.dts.id == locationID;
    }

    public IEnumerator Load(string locationID)
    {
        if( mCurrentLocation != null ) mCurrentLocation.Dispose();

        DTSLocation dts = ResourceManager.DTSLocationDatas.Get(locationID);
        ResourceRequest req = ResourceManager.Instance.LoadLocation( dts );
        
        yield return req;
        
        if (req.asset == null)
        {
            print("[World] " + locationID + " is load fail");
            yield break;
        }
        
        mCurrentLocation = GameObject.Instantiate(req.asset, Vector3.zero, Quaternion.identity) as BattleField;
        mCurrentLocation.transform.SetParent(transform);
        mCurrentLocation.dts = dts;
        
		yield return null;//wait start

        print("[World] " + locationID + " is load Complete");
    }

	//현재 실행되어야 할 미션이 있을 경우 실행한다. 없다면기본실행을 한다.
	virtual public void Run( DEPlayer player, string cpID )
	{
		mCurrentLocation.Run( player, cpID );

		if( OnUpdateLocation != null ) OnUpdateLocation( mCurrentLocation);
	}

    public ALocation currentLocation
    {
        get{ return mCurrentLocation; }
    }
}
