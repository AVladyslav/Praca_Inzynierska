using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMenuData : MonoBehaviour {

    public GameObject ActiveGameObject;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void EnableMoveForwardBackward()
    {
        ActiveGameObject.GetComponent<HandDraggable>().enabled = true;
        ActiveGameObject.GetComponent<HandDraggable>().MovingMode = HandDraggable.MovingModeEnum.ForwardBackward;
    }
    public void EnableMoveLeftRight()
    {
        ActiveGameObject.GetComponent<HandDraggable>().enabled = true;
        ActiveGameObject.GetComponent<HandDraggable>().MovingMode = HandDraggable.MovingModeEnum.LeftRight;
    }
    
    public void EnableMoveUpDown()
    {
        ActiveGameObject.GetComponent<HandDraggable>().enabled = true;
        ActiveGameObject.GetComponent<HandDraggable>().MovingMode = HandDraggable.MovingModeEnum.UpDown;
    }

    public void EnableRotationY()
    {
        ActiveGameObject.GetComponent<HandDraggable>().enabled = true;
        ActiveGameObject.GetComponent<HandDraggable>().RotationMode = HandDraggable.RotationModeEnum.YAxisRotation;
    }

    public void EnableAllAxesScale()
    {
        ActiveGameObject.GetComponent<HandDraggable>().enabled = true;
        ActiveGameObject.GetComponent<HandDraggable>().ScaleMode = HandDraggable.ScaleModeEnum.AllAxesScale;
    }

    public void DisableAll()
    {
        ActiveGameObject.GetComponent<HandDraggable>().MovingMode = HandDraggable.MovingModeEnum.Lock;
        ActiveGameObject.GetComponent<HandDraggable>().RotationMode = HandDraggable.RotationModeEnum.LockObjectRotation;
        ActiveGameObject.GetComponent<HandDraggable>().ScaleMode = HandDraggable.ScaleModeEnum.LockObjectScale;
        ActiveGameObject.GetComponent<HandDraggable>().StopDragging();
        ActiveGameObject.GetComponent<HandDraggable>().enabled = false;
    }

    public void RemoveGameObject()
    {
        transform.parent = null;
        Destroy(ActiveGameObject);
        gameObject.SetActive(false);
    }
}
