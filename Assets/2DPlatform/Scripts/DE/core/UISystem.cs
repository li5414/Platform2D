using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace druggedcode.engine
{
	public class UISystem : MonoBehaviour
    {
        [Header("POPUP")]
        public GameObject pauseScreen;
        public GameObject timeSplash;
        public Image fader;
        
        [Header("MAIN")]
        public GameObject main;
        
        [Header("HUD")]
        public GameObject HUD;
        public Text levelText;
        public Text goldText;
        
        [Header("DEBUG")]
        public GameObject debug;
        public Text debugText;
		Coroutine mFadeRoutine;

       	void Awake()
        {
            HUD.SetActive(false);
            main.SetActive(false);

            MotionUI.SetAlpha(fader, 0f);
            fader.gameObject.SetActive(false);
            
            if( Application.isEditor ) debug.SetActive( true );
            else debug.SetActive( false );
        }

		public void Init()
		{
			User.Instance.OnGold += OnGold;
		}
        
        #if UNITY_EDITOR
        void Update()
        {
            debugText.text = "character : " + DECharacter.All.Count;
        }
        #endif

        //--------------------------------------------------------------------------------------------------
        // main
        //--------------------------------------------------------------------------------------------------

        public void MainMode()
        {
            main.SetActive(true);
			HUD.SetActive(false);
        }

        public void OnClickStart()
        {
			GameManager.Instance.Login();
        }

        //--------------------------------------------------------------------------------------------------
        // main
        //--------------------------------------------------------------------------------------------------
        public void LocationMode()
        {
            main.SetActive(false);
            HUD.SetActive(true);

//			public Text LevelText;
//			public Text GoldText;
            //            // 현재 씬의 이름을 UI에 표시
            //            UISystem.Instance.SetLevelName (Application.loadedLevelName);

			//GameManager.Instance.world.OnUpdateLocation += OnUpdateLocation;
        }

        void OnUpdateLocation ( ALocation loc )
        {
			levelText.text = loc.dts.name + "_"+ loc.currentCheckPoint.name;
        }

		void OnGold (int gold )
		{
			goldText.text = "$ " +gold;
		}

        //--------------------------------------------------------------------------------------------------
        // main
        //--------------------------------------------------------------------------------------------------
        public void TownMode()
        {
            main.SetActive(false);
            HUD.SetActive(true);
        }

        //--------------------------------------------------------------------------------------------------
        // main
        //--------------------------------------------------------------------------------------------------
        public void WorldMapMode()
        {
            main.SetActive(false);
        }

        //--------------------------------------------------------------------------------------------------
        // behaviour
        //--------------------------------------------------------------------------------------------------
        public void SetPause(bool state)
        {
            pauseScreen.SetActive(state);
        }

        public void SetTimeSplash(bool state)
        {
            Debug.Log("SetTimeSplash:" + state);
            timeSplash.SetActive(state);
        }

        public Coroutine FadeOut(float duration = 0.3f )
        {
			if( mFadeRoutine != null ) StopCoroutine( mFadeRoutine );
			mFadeRoutine = StartCoroutine(MotionUI.FadeAlpha(fader, duration, 1f ));
			return mFadeRoutine;
        }

        public Coroutine FadeIn(float duration = 0.3f )
        {
			if( mFadeRoutine != null ) StopCoroutine( mFadeRoutine );
			mFadeRoutine = StartCoroutine(MotionUI.FadeAlpha(fader, duration, 0f ));
			return mFadeRoutine;
        }

        void Log(object message)
        {
            Debug.Log("[GUIManager] " + message);
        }
    }
}