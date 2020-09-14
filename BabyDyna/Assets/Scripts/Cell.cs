using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [Tooltip("Nought Prefab")]
    public GameObject Nought;
    [Tooltip("Cross Prefab")]
    public GameObject Cross;

    [Tooltip("Which team has the cell. 0 (free), 1, or 2")]
    public int TeamId;

    [Tooltip("Which action selects this cell")]
    public int Row;
    public int Column;
    public int Action;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (TeamId)
        {
            case 1:
                if (!Nought.activeInHierarchy)
                    Nought.SetActive(true);
                if (Cross.activeInHierarchy)
                    Cross.SetActive(false);
                break;
            case 2:
                if (!Cross.activeInHierarchy)
                    Cross.SetActive(true);
                if (Nought.activeInHierarchy)
                    Nought.SetActive(false);
                break;
            default:
                if (Nought.activeInHierarchy)
                    Nought.SetActive(false);
                if (Cross.activeInHierarchy)
                    Cross.SetActive(false);
                break;
        }
    }
}
