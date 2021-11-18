using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2Int position;
    public NodeType type;
    [Range(1, 10)]
    public int difficulty = 1;
    [Range(1,4)] public int nbrLinks;

    public List<Link> links = new List<Link>();
    //public List<LinkPos> linksPosition = new List<LinkPos>();

    public Node(int newNbrLink, NodeType newType=NodeType.DEFAULT, int newDifficulty = 4)
    {
        nbrLinks = newNbrLink;
        difficulty = newDifficulty;
        type = newType;
    }
}

public enum NodeType
{
    START,
    DEFAULT,
    END
}