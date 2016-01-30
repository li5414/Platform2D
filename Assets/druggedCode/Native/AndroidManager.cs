namespace druggedcode
{
    public class AndroidManager: ANativePlugin
    {

        #if UNITY_ANDROID

        AndroidJavaObject _activity;
        void Awake()
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            _activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
        }

        override public void Log(string strMsg)
        {
            _activity.Call("log",strMsg );
        }
        
        override public void Alert(string title, string message, string submit = "OK good")
        {
            _activity.Call("alert",title,message,submit);
        }
        
        override public void Confirm(string title, string message, string positive = "OK", string negative = "CANCEL")
        {
            _activity.Call("confirm",title,message,positive,negative);
        }

        #endif
    }
}

