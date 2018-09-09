using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name == "Capsule")
        {
            Debug.Log("Collision started");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.name == "Capsule")
        {
            Debug.Log("Collision ended");
        }
    }
}
