using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderBoxForButtonScript : MonoBehaviour {

    public GameObject Button
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

    public void SetButton(GameObject button)
    {
        Button = button;
        Button.transform.SetParent(transform);
        Button.transform.position = transform.TransformPoint(0, 0, -0.5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name == "VisibleArea")
        {
            Button.SetActive(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.name == "VisibleArea")
        {
            Button.SetActive(false);
        }
    }

    public void DisableButton()
    {
        Button.SetActive(false);
    }
}
