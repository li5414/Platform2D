using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

namespace druggedcode
{
    [RequireComponent( typeof( Image ))]
    public class PopupImage : MonoBehaviour, IPopup
    {

        protected bool _isOpen;

        public UnityAction OnSubmit{ get; set;}
        public UnityAction OnCancel{ get; set;}
        
        public UnityAction OnCloseStart{ get; set;}
        public UnityAction OnCloseComplete{ get; set; }

        Image _img;

        float _autoCloseDelay = 1;

        void Awake()
        {
            _img = GetComponent<Image>();

            gameObject.SetActive( false );
        }

        public Sprite sprite
        {
            get{ return _img.sprite; }
            set
            {
                _img.sprite = value;
                _img.SetNativeSize();
            }
        }

        public void SetDelay( float autoCloseDelay = 1.5f )
        {
            _autoCloseDelay = autoCloseDelay;
        }

        public void Open()
        {

            if( _isOpen )
                return;
            
            _isOpen = true;
            
            iTween.Stop( gameObject );
            iTween.FadeTo( gameObject,iTween.Hash(
                "alpha",1,
                "time",0.3f,
                "easeType",iTween.EaseType.easeOutQuint
                ));
            

            if( _autoCloseDelay > 0) Invoke( "Close", _autoCloseDelay );
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
            get{ return _isOpen; }
        }


    }
}


