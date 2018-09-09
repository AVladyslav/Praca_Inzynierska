using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ObjectListMenuManager : MonoBehaviour {

    private GameObject cursor;
    private Vector3 lastCursorPosition;
    public GameObject ObjectListMenu;
    private List<GameObject> toPlace;
    private UnityEngine.Object[] prefabs;
    public GameObject ButtonPrefab;
    public GameObject ColliderBoxForButton;   // just box (parent) for button
    private List<GameObject> buttons;
    private List<GameObject> buttonsInBox;

    //  Buttons, first up and first down positions
    //private readonly Vector3 leftUpPos = new Vector3(-0.16f, 0.08f, -0.005f);
    //private readonly Vector3 leftDownPos = new Vector3(-0.16f, -0.08f, -0.005f);
    private bool nextUpPos = true;
    private Vector3 lastPos;
    private Vector3 nextPos = new Vector3(-0.16f, 0.08f, -0.005f);
    private float distanceBetweenButtons = 0.16f;
    private float maxXPositionValue = 0.17f;
    private float minXPositionValue = -0.17f;
    private float lerpSpeed = 0.16f;

    // Use this for initialization
    void Start()
    {
        cursor = GameObject.Find("DefaultCursor") as GameObject;
        buttons = new List<GameObject>();
        buttonsInBox = new List<GameObject>();
        toPlace = new List<GameObject>();
        prefabs = Resources.LoadAll("Prefabs", typeof(GameObject));

        foreach (GameObject go in prefabs)
        {

            toPlace.Add(go);
            GameObject button = InstantiateButton();
#if UNITY_EDITOR    //  creating and saving icons for objects
            Texture2D texture2D = AssetPreview.GetAssetPreview(go);
            if (go.transform.rotation.x > 0 )
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
            button.SetActive(false);
            buttons.Add(button);

        }

        // Load all icons from folder
        var icons = Resources.LoadAll("Icons", typeof(Texture2D));
        foreach (GameObject button in buttons)
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

        ObjectListMenu.gameObject.SetActive(false);
    }

    private GameObject InstantiateButton()
    {
        GameObject colliderBoxForButton = GameObject.Instantiate(ColliderBoxForButton);
        colliderBoxForButton.transform.position = ObjectListMenu.transform.TransformPoint(nextPos);  //local position of button in menu
        colliderBoxForButton.transform.rotation = ObjectListMenu.transform.rotation; //same rotation as menu rotation
        colliderBoxForButton.transform.SetParent(ObjectListMenu.transform);  //colliderBoxForButton now is a child of menu

        GameObject button = Instantiate(ButtonPrefab);
        button.transform.position = ObjectListMenu.transform.TransformPoint(nextPos);  //local position of button in menu
        button.transform.rotation = ObjectListMenu.transform.rotation; //same rotation as menu rotation

        nextUpPos = !nextUpPos;
        lastPos = nextPos;
        if (nextUpPos)
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


        colliderBoxForButton.GetComponent<ColliderBoxForButtonScript>().SetButton(button);
        buttonsInBox.Add(colliderBoxForButton);

        return button;
    }

    private void WriteToFile(string name, Texture2D texture2D)
    {
        File.WriteAllBytes(Application.dataPath + "/Resources/Icons/" + name + ".png", texture2D.EncodeToPNG());
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ShowMenu()
    {
        ObjectListMenu.SetActive(true);
        ObjectListMenu.transform.position = CameraCache.Main.transform.TransformPoint(Vector3.forward * 2);
        ObjectListMenu.transform.rotation = CameraCache.Main.transform.rotation;
        lastCursorPosition = cursor.transform.position;
    }

    public void PlaceGameObject(GameObject toPlace)
    {
        LevelSolver.Instance.PlaceObject_OnFloor_NearPoint(toPlace, cursor.transform.position);
        ObjectListMenu.SetActive(false);
    }
    public void HideMenu()
    {
        ObjectListMenu.SetActive(false);
    }

    public void OnLeftButtonClick()
    {
        if (buttonsInBox.Count > 6)
        {
            if (buttonsInBox[0].transform.localPosition.x < minXPositionValue)
            {
                ChangePositionForButtons(distanceBetweenButtons*3);
            }
        }
    }

    public void OnRightButtonClick()
    {
        if (buttonsInBox.Count > 6)
        {
            if (buttonsInBox[buttonsInBox.Count - 1].transform.localPosition.x > maxXPositionValue)
            {
                ChangePositionForButtons(-distanceBetweenButtons*3);
            }
        }
    }

    private void ChangePositionForButtons(float xValue)
    {
        foreach (GameObject buttonBox in buttonsInBox)
        {
            //  Disable active buttons, because button visible out of visible area for a moment
            if (buttonBox.GetComponent<ColliderBoxForButtonScript>().Button.activeInHierarchy)
            {
                buttonBox.GetComponent<ColliderBoxForButtonScript>().DisableButton();
            }
            buttonBox.transform.localPosition = buttonBox.transform.localPosition + new Vector3(xValue, 0, 0);
        }
    }
}
