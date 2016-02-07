using UnityEngine;
using System.Collections;

namespace druggedcode.engine
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueZone : MonoBehaviour 
    {   
		public enum DialogueType
		{
			AUTO,
			MANUAL
		}

        public DialogueType type;


        [Header("DialogueZone Setting")]
        //true 인 경우 언제나 BoxCollider의 영역 상단에 인디케이터 표시
        public bool alwaysShowInticator=true;
        public bool canMoveWhileTalking=true;
        // true 인 경우 여러번 활성화 될 수 있다.
        public bool mutipleActivate=true;
        ///여러번 활성화 될 수 있는 경우 얼마나 뒤에 다시 활성화 될 것인지
        [Range (1, 10)]
        public float reactiveCooltime=2f;
        /// 메세지 지속 시간. AutoMode only. 추후 버튼 모드와 상관없이 별도의 플래그를 고려
        [Range (1, 10)]
        public float messageAutoDuration=3f;

        [Header("DialogueBox Setting")]
        public Color textBackgroundColor = Color.black;
        public Color textColor = Color.white;
        public bool arrowVisible = true;
        public float fadeDuration=0.2f;
        public float transitionTime=0.2f;
        public float distanceFromTop=0;
        ///대사. 캐릭터에 종속되어있을경우 캐릭터에게 넘겨받는 것 고려
        [Multiline]
        public string[] dialogue;

        //-----------------------------------------------------------------------------------------------
        // private
        //-----------------------------------------------------------------------------------------------

        bool mIsPlaying = false;
        bool mIsActivated = false;
        bool mUsable = true;

        BoxCollider2D mBoxCollider;

        DialogueBox _dialogueBox;
        int mCurrentIndex;
		DEPlayer mPlayer;

        GameObject mIndicator;
        Renderer mIndicatorRenderer;

        Coroutine mShowRoutine;
        Coroutine mHideRoutine;

        void Awake()
        {
            mBoxCollider = GetComponent<BoxCollider2D>();
            mBoxCollider.isTrigger = true;
            mCurrentIndex = 0;

            mIndicator = transform.Find("Indicator").gameObject;
            mIndicatorRenderer = mIndicator.GetComponent<SpriteRenderer>();
            Motion2D.SetAlpha( mIndicatorRenderer, 0f );
            mIndicator.gameObject.SetActive( false );
        }
        void Start () 
        {
            if (alwaysShowInticator) ShowPrompt();
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            DEPlayer player = collider.GetComponent<DEPlayer>();
            if (player==null) return;

            mPlayer = player;

            switch( type )
            {
                case DialogueType.MANUAL:
                    if ( mUsable && mIsPlaying == false ) ShowPrompt();
                    mPlayer.currentDialogueZone = this;
                    break;

                case DialogueType.AUTO:
                    StartDialogue();
                    break;
            }
        }

        void OnTriggerExit2D(Collider2D collider)
        {
            if( mPlayer == null ) return;

            DEPlayer exitedPlayer = collider.GetComponent<DEPlayer>();
            if ( exitedPlayer == null ) return;

            if( type == DialogueType.MANUAL )
            {
                if ( alwaysShowInticator == false ) HidePrompt();
            }

            mPlayer.currentDialogueZone = null;
        }

        void ShowPrompt()
        {
            mIndicator.transform.position = new Vector2( mBoxCollider.bounds.center.x, mBoxCollider.bounds.max.y + distanceFromTop ); 
            if( mHideRoutine != null )StopCoroutine( mHideRoutine );
            mShowRoutine =  StartCoroutine( Motion2D.FadeAlpha(mIndicatorRenderer, 0.3f, 1f )); 
        }

        void HidePrompt()
        {   
            if( mShowRoutine != null ) StopCoroutine( mShowRoutine );
            mHideRoutine = StartCoroutine( Motion2D.FadeAlpha( mIndicatorRenderer ,0.3f,0f ));   
        }

        void ResumPlayer()
        {
            GameManager.Instance.PlayerResume();
        }

        // 버튼을 누르거나 영역에 들어오거나 해서 실행되면 대화를 시작한다
        public void StartDialogue()
        {
            if ( mIsPlaying ) return;

            // 존이 이미 활성화 되었었고 두번 이상 작동할 수 없는 경우 리턴
            if ( mIsActivated && mutipleActivate == false ) return;
            if ( mUsable == false ) return;

            HidePrompt();

            // 대화도중 움직일 수 없게 설정 했다면 캐릭터를 멈추고 조정할 수 없도록 한다 
            if ( canMoveWhileTalking == false ) GameManager.Instance.PlayerPause();

            // 만약 재생이 아직 되지 않았다면 대화박스를 초기화 시킨다.
            if ( mIsPlaying == false )
            {   
                //대화박스 생성
                GameObject dialogueObject = (GameObject)Instantiate( ResourceManager.Instance.Load("GUI/DialogueBox"));
                _dialogueBox = dialogueObject.GetComponent<DialogueBox>();      
                _dialogueBox.transform.position=new Vector2(mBoxCollider.bounds.center.x,mBoxCollider.bounds.max.y+distanceFromTop); 

                return;
				//TODO
				/*
                // 컬러지정
                _dialogueBox.ChangeColor(textBackgroundColor,textColor);
                // 버튼을 눌러 활성화 시킬지 아닐지 설정(대화상자의 버튼 A아이콘 표시 여부)
				_dialogueBox.ButtonActive( type == DialogueType.MANUAL );

                // 화살표를 표시할지 안할지 설정
                if ( arrowVisible == false ) _dialogueBox.HideArrow();          

                mIsPlaying = true;
				*/
            }

            StartCoroutine( PlayNextDialogue() );
        }

        /// <summary>
        /// 큐에 등록된 다음 대화를 재생한다
        /// </summary>
        IEnumerator PlayNextDialogue()
        {       
            // 최초 메세지가 아니라면 기존 대화박스를 페이드아웃 하고 설정한 대사사이의 시간만큼 대기
            if ( mCurrentIndex != 0 )
            {
                _dialogueBox.FadeOut(fadeDuration); 
                yield return new WaitForSeconds(transitionTime);
            }   

            //마지막 대화 라인에 도달하면 종료
            if ( mCurrentIndex >= dialogue.Length )
            {
                mCurrentIndex = 0;

                Destroy(_dialogueBox.gameObject);

                mBoxCollider.enabled = false;
                mIsActivated = true;

                //플레이어의 움직임을 제한 했을 경우 다시 정상화 한다.
                if ( canMoveWhileTalking ==false )
                {
                    ResumPlayer();
                }

                //버튼모드 이고 캐릭터가 영역에 들어와 있는 상태라면 캐릭터의 대화영역 옵션을 리셋한다
				if ( type == DialogueType.MANUAL && mPlayer!=null )
                {               
                    mPlayer.currentDialogueZone = null;
                }

                // 두번 이상 활성화 되게 해놨다면 일정시간 이후 회복 되게 코루틴 생성
                if ( mutipleActivate )
                {
                    mUsable = false;
                    mIsPlaying = false;
                    StartCoroutine(Reactivate());
                }
                //한번만 활성이 된다면 마지막 대화 시 비활성
                else
                {
                    gameObject.SetActive(false);
                }
                yield break;
            }

            //대사창 
            _dialogueBox.FadeIn(fadeDuration);
            _dialogueBox.ChangeText(  dialogue[ mCurrentIndex ] );

            mCurrentIndex++;

            // 버튼 모드가 아니면 자동으로 다음 대화창이 표시되도록 한다
			if ( type == DialogueType.AUTO )
            {
                StartCoroutine(AutoNextDialogue());
            }
        }

        IEnumerator AutoNextDialogue()
        {
            // 일정 시간 이후 다음 대사창을 나오도록 한다
            yield return new WaitForSeconds(messageAutoDuration);
            StartCoroutine(PlayNextDialogue());
        }

        IEnumerator Reactivate()
        {
            yield return new WaitForSeconds(reactiveCooltime);
            mBoxCollider.enabled=true;
            mUsable=true;

            if (alwaysShowInticator) ShowPrompt();
        }
    }
}