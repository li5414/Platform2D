using UnityEngine;
using System.Collections;
using druggedcode;

[RequireComponent(typeof(Collider2D))]
[ExecuteInEditMode]
public class TestLayerChange : MonoBehaviour {

	// Use this for initialization
	void Start () {
		LayerUtil.ChangeLayer( gameObject,DruggedEngine.MASK_ENVIRONMENT);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
