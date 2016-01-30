using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using druggedcode;
using druggedcode.extensions.ui;

namespace druggedcode
{
    /// <summary>
    /// GraphicRaycaster 의 priority 가 현재 버전에서 readonly 로 되어있다.
    /// 기본이 3이므로 기존의 Canvas 가 있다면 입력 이벤트가 겹치므로
    /// 직접 하이어라키에 생성해 수정하던지 기존 canvas 의 priority 를 3이하로 조절한다.
    /// 
    /// sortingOrder 현재 4.6.0f3 에서 버그. 적용될려면 게임 오브젝트를 껏다 켜야한다. 추후 버그픽스되면 삭제
    /// </summary>
    [RequireComponent( typeof(Canvas),typeof(CanvasScaler), typeof(GraphicRaycaster) )]
    public class PopupManager : Singleton<PopupManager>
    {
        public GameObject basicPrefab;
        public GameObject consolePrefab;

        Canvas _canvas;
        CanvasScaler _scaler;
        GraphicRaycaster _raycaster;

        Image _dimmedLayer;
        bool _isDimmed;

        RectTransform _panel;
        RectTransform _openedContainer;
        RectTransform _closedContainer;

        IPopup _top;
        List<IPopup> _opendList;
        
        PopupBasic _basic;
        PopupImage _img;
        Console _console;

        public PopupManager Init()
        {
            _opendList = new List<IPopup>();
            
            InitCanvas();
            
            CreateDimmedLayer();
            CreateConsole();
            CretaeContainer();
            CreatePopupBasic();
            CreatePopupImag();

            return this;
        }

        void CreateConsole()
        {
            if( consolePrefab == null )
            {
                consolePrefab = Resources.Load("popupmanager/Console") as GameObject;
            }
            
            if( consolePrefab == null )
            {
                //console 기능은 필수가 아니므로 없어도 패스한다.
            }
            else
            {
                _console = Instantiate( consolePrefab ).GetComponent<Console>();
                _console.transform.SetParent( transform, false );
                _console.gameObject.SetActive( false );
            }
        }

        void CreatePopupBasic()
        {
             //기본 팝업  프리팹이 설정되어 있지 않다면 리소스 폴더에서 찾는다
             //리소스 폴더에도 존재 하지 않는다면 예외를 던짐

            if( basicPrefab == null )
            {
                basicPrefab = Resources.Load("popupmanager/PopupBasic") as GameObject;
            }

            if( basicPrefab == null )
            {
                throw new NullReferenceException("PopupBasic 프리팹이 설정되거나 리소스 폴더에 위치해야 합니다.");
            }

            _basic = Instantiate(basicPrefab).GetComponent<PopupBasic>();
            _basic.transform.SetParent( _closedContainer, false );
        }

        void CreatePopupImag()
        {
            _img = GameObjectUtil.Create<PopupImage>("PopupImage");
            _img.transform.SetParent( _closedContainer, false );

        }

        void InitCanvas()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.pixelPerfect = true;

            _raycaster = GetComponent<GraphicRaycaster>();
//            _raycaster.priority = 3;

            _scaler = GetComponent<CanvasScaler>();
            sortingOrder = 100;
        }

        void CretaeContainer()
        {
            _panel = GameObjectUtil.Create<RectTransform>("panel",transform );
            _panel.AsPanel();

            _openedContainer =  GameObjectUtil.Create<RectTransform>("openedPopup",_panel.transform );
            _openedContainer.AsPanel();

            _closedContainer =  GameObjectUtil.Create<RectTransform>("closedPopup",_panel.transform );
            _closedContainer.AsPanel();
        }

        void CreateDimmedLayer()
        {
            _dimmedLayer = GameObjectUtil.Create<Image>("dimmed",transform);
            _dimmedLayer.rectTransform.AsPanel();
            
            Dimmed(0f);
        }
        
        public void AddPopup( IPopup popup, UnityAction callback = null, float dimmedAmount = 0.5f )
        {
            if( _opendList.Contains( popup ))
            {
                print("contains");
                return;

            }
            
            if( popup.isOpen )
            {
                print("opend");
                return;
            }

            popup.OnCloseStart = delegate()
            {
                RemovePopup( popup );
                Dimmed( 0f );

                if( callback != null )
                callback();
            };

            popup.OnCloseComplete = delegate()
            {
                popup.gameObject.SetActive( false );
            };


            _opendList.Add( popup );

            Dimmed(dimmedAmount);

            popup.transform.SetParent( _openedContainer, false );
            popup.gameObject.SetActive( true );
            popup.Open();
            _top = popup;

        }
        
