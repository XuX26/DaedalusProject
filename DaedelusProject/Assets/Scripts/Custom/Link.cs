using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Link
{
    public bool hasLock = false;
    public bool isSecret = false;
    public Node[] nodes = new Node[2];
    public LinkPos position;

    public Link(LinkPos newPos, Node parentNode)
    {
        position = newPos;
        nodes[0] = parentNode;
    }

    public Link(Node nodeFrom, Node newNode, LinkPos dir)
    {
        nodes[0] = nodeFrom;
        nodes[1] = newNode;
        position = dir;
    }
}
public enum LinkPos
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}