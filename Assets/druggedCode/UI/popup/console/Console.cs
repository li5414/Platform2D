using UnityEngine;
using UnityEngine.UI;

namespace druggedcode
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Console : MonoBehaviour
    {   
        Text _txt;
        Image _bg;

        void Awake()
        {
            _txt = GetComponentInChildren<Text>();
            _bg = GetComponentInChildren<Image>();
        }

        public void Init( int fontSize, float bgAlpha )
        {
            gameObject.SetActive( true );

            MotionUI.SetAlphaInChildren( _bg.gameObject,bgAlpha );

            Clear();
            
            _txt.fontSize = fontSize;
        }
        
        public void Log( string msg )
        {
            gameObject.SetActive( true );
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            
            _txt.text += msg + "\n";

//            GraphicUtil.SetAlpha( _bg.gameObject, 0f );
        }
        
        public void Clear()
        {
            gameObject.SetActive( false );

            _txt.text = "";
            
        }
    }
}
