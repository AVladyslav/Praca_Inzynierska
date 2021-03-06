﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Examples.InteractiveElements;

/// <summary>
/// InteractiveButton exposes extra unity events for gaze, down and hold in the inspector.
/// 
/// Beyond the basic button functionality, Interactive also maintains the notion of selection and enabled, which allow for more robust UI features.
/// InteractiveEffects are behaviors that listen for updates from Interactive, which allows for visual feedback to be customized and placed on
/// individual elements of the Interactive GameObject
/// </summary>
public class InteractiveButton : Interactive
{

    public UnityEvent OnGazeEnterEvents;
    public UnityEvent OnGazeLeaveEvents;
    public UnityEvent OnDownEvents;
    public UnityEvent OnUpEvents;

    /// <summary>
    /// The gameObject received gaze
    /// </summary>
    public override void OnFocusEnter()
    {

    }

    /// <summary>
    /// The gameObject no longer has gaze
    /// </summary>
    public override void OnFocusExit()
    {

    }

    /// <summary>
    /// The user is initiating a tap or hold
    /// </summary>
    public override void OnInputDown(InputEventData eventData)
    {
    }

    /// <summary>
    /// All tab, hold, and gesture events are completed
    /// </summary>
    public override void OnInputUp(InputEventData eventData)
    {
        GameObject objectListMenuManager = GameObject.Find("ObjectListMenuManager");
        //objectListMenuManager.GetComponent<ObjectListMenuManager>().PlaceGameObject(gameObject.GetComponent<ButtonData>().ToPlace);
        var a = objectListMenuManager.GetComponent<ObjectListMenuManager>();
        var b = gameObject.GetComponent<ButtonData>().ToPlace;
        a.PlaceGameObject(b);
    }
}
