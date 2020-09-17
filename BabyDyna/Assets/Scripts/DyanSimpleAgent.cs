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
    public List<Vector4> Q;
    public Dictionary<int, Dictionary<int, (float, int)>> Model;
    public float TotalReward;
    public int EpisodeNum;
    public List<float> RunningAverage;
    public List<int> StateMemory;
    public Dictionary<int, List<int>> ActionMemory;
    

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

        Q = new List<Vector4>();
        Model = new Dictionary<int, Dictionary<int, (float, int)>>();
        foreach (var item in _env.States)
        {
            // var q = Enumerable.Range(0,4).Select(x=>UnityEngine.Random.value).ToList();
            // var m = Enumerable.Range(0,4).Select(x=>UnityEngine.Random.value).ToList();
            var q = new Vector4(
                UnityEngine.Random.value,
                UnityEngine.Random.value,
                UnityEngine.Random.value,
                UnityEngine.Random.value);
            // var m = new Vector4(
            //     UnityEngine.Random.value,
            //     UnityEngine.Random.value,
            //     UnityEngine.Random.value,
            //     UnityEngine.Random.value);
            Q.Add(q);
            // Model.Add(m);
            Model[item.Id] = new Dictionary<int, (float, int)>();

        }

        _gameBoard.InitializeBoard(_env);
        TotalReward = 0;
        EpisodeNum = 0;
        RunningAverage = new List<float>();
        StateMemory = new List<int>();
        ActionMemory = new Dictionary<int, List<int>>();
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
        var s = _env.PlayerIdx;
        var a = SampleAction(s);
        var priorState = s;
        StateMemory.Add(s);
        if (!ActionMemory.ContainsKey(s))
        {
            ActionMemory[s] = new List<int>();
        }
        ActionMemory[s].Add(a);
        float r;
        bool done;
        (s, r, done) = _env.Step(a);
        _gameBoard.RenderBoard(_env);
        TotalReward += r;
        //     self.Q[p_s][a] += alpha * (r + (gamma * np.max(self.Q[s])) - self.Q[p_s][a])
        float delta = Alpha * (r + (Gamma * Max(Q[s])) - GetIndex(Q[priorState], a));
        Q[priorState] = AddToIndex(Q[priorState], a, delta);
        //     self.model[p_s][a] = (r, s)
        if (!Model.ContainsKey(priorState))
        {
            Model[priorState] = new Dictionary<int, (float, int)>();
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
            delta = Alpha * (r1 + (Gamma * Max(Q[s_p1])) - GetIndex(Q[s1], a1));
            Q[s1] = AddToIndex(Q[s1], a1, delta);
        }
    }
    float GetIndex(Vector4 vector4, int idx)
    {
        switch (idx)
        {
            case 0:
                return vector4.x;
            case 1:
                return vector4.y;
            case 2:
                return vector4.z;
            case 3:
                return vector4.w;
            default:
                throw new System.ArgumentException();
        }
    }
    Vector4 SetIndex(Vector4 vector4, int idx, float value)
    {
        switch (idx)
        {
            case 0:
                vector4.x = value;
                break;
            case 1:
                vector4.y = value;
                break;
            case 2:
                vector4.z = value;
                break;
            case 3:
                vector4.w = value;
                break;
            default:
                throw new System.ArgumentException();
        }
        return vector4;
    }
    Vector4 AddToIndex(Vector4 vector4, int idx, float delta)
    {
        float curValue = GetIndex(vector4, idx);
        float value = curValue + delta;
        return SetIndex(vector4, idx, value);
    }
    int SampleAction(int state)
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

    int Argmax(Vector4 value)
    {
        var indexs = new List<int>{0};
        var max = value.x;
        // 1
        if (value.y == max)
            indexs.Add(1);
        else if (value.y > max)
        {
            indexs = new List<int>{1};
            max = value.y;
        }
        // 2
        if (value.z == max)
            indexs.Add(2);
        else if (value.z > max)
        {
            indexs = new List<int>{2};
            max = value.z;
        }
        // 3
        if (value.w == max)
            indexs.Add(3);
        else if (value.w > max)
        {
            indexs = new List<int>{3};
            max = value.w;
        }
        var idx = Random.Range (0, indexs.Count);
        return indexs[idx];
    }
    float Max(Vector4 vector4)
    {
        // if (
        //     vector4.x >= vector4.y && 
        //     vector4.x >= vector4.z && 
        //     vector4.x >= vector4.w)
        //     return vector4.x;
        // if (
        //     vector4.y >= vector4.x && 
        //     vector4.y >= vector4.z && 
        //     vector4.y >= vector4.w)
        //     return vector4.y;
        // if (
        //     vector4.z >= vector4.x && 
        //     vector4.z >= vector4.y && 
        //     vector4.z >= vector4.w)
        //     return vector4.z;
        // return vector4.w;
        int idx = Argmax(vector4);
        switch (idx)
        {
            case 0:
                return vector4.x;
            case 1:
                return vector4.y;
            case 2:
                return vector4.z;
            case 3:
                return vector4.w;
            default:
                throw new System.ArgumentException();
        }
    }
}
