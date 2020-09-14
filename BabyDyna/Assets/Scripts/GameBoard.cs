using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.MLAgents.Sensors;

public class GameBoard : MonoBehaviour
{
    public Cell Cell;

    List<Cell> _cells;
    public int Size;

    public int BestScoreTeamId1;
    public int BestScoreTeamId2;
    public int LastRow;
    public int LastColumn;
    public int LastTeamId;
    public List<int> LastHorizontal;
    public List<int> LastVertical;
    public List<int> LastLeftToRight;
    public List<int> LastRightToLeft;

    bool _hasInitializedBoard;
    int _nextPlayerId;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void InitializeBoard(int size)
    {
        if (_hasInitializedBoard)
        {
            // check is same size
            return;
        }
        Size = size;
        _cells = new List<Cell>();
        Vector3 position = this.transform.position;
        position.x -= ((float)size-1) / 2f;
        position.y = 0f;
        position.z += ((float)size-1) / 2f;
        for (int row = 0; row < size; row++)
        {
            for (int column = 0; column < size; column++)
            {
                var cell = GameObject.Instantiate(Cell, position, this.transform.rotation);
                cell.transform.parent = this.transform;
                position.x += 1f;
                _cells.Add(cell);
                cell.Action = _cells.IndexOf(cell);
                cell.Row = row;
                cell.Column = column;
            }
            position.x -= (float)size;
            position.z -= 1f;
        }
        ResetBoard();
        _hasInitializedBoard = true;
    }
    public void ResetBoard()
    {
        foreach (var cells in _cells)
        {
            cells.TeamId = 0;
        }
        _nextPlayerId = 1;
        BestScoreTeamId1=0;
        BestScoreTeamId2=0;
    }

    public void CollectObservationsForPlayer(VectorSensor sensor, int playerId)
    {
        if (playerId == 1)
        {
            DoCollectObservationsForPlayer(sensor, 1);
            DoCollectObservationsForPlayer(sensor, 2);
        }
        else if (playerId == 2)
        {
            DoCollectObservationsForPlayer(sensor, 2);
            DoCollectObservationsForPlayer(sensor, 1);
        }
        else
        {
            throw new System.ArgumentException($"{nameof(playerId)}");
        }
    }
    void DoCollectObservationsForPlayer(VectorSensor sensor, int playerId)
    {
        foreach (var cell in _cells)
        {
            bool status = cell.TeamId == playerId;
            sensor.AddObservation(status);
        }
    }

    public List<Cell> GetFreeSpaces()
    {
        var freeSpaces = _cells
            .Where(x=>x.TeamId == 0)
            .ToList();
        return freeSpaces;
    }


    public void TakeAction(int action, int playerId)
    {
        var cell = _cells.First(x=>x.Action == action);
        cell.TeamId = playerId;
        _nextPlayerId = playerId == 1 ? 2 : 1;
        UpdateScore(action);
    }

    void UpdateScore(int action)
    {
        LastHorizontal = new List<int>();
        LastVertical = new List<int>();
        LastLeftToRight = new List<int>();
        LastRightToLeft = new List<int>();
        Cell cell = _cells.First(x=>x.Action == action);
        int row = cell.Row;
        int col = cell.Column;
        LastRow = cell.Row;
        LastColumn = cell.Column;
        LastTeamId = cell.TeamId;
        int offset = _cells.First(x=>x.Row == row && x.Column == 0).Action;
        for (int i = 0; i < Size; i++)
        {
            int idx = offset+i;
            cell = _cells[idx];
            LastHorizontal.Add(cell.TeamId);
        }
        offset = _cells.First(x=>x.Row == 0 && x.Column == col).Action;
        for (int i = 0; i < Size; i++)
        {
            int idx = offset+(i*Size);
            cell = _cells[idx];
            LastVertical.Add(cell.TeamId);
        }
        int neg = row < col ? row : col;
        offset = _cells.First(x=>x.Row == row-neg && x.Column == col-neg).Action;
        for (int i = 0; i < Size; i++)
        {
            if (col-neg + i >= Size)
                break;
            int idx = offset + i + (i*Size);
            if (idx >= _cells.Count)
                break;
            if (idx < 0)
                continue;
            cell = _cells[idx];
            LastLeftToRight.Add(cell.TeamId);
        }
        var negCol = Size-1-col;
        var offsetA = row > negCol ? negCol : row;
        offset = _cells.First(x=>x.Row == row-offsetA && x.Column == col+offsetA).Action;
        for (int i = 0; i < Size; i++)
        {
            if ((col+offsetA) - i < 0)
                break;
            int idx = offset - i;
            idx += i*Size;
            if (idx >= _cells.Count)
                break;
            if (idx < 0)
                continue;
            cell = _cells[idx];
            LastRightToLeft.Add(cell.TeamId);
        }
        string lastHorizontalStr = string.Join("", LastHorizontal);
        string lastVerticalStr = string.Join("", LastVertical);
        string lastLeftToRightStr = string.Join("", LastLeftToRight);
        string lastRightToLeftStr = string.Join("", LastRightToLeft);
        foreach (var i in Enumerable.Range(0,Size))
        {
            var score = Size-i;
            if (score > BestScoreTeamId1)
            {
                var target = string.Join("", Enumerable.Range(0,score).Select(x=>1));
                bool newHighScore = 
                    lastHorizontalStr.Contains(target)
                    | lastVerticalStr.Contains(target)
                    | lastLeftToRightStr.Contains(target)
                    | lastRightToLeftStr.Contains(target);
                if (newHighScore)
                    BestScoreTeamId1 = score;
            }
            if (score > BestScoreTeamId2)
            {
                var target = string.Join("", Enumerable.Range(0,score).Select(x=>2));
                bool newHighScore = 
                    lastHorizontalStr.Contains(target)
                    | lastVerticalStr.Contains(target)
                    | lastLeftToRightStr.Contains(target)
                    | lastRightToLeftStr.Contains(target);
                if (newHighScore)
                    BestScoreTeamId2 = score;
            }
        }
    }

    public bool HasEnded()
    {
        var freeSpace = _cells.FirstOrDefault(x=>x.TeamId == 0);
        return freeSpace == null;
    }

    public bool ShouldRequestDecision(int playerId)
    {
        // TODO if human, return false
        if (HasEnded())
            return false;
        return _nextPlayerId == playerId;
    }
}
