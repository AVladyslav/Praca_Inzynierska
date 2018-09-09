using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectData : MonoBehaviour {

    public GameObject OriginalGameObject
    {
        get;
        private set;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetOriginalGameObject(GameObject originalGameObject)
    {
        OriginalGameObject = originalGameObject;
    }
}
