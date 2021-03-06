﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity;

/// <summary>
/// Component that allows dragging an object with your hand on HoloLens.
/// Dragging is done by calculating the angular delta and z-delta between the current and previous hand positions,
/// and then repositioning the object based on that.
/// </summary>
public class HandDraggable : MonoBehaviour, IFocusable, IInputHandler, ISourceStateHandler
{
    /// <summary>
    /// Event triggered when dragging starts.
    /// </summary>
    public event Action StartedDragging;

    /// <summary>
    /// Event triggered when dragging stops.
    /// </summary>
    public event Action StoppedDragging;

    [Tooltip("Transform that will be dragged. Defaults to the object of the component.")]
    public Transform HostTransform;

    [Tooltip("Scale by which hand movement in z is multiplied to move the dragged object.")]
    public float DistanceScale = 2f;

    public enum MovingModeEnum
    {
        ForwardBackward,
        LeftRight,
        UpDown,
        Lock
    }
    public MovingModeEnum MovingMode = MovingModeEnum.Lock;

    public enum RotationModeEnum
    {
        LockObjectRotation,
        YAxisRotation
    }
    public RotationModeEnum RotationMode = RotationModeEnum.LockObjectRotation;

    public enum ScaleModeEnum
    {
        LockObjectScale,
        AllAxesScale
    }
    public ScaleModeEnum ScaleMode = ScaleModeEnum.LockObjectScale;


    [Tooltip("Controls the speed at which the object will interpolate toward the desired position")]
    [Range(0.01f, 1.0f)]
    public float PositionLerpSpeed = 0.2f;

    [Tooltip("Controls the speed at which the object will interpolate toward the desired rotation")]
    [Range(0.01f, 1.0f)]
    public float RotationLerpSpeed = 0.2f;

    public bool IsDraggingEnabled = true;

    private bool isDragging;
    private bool isGazed;
    private Vector3 objRefForward;
    private Vector3 objRefUp;
    private float objRefDistance;
    private Quaternion gazeAngularOffset;
    private float handRefDistance;
    private Vector3 objRefGrabPoint;

    private Vector3 oldDraggingPosition;
    private Vector3 draggingPosition;
    private Quaternion draggingRotation;

    private IInputSource currentInputSource;
    private uint currentInputSourceId;

    private Transform startDraggingCameraTransform;
    private Matrix4x4 localToWorld;
    private Matrix4x4 worldToLocal;

    private void Start()
    {
        if (HostTransform == null)
        {
            HostTransform = transform;
        }
    }

    private void OnDestroy()
    {
        if (isDragging)
        {
            StopDragging();
        }

        if (isGazed)
        {
            OnFocusExit();
        }
    }

    private void Update()
    {
        if (IsDraggingEnabled && isDragging)
        {
            UpdateDragging();
        }
    }

    /// <summary>
    /// Starts dragging the object.
    /// </summary>
    public void StartDragging()
    {
        if (!IsDraggingEnabled)
        {
            return;
        }

        if (isDragging)
        {
            return;
        }


        // Add self as a modal input handler, to get all inputs during the manipulation
        InputManager.Instance.PushModalInputHandler(gameObject);

        isDragging = true;

        Vector3 gazeHitPosition = GazeManager.Instance.HitInfo.point;
        Transform cameraTransform = CameraCache.Main.transform;
        Vector3 handPosition;
        currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);

        Vector3 pivotPosition = GetHandPivotPosition(cameraTransform);
        handRefDistance = Vector3.Magnitude(handPosition - pivotPosition);
        objRefDistance = Vector3.Magnitude(gazeHitPosition - pivotPosition);

        Vector3 objForward = HostTransform.forward;
        Vector3 objUp = HostTransform.up;
        // Store where the object was grabbed from
        objRefGrabPoint = cameraTransform.InverseTransformDirection(HostTransform.position - gazeHitPosition);

        Vector3 objDirection = Vector3.Normalize(gazeHitPosition - pivotPosition);
        Vector3 handDirection = Vector3.Normalize(handPosition - pivotPosition);

        objForward = cameraTransform.InverseTransformDirection(objForward);       // in camera space
        objUp = cameraTransform.InverseTransformDirection(objUp);                 // in camera space
        objDirection = cameraTransform.InverseTransformDirection(objDirection);   // in camera space
        handDirection = cameraTransform.InverseTransformDirection(handDirection); // in camera space

