using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    void CreateDungeon(int nbrRoom)
    {
        CreateNode(NodeType.START);
        for (int i = 1; i < nbrRoom-1; ++i)
        {
            CreateNode(NodeType.DEFAULT);
        }
        CreateNode(NodeType.END);
    }

    Node CreateNode(NodeType type)
    {
        Node node = null;
        switch (type)
        {
            case NodeType.START:
                node = new Node(1, NodeType.START, Difficulty.EASY);
                break;
            
            case NodeType.END:
                node = new Node(1, NodeType.END);
                break;
            
            case NodeType.DEFAULT:
                node = new Node(2, NodeType.DEFAULT);
                break;
        }
        return node;
    }

    void CreateLink(Node firstNode)
    {
        
        
    }
    
    
    
    
    
}


