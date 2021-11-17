using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Link
{
    public bool hasLock = false;
    public Node[] nodes = new Node[2];
    public LinkPos position;

    public Link(LinkPos newPos, Node parentNode)
    {
        position = newPos;
        nodes[0] = parentNode;
    }
}
public enum LinkPos
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}