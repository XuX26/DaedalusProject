using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager instance;
    //public DungeonGenerator DungeonGenerator;
    public Dictionary<Vector2Int, Node> allNodes = new Dictionary<Vector2Int, Node>();

    [Range(3, 20)] public int nbrCriticalRooms = 12;
    [Range(1,8)] public int nbrLock = 2;
    [Range(0f, 1f)] public float coefSidePath = 0.5f;
    [Range(0.25f,1f)] public float maxSideSizeCoef = 0.25f;
    [Range(1,5)] public int minSideSize = 2;

    
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

    void Start()
    {
        //DungeonGenerator.CreateDungeon(nbrCriticalRooms);
    }

    void InitVar()
    {
        nbrLock = Mathf.Clamp(nbrLock, 1, nbrCriticalRooms-1);
        int maxSideSize = (int)(nbrCriticalRooms * maxSideSizeCoef);
        minSideSize = minSideSize > maxSideSize ? maxSideSize - 1 : minSideSize;
        //DungeonGenerator = GetComponent<DungeonGenerator>();
    }
}
