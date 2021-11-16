using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager instance;
    public Dictionary<Vector2, Node> allNodes;


    // Start is called before the first frame update
    void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
    }
}
