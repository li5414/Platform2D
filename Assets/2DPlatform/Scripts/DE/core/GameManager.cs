using UnityEngine;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using druggedcode;
using druggedcode.engine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public DECamera gameCamera;
	public UISystem UI;

    /// 현재 일시정지 상태인지 여부
    public bool Paused { get; private set; }

    public DEPlayer player { get; private set; }

	public ALocation Location{ get; private set;}
    
    float mSavedTimeScale;

	[RuntimeInitializeOnLoadMethodAttribute]
	public static void InitializeManager()
	{
		if( Instance == null )
		{
			GameManager prefab = Resources.Load<GameManager>("GameManager");
			GameManager manager = GameObject.Instantiate<GameManager>( prefab );
		}
	}

    void Awake()
    {
		Instance = this;
		name = typeof( GameManager ).Name;
		DontDestroyOnLoad( this );

		var cam = GameObject.Find("Main Camera");
		DestroyObject( cam );

		ServerCommunicator.SetParent( transform );
		User.SetParent( transform );
//        SoundManager.SetParent(sInstance.transform);
//        ResourceManager.SetParent(sInstance.transform);
    }

    IEnumerator Start()
    {
		UI.Init();
		Config.Init();

		string firstSceneName = SceneManager.GetActiveScene().name;
		print("[GM] Start At: " + firstSceneName );
     
		if( firstSceneName == Config.SC_TITLE )
		{
			StartCoroutine( StartedAtTitle());
		}
		else
		{
			StartCoroutine( StartedAtLevel());
		}

		yield break;
    }

	IEnumerator StartedAtTitle()
	{
		//뺑글뱅글 로딩 화면띄우자
		yield return StartCoroutine(ServerCommunicator.Instance.LoadDTS());

		UI.MainMode();
	}

	IEnumerator StartedAtLevel()
	{
		print("[GM] DevRoute");

		yield return UI.FadeOut(0f);
		yield return StartCoroutine(ServerCommunicator.Instance.LoadDTS());

		yield return StartCoroutine(ServerCommunicator.Instance.Login());
		User.Instance.SetData("loaded data");

		yield break;
		Location = GameObject.FindWithTag(Config.TAG_LEVEL).GetComponent<ALocation>();

		//현재 로케이션의 ID 를 알아야 한다.
		UI.WorldMode();

		print("[GM] StartWorld");

		Location = GameObject.FindWithTag(Config.TAG_LEVEL).GetComponent<ALocation>();
		UI.WorldMode();

		MoveLocation(User.Instance.locationID, User.Instance.checkPointID);

//		AssetBundleLoadOperation request = AssetBundleManager.LoadLevelAsync(sceneAssetBundle, levelName, isAdditive);
	}

	//called by Title
	public Coroutine Login()
	{
		return StartCoroutine( LoginRoutine());
	}

	IEnumerator LoginRoutine()
	{
		yield return StartCoroutine(ServerCommunicator.Instance.Login());
		User.Instance.SetData("loaded data");
	}

    //------------------------------------------------------------------------------------------
    //-- Starat
    //------------------------------------------------------------------------------------------

    

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
        
		yield return UI.FadeOut();
         
		//이동하고자 하는 위치를 저장해야한다.( 추후 서버 작업 필요 )
		yield return User.Instance.Move( locationID, cpID );

        //카메라를 리셋하자.
        gameCamera.Reset();
        
//        //만약 월드 씬이 아니라면 월드 씬으로 이동한다.
//        if (mScene.CurrentSceneName != Config.SC_WORLD)
//        {
//            yield return StartCoroutine(mScene.LoadLevelAsync( Config.SC_WORLD ));
//
//			world = GameObject.FindWithTag(Config.TAG_BATTLE_WORLD).GetComponent<World>();
//			mUI.WorldMode();
//        }

        //현재 location이 목표 location이 아니라면 해당 로케이션을 로드한다.
//        if ( world.checkLocation( locationID ) == false )
//        {
//            yield return StartCoroutine(world.Load(locationID));
//        }
//
//        if (world.currentLocation == null)
//        {
//            throw new UnityException("location is null!!!!");
//        }
//
//        yield return null;
//        
//        //시작
//        player = User.Instance.GetCharacter();
//		world.Run( player, cpID );
//        gameCamera.Run();
//		player.Active();
//
//		UI.FadeIn();

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
			UI.SetPause(true);
        }
        else
        {
            Instance.ResetTimeScale();
            Instance.Paused = false;
			UI.SetPause(false);
        }
    }

    //--------------------------------------------------------------------------------------
    // character controll
    //--------------------------------------------------------------------------------------

    public void PlayerPause()
    {
        if( player == null ) return;
        player.Stop();
		player.controllable = false;
    }

    public void PlayerResume()
    {
		player.controllable = true;
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
