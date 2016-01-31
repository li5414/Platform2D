using UnityEngine;
using System.Collections;

public class DruggedEngine
{
    //-- SortingLayer
    
    public const string SORTING_LAYER_SKYBOX = "SkyBox";
    public const string SORTING_LAYER_BACKGROUND = "Background";
    public const string SORTING_LAYER_BACKPARTICLE = "BackPartile";
    public const string SORTING_LAYER_PLATFORM = "Platform";
    public const string SORTING_LAYER_DEFAULT = "Default";
    public const string SORTING_LAYER_ENEMY = "Enemy";
    public const string SORTING_LAYER_PLAYER = "Player";
    public const string SORTING_LAYER_FRONTPARTICLE = "FrontParticle";
    public const string SORTING_LAYER_FOREGROUND = "Foreground";
    public const string SORTING_LAYER_UI = "UI";
    
	public static float Gravity;

    // public static LayerMask NormalPlatform;
    // public static LayerMask OneWayPlatform;
    
    public static LayerMask NormalPlatform = 1 << LayerMask.NameToLayer("Platform");
    public static LayerMask OneWayPlatform = 1 << LayerMask.NameToLayer("OnewayPlatform");
    
    private DruggedEngine()
    {
        //인스턴스를 생성할 수 없다.
    }
    
    static public void Init()
    {
        Gravity = -70f;
		//Gravity = -( 9.81f * 9.81f );
        
        //init layer
        // NormalPlatform = 1 << LayerMask.NameToLayer("Platform");
        // OneWayPlatform = 1 << LayerMask.NameToLayer("OnewayPlatform");
    }
    
    static public LayerMask AllPlatform
    {
        get
        {
            return NormalPlatform + OneWayPlatform;
        }
    }

    static public LayerMask ExceptOnewayPlatform
    {
        get
        {
            return AllPlatform - OneWayPlatform;
        }
    }
}