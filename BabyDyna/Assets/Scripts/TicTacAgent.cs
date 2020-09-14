using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using SpawnableEnvs;
using System.Linq;

public class TicTacAgent : Agent
{
    // Start is called before the first frame update

    [Header("Settings")]
    [Tooltip("The number of rows and columns in the grid.")]
    public int Size = 3;
    [Tooltip("How many units in a row are needed to win the game.")]
    public int WinCount = 3;
    [Tooltip("Pie rule allows the 2nd player to take the position of the 1st players 1st go")]
    public bool PieRule = false;
    [Tooltip("Which player. 1 or 2")]
    public int PlayerId = 1;


    SpawnableEnv _spawnableEnv;
    GameBoard _gameBoard;

    void FixedUpdate()
    {
        if (_gameBoard.ShouldRequestDecision(PlayerId))
            RequestDecision();
        else if (_gameBoard.HasEnded())
        {
            _gameBoard.ResetBoard();
        }   
    }

    void Update()
    {
         
    }

    override public void Initialize() 
    {
        // grab access to objects
        _spawnableEnv = GetComponentInParent<SpawnableEnv>();
        _gameBoard = _spawnableEnv.GetComponentInChildren<GameBoard>();
        // _mocapController = _spawnableEnv.GetComponentInChildren<MocapController>();

        // to do, error check the behavior paramaters
        
        _gameBoard.InitializeBoard(Size);
    }

    override public void CollectObservations(VectorSensor sensor)
    {
        _gameBoard.CollectObservationsForPlayer(sensor, PlayerId);
    }

    override public void CollectDiscreteActionMasks(DiscreteActionMasker actionMasker)
    {
        
    }

    override public void OnActionReceived(float[] vectorAction) 
    {
        int action = (int)vectorAction[0];
        _gameBoard.TakeAction(action, PlayerId);
    }

    override public void OnEpisodeBegin()
    {
        
    }

    override public void Heuristic(float[] actionsOut)
    {
        var freeSpaces = _gameBoard.GetFreeSpaces();
        if (freeSpaces.Count == 0)
            return;
        int actionIdx = Random.Range(0, freeSpaces.Count);
        var cell = freeSpaces[actionIdx];
        int action = cell.Action;
        actionsOut[0] = (float)action;
    }


    int GetMaxIndex(float[] vector)
    {
        float maxValue = float.MinValue;
        var draws = new List<int>();
        for (int i = 0; i < vector.Length; i++)
        {
            float value = vector[i];
            if (value > maxValue)
            {
                maxValue = value;
                draws = new List<int>();
                draws.Add(i);
            }
            if (value == maxValue)
            {
                draws.Add(i);
            }
        }
        var winIdx = Random.Range(0, draws.Count);
        var maxIdx = draws[winIdx];
        return maxIdx;

    } 


}
