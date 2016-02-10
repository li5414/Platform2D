using UnityEngine;
using System.Collections;
using druggedcode;

public class DruggedEngine : MonoBehaviour
{
    public const string SORTING_LAYER_SKYBOX = "SkyBox";
    public const string SORTING_LAYER_BACKGROUND = "Background";
    public const string SORTING_LAYER_BACK_FX = "BackFX";
    public const string SORTING_LAYER_ENVIRONMENT = "Environment";
    public const string SORTING_LAYER_DEFAULT = "Default";
    public const string SORTING_LAYER_CHARACTER = "Character";
    public const string SORTING_LAYER_FRONT_FX = "FrontFX";
    public const string SORTING_LAYER_FOREGROUND = "Foreground";
    public const string SORTING_LAYER_UI = "UI";

    public static LayerMask MASK_PLAYER;
    public static LayerMask MASK_ENEMY;
    public static LayerMask MASK_TRIGGER_AT_PLAYER;
	public static LayerMask MASK_ENVIRONMENT;
    public static LayerMask MASK_PLATFORM;
	public static LayerMask MASK_ALL_GROUND;

    public static float Gravity;
    
    public static UpdateType MOVE_PLATFORM;
    public static UpdateType MOVE_CHARACTER;
    
    //----------------------------------------------------------------------------------------------
    // instance
    //----------------------------------------------------------------------------------------------

    public LayerMask player;

    public LayerMask enemy;
    
    public LayerMask triggerAtPlayer;

	public LayerMask environment;
	public LayerMask platform;
    
    public UpdateType platformMove;
    public UpdateType characterMove;

    void Awake()
    {
        MASK_PLAYER = player;
        MASK_ENEMY = enemy;
        MASK_TRIGGER_AT_PLAYER = triggerAtPlayer;

        MASK_PLATFORM = platform;
		MASK_ENVIRONMENT = environment;
        
		MASK_ALL_GROUND = MASK_PLATFORM + MASK_ENVIRONMENT;

        //Ragdoll 은 안쓰는거 같은데
        MOVE_PLATFORM = platformMove;
        MOVE_CHARACTER = characterMove;

        Gravity = 0f;
    }
}