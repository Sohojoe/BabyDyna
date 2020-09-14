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

    [Tooltip("State of this cell (0=free, 1=rock, 2=goal, 3=hero)")]
    public int State;
    [Tooltip("State of this cell (Text)")]
    public string StateText;

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
    }
}
