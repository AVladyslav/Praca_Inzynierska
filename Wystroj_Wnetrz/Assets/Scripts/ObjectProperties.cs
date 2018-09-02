using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectProperties : MonoBehaviour {

    public struct ObjProperties
    {
        public Placement PlacementType;
        public ObjectType ObjectType;
        public string ObjectName;

        public ObjProperties(Placement placementType, ObjectType pObjectType, string objectName)
        {
            this.PlacementType = placementType;
            this.ObjectType = pObjectType;
            this.ObjectName = objectName;
        }
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