        startDraggingCameraTransform = CameraCache.Main.transform;
        localToWorld = CameraCache.Main.transform.localToWorldMatrix;
        worldToLocal = CameraCache.Main.transform.worldToLocalMatrix;

        objRefForward = objForward;
        objRefUp = objUp;

        // Store the initial offset between the hand and the object, so that we can consider it when dragging
        gazeAngularOffset = Quaternion.FromToRotation(handDirection, objDirection);
        draggingPosition = gazeHitPosition;

        StartedDragging.RaiseEvent();
    }

    /// <summary>
    /// Gets the pivot position for the hand, which is approximated to the base of the neck.
    /// </summary>
    /// <returns>Pivot position for the hand.</returns>
    private Vector3 GetHandPivotPosition(Transform cameraTransform)
    {
        Vector3 pivot = cameraTransform.position + new Vector3(0, -0.2f, 0) - cameraTransform.forward * 0.2f; // a bit lower and behind
        return pivot;
    }

    /// <summary>
    /// Enables or disables dragging.
    /// </summary>
    /// <param name="isEnabled">Indicates whether dragging should be enabled or disabled.</param>
    public void SetDragging(bool isEnabled)
    {
        if (IsDraggingEnabled == isEnabled)
        {
            return;
        }

        IsDraggingEnabled = isEnabled;

        if (isDragging)
        {
            StopDragging();
        }
    }

    /// <summary>
    /// Update the position of the object being dragged.
    /// </summary>
    private void UpdateDragging()
    {
        Vector3 newHandPosition;
        //Transform cameraTransform = CameraCache.Main.transform;
        currentInputSource.TryGetPosition(currentInputSourceId, out newHandPosition);

        Vector3 pivotPosition = GetHandPivotPosition(startDraggingCameraTransform);

        Vector3 newHandDirection = Vector3.Normalize(newHandPosition - pivotPosition);

        newHandDirection = startDraggingCameraTransform.InverseTransformDirection(newHandDirection); // in camera space
        Vector3 targetDirection = Vector3.Normalize(gazeAngularOffset * newHandDirection);
        targetDirection = startDraggingCameraTransform.TransformDirection(targetDirection); // back to world space

        float currentHandDistance = Vector3.Magnitude(newHandPosition - pivotPosition);

        float distanceRatio = currentHandDistance / handRefDistance;
        float distanceOffset = distanceRatio > 0 ? (distanceRatio - 1f) * DistanceScale : 0;
        float targetDistance = objRefDistance + distanceOffset;

        oldDraggingPosition = draggingPosition;
        draggingPosition = pivotPosition + (targetDirection * targetDistance);

        if (MovingMode != MovingModeEnum.Lock)
        {
            // Apply Final Position
            SetNewPositionForObject();
        }

        if (RotationMode != RotationModeEnum.LockObjectRotation)
        {
            // Apply Final Rotation
            SetNewRotationForObject();
        }

        if (ScaleMode != ScaleModeEnum.LockObjectScale)
        {
            //  Apply Final Scale
            SetNewScaleForObject();
        }
    }

    private void SetNewScaleForObject()
    {
        Vector3 oldDragingPositionCameraLocal = worldToLocal * oldDraggingPosition;
        Vector3 dragingPositionCameraLocal = worldToLocal * draggingPosition;

        switch (ScaleMode)
        {
            case ScaleModeEnum.AllAxesScale:
                //case MovingModeEnum.LeftRight:
                if (dragingPositionCameraLocal.y > oldDragingPositionCameraLocal.y)
                {
                    //  Scale up
                    ScaleByAllAxes(true);
                }
                else
                {
                    if (dragingPositionCameraLocal.y < oldDragingPositionCameraLocal.y)
                    {
                        //  Scale down
                        ScaleByAllAxes(false);
                    }
                }
                break;
            case ScaleModeEnum.LockObjectScale:
                return;
        }
    }

    private void ScaleByAllAxes(bool scaleUp)
    {
        float scaleFactor;
        if (scaleUp)
        {
            scaleFactor = 0.005f;
        }
        else
        {
            scaleFactor = -0.005f;
        }

        Transform objectMenuTransform = transform.Find("ObjectMenu");
        objectMenuTransform.parent = null;

        // Scaling GameObject
        HostTransform.localScale += HostTransform.localScale * scaleFactor;

        objectMenuTransform.parent = HostTransform;
    }

    private void SetNewPositionForObject()
    {
        Vector3 oldPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Vector3 newPosition = draggingPosition + startDraggingCameraTransform.TransformDirection(objRefGrabPoint);
        Vector3 newPositionCameraLocal = worldToLocal * newPosition;
        Vector3 hostTransformToCameraLocal = worldToLocal * HostTransform.position;

        switch (MovingMode)
        {
            case MovingModeEnum.ForwardBackward:
                newPositionCameraLocal.x = hostTransformToCameraLocal.x;
                newPositionCameraLocal.y = hostTransformToCameraLocal.y;
                break;
            case MovingModeEnum.LeftRight:
                newPositionCameraLocal.y = hostTransformToCameraLocal.y;
                newPositionCameraLocal.z = hostTransformToCameraLocal.z;
                break;
            case MovingModeEnum.UpDown:
                newPositionCameraLocal.x = hostTransformToCameraLocal.x;
                newPositionCameraLocal.z = hostTransformToCameraLocal.z;
                break;
            case MovingModeEnum.Lock:
                return;
        }

        newPosition = localToWorld * newPositionCameraLocal;

        switch (MovingMode)
        {
            case MovingModeEnum.ForwardBackward:
                newPosition.y = oldPosition.y;
                break;
            case MovingModeEnum.LeftRight:
                newPosition.y = oldPosition.y;
                break;
            case MovingModeEnum.UpDown:
                newPosition.x = oldPosition.x;
                newPosition.z = oldPosition.z;
                break;
            case MovingModeEnum.Lock:
                return;
        }
        HostTransform.position = Vector3.Lerp(HostTransform.position, newPosition, PositionLerpSpeed);
    }
    
    private void SetNewRotationForObject()
    {
        Vector3 oldDragingPositionCameraLocal = worldToLocal * oldDraggingPosition;
        Vector3 dragingPositionCameraLocal = worldToLocal * draggingPosition;

        switch (RotationMode)
        {
            case RotationModeEnum.YAxisRotation:
                //case MovingModeEnum.LeftRight:
                if (dragingPositionCameraLocal.x < oldDragingPositionCameraLocal.x)
                {
                    //  Rotate right
                    RotateAroundYAxis(true);
                }
                else
                {
                    if (dragingPositionCameraLocal.x > oldDragingPositionCameraLocal.x)
                    {
                        //  Rotate left
                        RotateAroundYAxis(false);
                    }
                }
                break;
            case RotationModeEnum.LockObjectRotation:
                return;
        }
    }

    private void RotateAroundYAxis(bool directionRight)
    {
        float angle;
        if (directionRight)
        {
            angle = 1f;
        }
        else
        {
            angle = -1f;
        }

        HostTransform.Rotate(Vector3.up, angle, Space.World);
    }

    /// <summary>
    /// Stops dragging the object.
    /// </summary>
    public void StopDragging()
    {
        if (!isDragging)
        {
            return;
        }

        // Remove self as a modal input handler
        InputManager.Instance.PopModalInputHandler();

        isDragging = false;
        currentInputSource = null;
        StoppedDragging.RaiseEvent();
    }

    public void OnFocusEnter()
    {
        if (!IsDraggingEnabled)
        {
            return;
        }

        if (isGazed)
        {
            return;
        }

        isGazed = true;
    }

    public void OnFocusExit()
    {
        if (!IsDraggingEnabled)
        {
            return;
        }

        if (!isGazed)
        {
            return;
        }

        isGazed = false;
    }

    public void OnInputUp(InputEventData eventData)
    {
        if (currentInputSource != null &&
            eventData.SourceId == currentInputSourceId)
        {
            StopDragging();
        }
    }

    public void OnInputDown(InputEventData eventData)
    {
        if (isDragging)
        {
            // We're already handling drag input, so we can't start a new drag operation.
            return;
        }

        if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
        {
            // The input source must provide positional data for this script to be usable
            return;
        }

        currentInputSource = eventData.InputSource;
        currentInputSourceId = eventData.SourceId;
        StartDragging();
    }

    public void OnSourceDetected(SourceStateEventData eventData)
    {
        // Nothing to do
    }

    public void OnSourceLost(SourceStateEventData eventData)
    {
        if (currentInputSource != null && eventData.SourceId == currentInputSourceId)
        {
            StopDragging();
        }
    }
}
