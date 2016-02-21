using UnityEngine;
using System.Collections;
using druggedcode;

public class DruggedEngine : MonoBehaviour
{
	public const string SORTING_LAYER_SKYBOX = "SkyBox";
	public const string SORTING_LAYER_BACKGROUND = "Background";
	public const string SORTING_LAYER_BACK_FX = "BackFX";
	public const string SORTING_LAYER_DEFAULT_BACK = "DefaultBack";
	public const string SORTING_LAYER_DEFAULT = "Default";
	public const string SORTING_LAYER_CHARACTER = "Character";
	public const string SORTING_LAYER_FRONT_FX = "FrontFX";
	public const string SORTING_LAYER_FOREGROUND = "Foreground";
	public const string SORTING_LAYER_UI = "UI";

	public static LayerMask MASK_PLAYER;
	public static LayerMask MASK_ENEMY;
	public static LayerMask MASK_TRIGGER_AT_PLAYER;
    public static LayerMask MASK_LADDER;
	public static LayerMask MASK_ENVIRONMENT;
	public static LayerMask MASK_PLATFORM;
	public static LayerMask MASK_ONEWAY;
	public static LayerMask MASK_ALL_GROUND;
	public static LayerMask MASK_EXCEPT_ONEWAY_GROUND;

	public static int LAYER_ONEWAY;

	public static float Gravity;
    
	//----------------------------------------------------------------------------------------------
	// instance
	//----------------------------------------------------------------------------------------------

	public LayerMask player;
	public LayerMask enemy;
	public LayerMask triggerAtPlayer;
    public LayerMask ladder;

	public LayerMask environment;
	public LayerMask platform;
	public LayerMask oneway;

	void Awake ()
	{
		MASK_PLAYER = player;
		MASK_ENEMY = enemy;
		MASK_TRIGGER_AT_PLAYER = triggerAtPlayer;
        MASK_LADDER = ladder;

		MASK_ENVIRONMENT = environment;
		MASK_PLATFORM = platform;
		MASK_ONEWAY = oneway;
        
		MASK_ALL_GROUND = MASK_ENVIRONMENT + MASK_PLATFORM + MASK_ONEWAY;
		MASK_EXCEPT_ONEWAY_GROUND = MASK_ENVIRONMENT + MASK_PLATFORM;

		LAYER_ONEWAY = LayerUtil.GetLayerIdFromLayerMask( MASK_ONEWAY );

		Gravity = -30f;///70
	}
}