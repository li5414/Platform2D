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

    virtual protected void Awake()
    {
        //mCam = DECamera.Instance;

        mCharacterList = new List<DECharacter>();

        mMission = GetComponent<Mission>();

        //모든 checkPoint 를 찾는다.
        // GetComponentsInChildren<CheckPoint>( mCheckPointList );
        mCheckPointList = FindObjectsOfType<CheckPoint>().OrderBy(t => t.transform.position.x).ToList();

		GetComponentsInChildren<EnemySpawner>( mSpawners );
    }
    
    virtual protected void Start()
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
            Debug.Log("[Location] Start! id: " + dts.id + " asset: " + dts.assetName +  " name: " + dts.name + " Run! (" + mStarted +")");
            mStarted = DateTime.UtcNow;

            mCam.AddTarget( player.transform, 0f, 2f );
			mCam.SetSkybox(skybox);
            AddCharacter(player);

            mIsRun = true;
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

		currentCheckPoint = cp;
		currentCheckPoint.active = true;
		currentCheckPoint.SpawnPlayer(player);

		BoundariesInfo bound = currentCheckPoint.GetComponent<BoundariesInfo>();
       if( bound == null ) bound = defaultBoundary;

        mCam.SetBound(bound);
        mCam.CenterOnTargets();

        PlayBGM();

        CheckEvent();
    }

    protected void PlayBGM()
    {
        // if (bgm != null)
        // {
        //     AudioSource levelBgm = gameObject.AddComponent<AudioSource>();
        //     levelBgm.playOnAwake = false;
        //     levelBgm.spatialBlend = 0;
        //     levelBgm.rolloffMode = AudioRolloffMode.Logarithmic;
        //     levelBgm.loop = true;
        //     levelBgm.clip = bgm;

        //     SoundManager.Instance.PlayBackgroundMusic(levelBgm);
        // }
    }

    void CheckEvent()
    {
        if (mMission == null) return;
        mMission.Check();
    }


    void AddCharacter( DECharacter ch )
    {
        if( mCharacterList.Contains( ch ) == false ) mCharacterList.Add(ch);
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