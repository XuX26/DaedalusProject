using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    Vector2 position;
    [SerializeField] NodeType type;
    [SerializeField] Difficulty difficulty;
    [SerializeField, Range(1,4)] int nbrLinks;

    private Link[] links;

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
