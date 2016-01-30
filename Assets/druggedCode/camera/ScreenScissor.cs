using UnityEngine;
using druggedcode;

namespace druggedcode
{
    public class ScreenScissor : Singleton<ScreenScissor>
    {

        public static ScreenScissor self = null;
        public Vector2 desiredSize;

        override protected void Awake()
        {
            base.Awake();
            UpdateResolution();
        }

        void OnLevelWasLoaded(int level)
        {
            if (self == this)
                UpdateResolution();
        }

        Camera GetScissorCamera()
        {
            GameObject go = new GameObject("ScissorCamera");
            Camera cam = go.AddComponent<Camera>();
            cam.backgroundColor = Color.black;
            cam.cullingMask = 0;
            return cam;
        }

        void UpdateResolution()
        {
            Camera[] objCameras = Camera.allCameras;

            float fResolutionX = Screen.width / desiredSize.x;
            float fResolutionY = Screen.height / desiredSize.y;

            if (fResolutionX > fResolutionY)
            {
                float fValue = (fResolutionX - fResolutionY) * 0.5f;
                fValue = fValue / fResolutionX;

                //fResolutionX fix, left & right Scissor (Viewport Re Setting)
                foreach (Camera obj in objCameras)
                {
                    obj.rect = new Rect(Screen.width * fValue / Screen.width + obj.rect.x * (1.0f - 2.0f * fValue), obj.rect.y
                                        , obj.rect.width * (1.0f - 2.0f * fValue), obj.rect.height);
                }

                Camera objLeftScissor = GetScissorCamera();
                objLeftScissor.rect = new Rect(0, 0, Screen.width * fValue / Screen.width, 1.0f);

                Camera objRightScissor = GetScissorCamera();
                objRightScissor.rect = new Rect((Screen.width - Screen.width * fValue) / Screen.width, 0
                                                , Screen.width * fValue / Screen.width, 1.0f);
            }
            else if (fResolutionX < fResolutionY)
            {

                float fValue = (fResolutionY - fResolutionX) * 0.5f;
                fValue = fValue / fResolutionY;

                //fResolutionY fix, Top & Bottom Scissor (Viewport Re Setting)
                foreach (Camera obj in objCameras)
                {
                    obj.rect = new Rect(obj.rect.x, Screen.height * fValue / Screen.height + obj.rect.y * (1.0f - 2.0f * fValue)
                                        , obj.rect.width, obj.rect.height * (1.0f - 2.0f * fValue));

                    obj.rect = new Rect(obj.rect.x, obj.rect.y + obj.rect.y * fValue, obj.rect.width, obj.rect.height - obj.rect.height * fValue);
                }

                Camera objTopScissor = GetScissorCamera();
                objTopScissor.rect = new Rect(0, 0, 1.0f, Screen.height * fValue / Screen.height);

                Camera objBottomScissor = GetScissorCamera();
                objBottomScissor.rect = new Rect(0, (Screen.height - Screen.height * fValue) / Screen.height
                                                 , 1.0f, Screen.height * fValue / Screen.height);
            }
            else
            {
                // Do Not Setting Camera
            }
        }
    }
}

