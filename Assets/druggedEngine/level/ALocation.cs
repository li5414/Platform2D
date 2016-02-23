using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using druggedcode;
using druggedcode.engine;

public class ALocation : MonoBehaviour
{
    public CheckPoint defaultCheckPoint;
    public BoundariesInfo defaultBoundary;
    public Transform skybox;
    public AudioClip bgm;

    public DTSLocation dts { get; set; }
	public CheckPoint currentCheckPoint{ get; private set;} 

    DateTime mStarted;
    TimeSpan runningTime{ get{ return DateTime.UtcNow - mStarted; }}

    List<CheckPoint> mCheckPointList;
    List<DECharacter> mCharacterList;

	List<EnemySpawner> mSpawners;

    bool mIsRun;

    DECamera mCam;
    Mission mMission;

    void Awake()
    {
		tag = Config.TAG_LOCATION;
        //mCam = DECamera.Instance;

        mCharacterList = new List<DECharacter>();

        mMission = GetComponent<Mission>();

        //모든 checkPoint 를 찾는다.
        // GetComponentsInChildren<CheckPoint>( mCheckPointList );
        mCheckPointList = FindObjectsOfType<CheckPoint>().OrderBy(t => t.transform.position.x).ToList();

		GetComponentsInChildren<EnemySpawner>( mSpawners );
    }
    
    void Start()
    {
        //IPlayerRespawnListener 를 구현하거나 상속한 모든 오브젝트를 찾는다.
        //각 오브젝트에서 왼쪽으로 가장 가가운 CheckPoint 객체를 찾아 해당 객체에 자신을 등록한다.(add)
        IPlayerRespawnListener[] listenerArr = GetComponentsInChildren<IPlayerRespawnListener>();
        foreach (IPlayerRespawnListener listener in listenerArr)
        {
            for (var i = mCheckPointList.Count - 1; i >= 0; i--)
            {
                var distance = ((MonoBehaviour)listener).transform.position.x - mCheckPointList[i].transform.position.x;
                if (distance < 0)
                    continue;

                mCheckPointList[i].AssignObjectToCheckPoint(listener);
                break;
            }
        }
    }

    //현재 실행되어야 할 미션이 있을 경우 실행한다. 없다면기본실행을 한다.
	virtual public void Run( DEPlayer player, string cpID )
    {
        if (mIsRun == false)
        {
			mCam = GameManager.Instance.gameCamera;
			mCam.AddPlayer( player );
			mCam.SetSkybox( skybox );
			mCam.Run();

            mStarted = DateTime.UtcNow;
            mIsRun = true;
			Debug.Log("[Location]" + dts.name + "Run! (" + mStarted +")");
        }

        SpawnPlayer( player, cpID );
    }

	public void SpawnPlayer( DEPlayer player, string cpID )
    {
        CheckPoint cp = GetCheckPoint( cpID );

		if ( currentCheckPoint != null && currentCheckPoint != cp)
        {
			currentCheckPoint.active = false;
        }

		if( cp == null )
		{
			player.Spawn( Vector3.zero );
		}
		else
		{
			currentCheckPoint = cp;
			currentCheckPoint.active = true;
			currentCheckPoint.Spawn();
			player.Spawn( currentCheckPoint.transform.position );
		}

		BoundariesInfo bound = GetBoundariesInfo();

        mCam.SetBound(bound);
        mCam.CenterOnTargets();

        CheckEvent();

//		player.Active();
    }

    void CheckEvent()
    {
        if (mMission == null) return;
        mMission.Check();
    }

	BoundariesInfo GetBoundariesInfo()
	{
		BoundariesInfo bound = currentCheckPoint == null ? null : currentCheckPoint.GetComponent<BoundariesInfo>();
		if( bound == null ) bound = defaultBoundary;
		return bound;
	}

    CheckPoint GetCheckPoint(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return defaultCheckPoint;
        }

        foreach (CheckPoint cp in mCheckPointList)
        {
            if (cp.id == id) return cp;
        }

        return null;
    }

    public void Dispose()
    {
		mCam.RemoveAllTarget();
        Destroy(gameObject);
    }
}