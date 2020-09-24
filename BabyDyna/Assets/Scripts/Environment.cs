using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class State
{
    public int Id;
    public Vector2Int Position;
    public float Reward;
    public bool IsRock;
    public bool IsGoal;
    public bool IsHero;
}

[System.Serializable]
public class Environment
{
    [Header("Settings")]

    public int BoardWidth;
    public int BoardHeight;
    [Header("State")]

    public List<State> States;
    public Vector2Int PlayerPos;
    public bool IsDone;
    public int PlayerIdx;

    List<Vector2Int> _initialRockPositions;
    Vector2Int _initialHeroPosition;
    Vector2Int _initialGoalPosition;

    public enum Actions
    {
        Left = 0,
        Up,
        Right,
        Down
    }

    public Environment(
        int width, 
        int height, 
        List<Vector2Int> initialRockPositions, 
        Vector2Int initialHeroPosition,
        Vector2Int initialGoalPosition)
    {
        BoardWidth = width;
        BoardHeight = height;
        _initialRockPositions = initialRockPositions;
        _initialHeroPosition = initialHeroPosition;
        _initialGoalPosition = initialGoalPosition;
        Reset();
    }
    public Vector2Int Reset()
    {
        IsDone = false;
        int id=0;
        if (States == null)
        {
            States = new List<State>();
            for (int y = 0; y < BoardHeight; y++)
            {
                for (int x = 0; x < BoardWidth; x++)
                {
                    var state = new State{
                        Id = id++,
                        Position = new Vector2Int(x,y),
                        Reward = -0.01f,
                        // Reward = -0.0f,
                    };
                    if (_initialRockPositions.Contains(state.Position))
                    {
                        state.IsRock = true;
                    }
                    if (state.Position == _initialGoalPosition)
                    {
                        state.IsGoal = true;
                        state.Reward = 1f;
                    }
                    if (state.Position == _initialHeroPosition)
                    {
                        state.IsHero = true;
                    }
                    States.Add(state);
                }                
            }
        }
        else
        {
            // reset player position
            var curState = States.First(x=>x.IsHero);
            curState.IsHero = false;
            var resetState = States.First(x=>x.Position == _initialHeroPosition);
            resetState.IsHero = true;
        }
        PlayerIdx = States.First(x=>x.IsHero).Id;
        PlayerPos = States.First(x=>x.IsHero).Position;
        return PlayerPos;
    }
    public (Vector2Int, float, bool) Step(int action)
    {
        Vector2Int targetPos = PlayerPos;
        switch ((Environment.Actions)action)
        {
            // case 1: // Up
            case Environment.Actions.Up:
                if (States[PlayerIdx].Position.y > 0)
                    targetPos.y -= 1;
                    TryMovePlayer(targetPos);
                    CheckDone();
                break;
            // case 0: // Left
            case Environment.Actions.Left:
                if (States[PlayerIdx].Position.x > 0)
                    targetPos.x -= 1;
                    TryMovePlayer(targetPos);
                    CheckDone();
                break;
            // case 3: // Down
            case Environment.Actions.Down:
               if (States[PlayerIdx].Position.y < BoardHeight-1)
                    targetPos.y += 1;
                    TryMovePlayer(targetPos);
                    CheckDone();
                break;
            // case 2: // Right
            case Environment.Actions.Right:
               if (States[PlayerIdx].Position.x < BoardWidth-1)
                    targetPos.x += 1;
                    TryMovePlayer(targetPos);
                    CheckDone();
                break;
            default:
                throw new System.NotImplementedException();
        }
        return (PlayerPos, States[PlayerIdx].Reward, IsDone);
    }
    void CheckDone()
    {
        if (States[PlayerIdx].IsGoal)
            IsDone = true;
    }
    void TryMovePlayer(Vector2Int pos)
    {
        var newState = States.First(x=>x.Position == pos);
        if (!newState.IsRock)
        {
            States[PlayerIdx].IsHero = false;
            PlayerPos = newState.Position;
            PlayerIdx = newState.Id;
            States[PlayerIdx].IsHero = true;
        }
    }

}