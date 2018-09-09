using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPickerCanvasScript : MonoBehaviour {

    public GameObject ObjectMenu;
    public Shader shader;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetPositionAndRotation()
    {
        transform.position = ObjectMenu.transform.position;
        transform.rotation = ObjectMenu.transform.rotation;
    }

    public void SetColor(Color color)
    {
        GameObject go = ObjectMenu.GetComponent<ObjectMenuData>().ActiveGameObject;
        ObjectMenu.transform.parent = null;

        if (go.transform.childCount > 0)
        {
            foreach (Transform child in go.transform)
            {
                child.GetComponent<Renderer>().material.color = color;
            }
        }
        else
        {
            go.GetComponent<Renderer>().material.color = color;
        }

        ObjectMenu.transform.parent = go.transform;
    }

    public void ResetColor()
    {
        GameObject go = ObjectMenu.GetComponent<ObjectMenuData>().ActiveGameObject;
        GameObject original = go.GetComponent<ObjectData>().OriginalGameObject;

        ObjectMenu.transform.parent = null;

        if (go.transform.childCount > 0)
        {
            foreach (Transform child in go.transform)
            {
                //child.GetComponent<Renderer>().material.color
                child.GetComponent<Renderer>().material.color = original.transform.Find(child.name).GetComponent<Renderer>().sharedMaterial.color;
            }
        }
        else
        {
            go.GetComponent<Renderer>().material.color = original.GetComponent<Renderer>().sharedMaterial.color;
        }

        ObjectMenu.transform.parent = go.transform;
    }
}
