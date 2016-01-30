using System.Runtime.InteropServices;

namespace druggedcode
{
    public class iOSManager : ANativePlugin
    {

        [DllImport("__Internal")]
        private extern static void LogNative(string strMsg);

        [DllImport("__Internal")]
        private extern static void AlertNative(string title, string message, string submit);

        [DllImport("__Internal")]
        private extern static void ConfirmNative(string title, string message, string positive, string negative);

        override public void Log(string strMsg)
        {
            strMsg += " to ios";
            PopupManager.Instance.Log(strMsg);
            LogNative(strMsg + " pass");

        }

        override public void Alert(string title, string message, string submit = "OK good")
        {
            AlertNative(title, message, submit);
        }

        override public void Confirm(string title, string message, string positive = "OK", string negative = "CANCEL")
        {
            ConfirmNative(title, message, positive, negative);
        }
    }
}
