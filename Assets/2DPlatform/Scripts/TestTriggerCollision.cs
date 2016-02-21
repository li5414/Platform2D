using UnityEngine;
using System.Collections;

public class TestTriggerCollision : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    
    void OnCollisionEnter2D( Collision2D collision )
    {
        print("collision : " + name + " + " + collision.gameObject.name );
    }
    
    void OnTriggerEnter2D( Collider2D other )
    {
        print("tirgger: " + name + " + " + other.name);
    }
}
