using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using SpawnableEnvs;
using System.Linq;

public class DyanSimpleAgent : Agent
{
    // Start is called before the first frame update

    [Header("Settings")]
    // [Tooltip("The number of rows and columns in the grid.")]
    // public int Size = 3;

    SpawnableEnv _spawnableEnv;
    DynaGameBoard _gameBoard;

    void FixedUpdate()
    {
        RequestDecision();
        // if (_gameBoard.ShouldRequestDecision(PlayerId))
        // else if (_gameBoard.HasEnded())
        // {
        //     _gameBoard.ResetBoard();
        // }   
    }

    void Update()
    {
         
    }

    override public void Initialize() 
    {
        // grab access to objects
        _spawnableEnv = GetComponentInParent<SpawnableEnv>();
        _gameBoard = _spawnableEnv.GetComponentInChildren<DynaGameBoard>();
        // _mocapController = _spawnableEnv.GetComponentInChildren<MocapController>();

        // to do, error check the behavior paramaters
        
        _gameBoard.InitializeBoard();
    }

    override public void CollectObservations(VectorSensor sensor)
    {
        // _gameBoard.CollectObservationsForPlayer(sensor, PlayerId);
    }

    override public void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        
    }

    override public void OnActionReceived(float[] vectorAction) 
    {
        int action = (int)vectorAction[0];
        // _gameBoard.TakeAction(action, PlayerId);
    }

    override public void OnEpisodeBegin()
    {
        
    }

    override public void Heuristic(float[] actionsOut)
    {
        // var freeSpaces = _gameBoard.GetFreeSpaces();
        // if (freeSpaces.Count == 0)
        //     return;
        // int actionIdx = Random.Range(0, freeSpaces.Count);
        // var cell = freeSpaces[actionIdx];
        // int action = cell.Action;
        // actionsOut[0] = (float)action;
    }

}
