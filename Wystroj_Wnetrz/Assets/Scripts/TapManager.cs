using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapManager : MonoBehaviour, IInputClickHandler
{
    //public GameObject desk;
    //private GameObject cursor;
    public GameObject ObjectListMenuManager;

    public void OnInputClicked(InputClickedEventData eventData)
    {
        switch (SpatialUnderstanding.Instance.ScanState)
        {
            case SpatialUnderstanding.ScanStates.None:
                break;
            case SpatialUnderstanding.ScanStates.ReadyToScan:
                break;
            case SpatialUnderstanding.ScanStates.Scanning:
                SpatialUnderstanding.Instance.RequestFinishScan();
                Debug.Log("Finishing Scan");
                break;
            case SpatialUnderstanding.ScanStates.Finishing:
                break;
            case SpatialUnderstanding.ScanStates.Done:
                HandleTap();
                break;
            default:
                break;
        }
    }

    private void HandleTap()
    {
        //LevelSolver.Instance.PlaceObject_OnFloor_NearPoint(desk, cursor.transform.position);
        //  if objectmenu active, disable and reset
        if (GameObject.Find("DataManager").GetComponent<PublicData>().ObjectMenu.activeInHierarchy)
        {
            GameObject.Find("DataManager").GetComponent<PublicData>().ObjectMenu.GetComponent<ObjectMenuData>().DisableAll();
            GameObject.Find("DataManager").GetComponent<PublicData>().ObjectMenu.GetComponent<ObjectMenuData>().ActiveGameObject = null;
            GameObject.Find("DataManager").GetComponent<PublicData>().ObjectMenu.SetActive(false);
        }
        
        ObjectListMenuManager.GetComponent<ObjectListMenuManager>().ShowMenu();
    }

    // Use this for initialization
    void Start () {
        InputManager.Instance.PushFallbackInputHandler(gameObject); //handle Tap gesture anywhere, excluding gameObjects
        //cursor = GameObject.Find("DefaultCursor") as GameObject;
        if (!LevelSolver.Instance.IsSolverInitialized)
            LevelSolver.Instance.InitializeSolver();

    }

    // Update is called once per frame
    void Update () {
		
	}
}
