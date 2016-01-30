using UnityEngine;
using System.Collections;

public class ButtonLink : MonoBehaviour {

	public string url;

	public void Go () {
		Application.OpenURL(url);
	}
}
