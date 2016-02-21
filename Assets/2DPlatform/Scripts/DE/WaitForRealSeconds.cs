using UnityEngine;
using System.Collections;

public class WaitForRealSeconds : CustomYieldInstruction
{
	float waitTime;
	public override bool keepWaiting
	{
		get{ return Time.realtimeSinceStartup < waitTime; } 
	}

	public WaitForRealSeconds( float time )
	{
		waitTime = Time.realtimeSinceStartup + time;

		//use WaitWhile and WaitUntil
		//yield return new WaitWhile(()=> Time.realtimeSinceStartup < waitTime );
		//yield return new WaitUntil());
	}
}
