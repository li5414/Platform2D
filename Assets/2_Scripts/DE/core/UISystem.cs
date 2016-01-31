using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace druggedcode.engine
{
    public class UISystem : Singleton<UISystem>
    {
        public GameObject HUD;
        /// 특수화면
        public GameObject PauseScreen;
        public GameObject TimeSplash;
        public Image Fader;

        public GameObject Main;

        /// 스테이지 레벨 표시 
        public Text LevelText;
        public Text GoldText;

		User mUser;

        override protected void Awake()
        {
            base.Awake();
            HUD.SetActive(false);
            Main.SetActive(false);

            MotionUI.SetAlpha(Fader, 0f);
            Fader.gameObject.SetActive(false);

			mUser = User.Instance;
        }

		public void Init()
		{
			mUser.OnGold += OnGold;
		}

        //--------------------------------------------------------------------------------------------------
        // main
        //--------------------------------------------------------------------------------------------------

        public void MainMode()
        {
            Main.SetActive(true);
			HUD.SetActive(false);
        }

        public void OnClickStart()
        {
            User user = User.Instance;
            GameManager.Instance.MoveLocation(user.locationID, user.checkPointID);
        }

        //--------------------------------------------------------------------------------------------------
        // main
        //--------------------------------------------------------------------------------------------------
        public void WorldMode()
        {
            Main.SetActive(false);
            HUD.SetActive(true);

//			public Text LevelText;
//			public Text GoldText;
            //            // 현재 씬의 이름을 UI에 표시
            //            UISystem.Instance.SetLevelName (Application.loadedLevelName);

			GameManager.Instance.world.OnUpdateLocation += OnUpdateLocation;
        }

        void OnUpdateLocation ( ALocation loc )
        {
			LevelText.text = loc.dts.name + "_"+ loc.currentCheckPoint.name;
        }

		void OnGold (int gold )
		{
			GoldText.text = "$ " +gold;
		}

        //--------------------------------------------------------------------------------------------------
        // main
        //--------------------------------------------------------------------------------------------------
        public void TownMode()
        {
            Main.SetActive(false);
            HUD.SetActive(true);
        }

        //--------------------------------------------------------------------------------------------------
        // main
        //--------------------------------------------------------------------------------------------------
        public void WorldMapMode()
        {
            Main.SetActive(false);
        }

        //--------------------------------------------------------------------------------------------------
        // behaviour
        //--------------------------------------------------------------------------------------------------
        public void SetPause(bool state)
        {
            PauseScreen.SetActive(state);
        }

        public void SetTimeSplash(bool state)
        {
            Debug.Log("SetTimeSplash:" + state);
            TimeSplash.SetActive(state);
        }

        public Coroutine FadeOut(float duration = 0.3f )
        {
            return StartCoroutine(MotionUI.FadeAlpha(Fader, duration, 1f ));
        }

        public Coroutine FadeIn(float duration = 0.3f )
        {
            return StartCoroutine(MotionUI.FadeAlpha(Fader, duration, 0f ));
        }
        
        void Log(object message)
        {
            Debug.Log("[GUIManager] " + message);
        }
    }
}