        void RemovePopup( IPopup popup )
        {
            if (_opendList.Remove(popup))
            {
                _top = _opendList.LastOrDefault();
                popup.transform.SetParent( _closedContainer, false );
            }
        }

        //-----------------------------------------------------------------------------------------------------------
        //init Method
        //-----------------------------------------------------------------------------------------------------------
        public PopupManager SetScaleWithScreenSize( Vector2 referenceResolution, float matchWidthOrHeight = 0, float referencePixelsPerUnit = 100f)
        {
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = referenceResolution;
            _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _scaler.matchWidthOrHeight = matchWidthOrHeight;
            _scaler.referencePixelsPerUnit = referencePixelsPerUnit;

            return this;
        }

        public PopupManager SetConstantPixelSize( float scaleFactor = 1f, float referencePixelsPerUnit = 100f)
        {
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            _scaler.scaleFactor = scaleFactor;
            _scaler.referencePixelsPerUnit = referencePixelsPerUnit;

            return this;
        }

        public PopupManager PopupBasicOption( Vector2 minSize, Vector2 maxSize, Vector2 padding )
        {
            _basic.minSize = minSize;
            _basic.maxSize = maxSize;
            _basic.padding = padding;
            
            return this;
        }

        
        public PopupManager ConsoleInit( int fontSize = 14, float bgAlpha = 0.5f )
        {
            if( _console == null )
                throw new NullReferenceException("Console 프리팹이 설정되거나 리소스 폴더에 위치해야 합니다.");
            
            _console.Init( fontSize, bgAlpha );
            
            return this;
        }
        
        public PopupManager SetRaycaster( GraphicRaycaster.BlockingObjects blockingObjects )
        {
            _raycaster.blockingObjects = blockingObjects;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------
        //util
        //-----------------------------------------------------------------------------------------------------------
        public void Dimmed( float dimmedAmount )
        {
            if (dimmedAmount > 0)
            {
                _dimmedLayer.gameObject.SetActive( true );
                _dimmedLayer.color = new Color(0f, 0f, 0f, dimmedAmount);
            } else
            {
                _dimmedLayer.color = new Color(0f, 0f, 0f, dimmedAmount);
                _dimmedLayer.gameObject.SetActive( false );
            }
        }

        //-----------------------------------------------------------------------------------------------------------
        //basic
        //-----------------------------------------------------------------------------------------------------------
        public PopupBasic OpenNotification(string msg, float delay = 1.5f, UnityAction callback = null, float dimmed = 0.5f, bool warning = false )
        {
            _basic.state = PopupBasic.PopupState.Notification;
            _basic.SetDelay( delay );
            _basic.SetMessage(msg);

            AddPopup( _basic, callback, dimmed );

            return _basic;
        }

        public PopupBasic OpenAlert( string msg, UnityAction callback = null, float dimmed = 0.5f )
        {
            _basic.state = PopupBasic.PopupState.Alet;
            _basic.SetMessage(msg);
            _basic.OnSubmit = delegate()
            {
                if( callback != null )
                    callback();
            };

            AddPopup( _basic, null, dimmed );

            return _basic;
        }

        public PopupBasic OpenConfirm( string msg, UnityAction<bool> callback = null, float dimmed = 0.5f, bool warning = false )
        {
            _basic.state = PopupBasic.PopupState.Confirm;
            _basic.SetMessage(msg);
            _basic.OnCancel = delegate()
            {
                if( callback != null )
                    callback( false );
            };

            _basic.OnSubmit = delegate()
            {
                if( callback != null )
                    callback( true );
            };

            AddPopup( _basic, null, dimmed );

            return _basic;
        }

        public PopupBasic GetBasic()
        {
            return _basic;
        }

        //-----------------------------------------------------------------------------------------------------------
        //image
        //-----------------------------------------------------------------------------------------------------------
        public PopupImage OpenImage( Sprite img, UnityAction callback = null, float delay = 1.5f, float dimmed = 0.5f)
        {
            _img.SetDelay( delay );
            _img.sprite = img;

            AddPopup( _img, callback, dimmed );

            return _img;
        }

        public PopupImage GetImage()
        {
            return _img;
        }

        //-----------------------------------------------------------------------------------------------------------
        //console
        //-----------------------------------------------------------------------------------------------------------
        public void Log( string msg )
        {
            if( _console == null )
                return;

            _console.Log( msg );
        }
        
        public void LogClear()
        {
            if( _console == null )
                return;

            _console.Clear();
        }

        public int sortingOrder
        {
            get{ return _canvas.sortingOrder; }
            set
            {
                _canvas.sortingOrder = value;
            }
        }

        public IPopup top
        {
            get{ return _top; }
        }

    }
}

