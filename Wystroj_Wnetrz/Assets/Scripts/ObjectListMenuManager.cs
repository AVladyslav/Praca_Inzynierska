using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectListMenuManager : MonoBehaviour {

    private GameObject cursor;
    private Vector3 lastCursorPosition;
    public GameObject ObjectListMenu;

	// Use this for initialization
	void Start ()
    {
        cursor = GameObject.Find("DefaultCursor") as GameObject;
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
}
