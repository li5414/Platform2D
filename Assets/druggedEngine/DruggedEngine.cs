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
	public static LayerMask MASK_PLAYER_TRIGGER;
	public static LayerMask MASK_ENVIRONMENT;
	public static LayerMask MASK_PLATFORM;
	public static LayerMask MASK_ONEWAY;
	public static LayerMask MASK_RAGDOLL;
	public static LayerMask MASK_LADDER;
	public static LayerMask MASK_DAMAGER;

	public static LayerMask MASK_MIXED_ALLGROUND;
	public static LayerMask MASK_MIXED_EXCEPT_ONEWAY_GROUND;

	public static int LAYER_ONEWAY;

	public static float Gravity = -9.81f * 2f;
//	public static float Gravity = 0f;
    
	//----------------------------------------------------------------------------------------------
	// instance
	//----------------------------------------------------------------------------------------------

	public LayerMask player;
	public LayerMask enemy;
	public LayerMask playerTrigger;
	public LayerMask environment;
	public LayerMask platform;
	public LayerMask oneway;
	public LayerMask ragdoll;
	public LayerMask ladder;
	public LayerMask damager;

	void Awake ()
	{
		Physics2D.gravity = new Vector2(0f, Gravity );

		MASK_PLAYER = player;
		MASK_ENEMY = enemy;
		MASK_PLAYER_TRIGGER = playerTrigger;
		MASK_ENVIRONMENT = environment;
		MASK_PLATFORM = platform;
		MASK_ONEWAY = oneway;
		MASK_RAGDOLL = ragdoll;
        MASK_LADDER = ladder;
		MASK_DAMAGER = damager;
        
		MASK_MIXED_ALLGROUND = MASK_ENVIRONMENT + MASK_PLATFORM + MASK_ONEWAY;
		MASK_MIXED_EXCEPT_ONEWAY_GROUND = MASK_ENVIRONMENT + MASK_PLATFORM;

		LAYER_ONEWAY = LayerUtil.GetLayerIdFromLayerMask( MASK_ONEWAY );
	}
}