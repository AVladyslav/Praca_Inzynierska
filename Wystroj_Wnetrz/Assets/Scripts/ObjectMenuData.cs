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
    public void DisableAll()
    {
        ActiveGameObject.GetComponent<HandDraggable>().MovingMode = HandDraggable.MovingModeEnum.Lock;
        ActiveGameObject.GetComponent<HandDraggable>().StopDragging();
        ActiveGameObject.GetComponent<HandDraggable>().enabled = false;
    }
}
