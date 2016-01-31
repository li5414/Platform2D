using UnityEngine;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using druggedcode;
using druggedcode.engine;
using UnityEngine.Assertions;

public class GameManager : Singleton<GameManager>
{

    UISystem mUI;
    SceneUtil mScene;

    /// 현재 일시정지 상태인지 여부
    public bool Paused { get; private set; }

    public DEPlayer player { get; private set; }

    public World world{ get; private set;}
    
    public User user{ get; private set; }
    public DECamera cam{ get; private set; }
    
    float mSavedTimeScale;

	[RuntimeInitializeOnLoadMethodAttribute]
	public static void InitializeManager()
	{
		//Debug.Log("GM StartUp once!");
	}

    override protected void Awake()
    {
        base.Awake();
        mScene = new SceneUtil();
        
        cam = DECamera.Instance;
        
        SoundManager.SetParent(sInstance.transform);
        ResourceManager.SetParent(sInstance.transform);
        ServerCommunicator.SetParent(sInstance.transform);
		user = User.SetParent(sInstance.transform);
		mUI = UISystem.SetParent( sInstance.transform );

        Config.Init();
        DruggedEngine.Init();

		mUI.Init();
    }

    IEnumerator Start()
    {
        print("[GM] Start");
     
        yield return mUI.FadeOut(0f);

        yield return StartCoroutine(ResourceManager.Instance.Load());

        yield return StartCoroutine(ServerCommunicator.Instance.Login());

        User.Instance.SetData("loaded data");

        switch (mScene.CurrentSceneName)
        {
            case Config.SC_MAIN:
                StartMain();
                break;

            case Config.SC_WORLD:
                StartWorld();
                break;

            case Config.SC_WORLDMAP:
                break;
        }
    }

    //------------------------------------------------------------------------------------------
    //-- Starat
    //------------------------------------------------------------------------------------------

    void StartMain()
    {
        print("[GM] StartMain");
        mUI.MainMode();
        mUI.FadeIn();
    }

    void StartWorld()
    {
        print("[GM] StartWorld");
        
		world = GameObject.FindWithTag(Config.TAG_BATTLE_WORLD).GetComponent<World>();
		mUI.WorldMode();

        MoveLocation(user.locationID, user.checkPointID);
    }
    
    //------------------------------------------------------------------------------------------
    //-- World
    //------------------------------------------------------------------------------------------
    
    public void MoveLocation( string locationID, string cpID )
    {
        StartCoroutine(MoveLocationRoutine( locationID, cpID ));
    }

    IEnumerator MoveLocationRoutine( string locationID, string cpID )
    {
        print("[GM] MoveLocation locationID: " + locationID + ", cpID: " + cpID );

        //플레이어의 움직임을 멈추고 화면을 페이드 아웃
		if( player != null ) player.DeActive();
        
		yield return mUI.FadeOut();
         
		//이동하고자 하는 위치를 저장해야한다.( 추후 서버 작업 필요 )
		yield return User.Instance.Move( locationID, cpID );

        //카메라를 리셋하자.
        cam.Reset();
        
        //만약 월드 씬이 아니라면 월드 씬으로 이동한다.
        if (mScene.CurrentSceneName != Config.SC_WORLD)
        {
            yield return StartCoroutine(mScene.LoadLevelAsync( Config.SC_WORLD ));

			world = GameObject.FindWithTag(Config.TAG_BATTLE_WORLD).GetComponent<World>();
			mUI.WorldMode();
        }

        //현재 location이 목표 location이 아니라면 해당 로케이션을 로드한다.
        if ( world.checkLocation( locationID ) == false )
        {
            yield return StartCoroutine(world.Load(locationID));
        }

        if (world.currentLocation == null)
        {
            throw new UnityException("location is null!!!!");
        }

        yield return null;
        
        //시작
        player = User.Instance.GetCharacter();
		world.Run( player, cpID );
        cam.Run();
		player.Active();

		 mUI.FadeIn();

    }

    public void Restart()
    {
        //Application.LoadLevel (Application.loadedLevelName);
    }

    //--------------------------------------------------------------------------------------
    // time controll
    //--------------------------------------------------------------------------------------
    // 전달된 값으로 타임스케일 설정
    public void SetTimeScale(float newTimeScale)
    {
        mSavedTimeScale = Time.timeScale;
        Time.timeScale = newTimeScale;
    }

    // 최근의 타임스케일 값으로 복원
    public void ResetTimeScale()
    {
        Time.timeScale = mSavedTimeScale;
    }

    // 게임을 일시 정지
    public void Pause()
    {
        if (Time.timeScale > 0.0f)
        {
            Instance.SetTimeScale(0.0f);
            Instance.Paused = true;
            UISystem.Instance.SetPause(true);
        }
        else
        {
            Instance.ResetTimeScale();
            Instance.Paused = false;
            UISystem.Instance.SetPause(false);
        }
    }

    //--------------------------------------------------------------------------------------
    // character controll
    //--------------------------------------------------------------------------------------

    public void PlayerPause()
    {
        if( player == null ) return;
        player.Stop();
        player.Controllable( false );
    }

    public void PlayerResume()
    {
        player.Controllable( true );
    }

    public void PlayerKill()
    {
        player.Kill();
    }

    IEnumerator PlayerKillRoutine()
    {
        yield break;
        //        _player.Kill();
        //        _cameraController.FollowsPlayer=false;
        //        yield return new WaitForSeconds(2f);
        //
        //        _cameraController.FollowsPlayer=true;
        //        if (_currentCheckPointIndex!=-1)
        //            _checkpoints[_currentCheckPointIndex].SpawnPlayer(_player);
        //
        //        _started = DateTime.UtcNow;
        //        GameManager.Instance.SetPoints(_savedPoints);
    }
}
