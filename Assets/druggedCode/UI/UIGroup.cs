using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;


namespace druggedcode
{
    [RequireComponent( typeof(CanvasGroup))]
    public class UIGroup : MonoBehaviour
    {   
        [HideInInspector]
        public CanvasGroup group;
        
        UnityAction _onCloseComplete;
        
        protected virtual void Awake()
        {
            group = GetComponent<CanvasGroup>();
            
            if( group == null )
                group = gameObject.AddComponent<CanvasGroup>();
        }
        
        public void Fade( float to, float duration = 0.3f, UnityAction callback = null, iTween.EaseType easeType = iTween.EaseType.easeOutQuint )
        {
            _onCloseComplete = callback;
            
            iTween.Stop( gameObject );
            iTween.ValueTo( gameObject,iTween.Hash("from",group.alpha ,
                                                   "to",to,
                                                   "time",duration,
                                                   "onupdate","SetAlpha",
                                                   "oncomplete","OnCloseComplete",
                                                   "easeType",easeType ));
        }
        
        public void SetAlpha( float a )
        {
            group.alpha = a ;
        }
        
        void  OnCloseComplete()
        {
            if( _onCloseComplete != null )
            {
                _onCloseComplete();
                _onCloseComplete = null;
            }
        }
    }
}

