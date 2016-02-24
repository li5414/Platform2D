using UnityEngine;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;

public class TestPro : MonoBehaviour {

	public ProCamera2D cam;

	void Start () {
		StartCoroutine( Move());
	}

	IEnumerator Move()
	{
		float t = 0f;
		int direct = 1;
		while( true )
		{
			transform.Translate( direct * new Vector3(10f,0f,0f) * Time.deltaTime );
			t += Time.deltaTime;
			if( t > 5f )
			{
				t = 0f;
				direct *= -1;
			}
		}
	}


	void Update () {
	}
}
