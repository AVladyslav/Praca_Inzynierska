using HoloToolkit.Examples.InteractiveElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class CreateAllButtons : MonoBehaviour {

    private List<GameObject> toPlace;
    private UnityEngine.Object[] prefabs;
    public GameObject ButtonPrefab;
    private List<GameObject> Buttons;
    private Quaternion rotation;

    //  Buttons, first up and first down positions
    //private readonly Vector3 leftUpPos = new Vector3(-0.16f, 0.08f, -0.005f);
    //private readonly Vector3 leftDownPos = new Vector3(-0.16f, -0.08f, -0.005f);
    private bool nexUpPos = true;
    private Vector3 lastPos;
    private Vector3 nextPos = new Vector3(-0.16f, 0.08f, -0.005f);
    private float distanceBetweenButtons = 0.16f;

	// Use this for initialization
	void Start () {
        Buttons = new List<GameObject>();
        rotation = this.transform.rotation;
        toPlace = new List<GameObject>();
        prefabs = Resources.LoadAll("Prefabs", typeof(GameObject));

        foreach (GameObject go in prefabs)
        {
            toPlace.Add(go);
            GameObject button = InstantiateButton();
#if UNITY_EDITOR
            Texture2D texture2D = AssetPreview.GetAssetPreview(go);
            if (go.transform.rotation.x > 0)
            {
                texture2D = Helper.RotatePreviewTexture(texture2D, true);
            }
            if (go.transform.rotation.x < 0)
            {
                texture2D = Helper.RotatePreviewTexture(texture2D, false);
            }
            //button.GetComponentInChildren<Renderer>().material.mainTexture = texture2D;
            WriteToFile(go.name, texture2D);
#endif
            button.GetComponent<ButtonData>().SetToPlaceGameObject(go);
            button.transform.SetParent(transform);  //button now is a child of menu gameobject
            Buttons.Add(button);
        }

        // Load all icons from folder
        var icons = Resources.LoadAll("Icons", typeof(Texture2D));
        foreach (GameObject button in Buttons)
        {
            //  For everyone icon check if this is have a same name as gameobject to place
            foreach (Texture2D icon in icons)
            {
                if (button.GetComponent<ButtonData>().ToPlace.name == icon.name)
                {
                    button.GetComponentInChildren<Renderer>().material.mainTexture = icon;
                    break;
                }
            }
        }

        this.gameObject.SetActive(false);
	}

    private GameObject InstantiateButton()
    {
        GameObject button = Instantiate(ButtonPrefab);
        button.transform.position = transform.TransformPoint(nextPos);  //local position of button in menu
        button.transform.rotation = transform.rotation; //same rotation as menu rotation

        nexUpPos = !nexUpPos;
        lastPos = nextPos;
        if (nexUpPos)
        {
            nextPos = lastPos;
            nextPos.y += distanceBetweenButtons;    //move up
            nextPos.x += distanceBetweenButtons;    //move right
        }
        else
        {
            nextPos = lastPos;
            nextPos.y -= distanceBetweenButtons;    //move down
        }

        return button;
    }

    private void WriteToFile(string name, Texture2D texture2D)
    {
        File.WriteAllBytes(Application.dataPath + "/Resources/Icons/" + name + ".png", texture2D.EncodeToPNG());
    }

    // Update is called once per frame
    void Update () {
		
	}
}
