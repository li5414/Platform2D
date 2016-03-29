using UnityEngine;
using System.Collections;

public class TriggerTester : MonoBehaviour
{
	void OnTriggerEnter2D( Collider2D other )
	{
		print("> Trigger Enter: " + name + " in " + other.name );
	}

	void OnTriggerStay2D( Collider2D other )
	{
		//print("    --- Trigger Stay: " + name + " in " + other.name );
	}

	void OnTriggerExit2D( Collider2D other )
	{
		print("\t< Trigger Exit: " + name + " in " + other.name );
	}
}
