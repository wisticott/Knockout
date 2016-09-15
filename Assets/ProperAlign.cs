using UnityEngine;
using System.Collections;

public class ProperAlign : MonoBehaviour {

	// Use this for initialization
	void Start () {
		transform.parent = null;
		Vector3 up = transform.up;

		//transform.forward = -Vector3.up;
		//transform.up = up;
		transform.rotation = Quaternion.LookRotation(-Vector3.up, up);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
