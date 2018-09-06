using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonData : MonoBehaviour {

    public GameObject ToPlace
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

    public bool SetToPlaceGameObject(GameObject toPlace)
    {
        bool value = false;
        if (ToPlace == null)
        {
            ToPlace = toPlace;
        }
        return value;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name == "VisibleArea")
        {
            //TODO dokończyć kolizje
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.name == "VisibleArea")
        {
            //TODO dokończyć kolizje
        }
    }
}
