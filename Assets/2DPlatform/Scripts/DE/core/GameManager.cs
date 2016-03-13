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
    public bool paused { get; private set; }

    public DEPlayer player { get; private set; }

	public ALocation location{ get; private set;}
	public bool playerControllable{ get; set; }

    float mSavedTimeScale;
	Scene mCurrentScene;

	[RuntimeInitializeOnLoadMethodAttribute]
	public static void InitializeManager()
	{
		if( Instance != null ) return;

		string startSceneName = SceneManager.GetActiveScene().name;

		if( string.IsNullOrEmpty( startSceneName )) return;

		GameManager prefab = Resources.Load<GameManager>("GameManager");
		GameObject.Instantiate<GameManager>( prefab );

	}

    void Awake()
    {
        if( Instance != null && Instance != this )
        {
            Destroy( gameObject );
            return;
        }
        
		Instance = this;
		name = typeof( GameManager ).Name;
		DontDestroyOnLoad( this );

		GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
		if( cam != null ) DestroyObject( cam );

		ServerCommunicator.SetParent( transform );
		User.SetParent( transform );
		ResourceManager.SetParent( transform);
//        SoundManager.SetParent(sInstance.transform);
    }

    IEnumerator Start()
    {
		UI.Init();
		Config.Init();

		mCurrentScene = SceneManager.GetActiveScene();

		print("[GM] Start At: " + mCurrentScene.name );
     
		if( mCurrentScene.name == Config.SC_TITLE )
		{
			StartCoroutine( StartedAtTitle());
		}
		else
		{
			StartCoroutine( StartedAtLocation());
		}

		yield break;
    }

	IEnumerator StartedAtTitle()
	{
		//뺑글뱅글 로딩 화면띄우자
		yield return StartCoroutine(ServerCommunicator.Instance.LoadDTS());

		UI.MainMode();
	}

	ALocation FindLocationInCurrentScene()
	{
		GameObject levelObj = GameObject.FindWithTag(Config.TAG_LOCATION);

		if( levelObj == null )
		{
			return null;
		}

		return levelObj.GetComponent<ALocation>();
	}

	IEnumerator StartedAtLocation()
	{
		print("[GM] DevRoute");

		yield return UI.FadeOut(0f);
		yield return StartCoroutine(ServerCommunicator.Instance.LoadDTS());
		yield return StartCoroutine( LoginRoutine() );

		ALocation loc = FindLocationInCurrentScene();

		if( loc == null )
		{
			Debug.Log("[GM] Level Object was null");
			UI.FadeIn();
			yield break;
		}

		DTSLocation dts = ResourceManager.Instance.GetDTSLocationByAssetName( mCurrentScene.name );

		if( dts == null )
		{
			Debug.LogError( mCurrentScene.name + " Level's DTSLocation not found");
			UI.FadeIn();
			yield break;
		}

		print( string.Format("[GM] Level id: {0}, name: {1}, assetName: {2}", dts.id, dts.name, dts.assetName ));

		loc.dts = dts;

		StartCoroutine( RunLocation( loc ));
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

    public void MoveLocation( string locationID, string cpID )
    {
        StartCoroutine(MoveLocationRoutine( locationID, cpID ));
    }

    IEnumerator MoveLocationRoutine( string locationID, string cpID )
    {
        print("[GM] MoveLocation locationID: " + locationID + ", cpID: " + cpID );

		PlayerPause();

		gameCamera.Reset();

		yield return UI.FadeOut();

		if( player != null )
		{
			player.transform.SetParent( User.Instance.transform );
			player.gameObject.SetActive( false );
		}

		yield return ServerCommunicator.Instance.Move( locationID, cpID );

		DTSLocation dts = string.IsNullOrEmpty( locationID ) ?  location.dts : ResourceManager.Instance.GetDTSLocation( locationID );

		//현재 Scene 에 이동해야 할 Locatino 이 없다면 해당 location Scene으로 이동한다.
		if( location == null || location.dts.name != dts.name )
		{
			yield return StartCoroutine( LoadScene( dts.assetName ));

			ALocation loc = FindLocationInCurrentScene();

			if( loc == null )
			{
				print( "'" + dts.assetName + "' Location was null in Scene '" + mCurrentScene.name + "'");
				yield break;
			}

			GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
			if( cam != null ) DestroyObject( cam );

			loc.dts = dts;
			location = loc;
		}

		yield return StartCoroutine ( RunLocation( location ));
    }

	IEnumerator RunLocation( ALocation loc )
	{
		if( loc == null )
		{
			Debug.LogError( "[GM] Location null. RunLocation Fail");
			yield break;
		}

		player = User.Instance.GetCharacter();
		player.transform.SetParent( transform.parent );

		if( location != loc )
		{
			location = loc;

			UI.LocationMode();

			gameCamera.SetSkybox( location.skybox );
		}

		location.SpawnPlayer( player );

		yield return null;

		gameCamera.AddPlayer( player );
		gameCamera.SetBound( location.GetBoundariesInfo());
		gameCamera.Run();
		location.Run();

		yield return UI.FadeIn();

		playerControllable = true;

	}

    public void Restart()
    {
        //Application.LoadLevel (Application.loadedLevelName);
    }

	IEnumerator LoadScene( string sceneName )
	{
		print( "CurrentScene: " + mCurrentScene.name );
		AsyncOperation async = SceneManager.LoadSceneAsync( sceneName );
		yield return async;
		mCurrentScene = SceneManager.GetActiveScene();

		System.GC.Collect();

		print("LoadedScene: " + mCurrentScene );
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
            Instance.paused = true;
			UI.SetPause(true);
        }
        else
        {
            Instance.ResetTimeScale();
            Instance.paused = false;
			UI.SetPause(false);
        }
    }

    //--------------------------------------------------------------------------------------
    // character controll
    //--------------------------------------------------------------------------------------

    public void PlayerPause()
    {
		if( playerControllable == false ) return;
		playerControllable = false;

		if( player != null )
		{
			player.Pause();
		}
    }

    public void PlayerResume()
    {
		if( playerControllable ) return;
		playerControllable = true;

		if( player != null )
		{
			player.Active();
		}
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
