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
    public bool haveKey;

    public List<Link> links = new List<Link>();
    public List<int> freeLinks = new List<int>() { 0, 1, 2, 3 };
    //public List<LinkPos> availableLink = new List<LinkPos>();

    public Node(int newNbrLink, NodeType newType=NodeType.DEFAULT, int newDifficulty = 4)
    {
        nbrLinks = newNbrLink;
        difficulty = newDifficulty;
        type = newType;
    }
    
    public Node(Vector2Int newPosition, NodeType newType=NodeType.DEFAULT, Difficulty newDifficulty= Difficulty.MEDIUM)
    {
        position = newPosition;
        difficulty = newDifficulty;
        type = newType;
    }

    // Create and add new link then remove it from freeLinks and return this newLink
    public Link AddNewLink(LinkPos dir)
    {
        Link newLink = new Link(dir, this);
        links.Add(newLink);
        freeLinks.Remove((int)dir);
        return newLink;
    }
    
    // public int? GetRdmFreeLinkPos()
    // {
    //     if (freeLinks.Count == 0)
    //         return null;
    //
    //     return Random.Range(0, freeLinks.Count);
    //
    // }
}

public enum NodeType
{
    START,
    DEFAULT,
    END
}