// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections;
using HoloToolkit.Unity;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

public class LevelSolver : MonoBehaviour
{
    // Singleton
    public static LevelSolver Instance;

    // Enums
    public enum QueryStates
    {
        None,
        Processing,
        Finished
    }

    // Structs
    private struct QueryStatus
    {
        public void Reset()
        {
            State = QueryStates.None;
            Name = "";
            CountFail = 0;
            CountSuccess = 0;
            QueryResult = new Queue<PlacementResult>();
        }

        public QueryStates State;
        public string Name;
        public int CountFail;
        public int CountSuccess;
        public Queue<PlacementResult> QueryResult;
    }

    private struct PlacementQuery
    {
        public PlacementQuery(
            GameObject gameObject,
            SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition placementDefinition,
            List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> placementRules = null,
            List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint> placementConstraints = null)
        {
            MyGameObject = gameObject;
            PlacementDefinition = placementDefinition;
            PlacementRules = placementRules;
            PlacementConstraints = placementConstraints;
        }

        public GameObject MyGameObject;
        public SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition PlacementDefinition;
        public List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> PlacementRules;
        public List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint> PlacementConstraints;
    }

    private class PlacementResult
    {
        public PlacementResult(SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult result, GameObject gameObject)
        {
            //Box = new AnimatedBox(timeDelay, result.Position, Quaternion.LookRotation(result.Forward, result.Up), Color.blue, result.HalfDims);
            Result = result;
            MyGameObject = gameObject;
        }

        public SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult Result;
        public GameObject MyGameObject;
    }

    // Properties
    public bool IsSolverInitialized { get; private set; }

    // Privates
    private List<PlacementResult> placementResults = new List<PlacementResult>();
    private QueryStatus queryStatus = new QueryStatus();

    // Functions
    private void Awake()
    {
        Instance = this;
    }


    private bool PlaceObjectAsync(
        string placementName,
        GameObject gameObject,
        SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition placementDefinition,
        List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> placementRules = null,
        List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint> placementConstraints = null)
    {
        return PlaceObjectAsync(
            placementName,
            new List<PlacementQuery>() { new PlacementQuery(gameObject, placementDefinition, placementRules, placementConstraints) });
    }

    private bool PlaceObjectAsync(
        string placementName,
        List<PlacementQuery> placementList)
    {
        // If we already mid-query, reject the request
        if (queryStatus.State != QueryStates.None)
        {
            return false;
        }

        // Mark it
        queryStatus.Reset();
        queryStatus.State = QueryStates.Processing;
        queryStatus.Name = placementName;

        // Kick off a thread to do process the queries
#if UNITY_EDITOR || !UNITY_WSA
        new System.Threading.Thread
#else
        System.Threading.Tasks.Task.Run
#endif
        (() =>
            {
                // Go through the queries in the list
                for (int i = 0; i < placementList.Count; ++i)
                {
                    // Do the query
                    bool success = PlaceObject(
                        placementName,
                        placementList[i].MyGameObject,
                        placementList[i].PlacementDefinition,
                        placementList[i].PlacementRules,
                        placementList[i].PlacementConstraints, 
                        true);

                    // Mark the result
                    queryStatus.CountSuccess = success ? (queryStatus.CountSuccess + 1) : queryStatus.CountSuccess;
                    queryStatus.CountFail = !success ? (queryStatus.CountFail + 1) : queryStatus.CountFail;
                }

                // Done
                queryStatus.State = QueryStates.Finished;
            }
        )
#if UNITY_EDITOR || !UNITY_WSA
        .Start()
#endif
        ;

        return true;
    }

