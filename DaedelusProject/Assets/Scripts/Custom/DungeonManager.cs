using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager instance;
    public Dictionary<Vector2Int, Node> allNodes = new Dictionary<Vector2Int, Node>();

    [Range(3, 20)] public int nbrCriticalRooms;
    [Range(1,8)] public int nbrLock = 2;
    [Range(0f, 1f)] public float coefSidePath;
    [Range(0.25f,1f)] public float maxSideSize;

    public Node currentNode;

    private bool RandomMadness; // TODO BONUS : full random mode, so generator random every nbr var

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance);
        }
        instance = this;
        
        InitVar();
    }

    private void Start()
    {
        //nbrAdditionalPath();
    }

    void InitVar()
    {
        nbrLock = Mathf.Clamp(nbrLock, 1, nbrCriticalRooms-1);
    }
}
