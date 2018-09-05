using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnObjectTap : MonoBehaviour, IInputClickHandler
{
    public void OnInputClicked(InputClickedEventData eventData)
    {
        GameObject objectMenu = GameObject.Find("DataManager").GetComponent<PublicData>().ObjectMenu;
        if (objectMenu.activeInHierarchy)
        {
            objectMenu.GetComponent<ObjectMenuData>().DisableAll();
            objectMenu.GetComponent<ObjectMenuData>().ActiveGameObject = null;
            objectMenu.SetActive(false);
        }

        //hide menu with list if it is visible
        GameObject objectListMenu = GameObject.Find("ObjectListMenu");
        if (objectListMenu != null && objectListMenu.activeInHierarchy)
        {
            objectListMenu.SetActive(false);
        }

        objectMenu.transform.position = CalculatePosition();
        objectMenu.transform.rotation = CalculateRotation();
        objectMenu.SetActive(true);
        objectMenu.GetComponent<ObjectMenuData>().ActiveGameObject = gameObject;
        objectMenu.transform.SetParent(transform);
    }

    private Quaternion CalculateRotation()
    {
        Quaternion rotation;

        Transform CameraTransform = CameraCache.Main.transform;
        GameObject cursor = GameObject.Find("DefaultCursor") as GameObject;
        Vector3 directionToCamera = CameraTransform.position - cursor.transform.position;
        directionToCamera.y = 0f;
        rotation = Quaternion.LookRotation(-directionToCamera);

        return rotation;
    }

    private Vector3 CalculatePosition()
    {
        Vector3 position;

        GameObject cursor = GameObject.Find("DefaultCursor") as GameObject;
        Vector3 cursorPositionToCameraLocal = CameraCache.Main.transform.InverseTransformPoint(cursor.transform.position);
        position = cursorPositionToCameraLocal + new Vector3(0, 0, -0.2f);  //przed objektem
        position = CameraCache.Main.transform.TransformPoint(position);
        position.y = 0;

        return position;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
