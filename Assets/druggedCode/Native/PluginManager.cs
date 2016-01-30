
namespace druggedcode
{
    public class PluginManager : Singleton<PluginManager>
    {   
        ANativePlugin _native;
        
        override protected void Awake()
        {
            base.Awake();

            #if UNITY_EDITOR
            _native = gameObject.AddComponent<ANativePlugin>();
            #elif UNITY_IPHONE
            _native = gameObject.AddComponent<iOSManager>();
            #elif UNITY_ANDROID
            _native = gameObject.AddComponent<AndroidManager>();
            #endif
        }
        
        public void Log(string strMsg)
        {
            _native.Log( strMsg );
        }
        
        public void Alert(string title, string message, string submit = "OK good")
        {
            _native.Alert( title, message, submit );
        }
        
        public void AlertOK()
        {
            PopupManager.Instance.Log("alert complete");
        }
        
        public void Confirm(string title, string message, string positive = "OK", string negative = "CANCEL")
        {
            _native.Confirm( title, message, positive, negative );
        }
        
        public void ConfirmOK()
        {
            PopupManager.Instance.Log("ConfirmOK complete");
        }
        
        public void ConfirmCancel()
        {
            PopupManager.Instance.Log("ConfirmCancel complete");
        }
    }
}

