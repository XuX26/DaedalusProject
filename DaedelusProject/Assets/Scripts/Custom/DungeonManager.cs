using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager instance;
    public Dictionary<Vector2, Node> allNodes = new Dictionary<Vector2, Node>();
    [Range(3, 20)] public int nbrCriticalRooms;
    [Range(2,10)] public int nbrAdditionalPath;

    [Range(0.25f,0.8f)] public float maxSideSize;


    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance);
        }
        instance = this;
    }

    private void Start()
    {
        //nbrAdditionalPath();
    }
}
