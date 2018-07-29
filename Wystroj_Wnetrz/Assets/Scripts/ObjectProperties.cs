using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectProperties : MonoBehaviour {

    public struct ObjProperties
    {
        Placement placement;
        ObjectType objectType;
        string objectName;
    }

    public bool IsPropertiesAreSet { get; private set; }
    public ObjProperties Properties { get; private set; }

	// Use this for initialization
	void Start () {
	    	
	}

    public bool SetProperties(ObjProperties objProperties)
    {
        if (IsPropertiesAreSet)
        {
            return false;
        }
        Properties = objProperties;
        IsPropertiesAreSet = true;
        return true;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
