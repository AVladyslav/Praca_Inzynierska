using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnChildTap : MonoBehaviour {
    public void OnInputClicked(InputClickedEventData eventData)
    {
        if (transform.parent != null)
        {
            transform.parent.gameObject.GetComponent<OnObjectTap>().OnInputClicked(eventData);
        }
    }
    
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
