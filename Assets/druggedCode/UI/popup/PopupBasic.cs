using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using druggedcode;

namespace druggedcode
{
    [RequireComponent( typeof( RectTransform))]
    public class PopupBasic : MonoBehaviour, IPopup
    {

        public Vector2 minSize = new Vector2(200f,50f);
        public Vector2 maxSize = new Vector2(600f,500f);
        public Vector2 padding = new Vector2( 5f,5f );
        
        public RectTransform container;
        public Text textMessage;
        public Button btnSubmit;
        public Button btnCancel;
        
        public UnityAction OnSubmit{ get; set;}
        public UnityAction OnCancel{ get; set;}
        
        public UnityAction OnCloseStart{ get; set;}
        public UnityAction OnCloseComplete{ get; set; }

        public enum PopupState
        {
            Notification = 0,
            Alet,
            Confirm
        }

        PopupState _currentState;

        Text _positiveText;
        Text _negativeText;

        Text _btnText;

        RectTransform _bgTr;
        RectTransform _textTr;
        RectTransform _btnSubmitTr;
        RectTransform _btnCancelTr;

        bool _isOpen;

        Image _bg;

        float _autoCloseDelay = 1;

        void Awake()
        {
            _bg = GetComponent<Image>();
            _bgTr = _bg.rectTransform;
            _bgTr.anchorMin = new Vector2(0.5f, 0.5f);
            _bgTr.anchorMax = new Vector2(0.5f, 0.5f);
            _bgTr.sizeDelta = new Vector2( minSize.x, minSize.y );

            textMessage.alignment = TextAnchor.UpperCenter;
            textMessage.resizeTextForBestFit = false;
            textMessage.supportRichText = true;
            _textTr = textMessage.rectTransform;
            _textTr.anchorMin = new Vector2(0.5f, 1f);
            _textTr.anchorMax = new Vector2(0.5f, 1f);
            _textTr.pivot = new Vector2(0.5f,1f);

            _btnSubmitTr = btnSubmit.GetComponent<RectTransform>();
            _btnSubmitTr.anchorMin = new Vector2(0.5f, 1f);
            _btnSubmitTr.anchorMax = new Vector2(0.5f, 1f);
            _btnSubmitTr.pivot = new Vector2(0.5f,1f);
            btnSubmit.onClick.AddListener( delegate(){
                Submit();
            });

            _btnCancelTr = btnCancel.GetComponent<RectTransform>();
            _btnCancelTr.anchorMin = new Vector2(0.5f, 1f);
            _btnCancelTr.anchorMax = new Vector2(0.5f, 1f);
            _btnCancelTr.pivot = new Vector2(0.5f,1f);
            btnCancel.onClick.AddListener( delegate(){
                Cancel();
            });

            MotionUI.SetAlphaInChildren( gameObject, 0f );

            gameObject.SetActive( false );
        }

        public void SetButtonText( string bo, string b )
        {

        }

        public void SetMessage( string msg )
        {
            textMessage.text = msg;
        }

        public void SetDelay( float autoCloseDelay = 1.5f )
        {
            _autoCloseDelay = autoCloseDelay;
        }

        void Submit()
        {
            if( OnSubmit != null )
            {
                OnSubmit();
                OnSubmit = null;
            }

            Close();

        }

        void Cancel()
        {
            if( OnCancel != null )
            {
                OnCancel();
                OnCancel = null;
            }

            Close();
        }


        public void Open()
        {
            if( _isOpen )
                return;
            
            _isOpen = true;

            UpdateElement();
            UpdateSizeAndPosition();

            iTween.Stop( gameObject );
            iTween.FadeTo( gameObject,iTween.Hash(
                "alpha",1,
                "time",0.3f,
                "easeType",iTween.EaseType.easeOutQuint
            ));

            if( PopupState.Notification == _currentState && _autoCloseDelay > 0) Invoke( "Close", _autoCloseDelay );
        }

        void UpdateSizeAndPosition()
        {
            float txtw = textMessage.preferredWidth;
            float txth = textMessage.preferredHeight;

            float conW = 0f;
            float conH = 0f;

            float w = 0f;
            float h = 0f;

            switch( _currentState )
            {
                case PopupState.Notification:
                    conW = txtw;
                    conH = txth;

                    _textTr.anchoredPosition = new Vector2(0,0);
                    break;
                    
                case PopupState.Alet:
                    conW = Mathf.Max( txtw, _btnSubmitTr.sizeDelta.x );
                    conH = txth + _btnSubmitTr.sizeDelta.y + padding.y;
                        
                    _textTr.anchoredPosition = new Vector2(0, 0 );
                    _btnSubmitTr.anchoredPosition = new Vector2(0, -txth - padding.y );
                    
                    break;
                    
                case PopupState.Confirm:
                    conW = Mathf.Max( txtw, _btnSubmitTr.sizeDelta.x + _btnCancelTr.sizeDelta.x + padding.x );
                    conH = txth + _btnSubmitTr.sizeDelta.y + padding.y;

                    _textTr.anchoredPosition = new Vector2(0, 0 );
                    _btnSubmitTr.anchoredPosition = new Vector2( +(_btnSubmitTr.sizeDelta.x + padding.x) * 0.5f , -txth - padding.y );
                    _btnCancelTr.anchoredPosition = new Vector2( -( _btnCancelTr.sizeDelta.x + padding.x ) * 0.5f, -txth - padding.y );
                    break;
            }

            container.sizeDelta = new Vector3( conW, conH );

            w = conW + padding.x * 2;
            h = conH + padding.y * 2;
            w = Mathf.Clamp( w, minSize.x, maxSize.x );
            h = Mathf.Clamp( h, minSize.y, maxSize.y );

            print( "w: " + w + " h : " + h +", minSize : " +minSize + ", maxSize : " +maxSize );

            _bg.rectTransform.sizeDelta = new Vector2( w,h );
        }
        
        void UpdateElement()
        {
            switch( _currentState )
            {
                case PopupState.Notification:
                    btnSubmit.gameObject.SetActive( false );
                    btnCancel.gameObject.SetActive( false );

                    break;
                    
                case PopupState.Alet:
                    btnSubmit.gameObject.SetActive( true );
                    btnCancel.gameObject.SetActive( false );
                    break;
                    
                case PopupState.Confirm:
                    btnSubmit.gameObject.SetActive( true );
                    btnCancel.gameObject.SetActive( true );
                    break;
            }
        }
        
        public void Close()
        {
            if( _isOpen == false )
                return;

            _isOpen = false;

            if( OnCloseStart != null )
            {
                OnCloseStart();
                OnCloseStart = null;
            }

            iTween.Stop( gameObject );
            iTween.FadeTo( gameObject,iTween.Hash(
                "alpha",0,
                "time",0.3f,
                "easeType",iTween.EaseType.easeOutQuint,
                "oncomplete","CloseComplete"
            ));
        }

        void CloseComplete()
        {
            if( OnCloseComplete != null )
            {
                OnCloseComplete();
                OnCloseComplete = null;
            }
        }

        public bool isOpen
        {
            get{ return _isOpen;}
        }

        public PopupState state
        {
            get{ return _currentState; }
            set
            {
                _currentState = value;
            }
        }
    }
}