    private bool PlaceObject(
        string placementName,
        GameObject gameObject,
        SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition placementDefinition,
        List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule> placementRules = null,
        List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint> placementConstraints = null,
        bool isASync = false)
    {
        if (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
        {
            return false;
        }

        // New query
        if (SpatialUnderstandingDllObjectPlacement.Solver_PlaceObject(
                placementName,
                SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(placementDefinition),
                (placementRules != null) ? placementRules.Count : 0,
                ((placementRules != null) && (placementRules.Count > 0)) ? SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(placementRules.ToArray()) : IntPtr.Zero,
                (placementConstraints != null) ? placementConstraints.Count : 0,
                ((placementConstraints != null) && (placementConstraints.Count > 0)) ? SpatialUnderstanding.Instance.UnderstandingDLL.PinObject(placementConstraints.ToArray()) : IntPtr.Zero,
                SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticObjectPlacementResultPtr()) > 0)
        {
            SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult placementResult = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticObjectPlacementResult();
            if (!isASync)
            {
                // If not running async, we can just add the results to the draw list right now
                placementResults.Add(new PlacementResult(placementResult.Clone() as SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult, gameObject));
            }
            else
            {
                queryStatus.QueryResult.Enqueue(new PlacementResult(placementResult.Clone() as SpatialUnderstandingDllObjectPlacement.ObjectPlacementResult, gameObject));
            }
            return true;
        }
        return false;
    }

    private void ProcessPlacementResults()
    {
        // Check it
        if (queryStatus.State != QueryStates.Finished)
        {
            return;
        }
        if (!SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
        {
            return;
        }

        // We will reject any above or below the ceiling/floor
        SpatialUnderstandingDll.Imports.QueryPlayspaceAlignment(SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceAlignmentPtr());
        SpatialUnderstandingDll.Imports.PlayspaceAlignment alignment = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceAlignment();

        // Copy over the results
        for (int i = 0; i < queryStatus.QueryResult.Count; ++i)
        {
            PlacementResult result = queryStatus.QueryResult.Dequeue();
            if ((result.Result.Position.y < alignment.CeilingYValue) &&
                (result.Result.Position.y > alignment.FloorYValue))
            {
                GameObject newGameObject = UnityEngine.Object.Instantiate(result.MyGameObject, result.Result.Position, Quaternion.LookRotation(result.Result.Forward, Vector3.up));
                newGameObject.AddComponent<HandDraggable>().enabled = false;
                newGameObject.AddComponent<BoxCollider>();
                newGameObject.AddComponent<ObjectProperties>();
                newGameObject.AddComponent<OnObjectTap>();
                ObjectProperties newObjectProperties = newGameObject.GetComponent<ObjectProperties>();
                if (!newObjectProperties.IsPropertiesAreSet)
                {
                    newObjectProperties.SetProperties(new ObjectProperties.ObjProperties(Placement.Floor, ObjectType.Table, "Table"));
                }
            }
        }

        // Mark done
        queryStatus.Reset();
    }
/*
    public void Query_OnFloor()
    {
        List<PlacementQuery> placementQuery = new List<PlacementQuery>();
        for (int i = 0; i < 4; ++i)
        {
            float halfDimSize = UnityEngine.Random.Range(0.15f, 0.35f);
            placementQuery.Add(
                new PlacementQuery(SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnFloor(new Vector3(halfDimSize, halfDimSize, halfDimSize * 2.0f)),
                                    new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>() {
                                        SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromOtherObjects(halfDimSize * 3.0f),
                                    }));
        }
        PlaceObjectAsync("OnFloor", placementQuery);
    }

    public void Query_OnWall()
    {
        List<PlacementQuery> placementQuery = new List<PlacementQuery>();
        for (int i = 0; i < 6; ++i)
        {
            float halfDimSize = UnityEngine.Random.Range(0.3f, 0.6f);
            placementQuery.Add(
                new PlacementQuery(SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnWall(new Vector3(halfDimSize, halfDimSize * 0.5f, 0.05f), 0.5f, 3.0f),
                                    new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>() {
                                        SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromOtherObjects(halfDimSize * 4.0f),
                                    }));
        }
        PlaceObjectAsync("OnWall", placementQuery);
    }

    public void Query_OnCeiling()
    {
        List<PlacementQuery> placementQuery = new List<PlacementQuery>();
        for (int i = 0; i < 2; ++i)
        {
            float halfDimSize = UnityEngine.Random.Range(0.3f, 0.4f);
            placementQuery.Add(
                new PlacementQuery(SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnCeiling(new Vector3(halfDimSize, halfDimSize, halfDimSize)),
                                    new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>() {
                                        SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromOtherObjects(halfDimSize * 3.0f),
                                    }));
        }
        PlaceObjectAsync("OnCeiling", placementQuery);
    }

    public void Query_OnEdge()
    {
        List<PlacementQuery> placementQuery = new List<PlacementQuery>();
        for (int i = 0; i < 8; ++i)
        {
            float halfDimSize = UnityEngine.Random.Range(0.05f, 0.1f);
            placementQuery.Add(
                new PlacementQuery(SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnEdge(new Vector3(halfDimSize, halfDimSize, halfDimSize),
                                                                                                                    new Vector3(halfDimSize, halfDimSize, halfDimSize)),
                                    new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>() {
                                        SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromOtherObjects(halfDimSize * 3.0f),
                                    }));
        }
        PlaceObjectAsync("OnEdge", placementQuery);
    }

    public void Query_OnFloorAndCeiling()
    {
        SpatialUnderstandingDll.Imports.QueryPlayspaceAlignment(SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceAlignmentPtr());
        SpatialUnderstandingDll.Imports.PlayspaceAlignment alignment = SpatialUnderstanding.Instance.UnderstandingDLL.GetStaticPlayspaceAlignment();
        List<PlacementQuery> placementQuery = new List<PlacementQuery>();
        for (int i = 0; i < 4; ++i)
        {
            float halfDimSize = UnityEngine.Random.Range(0.1f, 0.2f);
            placementQuery.Add(
                new PlacementQuery(SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnFloorAndCeiling(new Vector3(halfDimSize, (alignment.CeilingYValue - alignment.FloorYValue) * 0.5f, halfDimSize),
                                                                                                                            new Vector3(halfDimSize, (alignment.CeilingYValue - alignment.FloorYValue) * 0.5f, halfDimSize)),
                                    new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>() {
                                        SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromOtherObjects(halfDimSize * 3.0f),
                                    }));
        }
        PlaceObjectAsync("OnFloorAndCeiling", placementQuery);
    }

    public void Query_RandomInAir_AwayFromMe()
    {
        List<PlacementQuery> placementQuery = new List<PlacementQuery>();
        for (int i = 0; i < 8; ++i)
        {
            float halfDimSize = UnityEngine.Random.Range(0.1f, 0.2f);
            placementQuery.Add(
                new PlacementQuery(SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_RandomInAir(new Vector3(halfDimSize, halfDimSize, halfDimSize)),
                                    new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>() {
                                        SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromOtherObjects(halfDimSize * 3.0f),
                                        SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromPosition(CameraCache.Main.transform.position, 2.5f),
                                    }));
        }
        PlaceObjectAsync("RandomInAir - AwayFromMe", placementQuery);
    }

    public void Query_OnEdge_NearCenter()
    {
        List<PlacementQuery> placementQuery = new List<PlacementQuery>();
        for (int i = 0; i < 4; ++i)
        {
            float halfDimSize = UnityEngine.Random.Range(0.05f, 0.1f);
            placementQuery.Add(
                new PlacementQuery(SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnEdge(new Vector3(halfDimSize, halfDimSize, halfDimSize), new Vector3(halfDimSize, halfDimSize, halfDimSize)),
                                    new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>() {
                                        SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromOtherObjects(halfDimSize * 2.0f),
                                    },
                                    new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint>() {
                                        SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint.Create_NearCenter(),
                                    }));
        }
        PlaceObjectAsync("OnEdge - NearCenter", placementQuery);

    }

    public void Query_OnFloor_AwayFromMe()
    {
        List<PlacementQuery> placementQuery = new List<PlacementQuery>();
        for (int i = 0; i < 4; ++i)
        {
            float halfDimSize = UnityEngine.Random.Range(0.05f, 0.15f);
            placementQuery.Add(
                new PlacementQuery(SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnFloor(new Vector3(halfDimSize, halfDimSize, halfDimSize)),
                                    new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>() {
                                        SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromPosition(CameraCache.Main.transform.position, 2.0f),
                                        SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromOtherObjects(halfDimSize * 3.0f),
                                    }));
        }
        PlaceObjectAsync("OnFloor - AwayFromMe", placementQuery);
    }
    */


    //Placing object on floor near point
    public void PlaceObject_OnFloor_NearPoint(GameObject gameObject, Vector3 point)
    {
        List<PlacementQuery> placementQuery = new List<PlacementQuery>();
        Vector3 halfDimSize = gameObject.GetComponent<Renderer>().bounds.size * 0.5f;

        placementQuery.Add(
            new PlacementQuery(gameObject, SpatialUnderstandingDllObjectPlacement.ObjectPlacementDefinition.Create_OnFloor(halfDimSize),
                                new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule>() {
                                    SpatialUnderstandingDllObjectPlacement.ObjectPlacementRule.Create_AwayFromOtherObjects(gameObject.GetComponent<Renderer>().bounds.size.x),
                                },
                                new List<SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint>() {
                                    SpatialUnderstandingDllObjectPlacement.ObjectPlacementConstraint.Create_NearPoint(point)
                                }));
        PlaceObjectAsync("OnFloor - NearPoint", placementQuery);
    }

    public bool InitializeSolver()
    {
        if (IsSolverInitialized ||
            !SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
        {
            return IsSolverInitialized;
        }

        if (SpatialUnderstandingDllObjectPlacement.Solver_Init() == 1)
        {
            IsSolverInitialized = true;
        }
        return IsSolverInitialized;
    }

    private void Update()
    {
        // Can't do any of this till we're done with the scanning phase
        if (SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Done)
        {
            return;
        }

        // Make sure the solver has been initialized
        if (!IsSolverInitialized &&
            SpatialUnderstanding.Instance.AllowSpatialUnderstanding)
        {
            InitializeSolver();
        }

        // Handle async query results
        ProcessPlacementResults();
    }
}
