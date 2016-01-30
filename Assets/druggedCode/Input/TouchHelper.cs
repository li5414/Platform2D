using UnityEngine;

namespace druggedcode
{
	public class TouchHelper
    {
		static bool sIsMobile;

        static public void Init()
        {
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
                sIsMobile = false;
            else
                sIsMobile = true;
        }

        static public Vector3 TouchPosition
        {
            get
            {
                if( sIsMobile )
                {
                    return Input.GetTouch(0).position;
                }
                else
                {
                    return Input.mousePosition;
                }
            }
        }

        static public Vector2 TouchPosition2D
        {
            get
            {
                if( sIsMobile )
                {
                    return Input.GetTouch(0).position;
                }
                else
                {
                    return Input.mousePosition;
                }
            }
        }

        static public bool OnTouchBegan
        {
            get
            {
                if (sIsMobile)
                {
                    if (1 == Input.touchCount && TouchPhase.Began == Input.GetTouch(0).phase)
                        return true;
                    else
                        return false;
                } else
                    return Input.GetMouseButtonDown(0);
            }
        }
        
        static public bool OnTouchMove
        {
            get
            {
                if (sIsMobile)
                {
                    if (1 == Input.touchCount && TouchPhase.Moved == Input.GetTouch(0).phase)
                        return true;
                    else
                        return false;
                } else
                    return Input.GetMouseButton(0);
            }
        }
        
        static public bool OnTouchEnd
        {
            get
            {
                if (sIsMobile)
                {
                    if (1 == Input.touchCount && TouchPhase.Ended == Input.GetTouch(0).phase)
                        return true;
                    else
                        return false;
                } else
                    return Input.GetMouseButtonUp(0);
            }
            
        }
    }
}

