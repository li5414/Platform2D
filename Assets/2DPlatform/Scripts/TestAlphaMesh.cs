using UnityEngine;
using System.Collections;

public class TestAlphaMesh : MonoBehaviour
{
	Renderer ren;
	Material mt;
	void Awake()
	{
		ren = GetComponent<Renderer>();
		mt = ren.material;
	}

	void Update ()
	{
		if( Input.GetKeyDown( KeyCode.A) == false ) return;
//		mt.SetColor("Albedo", new Color(1f,1f,1f,0.5f));
		mt.SetColor ("_Color", Color.red );
		print("ok");
//		ren.materials[0].color = Color.red;
	}
}
