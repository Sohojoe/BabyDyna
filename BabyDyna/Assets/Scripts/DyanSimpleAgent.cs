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
    public int BoardWidth = 9;
    public int BoardHeight = 6;

    public int NumEpisodes = 50;
    public float Alpha = 0.1f;
    public float Gamma = 0.95f;
    public int EvalEpochs = 100;


    public List<Vector2Int> InitialRockPositions = new List<Vector2Int>{
        new Vector2Int(2,1),
        new Vector2Int(2,2),
        new Vector2Int(2,3),
        new Vector2Int(7,0),
        new Vector2Int(7,1),
        new Vector2Int(7,2),
        new Vector2Int(5,4),
    };
    public Vector2Int InitialHeroPosition = new Vector2Int(0,2);
    public Vector2Int InitialGoalPosition = new Vector2Int(8,0);


    SpawnableEnv _spawnableEnv;
    DynaGameBoard _gameBoard;

    Environment _env;

    
    [Header("State")]
    public Dictionary<Vector2Int, Dictionary<int, float>> Q;
    public Dictionary<Vector2Int, Dictionary<int, (float, Vector2Int)>> Model;
    public float TotalReward;
    public int EpisodeNum;
    public List<float> RunningAverage;
    public List<Vector2Int> StateMemory;
    public Dictionary<Vector2Int, List<int>> ActionMemory;
    

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

        _env = new Environment(
            BoardWidth, BoardHeight,
            InitialRockPositions,
            InitialHeroPosition,
            InitialGoalPosition
        );

        Q = new Dictionary<Vector2Int, Dictionary<int, float>>();
        Model = new Dictionary<Vector2Int, Dictionary<int, (float, Vector2Int)>>();
        foreach (var item in _env.States)
        {
            Q[item.Position] = new Dictionary<int, float>();
            Q[item.Position][0] = UnityEngine.Random.value * -.01f;
            Q[item.Position][1] = UnityEngine.Random.value * -.01f;
            Q[item.Position][2] = UnityEngine.Random.value * -.01f;
            Q[item.Position][3] = UnityEngine.Random.value * -.01f;
            Model[item.Position] = new Dictionary<int, (float, Vector2Int)>();
            Model[item.Position][(int)Environment.Actions.Up] = (0f, item.Position);
            Model[item.Position][(int)Environment.Actions.Down] = (0f, item.Position);
            Model[item.Position][(int)Environment.Actions.Left] = (0f, item.Position);
            Model[item.Position][(int)Environment.Actions.Right] = (0f, item.Position);
        }

        _gameBoard.InitializeBoard(_env);
        TotalReward = 0;
        EpisodeNum = 0;
        RunningAverage = new List<float>();
        StateMemory = new List<Vector2Int>();
        ActionMemory = new Dictionary<Vector2Int, List<int>>();
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
        TakeStep();
    }

    void TakeStep()
    {
        var s = _env.PlayerPos;
        var a = SampleAction(s);
        var priorState = s;
        if (!StateMemory.Contains(s))
        {
            StateMemory.Add(s);
        }
        if (!ActionMemory.ContainsKey(s))
        {
            ActionMemory[s] = new List<int>();
        }
        if (!ActionMemory[s].Contains(a))
        {
            ActionMemory[s].Add(a);
        }
        float r;
        bool done;
        (s, r, done) = _env.Step(a);
        _gameBoard.RenderBoard(_env);
        TotalReward += r;
        //     self.Q[p_s][a] += alpha * (r + (gamma * np.max(self.Q[s])) - self.Q[p_s][a])
        float delta = Alpha * (r + (Gamma * Max(Q[s])) - Q[priorState][a]);
        Q[priorState][a] += delta;
        //     self.model[p_s][a] = (r, s)
        if (!Model.ContainsKey(priorState))
        {
            Model[priorState] = new Dictionary<int, (float, Vector2Int)>();
        }
        Model[priorState][a] = (r,s);
        //     if done:
        //         s = env.reset()
        //         env.clear()
        //         episode_num += 1
        //         # print("Attained total reward at {}th episode: {}".format(episode_num, total_reward))
        //         # sleep(1.5)
        //         running_average.append(total_reward)
        //         total_reward = 0
        if (done)
        {
            s = _env.Reset();
            EpisodeNum += 1;
            print($"Attained total reward at {EpisodeNum}th episode: {TotalReward}");
            RunningAverage.Add(TotalReward);
            TotalReward = 0f;
        }
        //     for n in range(eval_epochs):
        //         s1 = np.random.choice(StateMemory)
        //         a1 = np.random.choice(ActionMemory[s1])
        //         r1, s_p1 = self.model[s1][a1]
        //         self.Q[s1][a1] += alpha * (r1 + (gamma * np.max(self.Q[s_p1])) - self.Q[s1][a1])
        // return running_average
        for (int i = 0; i < EvalEpochs; i++)
        {
            var s1Idx = Random.Range(0, StateMemory.Count);
            var s1 = StateMemory[s1Idx];
            var a1Idx = Random.Range(0, ActionMemory[s1].Count);
            int a1 = ActionMemory[s1][a1Idx];
            (var r1, var s_p1) = Model[s1][a1];
            delta = Alpha * (r1 + (Gamma * Max(Q[s_p1])) - Q[s1][a1]);
            Q[s1][a1] += delta;
        }
        _gameBoard.RenderModel(Model);
        _gameBoard.RenderQ(Q);
    }

    int SampleAction(Vector2Int state)
    {
        if (Random.value < 0.1f)
        {
            // return Random.Choice(new {0,1,2,3});
            return Random.Range(0, 3);
        }
        // argmax(Q[s])
        var qValues = Q[state];
        return Argmax(qValues);
    }

    int Argmax(Dictionary<int, float> values)
    {
        var indexs = new List<int>{};
        float max = float.MinValue;
        foreach (var item in values)
        {
            if (Mathf.Approximately(max, item.Value))
            {
                indexs.Add(item.Key);
            }
            else if (item.Value > max)
            {
                max = item.Value;
                indexs = new List<int>{item.Key};
            }
        }
        var idx = Random.Range (0, indexs.Count);
        return indexs[idx];
    }    
    float Max(Dictionary<int, float> values)
    {
        int idx = Argmax(values);
        return values[idx];
    }

}
