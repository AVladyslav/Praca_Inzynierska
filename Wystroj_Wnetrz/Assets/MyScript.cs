using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyScript : MonoBehaviour {

    public GameObject capsule;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //capsule.transform.RotateAroundLocal(Vector3.back, 0.01f);
        //capsule.transform.RotateAround(transform.TransformVector(Vector3.up), 0.01f);
        capsule.transform.Rotate(transform.TransformVector(Vector3.up), 1f);
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
