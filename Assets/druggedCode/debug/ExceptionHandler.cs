using UnityEngine;
using System.Collections;

namespace druggedcode
{
	public class ExceptionHandler : MonoBehaviour
	{
		public void Enable ()
		{
			//4.x
			//Application.RegisterLogCallback( HandlelogMessageReceived );
			//5.x
			Application.logMessageReceived += HandlelogMessageReceived;
		}
		
		public void Disable ()
		{
			//4.x
			//Application.RegisterLogCallback( null );
			//5.x
			Application.logMessageReceived -= HandlelogMessageReceived;
		}
		
		void HandlelogMessageReceived (string condition, string stackTrace, LogType type)
		{
			switch (type) {
			case LogType.Exception:
				// native call.( send error log, exception popup );
				//main thread close
				Application.Quit ();
				break;
			case LogType.Error:
				//send error log
				break;
			}
		}
	}
}

