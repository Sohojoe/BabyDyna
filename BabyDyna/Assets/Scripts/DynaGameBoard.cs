using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.MLAgents.Sensors;

public class DynaGameBoard : MonoBehaviour
{
    public DynaCell Cell;

    List<DynaCell> _cells;

    public bool _holdingGoal;
    public bool _holdingRock;
    public Vector2Int _lastHoldPosition;
    public Vector2Int _initialHoldPosition;
    DyanSimpleAgent _agent;

    bool _hasInitializedBoard;
    // Start is called before the first frame update
    void Start()
    {
        var parent = this.transform.parent;
        _agent = parent.GetComponentInChildren<DyanSimpleAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void InitializeBoard(Environment _env)
    {
        if (_hasInitializedBoard)
        {
            // check is same size
            return;
        }
        this.transform.localScale = new Vector3(
            _env.BoardWidth,
            this.transform.localScale.y,
            _env.BoardHeight
        );
        _cells = new List<DynaCell>();
        Vector3 position = this.transform.position;
        position.x -= ((float)_env.BoardWidth-1) / 2f;
        position.y = 0f;
        position.z += ((float)_env.BoardHeight-1) / 2f;
        for (int row = 0; row < _env.BoardHeight; row++)
        {
            for (int column = 0; column < _env.BoardWidth; column++)
            {
                var cell = GameObject.Instantiate(Cell, position, this.transform.rotation);
                cell.transform.parent = this.transform;
                position.x += 1f;
                _cells.Add(cell);
                // cell.Action = _cells.IndexOf(cell);
                // cell.Row = row;
                // cell.Column = column;
            }
            position.x -= (float)_env.BoardWidth;
            position.z -= 1f;
        }
        _holdingGoal = false;
        _holdingRock = false;
        _lastHoldPosition = new Vector2Int(-1,-1);
        RenderBoard(_env);
        RenderModel();
        RenderQ();
        _hasInitializedBoard = true;
    }
    public void RenderBoard(Environment _env)
    {
        for (int idx = 0; idx < _cells.Count; idx++)
        {
            _cells[idx].State = 0;
            _cells[idx].Position = _env.States[idx].Position;
            if (_env.States[idx].IsRock)
            {
                _cells[idx].State = 1;
            }
            else if (_env.States[idx].IsGoal)
            {
                _cells[idx].State = 2;
            }
            else if (_env.States[idx].IsHero)
            {
                _cells[idx].State = 3;
            }
        }
    }
    public void RenderModel(Dictionary<Vector2Int, Dictionary<int, (float, Vector2Int)>> model = null)
    {
        for (int idx = 0; idx < _cells.Count; idx++)
        {
            var position = _cells[idx].Position;
            if (model == null)
            {
                _cells[idx].ModelUp = 0f;
                _cells[idx].ModelDown = 0f;
                _cells[idx].ModelLeft = 0f;
                _cells[idx].ModelRight = 0f;
            }
            else
            {
                _cells[idx].ModelUp = model[position][(int)Environment.Actions.Up].Item1;
                _cells[idx].ModelDown = model[position][(int)Environment.Actions.Down].Item1;
                _cells[idx].ModelLeft = model[position][(int)Environment.Actions.Left].Item1;
                _cells[idx].ModelRight = model[position][(int)Environment.Actions.Right].Item1;
            }
        }        
    }
    public void RenderQ(Dictionary<Vector2Int, Dictionary<int, float>> q = null)
    {
        for (int idx = 0; idx < _cells.Count; idx++)
        {
            var position = _cells[idx].Position;
            if (q == null)
            {
                _cells[idx].QUp = 0f;
                _cells[idx].QDown = 0f;
                _cells[idx].QLeft = 0f;
                _cells[idx].QRight = 0f;
            }
            else
            {
                _cells[idx].QUp = q[position][(int)Environment.Actions.Up];
                _cells[idx].QDown = q[position][(int)Environment.Actions.Down];
                _cells[idx].QLeft = q[position][(int)Environment.Actions.Left];
                _cells[idx].QRight = q[position][(int)Environment.Actions.Right];
            }
        }        
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
            // bool status = cell.TeamId == playerId;
            // sensor.AddObservation(status);
        }
    }

    public List<DynaCell> GetFreeSpaces()
    {
        var freeSpaces = _cells
            .Where(x=>x.State == 0)
            .ToList();
        return freeSpaces;
    }

    public bool HasEnded()
    {
        // var freeSpace = _cells.FirstOrDefault(x=>x.TeamId == 0);
        // return freeSpace == null;
        return false;
    }

    public bool ShouldRequestDecision(int playerId)
    {
        // TODO if human, return false
        if (HasEnded())
            return false;
        return true;
    }
    public void CellOnMouseUpAsButton(Vector2Int position)
    {
        if (_holdingGoal)
        {
            return;
        }
        _agent.TryTogglePosition(position);
    }
    public void OnMouseDown(Vector2Int position, int state)
    {
        _holdingRock = false;
        _holdingGoal = false;
        _initialHoldPosition = position;
        _lastHoldPosition = position;
        // (0=free, 1=rock, 2=goal, 3=hero)
        if (state == 1) // rock
        {
            _holdingRock = true;
        }
        else if (state == 2) // goal
        {
            _holdingGoal = true;
        }
    }
    public void OnMouseDrag(Vector2Int position, int state)
    {
        if (position == _lastHoldPosition)
            return;
        if (_holdingGoal)
        {
            // (0=free, 1=rock, 2=goal, 3=hero)
            if (state == 0 || state == 1)
            {
                _agent.MoveGoal(position, _lastHoldPosition);
                _lastHoldPosition = position;
            }
        }
        
        if (_holdingRock)
        {
            // (0=free, 1=rock, 2=goal, 3=hero)
            if (state == 0)
            {
                _agent.MoveRock(position, _lastHoldPosition);
                _lastHoldPosition = position;
            }
        }
    }        
    public void OnMouseUp(Vector2Int position)
    {
        _holdingRock = false;
        _holdingGoal = false;
    }    
}
