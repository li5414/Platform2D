using UnityEngine;
using System.Collections;

public class Config
{
	//-- Scene
    public const string SC_MAIN = "Title";
    public const string SC_WORLD = "World";
    public const string SC_WORLDMAP = "WorldMap";

	//-- Tag
	public const string TAG_BATTLE_WORLD = "BattleWorld";
	public const string TAG_CHECKPOINT= "CheckPoint";
	public const string TAG_UICAMERA= "UICamera";
	public const string TAG_PLAYER= "Player";

	static public bool IsMobile = false;
	
    static public void Init()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = 60;
		
		if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
            IsMobile = false;
        else
            IsMobile = true;
    }

}
