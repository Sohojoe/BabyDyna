using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynaCell : MonoBehaviour
{
    [Tooltip("Hero Prefab")]
    public GameObject DynaHero;
    [Tooltip("Goal Prefab")]
    public GameObject DynaGoal;
    [Tooltip("Rock Prefab")]
    public GameObject DynaRock;

    [Tooltip("Debug Background")]
    public MeshRenderer DebugBackground;
    [Tooltip("Debug Up")]
    public GameObject DebugUp;
    [Tooltip("Debug Down")]
    public GameObject DebugDown;
    [Tooltip("Debug Left")]
    public GameObject DebugLeft;
    [Tooltip("Debug Right")]
    public GameObject DebugRight;



    [Tooltip("State of this cell (0=free, 1=rock, 2=goal, 3=hero)")]
    public int State;
    [Tooltip("State of this cell (Text)")]
    public string StateText;

    public Vector2Int Position;
    public float ModelUp;
    public float ModelRight;
    public float ModelDown;
    public float ModelLeft;
    public float QUp;
    public float QRight;
    public float QDown;
    public float QLeft;

    static float _maxQ;
    public float MaxQ;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (State)
        {
            case 1:
                if (!DynaRock.activeInHierarchy)
                    DynaRock.SetActive(true);
                if (DynaHero.activeInHierarchy)
                    DynaHero.SetActive(false);
                if (DynaGoal.activeInHierarchy)
                    DynaGoal.SetActive(false);
                StateText = DynaRock.name;
                break;
            case 2:
                if (!DynaGoal.activeInHierarchy)
                    DynaGoal.SetActive(true);
                if (DynaHero.activeInHierarchy)
                    DynaHero.SetActive(false);
                if (DynaRock.activeInHierarchy)
                    DynaRock.SetActive(false);
                StateText = DynaGoal.name;
                break;
            case 3:
                if (!DynaHero.activeInHierarchy)
                    DynaHero.SetActive(true);
                if (DynaGoal.activeInHierarchy)
                    DynaGoal.SetActive(false);
                if (DynaRock.activeInHierarchy)
                    DynaRock.SetActive(false);
                StateText = DynaHero.name;
                break;
            case 0:
            default:
                if (DynaHero.activeInHierarchy)
                    DynaHero.SetActive(false);
                if (DynaGoal.activeInHierarchy)
                    DynaGoal.SetActive(false);
                if (DynaRock.activeInHierarchy)
                    DynaRock.SetActive(false);
                StateText = "free";
                break;
        }
        var maxQ = Mathf.Max(QUp, QDown, QLeft, QRight);
        SetDebugArrows(maxQ);
        SetDebugBackground(maxQ);
        _maxQ = Mathf.Max(_maxQ, maxQ);
        MaxQ = _maxQ;
    }

    void SetDebugBackground(float maxQ)
    {
        Color color = new Color(0f,0f,0f,0f);
        if (DynaRock.activeInHierarchy)
        {
            DebugBackground.material.color = color;
            return;
        }

        if (maxQ >= 0f)
        {
            maxQ = Mathf.Min(maxQ, 1f);
            color = Color.green;
            color.a = maxQ;
            DebugBackground.material.color = color;
        }
        else
        {
            maxQ = Mathf.Min(-maxQ, 1f);
            color = Color.red;
            color.a = maxQ;
            DebugBackground.material.color = color;
        }
    }
    void SetDebugArrows(float maxQ)
    {
        bool setUp = false;
        bool setDown = false;
        bool setLeft = false;
        bool setRight = false;

        if (!DynaRock.activeInHierarchy)
        {
            if (Mathf.Approximately(maxQ, QUp))
                setUp = true;
            if (Mathf.Approximately(maxQ, QDown))
                setDown = true;
            if (Mathf.Approximately(maxQ, QLeft))
                setLeft = true;
            if (Mathf.Approximately(maxQ, QRight))
                setRight = true;
        }

        if (setUp && !DebugUp.activeInHierarchy)
            DebugUp.SetActive(true);
        else if (!setUp && DebugUp.activeInHierarchy)
            DebugUp.SetActive(false);

        if (setDown && !DebugDown.activeInHierarchy)
            DebugDown.SetActive(true);
        else if (!setDown && DebugDown.activeInHierarchy)
            DebugDown.SetActive(false);

        if (setLeft && !DebugLeft.activeInHierarchy)
            DebugLeft.SetActive(true);
        else if (!setLeft && DebugLeft.activeInHierarchy)
            DebugLeft.SetActive(false);

        if (setRight && !DebugRight.activeInHierarchy)
            DebugRight.SetActive(true);
        else if (!setRight && DebugRight.activeInHierarchy)
            DebugRight.SetActive(false);
    }
}
