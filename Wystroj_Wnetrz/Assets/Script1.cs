using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script1 : MonoBehaviour {

    GameObject filing;

	// Use this for initialization
	void Start () {
        filing = GameObject.Find("filing-cabinet");
        filing.transform.parent = transform;
        filing.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
