using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2Int position;
    public NodeType type;
    public Difficulty difficulty;
    [Range(1,4)] public int nbrLinks;

    public Link[] links;
    public List<LinkPos> linksPosition = new List<LinkPos>();

    public Node(int newNbrLink, NodeType newType=NodeType.DEFAULT, Difficulty newDifficulty= Difficulty.MEDIUM)
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

public enum Difficulty
{
    EASY,
    MEDIUM,
    HARD,
    INSANE
}
public enum LinkPos
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}
